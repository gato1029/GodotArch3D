using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.utils;
using System;

namespace GodotFlecs.sources.Flecs.Systems
{
    public abstract class FlecsSystemBase
    {
        protected World World { get; private set; }
        protected SystemBuilder SystemBuilder { get; private set; }

        protected virtual string SystemName => GetType().Name;
        protected virtual ulong Phase => flecs.EcsOnUpdate;
        protected virtual bool MultiThreaded => true;
        protected virtual bool Enabled => true;
        protected virtual bool HasQuery => true;
        public Entity SystemEntity { get; private set; }

        public void Initialize(World world)
        {
            if (!Enabled)
            {
                GameLog.LogCat($"[FLECS] Sistema {SystemName} desactivado");
                return;
            }

            World = world;

            var sysBuilder = world.System(SystemName)
                .Kind(Phase)
                .MultiThreaded(MultiThreaded);
           
            if (HasQuery)
            {
                ref var qb = ref sysBuilder.QueryBuilder;
                BuildQuery(ref qb);
                qb.Build();
            }
   

            // 🔹 Si la clase derivada sobreescribe CustomRunner, se usa; sino, normal
            //var method = GetType().GetMethod(nameof(CustomRunner),
            //    System.Reflection.BindingFlags.Instance |
            //    System.Reflection.BindingFlags.NonPublic |
            //    System.Reflection.BindingFlags.Public |
            //    System.Reflection.BindingFlags.DeclaredOnly);

            //if (method != null)
            //{
            //    // ✅ El sistema define su propio runner
            //    SystemEntity = sysBuilder
            //        .Run(CustomRunner)
            //        .Iter(OnIter);
            //}
            //else
            {
                // ⚙️ Sin runner custom → comportamiento por defecto
                SystemEntity = sysBuilder.Iter(OnIter);
            }

            GameLog.LogCat($"[FLECS] Sistema registrado: {SystemName} " +
                              $"(Phase: {GetPhaseName(Phase)}, Threads: {MultiThreaded})");
        }

        protected virtual void CustomRunner(Iter it, Action<Iter> callback) { }
        // 🔹 Cambia la firma: recibe el QueryBuilder por referencia
        protected abstract void BuildQuery(ref QueryBuilder qb);

        protected abstract void OnIter(Iter it);

        private string GetPhaseName(ulong phase)
        {
            if (phase == flecs.EcsOnUpdate) return "OnUpdate";
            if (phase == flecs.EcsPreUpdate) return "PreUpdate";
            if (phase == flecs.EcsPostUpdate) return "PostUpdate";
            if (phase == flecs.EcsOnValidate) return "OnValidate";
            if (phase == flecs.EcsOnStore) return "OnStore";
            return "CustomPhase";
        }
    }
}
