using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using System.Collections.Concurrent;

internal static class AttackSlotHelper
{
    // -------------------------------------------------
    // Cola de liberaciones pendientes, para usar desde
    // sistemas MultiThreaded. ConcurrentQueue es seguro
    // para múltiples threads llamando Enqueue en paralelo.
    // -------------------------------------------------
    private static readonly ConcurrentQueue<Entity> PendingReleases = new();

    // ---------------------------------------------------------
    // Reserva el primer slot libre en el anillo del target
    // y lo registra en la unidad atacante.
    //
    // ⚠️ Solo llamar desde sistemas single-threaded: escribe
    // directamente sobre AttackSlotsComponent del target.
    // ---------------------------------------------------------
    public static int AcquireAttackSlot(Entity attacker, Entity target, int maxSlots)
    {
        if (!target.IsAlive() || target.Has<DeadTag>())
        {
            return -1;
        }

        // AttackSlotsComponent ahora se agrega en el spawn de toda unidad/edificio
        // combatiente — aquí siempre debería existir. Igual dejamos el fallback
        // por seguridad, pero ya no debería ser el camino común.
        if (target.Has<AttackSlotsComponent>())
        {
            ref var  targetSlots = ref target.GetMut<AttackSlotsComponent>();
            int slotIndex = -1;

            for (int s = 0; s < maxSlots; s++)
            {
                if ((targetSlots.OccupiedMask & (1u << s)) == 0)
                {
                    slotIndex = s;
                    break;
                }
            }

            if (slotIndex == -1) slotIndex = 0;

            targetSlots.OccupiedMask |= (1u << slotIndex);
       

            attacker.Set(new AttackSlotComponent { Target = target, SlotIndex = slotIndex });

            return slotIndex;
        }
        return -1;

      
    }

    // ---------------------------------------------------------
    // Libera el slot que la unidad tenía reservado, si tenía.
    //
    // ⚠️ Solo llamar desde sistemas single-threaded: escribe
    // directamente sobre AttackSlotsComponent del target.
    // ---------------------------------------------------------
    public static void ReleaseAttackSlot(Entity attacker)
    {
        if (!attacker.Has<AttackSlotComponent>()) return;

        var slot = attacker.Get<AttackSlotComponent>(); // copia, no ref

        if (slot.Target.IsAlive() && slot.Target.Has<AttackSlotsComponent>())
        {
            var targetSlots = slot.Target.Get<AttackSlotsComponent>();
            targetSlots.OccupiedMask &= ~(1u << slot.SlotIndex);
            slot.Target.Set(targetSlots);
        }

        attacker.Remove<AttackSlotComponent>();
    }

    // ---------------------------------------------------------
    // Versión segura para llamar desde sistemas MultiThreaded.
    // No toca memoria de otra entidad directamente: solo encola
    // la solicitud para que AttackSlotReleaseSystem la procese
    // luego, single-threaded.
    // ---------------------------------------------------------
    public static void RequestReleaseAttackSlot(Entity attacker)
    {
        if (!attacker.Has<AttackSlotComponent>()) return;
        PendingReleases.Enqueue(attacker);
    }

    // ---------------------------------------------------------
    // Drena la cola de liberaciones pendientes. Llamar solo
    // desde un sistema single-threaded (ej. AttackSlotReleaseSystem).
    // ---------------------------------------------------------
    public static void ProcessPendingReleases()
    {
        while (PendingReleases.TryDequeue(out Entity attacker))
        {
            if (attacker.IsAlive())
            {
                ReleaseAttackSlot(attacker);
            }
        }
    }
}