using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using GodotFlecs.sources.Flecs.Components;

namespace GodotFlecs.sources.Flecs.Systems.Units;

internal class UnitChaseSystem : FlecsSystemBase
{
    private const int SlotsPerRing = 8;  // Unidades por anillo alrededor del target
    private const int MaxSlots = 32;     // Debe calzar con el ancho de OccupiedMask (uint)

    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<AttackPendingComponent>()
          .With<ChaseTargetTag>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var posArray = it.Field<PositionComponent>(0);
        var attackPendingArray = it.Field<AttackPendingComponent>(1);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var atp = ref attackPendingArray[i];
            var e = it.Entity(i);

            // 1. Validar si el objetivo sigue vivo
            if (!atp.Target.IsAlive() || atp.Target.Has<DeadTag>())
            {
                CleanupChase(e);
                continue;
            }

            Vector2 currentTargetPos = atp.Target.Get<PositionComponent>().position;

            // 2. Comportamiento para UNIDADES MELEE
            if (e.Has<MeleeAttackComponent>())
            {
                ref var melee = ref e.GetMut<MeleeAttackComponent>();
                float distSqr = pos.position.DistanceSquaredTo(currentTargetPos);
                float attackRangeSqr = melee.RangeAttack * melee.RangeAttack;

                if (distSqr <= attackRangeSqr)
                {
                    // ¡Llegó al rango cuerpo a cuerpo! 
                    // Ya no necesita el slot de aproximación para caminar, lo liberamos o mantenemos según tu diseño de combate,
                    // y activamos el estado de ataque pendiente.
                    AttackSlotHelper.RequestReleaseAttackSlot(e);

                    e.Remove<ChaseTargetTag>();
                    e.Add<AttackPendingTag>();
                    e.Add<StoppedTag>();
                    if (e.Has<MoveTargetComponent>()) e.Remove<MoveTargetComponent>();
                }
                else
                {
                    // AÚN LEJOS: Gestionar slot de rodeo/aproximación en anillo
                    int slotIndex = GetOrAcquireAttackSlot(e, atp.Target);
                    if (slotIndex == -1) continue; // Si todos los slots están llenos, esperamos al siguiente ciclo

                    // Calcular el punto de aproximación en el anillo alrededor del objetivo
                    int ring = slotIndex / SlotsPerRing;
                    int slotInRing = slotIndex % SlotsPerRing;
                    float angle = slotInRing * (MathF.Tau / SlotsPerRing);
                    float ringRadius = melee.RangeAttack - 0.85f + ring * (melee.RangeAttack * 0.5f);

                    Vector2 slotDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                    Vector2 approachPoint = currentTargetPos + slotDir * ringRadius;

                    // Actualizar el destino de movimiento hacia el slot asignado
                    UpdateMoveTargetIfNeeded(e, ref atp, approachPoint);
                    if (e.Has<StoppedTag>()) e.Remove<StoppedTag>();
                }
            }
            else if (e.Has<RangedAttackComponent>())
            {
                // COMOPORTAMIENTO PARA RANGO (Los arqueros no usan slots de cerco, van directo a su radio de tiro)
                ref var ranged = ref e.GetMut<RangedAttackComponent>();
                float distSqr = pos.position.DistanceSquaredTo(currentTargetPos);
                float rangeSqr = ranged.Range * ranged.Range;

                if (distSqr <= rangeSqr)
                {
                    e.Remove<ChaseTargetTag>();
                    e.Add<AttackPendingTag>();
                    e.Add<StoppedTag>();
                    if (e.Has<MoveTargetComponent>()) e.Remove<MoveTargetComponent>();
                }
                else
                {
                    UpdateMoveTargetIfNeeded(e, ref atp, currentTargetPos);
                    if (e.Has<StoppedTag>()) e.Remove<StoppedTag>();
                }
            }
            else
            {
                CleanupChase(e);
            }
        }
    }

    private int GetOrAcquireAttackSlot(Entity attacker, Entity target)
    {
        int slotIndex;

        if (attacker.Has<AttackSlotComponent>())
        {
            var mySlot = attacker.Get<AttackSlotComponent>();
            if (mySlot.Target == target && mySlot.Target.IsAlive() && !mySlot.Target.Has<DeadTag>())
            {
                slotIndex = mySlot.SlotIndex;
            }
            else
            {
                AttackSlotHelper.RequestReleaseAttackSlot(attacker);
                slotIndex = AttackSlotHelper.AcquireAttackSlot(attacker, target, MaxSlots);
            }
        }
        else
        {
            slotIndex = AttackSlotHelper.AcquireAttackSlot(attacker, target, MaxSlots);
        }

        return slotIndex;
    }

    private void UpdateMoveTargetIfNeeded(Entity e, ref AttackPendingComponent atp, Vector2 newTargetPos)
    {
        const float repositionThresholdSqr = 0.5f * 0.5f;
        bool needsUpdate = false;

        if (!e.Has<MoveTargetComponent>())
        {
            needsUpdate = true;
        }
        else
        {
            var existingTarget = e.Get<MoveTargetComponent>();
            if (existingTarget.Value.DistanceSquaredTo(newTargetPos) >= repositionThresholdSqr)
            {
                needsUpdate = true;
            }
        }

        if (needsUpdate)
        {
            atp.targetPosition = newTargetPos;
            e.Set(new MoveTargetComponent(newTargetPos));
        }
    }

    private void CleanupChase(Entity e)
    {
        e.Remove<ChaseTargetTag>();
        e.Remove<AttackPendingTag>();
        if (e.Has<MoveTargetComponent>()) e.Remove<MoveTargetComponent>();
        AttackSlotHelper.RequestReleaseAttackSlot(e);
        
        if (e.Has<AttackPendingComponent>())
        {
            ref var atp = ref e.GetMut<AttackPendingComponent>();
            atp.Active = false;
            atp.Target = default;
        }
    }
}