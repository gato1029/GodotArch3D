using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
internal class ApplyDamageSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // ✅ single-thread, para aplicar daño real
    protected override bool HasQuery => false;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        
    }

    protected override void OnIter(Iter it)
    {
        // TryDequeue saca el elemento y devuelve true si había uno, o false si está vacía.
        while (GlobalData.EventsDamage.TryDequeue(out var ev))
        {
            // ✅ Procesá el evento de forma segura
            if (ev.Target.IsAlive() &&
                !ev.Target.Has<DestroyRequestTag>() &&
                !ev.Target.Has<DeadTag>())
            {
                ev.Target.Set(new DamagePendingComponent
                {
                    Amount = ev.Amount,
                    Source = ev.Source
                });
            }
        }

    }
}
