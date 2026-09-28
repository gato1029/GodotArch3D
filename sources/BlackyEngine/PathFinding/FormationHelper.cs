using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

public enum FormationType
{
    Grid,
    Circle
    // Futuras formaciones van aquí: ej. Wedge, Line, Column, etc.
}
internal static class FormationOffsetHelper
{
    // Distancia (unidades del mundo) desde el destino final a partir de la cual
    // la formación empieza a abrirse. Con spacing 0.9 un valor de ~5-6 funciona bien.
    public const float BlendDistance = 6.4f; //5.4 medio

    public static Vector2 ComputeOffset(
        BlackyPathfinder pathfinder,
        Vector2 waypoint,
        Vector2 finalWaypoint,
        Vector2 pathDir,
        Vector2 offset,
        float radiusTiles)
    {
        if (offset == Vector2.Zero)
            return Vector2.Zero;

        // 0 = lejos del destino (sin offset), 1 = en el destino (offset completo)
        float distToEnd = waypoint.DistanceTo(finalWaypoint);
        float t = 1f - Mathf.Clamp(distToEnd / BlendDistance, 0f, 1f);

        if (t <= 0.001f)
            return Vector2.Zero;

        return FitOffset(pathfinder, waypoint, pathDir, offset * t, radiusTiles);
    }

    // Si el punto waypoint + offset no es transitable, comprime el offset:
    // primero el componente lateral (queda en columna) y, si no alcanza, lo anula.
    private static Vector2 FitOffset(
        BlackyPathfinder pathfinder,
        Vector2 waypoint,
        Vector2 pathDir,
        Vector2 offset,
        float radiusTiles)
    {
        Vector2 longitudinal = Vector2.Zero;
        if (pathDir.LengthSquared() > 0.001f)
            longitudinal = pathDir * offset.Dot(pathDir);

        Vector2 lateral = offset - longitudinal;

        float[] lateralScales = { 1f, 0.5f, 0f };

        foreach (float k in lateralScales)
        {
            Vector2 candidate = longitudinal + lateral * k;
            Vector2I tile = TilesHelper.WorldPositionToTile(waypoint + candidate);

            if (pathfinder.IsWalkableForRadius(tile, radiusTiles))
                return candidate;
        }

        return Vector2.Zero; // el waypoint central siempre es válido (lo validó el A*)
    }
}
public static class FormationHelper
{
    // ---------------------------------------------------------
    // Punto de entrada único: decide qué generador usar según
    // el tipo pedido. Todo el resto del código (CommandSelectedUnitsToMove)
    // solo necesita llamar a este método.
    // ---------------------------------------------------------
    public static List<Vector2> GenerateSlots(
        FormationType type,
        int unitCount,
        float spacing)
    {
        switch (type)
        {
            case FormationType.Circle:
                return GenerateCircleSlots(unitCount, spacing);

            case FormationType.Grid:
            default:
                return GenerateGridSlots(unitCount, spacing);
        }
    }

