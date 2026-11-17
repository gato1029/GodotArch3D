
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Simulation;
using GodotEcsArch.sources.Flecs.Systems.Collisions;
using GodotEcsArch.sources.Flecs.Systems.Debug;
using GodotEcsArch.sources.Flecs.Systems.Generic;
using GodotEcsArch.sources.Flecs.Systems.Human;
using GodotEcsArch.sources.Flecs.Systems.Projectiles;
using GodotEcsArch.sources.Flecs.Systems.Transforms;
using GodotEcsArch.sources.Flecs.Systems.Units;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using GodotFlecs.sources.Flecs.Systems.Generic;
using GodotFlecs.sources.Flecs.Systems.Units;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using GodotEcsArch.sources.managers.Characters;

namespace GodotFlecs.sources.Flecs;

[AttributeUsage(AttributeTargets.Struct)]
public class RegisterComponentFlecsAttribute : Attribute
{
}

public class FlecsManager 
{
    private World worldFlecs;
    private Rid ridWorld3D;    
    private Node3D main3D;
    private readonly List<FlecsSystemBase> systems = new();
    public World WorldFlecs { get => worldFlecs; set => worldFlecs = value; }
// 🔹 Conjunto para llevar el control de los tipos ya registrados y evitar duplicados
    private readonly HashSet<Type> _registeredComponents = new();
    // Cola thread-safe para ejecutar acciones en el hilo principal
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new();

    public FlecsManager(Node3D node3D)
    {
        //WorldFlecs.Reset();
        WorldFlecs = World.Create();
        WorldFlecs.SetThreads(15);
        WorldFlecs.App();
      

      //  FlecsComponentRegistry.RegisterAll(this);
       // RegisterComponentsAssemblies();
       
       GeneratedFlecsRegistry.Build(worldFlecs);
       RegisterSystems();
        
        //WorldFlecs.Component<EnemySearchComponent>().Entity.Add(Ecs.CanToggle);
        //WorldFlecs.Component<AttackPendingComponent>().Entity.Add(Ecs.CanToggle);
        WorldFlecs.Import<Ecs.Stats>();
        WorldFlecs.Set(new flecs.EcsRest { port = 27750});
        
       
        
        SetNode3DMain(node3D);
    }

    //protected override void Initialize()
    //{  // 🔹 Si ya había un mundo, lo destruimos correctamente
    //    if (WorldFlecs != null )
    //    {
    //        WorldFlecs.Dispose();
    //        GameLog.LogCat("[Flecs] Destruyendo mundo anterior...");            
    //        WorldFlecs = null;
    //    }

    //    WorldFlecs = World.Create();
    //    WorldFlecs.SetThreads(15);
    //    WorldFlecs.App();

    //    RegisterComponentsAssemblies();        
    //    RegisterSystems();

