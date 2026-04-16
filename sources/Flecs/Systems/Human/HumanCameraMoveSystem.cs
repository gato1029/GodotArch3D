using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Human;
internal class HumanCameraMoveSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PlayerInputComponent>()           
            .With<PositionComponent>()
         ;

    }

    protected override void OnIter(Iter it)
    {
        var playerArray = it.Field<PlayerInputComponent>(0);       
        var posArray = it.Field<PositionComponent>(1);
        var camera = RenderManager.Instance.camera3D;
        float lerpSpeed = 8f;
        for (int i = 0; i < it.Count(); i++)
        {        
            ref var pos = ref posArray[i];
            ref var inp = ref playerArray[i];
            if (inp.moveDirection!=Godot.Vector2.Zero)
            {
                camera.GlobalPosition = camera.GlobalPosition.Lerp(
                    new Godot.Vector3(pos.position.X, pos.position.Y, camera.Position.Z), lerpSpeed * (float)it.DeltaTime());
            }
            
        }
    }
}