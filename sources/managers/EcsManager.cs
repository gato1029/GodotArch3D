using Arch.Buffer;
using Arch.Core;
using Arch.LowLevel;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.systems;
using GodotEcsArch.sources.systems.Buildings;
using GodotEcsArch.sources.systems.Character;
using GodotEcsArch.sources.systems.Collisions;
using GodotEcsArch.sources.systems.Combat;
using GodotEcsArch.sources.systems.Generics;
using GodotEcsArch.sources.systems.Movement;
using GodotEcsArch.sources.systems.Movements;
using GodotEcsArch.sources.systems.Player;
using GodotEcsArch.sources.systems.Rendering;
using Schedulers;
using System;
using System.Collections.Generic;




public class EcsManager : SingletonBase<EcsManager>
{
    private Rid canvasItem;
    private Rid ridWorld3D;

    private Node2D main2D;
    private Node3D main3D;
    private World world;
    private JobScheduler jobScheduler;
    // 🔹 Nuevo: buffer compartido
    private CommandBuffer sharedCommandBuffer;

    public World World { get => world; set => world = value; }
    public JobScheduler JobScheduler { get => jobScheduler; set => jobScheduler = value; }
    public Rid CanvasItem { get => canvasItem; set => canvasItem = value; }
    public Node2D Main2D { get => main2D; set => main2D = value; }
    public Node3D Main3D { get => main3D; set => main3D = value; }
    public Rid RidWorld3D { get => ridWorld3D; set => ridWorld3D = value; }

    private Group<float> groupColliderPhysics;
    private Group<float> groupCollider;
    private Group<float> groupMovement;
    private Group<float> groupRender;
    private Group<float> groupMainCharacter;
    private Group<float> groupUnits;
    private Group<float> groupRemove;
    private Group<float> groupDebugerArch;
    private Group<float> groupDebuger;
    private Group<float> groupTransform;
    private Group<float> groupAttack;
    //private Group<float> groupAttack;
    protected override void Initialize()
    {
        world = World.Create();
        sharedCommandBuffer = new CommandBuffer();
        //// Register
        //var componentType = new Arch.Core.Utils.ComponentType(); // 8 = Size in bytes of the managed struct.
        //Arch.Core.Utils.ComponentRegistry.Add(typeof(ManagedStruct),componentType);

        ConfigScheduler();       
        InitSystems();
    }

    private void InitSystems()
    {
        groupColliderPhysics = new Group<float>("Collider Fisico", new CollisionSystem(world,sharedCommandBuffer));
        groupCollider = new Group<float>("Collider", new MovementCollisionSystem(world, sharedCommandBuffer));
        groupUnits = new Group<float>("Units",
            new RangedTargetingSystem(world, sharedCommandBuffer),
            new MeleeTargetingSystem(world, sharedCommandBuffer)            
            );
        groupMovement = new Group<float>("Movement",new EnemySearchSystem(world, sharedCommandBuffer), new RadiusMovementSystem(world, sharedCommandBuffer),new TargetMovementSystem(world, sharedCommandBuffer) );// , new RadiusMovementSystem(world)
        groupAttack = new Group<float>("Ataque",  new RangedAttackSystem(world, sharedCommandBuffer), new MeleeAttackSystem(world, sharedCommandBuffer), new ProjectileSystem(world, sharedCommandBuffer), new TakeHitSystem(world, sharedCommandBuffer)); //new MeleeAttackSystem(world),

        groupRender = new Group<float>("Render", new PendingSpriteSystem(world, sharedCommandBuffer),  new StateSystem(world,sharedCommandBuffer), new RefreshAnimationSystem(world), new CharacterAnimationSystem(world),  new AnimationSystem(world), new AnimationTileSystem(world), new RenderSystem(world));

        groupMainCharacter = new Group<float>("MainCharacter", new CharacterStateSystem(world), new PlayerInputSystem(world),  new HumanCharacterSystem(world));
        
        
        groupRemove = new Group<float>("Remove", new DeathSystem(world, sharedCommandBuffer),new PendingDestroySystem(world, sharedCommandBuffer));

        
        groupDebuger = new Group<float>("DebugerSystem", new DebugerSystem(world));

        groupTransform = new Group<float>("Transforms", new TransformSystem(world));
        groupCollider.Initialize(); // Inits all registered systems
        groupMainCharacter.Initialize();
        groupRemove.Initialize();
        groupMovement.Initialize();
        groupRender.Initialize();
        groupUnits.Initialize();
        groupColliderPhysics.Initialize();
        groupDebuger.Initialize();
        groupTransform.Initialize();
        groupAttack.Initialize();

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
        
        groupCollider.Dispose();
        groupUnits.Dispose();
        groupMovement.Dispose();
        groupRender.Dispose();
        
        groupRemove.Dispose();
        
        groupDebuger.Dispose();
        groupTransform.Dispose();
        groupMainCharacter.Dispose();
        groupColliderPhysics.Dispose();
        groupAttack.Dispose();
        world.Clear();
        jobScheduler.Dispose(); // <- aquí cierras el pool de threads
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
        
        groupCollider.Update(in deltaTime);               
        groupAttack.Update(in deltaTime);
        groupUnits.Update(in deltaTime);
        groupMovement.Update(in deltaTime);
        groupMainCharacter.Update(in deltaTime);

        
       

        groupRender.Update(in deltaTime);
        groupTransform.Update(in deltaTime);
        groupRender.BeforeUpdate(in deltaTime);

        groupRemove.Update(in deltaTime);
        sharedCommandBuffer.Playback(world);
    }

    public void UpdateSystemsPhysics(float deltaTime, int tick)
    {
        
        groupDebuger.Update(in deltaTime);
       
        groupColliderPhysics.BeforeUpdate(in deltaTime);    // Calls .BeforeUpdate on all systems ( can be overriden )
        groupColliderPhysics.Update(in deltaTime);   // Calls .Update on all systems ( can be overriden )
        groupColliderPhysics.AfterUpdate(in deltaTime);     // Calls .AfterUpdate on all System ( can be overriden )          



        
    }

    public void UpdateSystemDebug(float deltaTime)
    {
        groupRender.AfterUpdate(in deltaTime);
    }
}