    //    WorldFlecs.Import<Ecs.Stats>();
    //    WorldFlecs.Set<flecs.EcsRest>(default);
    //}
    public void Destroy()
    {
        WorldFlecs.Dispose();
        //WorldFlecs.Quit();
        //WorldFlecs.DeleteWith();
    }
    public void SetNode3DMain(Node3D node3D)
    {
        main3D = node3D;
        ridWorld3D = node3D.GetWorld3D().Scenario;
    }    
    /// <summary>
    /// Encola una acción para ejecutarse en el hilo principal.
    /// </summary>
    public void RunOnMainThread(Action action)
    {
        if (action == null)
            return;

        _mainThreadQueue.Enqueue(action);
    }
    public void Update(float dt)
    {
        // Ejecuta todas las acciones pendientes en la cola
        while (_mainThreadQueue.TryDequeue(out var action))
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"❌ Error ejecutando acción en main thread: {ex}");
            }
        }
        WorldFlecs.Progress();
    }

    /// <summary>
    /// Limpia recursos o destruye entidades desde el hilo principal.
    /// </summary>
    public void DestroyEntitySafe(Entity entity)
    {
        if (entity.IsAlive())
        {
            RunOnMainThread(() =>
            {
                if (entity.IsAlive())
                {
                    entity.Destruct();
                    //GD.Print($"🧹 Entidad {entity} destruida correctamente en main thread.");
                }
            });
        }
    }
    public void SystemInternals()
    {
 

    }
    private void RegisterSystems()
    {
        //pre
        RegisterSystem<SimulationTickSystem>();
        //RegisterSystem<BatchPrecalSystem>();  // revisar si no se usa, de ser asi quitarlo
        //RegisterSystem<BuildingSpawnSystem>(); // revisar creo q no se usa quitarlo

        //update
        //RegisterSystem<HumanInputSystem>();
        //RegisterSystem<HumanCharacterSystem>();
        //RegisterSystem<HumanCharacterSimpleSystem>();
        // RegisterSystem<UnitMelleEnemySearchSystem>();
        //   RegisterSystem<RangedEnemySearchSystem>();
        RegisterSystem<UniversalEnemySearchSystem>();
        RegisterSystem<UnitChaseSystem>();
        RegisterSystem<PathManagerSystem>();
        RegisterSystem<AttackingPathTransitionSystem>();
        RegisterSystem<UnitChaseSystem>();
        // RegisterSystem<MoveTargetAttackMelleSystem>();
        RegisterSystem<MoveTargetSystem>();
        RegisterSystem<FlowFieldFollowSystem>();

        // aqui antes el hash de colliders

        //RegisterSystem<ResolveTerrainCollisionSystem>(); // resuelve colisiones con el terreno, debe ir antes de movimiento para ajustar la posición       





        //RegisterSystem<ApplyOverlapCorrectionsSystem>(); // <- nuevo
        //RegisterSystem<FlowFieldUnstickSystem>();
        //RegisterSystem<SteeringSystem>();
        RegisterSystem<GridSteeringSystem>();
        RegisterSystem<CrowdSteeringSystem>();
        //RegisterSystem<MoveSeparationSystem>(); // resuelve colisiones entre entidades, debe ir antes de movimiento para ajustar la posición
        RegisterSystem<MovementResolutionSystem>();

        //RegisterSystem<DebugOverlapCheckSystem>(); // solo para debug
        RegisterSystem<ProjectileMovementSystem>();

        //RegisterSystem<MovementFreeUnitTargetSytem>(); // libera el target de movimiento si la unidad está bloqueada o no puede llegar al target

        RegisterSystem<DirectionSystem>();
        RegisterSystem<UnitMeleeAttackSystem>();
        RegisterSystem<UnitRangedAttackExecutionSystem>();
        //RegisterSystem<UnitRangedAttackSystem>();

        //RegisterSystem<BuildRangedAttackSystem>();

        //RegisterSystem<AttackHitSystem>();
        //RegisterSystem<ProjectileMoveSystem>(); 

        RegisterSystem<ApplyDamageSystem>(); // aplica el daño a las unidades/buildings single thread
        RegisterSystem<UnitDamageApplySystem>(); // aplica efectos de daño a las unidades
        RegisterSystem<BuildingDamageApplySystem>(); // aplica efectos de daño a los edificios

        // transform
        // RegisterSystem<SpriteTransformStaticSystem>(); revisar si no usa, de ser asi quitarlo
        RegisterSystem<ProjectileTransformSystem>();
        RegisterSystem<SpriteTransformSystem>();
        RegisterSystem<SpriteSelectorTransformSystem>();
        //RegisterSystem<SpriteTransformLayerSystem>();
        RegisterSystem<TileSpriteTextureTransformSystem>();

        // estados
        //RegisterSystem<CharacterStateLayerSystem>();        
        //RegisterSystem<StateCharacterSystem>();

        // animaciones
      //  RegisterSystem<AnimationLayerUpdateSystem>();
      //  RegisterSystem<AnimationSystem>();
        // RegisterSystem<AnimationSimpleSpriteSystem>(); // ya no es necesario

        // render
        RegisterSystem<BarLifeTransformSystem>();
  //      RegisterSystem<RenderSimpleSpriteSystem>();
   //     RegisterSystem<RenderSpriteSystem>();
 //       RegisterSystem<LayeredSpriteRenderSystem>();
   //     RegisterSystem<RenderTileSpriteTextureSystem>();
        RegisterSystem<HumanCameraMoveSystem>();

        RegisterSystem<RvoDebugSystem>();

        RegisterSystem<RegisterFastHashSystem>();
        RegisterSystem<GridSparseClearSystem>();
        RegisterSystem<GridSparseRegisterSystem>();
        RegisterSystem<CrowdRegisterSystem>();

        // creadores y spawn
        RegisterSystem<GpuStateBufferSystem>();
        RegisterSystem<ProjectileSpawnSystem>();
        RegisterSystem<ArrowVisualSyncSystem>();
        RegisterSystem<RefreshUpdateCreationGlobalSystem>();
        //post        
        RegisterSystem<DeathCleanupSystem>(); // limpieza de unidades muertas
        RegisterSystem<DestroyCleanupSystem>(); // limpieza de edificios destruidos
        RegisterSystem<RefreshUpdateGpuDriven>();


    }


    // 🔹 Método genérico para registrar cualquier sistema que herede FlecsSystemBase
    public T RegisterSystem<T>() where T : FlecsSystemBase, new()
    {
        var system = new T();
        system.Initialize(worldFlecs);

        systems.Add(system);

        GameLog.LogCat($"[FLECS] Registrado sistema: {typeof(T).Name}");
        return system;
    }
   
}
