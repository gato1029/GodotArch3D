using Arch.Core;
using Arch.LowLevel;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.systems;
using GodotEcsArch.sources.systems.Character;
using Schedulers;
using System;
using System.Collections.Generic;


public struct ManagedStruct {
    public UnsafeList<Entity> datos;
    public ManagedStruct()
    {
        this.datos = new UnsafeList<Entity>();
    }

}



public class EcsManager : SingletonBase<EcsManager>
{
    private Rid canvasItem;
    private Rid ridWorld3D;

    private Node2D main2D;
    private Node3D main3D;
    private World world;
    private JobScheduler jobScheduler;
    public World World { get => world; set => world = value; }
    public JobScheduler JobScheduler { get => jobScheduler; set => jobScheduler = value; }
    public Rid CanvasItem { get => canvasItem; set => canvasItem = value; }
    public Node2D Main2D { get => main2D; set => main2D = value; }
    public Node3D Main3D { get => main3D; set => main3D = value; }
    public Rid RidWorld3D { get => ridWorld3D; set => ridWorld3D = value; }

    private Group<float> groupCollider;
    private Group<float> groupMovement;
    private Group<float> groupRender;
    private Group<float> groupMainCharacter;
    private Group<float> groupUnits;
    private Group<float> groupRemove;
    private Group<float> groupDebugerArch;
    private Group<float> groupDebuger;
    private Group<float> groupTransform;

    protected override void Initialize()
    {
        world = World.Create();

        //// Register
        //var componentType = new Arch.Core.Utils.ComponentType(); // 8 = Size in bytes of the managed struct.
        //Arch.Core.Utils.ComponentRegistry.Add(typeof(ManagedStruct),componentType);

        ConfigScheduler();       
        InitSystems();
    }

    private void InitSystems()
    {
        groupCollider = new Group<float>("Collider", new CollisionSystem(world));
        groupMovement = new Group<float>("Movement", new SearchMovementTargetSystem(world));

        groupRender = new Group<float>("Render", new StateSystem(world), new RefreshAnimationSystem(world), new CharacterAnimationSystem(world),  new AnimationSystem(world), new AnimationTileSystem(world), new RenderSystem(world));

        groupMainCharacter = new Group<float>("MainCharacter", new MainCharacterSystem(world));

        groupUnits = new Group<float>("Units" ,new BehaviorCharacterSystem(world), new CharacterBehaviorSystem(world), new CharacterCommonBehaviorSystem(world));
        groupRemove = new Group<float>("Remove", new RemoveSystem(world));

        groupDebugerArch = new Group<float>("DebugerArch", new DebugerManager(world));
        groupDebuger = new Group<float>("DebugerSystem", new DebugerSystem(world));

        groupTransform = new Group<float>("Transforms", new TransformSystem(world));
        groupCollider.Initialize(); // Inits all registered systems
        groupMainCharacter.Initialize();
        groupRemove.Initialize();
        groupMovement.Initialize();
        groupRender.Initialize();
        groupUnits.Initialize();
        groupDebugerArch.Initialize();
        groupDebuger.Initialize();
        groupTransform.Initialize();


    }


    private void ConfigScheduler()
    {
        jobScheduler = new(
        new JobScheduler.Config
        {
            ThreadPrefixName = "Arch.EngineDreamCat",
            ThreadCount = 0,                           // 0 = Determine at runtime of count threads
            MaxExpectedConcurrentJobs = 64,
            StrictAllocationMode = false,
        }
        );
        World.SharedJobScheduler = jobScheduler;
    }
    protected override void Destroy()
    {
        world.Clear();
        groupCollider.Dispose();
        groupMovement.Dispose();
        groupRender.Dispose();
        groupUnits.Dispose();
        groupRemove.Dispose();
        groupDebugerArch.Dispose();
        groupDebuger.Dispose();
        groupTransform.Dispose();
        groupMainCharacter.Dispose();
    }

    public void SetCanvasItemRid(Rid id, Node2D node2D)
    {
        this.CanvasItem = id;
        this.main2D = node2D;
    }
    public void SetNode3DMain( Node3D node3D)
    {        
        this.main3D = node3D;
        this.ridWorld3D = node3D.GetWorld3D().Scenario;
    }
  
    public void UpdateSystems(float deltaTime, int tick)
    {
        

    
        groupMainCharacter.Update(in deltaTime);

        groupUnits.BeforeUpdate(in deltaTime);
        groupUnits.Update(in deltaTime);
        groupUnits.AfterUpdate(in deltaTime);

        //groupMovement.BeforeUpdate(in deltaTime);
        //groupMovement.Update(in deltaTime);
      
        groupTransform.Update(in deltaTime);
        groupRender.BeforeUpdate(in deltaTime);
        groupRender.Update(in deltaTime);

  
    }

    public void UpdateSystemsPhysics(float deltaTime, int tick)
    {
        
        groupDebuger.Update(in deltaTime);

        groupCollider.BeforeUpdate(in deltaTime);    // Calls .BeforeUpdate on all systems ( can be overriden )
        groupCollider.Update(in deltaTime);          // Calls .Update on all systems ( can be overriden )
        groupCollider.AfterUpdate(in deltaTime);     // Calls .AfterUpdate on all System ( can be overriden )          



        groupRemove.Update(in deltaTime);
        groupRemove.AfterUpdate(in deltaTime);
        
    }

    public void UpdateSystemDebug(float deltaTime)
    {
        groupRender.AfterUpdate(in deltaTime);
    }
}

