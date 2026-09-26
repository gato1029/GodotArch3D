using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Units;

internal class CombatCollisionHelper
{
    public static bool CheckCollisionWithTargetBuild(Vector2 origin, Vector2 dirNormalized, float range, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.buildingPalette.GetData(templateId);
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.CheckAttackHalfCircle(
                origin.X, origin.Y,
                dirNormalized.X, dirNormalized.Y,
                range,
                targetPos.X, targetPos.Y,
                ref fast
            ))
            {
                return true;
            }
        }
        return false;
    }

    public static bool CheckCollisionWithTargetUnit(Vector2 origin, Vector2 dirNormalized, float range, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.characterPalette.GetData(templateId);
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.CheckAttackHalfCircle(
                origin.X, origin.Y,
                dirNormalized.X, dirNormalized.Y,
                range,
                targetPos.X, targetPos.Y,
                ref fast
            ))
            {
                return true;
            }
        }
        return false;
    }
}
