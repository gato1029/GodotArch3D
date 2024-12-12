using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Behaviors.Attack;
using GodotEcsArch.sources.managers.Behaviors.Move;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class CharacterManager : SingletonBase<CharacterManager>
{



    private Dictionary<int, Func<Vector2, Entity>> functionMap = new Dictionary<int, Func<Vector2,Entity>>();
    protected override void Initialize()
    {
        ConfigMushroom();
    }

    protected override void Destroy()
    {
     
    }

    public Entity CreateCharacter(int idCharacter,Vector2 position)
    {
        if (functionMap.TryGetValue(idCharacter, out Func<Vector2, Entity> creationFunc))
        {
            return creationFunc(position); 
        }
        else
        {
            Console.WriteLine($"No hay función registrada para el ID {idCharacter}");
            return default;
        }
    }


    void ConfigMushroom () {
        int idAnimation = 2;
        int atlasAnimation = SpriteManager.Instance.LoadTextureAnimation("res://resources/Textures/Monster/Hongito.png", new Vector3(80, 64, 32), new Vector3(0, -0.5f, 0), new Vector2(40, 32));

        SpriteManager.Instance.CreateAnimation(atlasAnimation,idAnimation, "Mushroom", 8);
        AnimationIndividual animationIndividual = SpriteManager.Instance.GetAnimation(idAnimation);
        float timeFrame = 0.1f;
        TypeAnimation typeAnimation = new TypeAnimation(4, "Idle");
        typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(144, 7, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(144, 7, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(144, 7, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(144, 7, timeFrame));
        animationIndividual.AddTypeAnimation((int)AnimationAction.IDLE, typeAnimation);

        typeAnimation = new TypeAnimation(4, "Walk");
        typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(72, 8, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(72, 8, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(72, 8, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(72, 8, timeFrame));             
        animationIndividual.AddTypeAnimation((int)AnimationAction.WALK, typeAnimation);

        timeFrame = 0.05f;
        typeAnimation = new TypeAnimation(4, "Atack");
        typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(0, 10, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(0, 10, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(0, 10, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(0, 10, timeFrame));
        animationIndividual.AddTypeAnimation((int)AnimationAction.ATACK, typeAnimation);

        typeAnimation = new TypeAnimation(4, "Death",false);
        typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(24, 15, timeFrame   ));
        typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(24,  15, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(24, 15, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(24, 15, timeFrame));
        animationIndividual.AddTypeAnimation((int)AnimationAction.DEATH, typeAnimation);

        typeAnimation = new TypeAnimation(4, "Hit");
        typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(48, 5, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(48, 5, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(48, 5, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(48, 5, timeFrame));
        animationIndividual.AddTypeAnimation((int)AnimationAction.HIT, typeAnimation);

        typeAnimation = new TypeAnimation(4, "Stun");
        typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(96, 18, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(96, 18, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(96, 18, timeFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(96, 18, timeFrame));
        animationIndividual.AddTypeAnimation((int)AnimationAction.STUN , typeAnimation);

        functionMap[idAnimation] = CreateMushrrom;
    }

    Entity CreateMushrrom(Vector2 positionValue)
    {
        Sprite3D sprite3d = SpriteManager.Instance.CreateAnimationInstance(2);
        
        sprite3d.layer = 5;
        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);

        Position position = new Position { value = positionValue };
        Entity entity = EcsManager.Instance.World.Create();
        entity.Add(sprite3d);
        entity.Add<RefreshPositionAlways>();

        AreaMovement areaMovementInternal = new AreaMovement { type = MovementType.CIRCLE_STATIC, widthRadius = 5, origin = position.value };
        IAController iaControllerInternal = new IAController { areaMovement = areaMovementInternal, targetMovement = new TargetMovement { arrive = true }  };
        

        //entity.Add<MushroomCharacter>();
        entity.Add<Transform>(new Transform { transformInternal = xform });
        entity.Add<Collider>(new Collider { rect = new Rect2(new Vector2(0f, 0.0f), new Vector2(1, 1)), rectTransform = new Rect2(new Vector2(0,0f), new Vector2(1, 1)), aplyRotation = false });
        entity.Add<Rotation>();
        entity.Add(position);
        entity.Add<Direction>();
        entity.Add<Animation>(new Animation { TimePerFrame = 0.1f, TimeSinceLastFrame = 0, currentAction = AnimationAction.NONE, horizontalOrientation = -1, complete=false });
        entity.Add(new Velocity { value = 2f });
        
        entity.Add<UnitController>(new UnitController { controllerMode= ControllerMode.IA, iaController = iaControllerInternal  });      
        entity.Add<Unit>(new Unit { team = 2, health=100, damage =1});
        entity.Add<MelleCollider>(new MelleCollider { collider = new Collider { rect = new Rect2(new Vector2(0, 0f), new Vector2(1, 1)), rectTransform = new Rect2(new Vector2(0, 0f), new Vector2(1, 1)), aplyRotation = false } });
        entity.Add<StateComponent>(new StateComponent { currentType = StateType.IDLE});
        entity.Add<BehaviorCharacter>(new BehaviorCharacter { moveBehavior = new DefaultMove(), attackBehavior = new MelleAttackBehavior() });

        CollisionManager.Instance.dynamicCollidersEntities.AddUpdateItem(position.value, entity.Reference());

        return entity;
    }
}
