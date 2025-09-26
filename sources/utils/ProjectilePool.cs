using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Multimesh;
using System;
using System.Collections.Generic;
using System.Threading;

public class ProjectilePool:SingletonBase<ProjectilePool>
{
    private World _world;
    public CommandBuffer _commandBuffer;

    //private static readonly ThreadLocal<Queue<Entity>> availableProjectiles =
    //new(() => new Queue<Entity>());

    private Queue<Entity> _availableProjectiles = new Queue<Entity>();

    private int _poolSize = 100;
    private int _maxPoolSize = 500;
    private float _cleanupTimer = 0f;
    private  float _cleanupInterval = 5f;
    protected override void Initialize()
    {
        _world = EcsManager.Instance.World;
        _commandBuffer = new CommandBuffer();        
        
        //_availableProjectiles = new Queue<Entity>();

        // Crear proyectiles iniciales
      //  ExpandPool(_poolSize);
    }

    protected override void Destroy()
    {
        //var _availableProjectiles = availableProjectiles.Value!;
        //int total = _availableProjectiles.Count;
        //for (int i = 0; i < total; i++)
        //{
        //    Entity projectile = _availableProjectiles.Dequeue();
        //    FreeProjectileGPU(projectile);   // liberar GPU
        //    _commandBuffer.Destroy(projectile);
        //}
        //if (total > 0)
           //_commandBuffer.Playback(_world);
    }
    public int GetTotal()
    {
        return _availableProjectiles.Count;
    }
    // Llamar desde tu sistema Update con deltaTime
    public void Update(float deltaTime)
    {
        _cleanupTimer += deltaTime;
        if (_cleanupTimer >= _cleanupInterval)
        {
            CleanupExcess();
            _cleanupTimer = 0f;
        }
    }

    // Obtener un proyectil del pool
    public Entity SpawnProjectile(CommandBuffer commandBuffer)
    {
        //var _availableProjectiles = availableProjectiles.Value!;
        if (_availableProjectiles.Count == 0)
        {
            // Si se acabaron, crear más proyectiles
            ExpandPool(1, commandBuffer);
        }

        Entity projectile = _availableProjectiles.Dequeue();
        commandBuffer.Set(projectile, new ActiveProjectileComponent { isActive = true });
        //   _commandBuffer.Playback(_world);
        return projectile;
    }

    // Devolver un proyectil al pool
    public void ReturnProjectile(Entity projectile, CommandBuffer commandBuffer)
    {
        //var _availableProjectiles = availableProjectiles.Value!;
        // Limpiar componentes
        //commandBuffer.Remove<ProjectileComponent>(projectile);
        //commandBuffer.Remove<DamageOnHitComponent>(projectile);
        //commandBuffer.Remove<TakeHitComponent>(projectile);
        FreeProjectileGPU(projectile);
        commandBuffer.Remove<SpriteRenderGPUComponent>(projectile);
        // Marcar como inactivo
        commandBuffer.Set(projectile, new ActiveProjectileComponent { isActive = false });
        _availableProjectiles.Enqueue(projectile);

       // _commandBuffer.Playback(_world);

       
    }

    // Expande el pool creando N proyectiles más
    private void ExpandPool(int count, CommandBuffer commandBuffer)
    {
        //var _availableProjectiles = availableProjectiles.Value!;
        for (int i = 0; i < count; i++)
        {
            Entity projectile = commandBuffer.Create(
                [

                    typeof(ProjectileComponent),
                    typeof(PositionComponent),
                    //typeof(SpriteRenderGPUComponent),
                    typeof(DamageOnHitComponent),
                    typeof(TakeHitComponent),
                    typeof(ActiveProjectileComponent),
                ]
            );

            //commandBuffer.Add<ActiveProjectileComponent>(projectile);
            _availableProjectiles.Enqueue(projectile);
        }

     //   _commandBuffer.Playback(_world);
        _poolSize += count;
    }

    // Elimina proyectiles inactivos si hay exceso
    private void CleanupExcess()
    {
        //var _availableProjectiles = availableProjectiles.Value!;
        int excess = _availableProjectiles.Count - _maxPoolSize;
        for (int i = 0; i < excess; i++)
        {
            Entity projectile = _availableProjectiles.Dequeue();
            FreeProjectileGPU(projectile);   // liberar GPU
            _commandBuffer.Destroy(projectile);
        }
        if (excess > 0)
            _commandBuffer.Playback(_world);
    }
    private  void FreeProjectileGPU(Entity projectile)
    {
        if (projectile.Has<SpriteRenderGPUComponent>())
        {
            ref var sprite = ref projectile.Get<SpriteRenderGPUComponent>();
            // Aquí llamas a tu función que libera la textura en GPU
            MultimeshManager.Instance.FreeInstance(sprite.rid, sprite.instance, sprite.idMaterial);
            RenderingServer.MultimeshInstanceSetCustomData(sprite.rid, sprite.instance, new Color(-1, -1, -1, -1));
            
        }
    }


}





