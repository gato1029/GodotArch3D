using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

internal class ApplyOverlapCorrectionsSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate; // después de SteeringSystem
    protected override bool MultiThreaded => false;
    protected override bool HasQuery => false;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        // Sin query de entidades: solo drena la cola.
        // Ajusta según cómo manejes "sistemas sin query" en tu engine.
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var dynGrid = world.State.DynamicHash;
        var queue = world.State.PendingOverlapCorrections;

        const float correctionFactor = 0.5f;

        while (queue.TryDequeue(out var pair))
        {
            Entity self = dynGrid.GetEntity(pair.SelfSID);
            Entity other = dynGrid.GetEntity(pair.OtherSID);

            if (!self.IsAlive() || !other.IsAlive()) continue;
            if (!self.Has<PositionComponent>() || !other.Has<PositionComponent>()) continue;

            ref var selfPos = ref self.GetMut<PositionComponent>();
            ref var otherPos = ref other.GetMut<PositionComponent>();

            Vector2 correction = new Vector2(pair.DirX, pair.DirY) * (pair.Overlap * correctionFactor);

            selfPos.position += correction;
            otherPos.position -= correction;
        }
    }
}