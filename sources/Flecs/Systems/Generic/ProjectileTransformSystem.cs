using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;

namespace GodotFlecs.sources.Flecs.Systems.Generic;

internal class ProjectileTransformSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<ProjectilePositionComponent>()
          .With<RenderGPUComponent>()
          .With<RenderTransformComponent>()
          .With<ActiveProjectileTag>()
          .With<ProjectileInitializedTag>();
    }

    protected override void OnIter(Iter it)
    {
        var pos = it.Field<ProjectilePositionComponent>(0);
        var ren = it.Field<RenderGPUComponent>(1);
        var trans = it.Field<RenderTransformComponent>(2);
        for (int i = 0; i < it.Count(); i++)
        {
            var e = it.Entity(i);
            ref var p = ref pos[i];
            ref var r = ref ren[i];
            ref var t = ref trans[i];

            float depthOffset = (r.depthOffset);

            float z = CommonAtributes.Calculate(depthOffset, 10, r.layerRender, p.Position); // debemos usar esto apartir de ahora
            var tt = t.transform;
            tt.Origin = new Vector3(p.Position.X + r.originOffset.X, p.Position.Y + r.originOffset.Y, z);
            t.transform = tt;
        }
    }
}
