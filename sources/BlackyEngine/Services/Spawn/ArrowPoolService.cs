using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using System.Collections.Concurrent;

namespace GodotFlecs.sources.Flecs.Services.Spawn;

public class ArrowPoolService
{
    private readonly ConcurrentStack<Entity> _pool;
    private readonly World _world;
    private readonly int _initialCapacity;

    public ArrowPoolService(World world, int initialCapacity = 2)
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

            // Aseguramos que inicie limpia y fuera de las consultas activas
            arrow.Set(new ProjectilePositionComponent());
            arrow.Set(new ProjectileVelocityComponent());
            arrow.Set(new ProjectileTargetComponent());

            // Se guarda en el stack inactivo
            _pool.Push(arrow);
        }
    }

    public bool TryGetAvailableArrow(out Entity arrowEntity)
    {
        if (_pool.TryPop(out arrowEntity))
        {
            return true;
        }

        // Si el pool se queda sin flechas, expandimos dinámicamente de forma segura
        arrowEntity = _world.Entity();
        //arrowEntity.Set(new ProjectilePositionComponent());
        //arrowEntity.Set(new ProjectileVelocityComponent());
        //arrowEntity.Set(new ProjectileTargetComponent());
        return true;
    }

    public void ReturnArrow(Entity arrowEntity)
    {
        // Limpiamos componentes o datos anteriores si es necesario antes de guardarla
        _pool.Push(arrowEntity);
    }
}