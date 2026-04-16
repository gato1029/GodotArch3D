using Arch.System;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Systems.Animation;
using GodotEcsArch.sources.Flecs.Systems.Building;
using GodotEcsArch.sources.Flecs.Systems.Collisions;
using GodotEcsArch.sources.Flecs.Systems.Debug;
using GodotEcsArch.sources.Flecs.Systems.Generic;
using GodotEcsArch.sources.Flecs.Systems.Human;
using GodotEcsArch.sources.Flecs.Systems.Rendering;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using GodotFlecs.sources.Flecs.Systems.Building;
using GodotFlecs.sources.Flecs.Systems.Delta;
using GodotFlecs.sources.Flecs.Systems.Generic;
using GodotFlecs.sources.Flecs.Systems.Human;
using GodotFlecs.sources.Flecs.Systems.Units;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

    // Cola thread-safe para ejecutar acciones en el hilo principal
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new();

    public FlecsManager(Node3D node3D)
    {
        WorldFlecs = World.Create();
        WorldFlecs.SetThreads(15);
        WorldFlecs.App();

       FlecsComponentRegistry.RegisterAll(this);
      //RegisterComponentsAssemblies();
        RegisterSystems();

        WorldFlecs.Import<Ecs.Stats>();
        WorldFlecs.Set<flecs.EcsRest>(default);
        
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
        RegisterSystem<BatchPrecalSystem>();
        RegisterSystem<BuildingSpawnSystem>();

        //update
        RegisterSystem<HumanInputSystem>();
        RegisterSystem<HumanCharacterSystem>();
        RegisterSystem<UnitEnemySearchSystem>();
        RegisterSystem<MoveTargetSystem>(); 
                                                                             
        RegisterSystem<RegisterFastHashSystem>();
        
        RegisterSystem<ResolveTerrainCollisionSystem>(); // resuelve colisiones con el terreno, debe ir antes de movimiento para ajustar la posición       
        
        RegisterSystem<SteeringSystem>();
        RegisterSystem<MoveSeparationSystem>(); // resuelve colisiones entre entidades, debe ir antes de movimiento para ajustar la posición                                                        

        RegisterSystem<MovementResolutionSystem>();
        //RegisterSystem<MovementFreeUnitTargetSytem>(); // libera el target de movimiento si la unidad está bloqueada o no puede llegar al target

        RegisterSystem<DirectionSystem>();
        //RegisterSystem<UnitMeleeAttackSystem>();
        //RegisterSystem<UnitRangedAttackSystem>();

        //RegisterSystem<BuildRangedAttackSystem>();

        //RegisterSystem<AttackHitSystem>();
        //RegisterSystem<ProjectileMoveSystem>(); 

        //RegisterSystem<ApplyDamageSystem>(); // aplica el daño a las unidades/buildings single thread
        //RegisterSystem<UnitDamageApplySystem>(); // aplica efectos de daño a las unidades
        //RegisterSystem<BuildDamageApplySystem>(); // aplica efectos de daño a los edificios



        // transform
        RegisterSystem<SpriteTransformStaticSystem>();
        RegisterSystem<SpriteTransformSystem>();
        RegisterSystem<SpriteTransformLayerSystem>();

        // estados
        RegisterSystem<CharacterStateLayerSystem>();        
        RegisterSystem<StateCharacterSystem>();

        // animaciones
        RegisterSystem<AnimationLayerUpdateSystem>();
        RegisterSystem<GodotEcsArch.sources.Flecs.Systems.Animation.AnimationSystem>();
        RegisterSystem<AnimationTileSpriteSystem>();

        // render
        RegisterSystem<RenderSpriteTileSystem>();
        RegisterSystem<RenderSpriteSystem>();
        RegisterSystem<LayeredSpriteRenderSystem>();
        RegisterSystem<HumanCameraMoveSystem>();
        
        //RegisterSystem<RvoDebugSystem>();

        //post

        RegisterSystem<ProjectileSpawnSystem>();        
        RegisterSystem<DeathCleanupSystem>();
        RegisterSystem<CleanupSystem>();
        RegisterSystem<BuildCleanupSystem>();
        



    }
    private void RegisterComponentsAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsValueType && type.IsPublic &&
                    type.GetCustomAttribute<RegisterComponentFlecsAttribute>() != null)
                {
                    MethodInfo method = typeof(FlecsManager)
                        .GetMethod(nameof(RegisterComponentWithMembersSimple), BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.MakeGenericMethod(type);

                    method?.Invoke(this, null);

                    GameLog.LogCat($"[FLECS] Registrado componente automáticamente: {type.Name}");
                }
            }
        }
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
    private void RegisterComponentWithMembersSimple<T>() where T : struct
    {
        var compBuilder = WorldFlecs.Component<T>().SetName(typeof(T).Name);

        foreach (var field in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var type = field.PropertyType;
            var name = field.Name;

            // Tipos primitivos
            if (type == typeof(float))
                compBuilder.Member<float>(name);
            else if (type == typeof(int))
                compBuilder.Member<int>(name);
            else if (type == typeof(bool))
                compBuilder.Member<bool>(name);
            else if (type == typeof(double))
                compBuilder.Member<double>(name);
            else if (type == typeof(uint))
                compBuilder.Member<uint>(name);
            else if (type == typeof(Int64))
                compBuilder.Member<Int64>(name);
            else if (type == typeof(Array))
                compBuilder.Member<Array>(name);

        
            else if (type == typeof(Vector2))
            {
                // Registrar Vector2 si no está
                if (WorldFlecs.Lookup(nameof(Vector2)) == 0)
                    WorldFlecs.Component<Vector2>().SetName(nameof(Vector2))
                        .Member<float>("X")
                        .Member<float>("Y");

                compBuilder.Member<Vector2>(name);
            }
            else if (type == typeof(Vector2I))
            {
                // Registrar Vector2 si no está
                if (WorldFlecs.Lookup(nameof(Vector2I)) == 0)
                    WorldFlecs.Component<Vector2I>().SetName(nameof(Vector2I))
                        .Member<int>("X")
                        .Member<int>("Y");

                compBuilder.Member<Vector2>(name);
            }
            else if (type == typeof(Vector3))
            {
                if (WorldFlecs.Lookup(nameof(Vector3)) == 0)
                    WorldFlecs.Component<Vector3>().SetName(nameof(Vector3))
                        .Member<float>("X")
                        .Member<float>("Y")
                        .Member<float>("Z");

                compBuilder.Member<Vector3>(name);
            }
            else if (type == typeof(Transform3D))
            {
                // Registrar Vector2 si no está
                if (WorldFlecs.Lookup(nameof(Transform3D)) == 0)
                    WorldFlecs.Component<Transform3D>().SetName(nameof(Transform3D))
                        .Member<Vector3>("Origin");

                compBuilder.Member<Transform3D>(name);
            }
            else if (type == typeof(Rect2))
            {
                if (WorldFlecs.Lookup(nameof(Rect2)) == 0)
                    WorldFlecs.Component<Rect2>().SetName(nameof(Rect2))
                        .Member<Vector2>("Position")
                        .Member<Vector2>("Size");                        
                compBuilder.Member<Rect2>(name);
            }
            else if (type == typeof(Entity))
            {
                if (WorldFlecs.Lookup(nameof(Entity)) == 0)
                    WorldFlecs.Component<Entity>().SetName(nameof(Entity))
                        .Member<ulong>("Id");
                compBuilder.Member<Entity>(name);
            }
            else if (type == typeof(Rid))
            {
                if (WorldFlecs.Lookup(nameof(Rid)) == 0)
                    WorldFlecs.Component<Rid>().SetName(nameof(Rid))
                        .Member<ulong>("Id");                        
                compBuilder.Member<Rid>(name);
            }
            else if (type == typeof(Color))
            {
                if (WorldFlecs.Lookup(nameof(Color)) == 0)
                    WorldFlecs.Component<Color>().SetName(nameof(Color))
                        .Member<float>("R")
                        .Member<float>("G")
                        .Member<float>("B")
                        .Member<float>("A");
                compBuilder.Member<Color>(name);
            }
            else if (type == typeof(GodotEcsArch.sources.components.AnimationDirection))
            {
                // Si no está ya registrado el enum en Flecs
                if (WorldFlecs.Lookup(nameof(GodotEcsArch.sources.components.AnimationDirection)) == 0)
                {
                    var dataEnyu =WorldFlecs.Component<GodotEcsArch.sources.components.AnimationDirection>();
                    foreach (var enumName in Enum.GetNames(type))
                    {
                        var value = Enum.Parse(type, enumName);
                        dataEnyu.Constant(enumName, Convert.ToInt32(value));
                    }
                }
                compBuilder.Member<GodotEcsArch.sources.components.AnimationDirection>(name);
            }
            else if (type == typeof(EntityType))
            {
                // Si no está ya registrado el enum en Flecs
                if (WorldFlecs.Lookup(nameof(EntityType)) == 0)
                {
                    var dataEnyu = WorldFlecs.Component<EntityType>();
                    foreach (var enumName in Enum.GetNames(type))
                    {
                        var value = Enum.Parse(type, enumName);
                        dataEnyu.Constant(enumName, Convert.ToInt32(value));
                    }
                }
                compBuilder.Member<EntityType>(name);
            }
            else if (type == typeof(ShapeType))
            {
                // Si no está ya registrado el enum en Flecs
                if (WorldFlecs.Lookup(nameof(ShapeType)) == 0)
                {
                    var dataEnyu = WorldFlecs.Component<ShapeType>();
                    foreach (var enumName in Enum.GetNames(type))
                    {
                        var value = Enum.Parse(type, enumName);
                        dataEnyu.Constant(enumName, Convert.ToByte(value));
                    }
                }
                compBuilder.Member<ShapeType>(name);
            }
            //else if (type == typeof(FastCollider[]))
            //{
            //    var elementType = type.GetElementType();
            //    var arrayTypeName = $"{elementType.Name}Array";

   
            //    // Registrar el array como tipo opaco
            //    if (WorldFlecs.Lookup(arrayTypeName) == 0)
            //    {
            //        WorldFlecs.Component<FastCollider[]>().SetName(arrayTypeName);
            //    }

            //    compBuilder.Member<FastCollider[]>(name);
            //}
            else
            {
                GameLog.LogCat($"[Flecs] Campo '{name}' de tipo {type.Name} no soportado.");
            }
        }
    }
}
