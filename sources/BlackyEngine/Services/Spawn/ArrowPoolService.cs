using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotFlecs.sources.Flecs.Components;
using System.Collections.Concurrent;

namespace GodotFlecs.sources.Flecs.Services.Spawn;

public struct PendingSpawnRequest
{
    public Vector2 Position;
    public Vector2 Velocity;
    public ProjectileTargetComponent TargetData;
}

public class ArrowPoolService
{
    private readonly ConcurrentStack<Entity> _pool;
    private readonly World _world;
    private readonly int _initialCapacity;
    private readonly ConcurrentQueue<PendingSpawnRequest> _pendingSpawns = new();
    private readonly ConcurrentQueue<Entity> _pendingRecycles = new();

    public ArrowPoolService(World world, int initialCapacity = 10)
    {
        _world = world;
        _initialCapacity = initialCapacity;
        _pool = new ConcurrentStack<Entity>();

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialCapacity; i++)
        {
            Entity arrow = _world.Entity();

            arrow.Set(new ProjectilePositionComponent());
            arrow.Set(new ProjectileVelocityComponent());
            arrow.Set(new ProjectileTargetComponent());

            _pool.Push(arrow);
        }
    }

    public bool TryGetAvailableArrow(out Entity arrowEntity)
    {
        return _pool.TryPop(out arrowEntity);
    }

    public void EnqueueSpawn(Vector2 pos, Vector2 vel, ProjectileTargetComponent targetData)
    {
        _pendingSpawns.Enqueue(new PendingSpawnRequest
        {
            Position = pos,
            Velocity = vel,
            TargetData = targetData
        });
    }

    // 🟢 Seguro para invocar desde hilos secundarios en el sistema de movimiento
    public void EnqueueRecycle(Entity arrowEntity)
    {
        _pendingRecycles.Enqueue(arrowEntity);
    }

    // 🔴 ESTO SE EJECUTA ESTRICTAMENTE EN EL HILO PRINCIPAL
    public void ProcessPendingSpawns()
    {
        while (_pendingSpawns.TryDequeue(out var req))
        {
            if (req.TargetData.Target.IsAlive() && !req.TargetData.Target.Has<DeadTag>())
            {
                if (!_pool.TryPop(out var arrowEntity))
                {
                    arrowEntity = _world.Entity();
                }
                if (arrowEntity.Has<RenderGPUComponent>())
                {
                    var gpu = arrowEntity.Ensure<RenderGPUComponent>();
                    if (gpu.instance!=-1) // quedo sucia debemos limpiarla
                    {
                        AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
                        gpu.rid = default;
                        gpu.instance = -1;
                    }
                }

                arrowEntity.Set(new ProjectilePositionComponent { Position = req.Position });
                arrowEntity.Set(new ProjectileVelocityComponent { Velocity = req.Velocity });
                arrowEntity.Set(req.TargetData);
                arrowEntity.Add<ActiveProjectileTag>();
            }


                        
        }
    }

    // 🔴 ESTO SE EJECUTA ESTRICTAMENTE EN EL HILO PRINCIPAL
    public void ProcessPendingRecycles()
    {
        while (_pendingRecycles.TryDequeue(out var arrowEntity))
        {
            // Limpieza segura de componentes visuales si la flecha llegó a renderizarse
            if (arrowEntity.Has<RenderGPUComponent>())
            {
                ref var gpu = ref arrowEntity.Ensure<RenderGPUComponent>();
                AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
                gpu.rid = default;
                gpu.instance = -1;
                
                
                
                //arrowEntity.Remove<RenderGPUComponent>();
                arrowEntity.Remove<RenderTransformComponent>();
                arrowEntity.Remove<RenderFrameDataComponent>();

            }

            // Cambios estructurales seguros en hilo principal
            arrowEntity.Remove<ActiveProjectileTag>();
            arrowEntity.Remove<ProjectileInitializedTag>();

            _pool.Push(arrowEntity);
        }
    }

    // Mantener por compatibilidad interna si se requiere, pero preferir EnqueueRecycle
    public void ReturnArrow(Entity arrowEntity)
    {
        _pool.Push(arrowEntity);
    }
}