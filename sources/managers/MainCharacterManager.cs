using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Behaviors.Attack;
using GodotEcsArch.sources.managers.Behaviors.Move;
using GodotEcsArch.sources.managers.Behaviors.States;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers
{
    public class MainCharacterManager : SingletonBase<MainCharacterManager>
    {
        Dictionary<int, CharacterWeapon> weapons = new Dictionary<int, CharacterWeapon>();
        public Entity CreateCharacter(Vector2 positionValue)
        {
            Sprite3D sprite3d = SpriteManager.Instance.CreateAnimationInstance(1);

            sprite3d.layer = 5;
            Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);

            Position position = new Position { value = positionValue };
            Entity entity = EcsManager.Instance.World.Create();
            entity.Add(sprite3d);
            entity.Add<RefreshPositionAlways>();
            entity.Add<Transform>(new Transform { transformInternal = xform });
            entity.Add<ColliderSprite>(new ColliderSprite { shapeMove = new Rectangle(.5f, .3f, new Vector2(0, -0.8f)), shapeBody = new Rectangle(.8f, 1.5f, new Vector2(0f, -0.2f)) });
            entity.Add<Rotation>();
            entity.Add(position);
            entity.Add<Direction>(new Direction { value = new Vector2(1, 0), directionAnimation = DirectionAnimation.RIGHT });
            entity.Add<Animation>(new Animation { TimePerFrame = 0.1f, TimeSinceLastFrame = 0, currentAction = AnimationAction.NONE, horizontalOrientation = 0, complete = true });
            entity.Add(new Velocity { value = 5f });

            entity.Add<MainCharacter>();
            entity.Add<Unit>(new Unit { team = 1, health = 10000, damage = 10 });
            entity.Add<MelleCollider>(new MelleCollider { shapeCollider = new Rectangle(2f, 2f, new Vector2(0f, 0f)) });

            entity.Add<StateComponent>(new StateComponent { currentType = StateType.IDLE });
            entity.Add<BehaviorCharacter>(new BehaviorCharacter { moveBehavior = new DefaultMove(), attackBehavior = new MelleAtackHorizontalBehavior(), stateBehavior = new MainCharacterState() });
            CollisionManager.Instance.dynamicCollidersEntities.AddUpdateItem(position.value, entity.Reference());


            Entity entityWeapon = EcsManager.Instance.World.Create();

            Sprite3D spriteWeapon = SpriteManager.Instance.CreateAnimationInstance(2);

            spriteWeapon.layer = 6;

            entityWeapon.Add(spriteWeapon);
            entityWeapon.Add<Position>(position);
            entityWeapon.Add<RefreshPositionAlways>();
            entityWeapon.Add(new Animation { TimePerFrame = 0.1f, TimeSinceLastFrame = 0, currentAction = AnimationAction.IDLE_WEAPON, horizontalOrientation = 0, complete = true });
            entityWeapon.Add<Transform>(new Transform { transformInternal = new Transform3D(Basis.Identity, Vector3.Zero) });
            entityWeapon.Add<Direction>(new Direction { value = new Vector2(1, 0), directionAnimation = DirectionAnimation.RIGHT });
            CharacterWeapon characterWeapon = new CharacterWeapon();
            characterWeapon.id = 1;
            characterWeapon.shapeColliderLeftRight = new Rectangle(2f, 2f, new Vector2(1.5f, 0f));
            characterWeapon.shapeColliderTopDown = new Rectangle(3f, 1.2f, new Vector2(0f, .5f));
            entity.AddRelationship<CharacterWeapon>(entityWeapon,characterWeapon);

            return entity;
        }
        void ConfigWeapon()
        {
            int idAnimation = 2;
            int atlasAnimation = SpriteManager.Instance.LoadTextureAnimation(2,"res://resources/Textures/Character/Weapons/longsword_male.png", new Vector3(192, 192, 32), new Vector3(0, 0, 0), new Vector2(32, 32));

            SpriteManager.Instance.CreateAnimation(atlasAnimation, idAnimation, "WeaponCharacter", 1);
            AnimationIndividual animationIndividual = SpriteManager.Instance.GetAnimation(idAnimation);

            float timeFrame = 0.05f;
            TypeAnimation typeAnimation = new TypeAnimation(4, "IDLE_WEAPON");
            typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(0, 6, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(6, 6, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(12, 6, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(18, 6, timeFrame));
            animationIndividual.AddTypeAnimation((int)AnimationAction.IDLE_WEAPON, typeAnimation);
        }
        void ConfigCharacter()
        {
            int idAnimation = 1;
            int atlasAnimation = SpriteManager.Instance.LoadTextureAnimation(1,"res://resources/Textures/Character/baseCharacter.png", new Vector3(64, 64, 32), new Vector3(0, 0, 0), new Vector2(32, 32));

            SpriteManager.Instance.CreateAnimation(atlasAnimation, idAnimation, "Personaje", 8);
            AnimationIndividual animationIndividual = SpriteManager.Instance.GetAnimation(idAnimation);
            
            float timeFrame = 0.1f;
            TypeAnimation typeAnimation = new TypeAnimation(4, "WALK");
            typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(105, 8, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(118, 8, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(131, 8, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(144, 8, timeFrame));
            animationIndividual.AddTypeAnimation((int)AnimationAction.WALK, typeAnimation);

            timeFrame = 0.2f;
            typeAnimation = new TypeAnimation(4, "IDLE");
            typeAnimation.AddFrame((int)DirectionAnimation.UP, new CustomFrameAnimation(timeFrame, [286, 286, 287]));
            typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new CustomFrameAnimation(timeFrame, [299, 299, 300]));
            typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new CustomFrameAnimation(timeFrame, [312, 312, 313]));
            typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new CustomFrameAnimation(timeFrame, [325, 325, 326]));
            animationIndividual.AddTypeAnimation((int)AnimationAction.IDLE_WEAPON, typeAnimation);

            timeFrame = 0.05f;
            typeAnimation = new TypeAnimation(4, "ATACK");
            typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(156, 6, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(169, 6, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(182, 6, timeFrame));
            typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(195, 6, timeFrame));
            animationIndividual.AddTypeAnimation((int)AnimationAction.ATACK, typeAnimation);        
        }
        protected override void Initialize()
        {
            ConfigCharacter();
            ConfigWeapon();
        }

        protected override void Destroy()
        {
            
        }
    }
}
