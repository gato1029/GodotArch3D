using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Multimesh;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
internal class CleanupSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsPostUpdate; // se ejecuta al final del frame
    protected override bool MultiThreaded => false; // importante: sólo main thread

    protected override void BuildQuery(ref QueryBuilder qb)
    {        
          qb.With<RenderGPUComponent>()
          .With<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var gpuArray = it.Field<RenderGPUComponent>(0);
        
        for (int i = 0; i < it.Count(); i++)
        {
            ref var gpu = ref gpuArray[i];
            var entity = it.Entity(i);

            MultimeshManager.Instance.FreeInstance(gpu.rid, gpu.instance, gpu.idMaterial);        
            // Eliminar del mundo Flecs
            entity.Destruct();
        }
    }
}