    public static List<Vector2> GenerateGridSlots(
        int unitCount,
        float spacing)
    {
        var slots = new List<Vector2>();

        if (unitCount <= 0)
            return slots;

        int columns =
            Mathf.CeilToInt(Mathf.Sqrt(unitCount));

        int rows =
            Mathf.CeilToInt(
                (float)unitCount / columns
            );

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (slots.Count >= unitCount)
                    break;

                float x =
                    (column - (columns - 1) * 0.5f)
                    * spacing;

                float y =
                    (row - (rows - 1) * 0.5f)
                    * spacing;

                slots.Add(
                    new Vector2(x, y)
                );
            }
        }

        return slots;
    }

    // ---------------------------------------------------------
    // Formación circular: distribuye las unidades en uno o más
    // anillos concéntricos alrededor del centro del grupo.
    // Puramente estética — no tiene relación con AttackSlotHelper.
    // ---------------------------------------------------------

   public static List<Vector2> GenerateCircleSlots(
    int unitCount,
    float spacing)
    {
        var slots = new List<Vector2>();

        if (unitCount <= 0)
            return slots;

        // Primer slot: el centro mismo (radio 0). Así con pocas unidades
        // (ej. 1) queda una parada en el centro exacto, en vez de arrancar
        // directamente en el primer anillo a distancia 'spacing'.
        slots.Add(Vector2.Zero);

        int remaining = unitCount - 1;

        if (remaining <= 0)
            return slots;

        float baseRadius = spacing;
        int ring = 0;

        while (remaining > 0)
        {
            float ringRadius = baseRadius * (ring + 1);

            int slotsInRing = Mathf.Min(
                remaining,
                Mathf.Max(1, Mathf.FloorToInt((2f * Mathf.Pi * ringRadius) / spacing))
            );

            for (int i = 0; i < slotsInRing; i++)
            {
                float angle = (Mathf.Tau / slotsInRing) * i;

                // Pequeño offset de fase por anillo para que no
                // queden todos alineados radialmente (más orgánico).
                angle += ring * 0.35f;

                float x = Mathf.Cos(angle) * ringRadius;
                float y = Mathf.Sin(angle) * ringRadius;

                slots.Add(new Vector2(x, y));
            }

            remaining -= slotsInRing;
            ring++;
        }

        return slots;
    }

    public static Dictionary<Entity, Vector2> AssignNearestSlots(
       List<Entity> entities,
       List<Vector2> slots,
       Vector2 groupCenter,
       Vector2 movementDirection)
    {
        var result = new Dictionary<Entity, Vector2>();

        var availableSlots = new HashSet<int>();

        for (int i = 0; i < slots.Count; i++)
        {
            availableSlots.Add(i);
        }

        foreach (var entity in entities)
        {
            if (!entity.IsAlive())
                continue;

            Vector2 position =
                entity.Get<PositionComponent>().position;

            int bestSlot = -1;
            float bestDistance = float.MaxValue;

            foreach (int slotIndex in availableSlots)
            {
                Vector2 worldSlot =
                    groupCenter + slots[slotIndex];

                float distance =
                    position.DistanceSquaredTo(worldSlot);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSlot = slotIndex;
                }
            }

            if (bestSlot >= 0)
            {
                availableSlots.Remove(bestSlot);
                result[entity] = slots[bestSlot];
            }
        }

        return result;
    }
}
//public static class FormationHelper
//{
//    public static List<Vector2> GenerateGridSlots(
//        int unitCount,
//        float spacing)
//    {
//        var slots = new List<Vector2>();

//        if (unitCount <= 0)
//            return slots;

//        int columns =
//            Mathf.CeilToInt(Mathf.Sqrt(unitCount));

//        int rows =
//            Mathf.CeilToInt(
//                (float)unitCount / columns
//            );

//        for (int row = 0; row < rows; row++)
//        {
//            for (int column = 0; column < columns; column++)
//            {
//                if (slots.Count >= unitCount)
//                    break;

//                float x =
//                    (column - (columns - 1) * 0.5f)
//                    * spacing;

//                float y =
//                    (row - (rows - 1) * 0.5f)
//                    * spacing;

//                slots.Add(
//                    new Vector2(x, y)
//                );
//            }
//        }

//        return slots;
//    }


//    public static Dictionary<Entity, Vector2> AssignNearestSlots(
//       List<Entity> entities,
//       List<Vector2> slots,
//       Vector2 groupCenter,
//       Vector2 movementDirection)
//    {
//        var result = new Dictionary<Entity, Vector2>();

//        var availableSlots = new HashSet<int>();

//        for (int i = 0; i < slots.Count; i++)
//        {
//            availableSlots.Add(i);
//        }

//        float angle = movementDirection.Angle();

//        foreach (var entity in entities)
//        {
//            if (!entity.IsAlive())
//                continue;

//            Vector2 position =
//                entity.Get<PositionComponent>().position;

//            int bestSlot = -1;
//            float bestDistance = float.MaxValue;

//            foreach (int slotIndex in availableSlots)
//            {
//                Vector2 rotatedSlot =
//                    slots[slotIndex].Rotated(angle);

//                Vector2 worldSlot =
//                    groupCenter + rotatedSlot;

//                float distance =
//                    position.DistanceSquaredTo(worldSlot);

//                if (distance < bestDistance)
//                {
//                    bestDistance = distance;
//                    bestSlot = slotIndex;
//                }
//            }

//            if (bestSlot >= 0)
//            {
//                availableSlots.Remove(bestSlot);

//                Vector2 rotatedOffset =
//                    slots[bestSlot].Rotated(angle);

//                result[entity] = rotatedOffset;
//            }
//        }

//        return result;
//    }
//}
