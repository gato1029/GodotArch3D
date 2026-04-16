using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;

namespace GodotEcsArch.sources.BlackyTiles;

public class BlackyCharacterCreator 
{
    private int _characterCount = 0;
    private int layer =4;
    private readonly FlecsManager flecsManager;
    private readonly FastSpatialHash dynamicHash;
    private bool DEBUG_COLLIDERS = false;
    public BlackyCharacterCreator(FlecsManager flecsManager, FastSpatialHash dynamicHash)
    {
        this.flecsManager = flecsManager;
        this.dynamicHash = dynamicHash;
    }

    public Entity Create(int id, Godot.Vector2 position)
    {
        _characterCount++;
        CharacterModelBaseData characterBaseData = CharacterLocalBase.Instance.GetCharacterBaseData(id);
        var entity = flecsManager.WorldFlecs.Entity();

        

        switch (characterBaseData.characterType)
        {
            case CharacterType.MAIN:
                return CreateMain(entity, characterBaseData, position);
                break;

            case CharacterType.NPC:
                break;

            case CharacterType.ENEMIGO:
                return CreateEnemy(entity, characterBaseData,position);
                break;

            default:
                break;
        }
        return entity;
    }

    private void AddCollider(Entity entity, Vector2 position, GeometricShape2D collisionBody, GeometricShape2D collisionFeet,    out int idDebugMove, out int idDebugBody)
    {
        // cuerpo
        ShapeType shapeType = ShapeType.Rect;
        float width = 0;
        float height = 0;
        float offsetX = 0;
        float offsetY = 0;

        switch (collisionBody)
        {
            case Circle circle:
                shapeType = ShapeType.Circle;
                width = circle.Radius;
                height = circle.Radius;
                offsetX = circle.OriginCurrent.X;
                offsetY = circle.OriginCurrent.Y;
                break;
            case Rectangle rectangle:
                shapeType = ShapeType.Rect;
                width = rectangle.Width;
                height = rectangle.Height;
                offsetX = rectangle.OriginCurrent.X;
                offsetY = rectangle.OriginCurrent.Y;
                break;
            default:
                break;
        }

        // collider de los pies (huella física)

        ShapeType shapeTypePies = ShapeType.Rect;
        float widthPies = 0;
        float heightPies = 0;
        float offsetXPies = 0;
        float offsetYPies = 0;

        switch (collisionFeet)
        {
            case Circle circle:
                shapeTypePies = ShapeType.Circle;
                widthPies = circle.Radius;
                heightPies = circle.Radius;
                offsetXPies = circle.OriginCurrent.X;
                offsetYPies = circle.OriginCurrent.Y;
                break;
            case Rectangle rectangle:
                shapeTypePies = ShapeType.Rect;
                widthPies = rectangle.Width;
                heightPies = rectangle.Height;
                offsetXPies = rectangle.OriginCurrent.X;
                offsetYPies = rectangle.OriginCurrent.Y;
                break;
            default:
                break;
        }

        if (true)
        {

        }
        var SpatialIDComponent = new SpatialIDComponent
        {
            Layer = CollisionConfig.TypePlayer ,
            Mask = CollisionConfig.TypeBullet | CollisionConfig.TypePlayer,
            Value = dynamicHash.GetNewNodeIndex()
        };
        var MoveComponent = new MoveColliderComponent
        (
            widthPies,new Vector2(offsetXPies, offsetYPies), widthPies * 1.5f
        );

        FastCollider[] bodyColliders = new FastCollider[1]
        {
            new FastCollider
            {
                Shape = shapeType,
                Width = width,
                Height = height,
                Offset = new Vector2(offsetX, offsetY)
            }
        };
        var BodyComponent = new BodyColliderComponent
        (
           bodyColliders
        );

        entity.Set(SpatialIDComponent);
        entity.Set(BodyComponent);
        entity.Set(MoveComponent);
        entity.Add<UnitTag>();

        dynamicHash.Register(SpatialIDComponent.Value, position.X, position.Y, entity);

        if (DEBUG_COLLIDERS)
        {
            idDebugMove = CollisionShapeDraw.Instance.DrawCircleShape(widthPies, position, Colors.IndianRed);
            idDebugBody = CollisionShapeDraw.Instance.DrawCollisionShapes(collisionBody, position, Colors.Green);
        }
        else
        {
            idDebugBody = -1;
            idDebugMove = -1;
        }


    }
    private Entity CreateMain(Entity entity, CharacterModelBaseData characterBaseData, Vector2 position)
    {
        var tileData = MasterDataManager.GetData<TileSpriteData>(characterBaseData.idTileSpriteData);
        var MoveData =tileData.spriteMultipleAnimationDirection.animationsTypes[AnimationType.CAMINANDO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];

        

        //GeometricShape2D colliderMove = characterBaseData.animationCharacterBaseData.collisionMove.Multiplicity(characterBaseData.scale);
        //Circle circle = (Circle)colliderMove;
        //GeometricShape2D colliderBody = characterBaseData.animationCharacterBaseData.collisionBody.Multiplicity(characterBaseData.scale);


        GeometricShape2D colliderMove = MoveData.collisionBodyDictionary["Base"].Multiplicity(characterBaseData.scale);
        //Circle circle = (Circle)colliderMove;
        GeometricShape2D colliderBody = MoveData.collisionBodyDictionary["Cuerpo"].Multiplicity(characterBaseData.scale);

       

        entity.Set(new GodotFlecs.sources.Flecs.Components.CharacterComponent
        {
            characterStateType = CharacterStateType.IDLE,
            characterBehaviorType = CharacterBehaviorType.PERSONAJE_PRINCIPAL
        });

        CreateMainBodyWeapons(entity, characterBaseData, position);

        //entity.Set(new RenderTransformComponent(transform));
        //entity.Set(new RenderGPUComponent( instance.rid,   instance.instance, instance.material, instance.layerTexture, 20, 0, 1, originOffset));
        //entity.Set(new AnimationComponent(characterBaseData.id, EntityType.PERSONAJE, 0, -1, 0, 0, 0, false, true));
        //entity.Set(new RenderFrameDataComponent { uvMap = new Godot.Color(0, 0, 0, 0) });

        entity.Set(new TeamComponent(1));
        entity.Set(new IdGenericComponent(characterBaseData.id, EntityType.PERSONAJE));
   
        

        entity.Set(new PositionComponent(position, Vector2I.Zero,4));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero,DirectionAnimationType.CUATRO, GodotEcsArch.sources.components.AnimationDirection.LEFT));

       // int idAgent = RvoManager.Instance.RegisterAgent(position, circle.Radius, 3);
       // entity.Set(new RvoAgentIdComponent(idAgent, circle.Radius));

        entity.Set(new VelocityComponent(new Vector2(0, 0), 3, new Vector2(0, 0)));
        entity.Set(new MoveResolutorComponent(false, 0, position,0,0));
        entity.Set(new PlayerInputComponent());

        // este collider component tiene que salir luego
        //int idCollider = CollisionManager.Instance.characterEntitiesFlecs.AddColliderObject(entity, colliderBody, position,1,colliderMove);
        entity.Set(new ColliderComponent(0, new Rect2(), colliderBody.OriginCurrent, new Rect2(position -(colliderMove.GetSizeQuad() / 2), colliderMove.GetSizeQuad()), colliderMove.OriginCurrent,0));

        entity.Set(new HumanAttackComponent(10, 1f, 0.0f, 0.2f, 0));
        entity.Set(new HealthComponent(6000));
        
        float rvoRadius = MeshCreator.PixelsToUnits(12);
        entity.Set(new MeleeAttackComponent(20, 1, 0f, 0));
        entity.Set(new SteeringComponent(rvoRadius,4, Vector2.Zero));
        //entity.Set(new StuckComponent(position, 0, false));

        AddCollider(entity, position, colliderBody, colliderMove, out int idDebugMove, out int idDebugBody);

        if (DEBUG_COLLIDERS)
        {
            entity.Set(new RvoAgentDebugComponent(idDebugMove, idDebugBody, 0));
        }
        
       
        entity.Add<UseBoidTag>();
        

        

        return entity;
    }
    private void CreateMainBodyWeapons(Entity entity, CharacterModelBaseData characterBaseData, Vector2 position)
    {
        var tileData = MasterDataManager.GetData<TileSpriteData>(characterBaseData.idTileSpriteData);
        var MoveData = tileData.spriteMultipleAnimationDirection.animationsTypes[AnimationType.CAMINANDO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];

        Godot.Vector2 originOffset = new Vector2(MoveData.offsetInternal.X * characterBaseData.scale, MoveData.offsetInternal.Y * characterBaseData.scale);
        int idMaterial = MoveData.idMaterial;
        var instance = MultimeshManager.Instance.CreateInstance(idMaterial);

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X, position.Y, (position.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        int numLayers = 2; // Cuerpo + arma
        AnimationComponent[] animations = new AnimationComponent[numLayers];
        RenderGPUComponent[] GPUData = new RenderGPUComponent[numLayers];
        RenderFrameDataComponent[] frames = new RenderFrameDataComponent [numLayers];
        RenderTransformComponent[] transforms = new RenderTransformComponent [numLayers];

        // Capa del cuerpo principal
        animations[0] = new AnimationComponent(characterBaseData.idTileSpriteData, EntityType.PERSONAJE, AnimationType.PARADO, AnimationType.NINGUNA, 0, 0, 0, false, true,true);
        GPUData[0] = new RenderGPUComponent(instance.rid, instance.instance, instance.material, instance.layerTexture, layer, MoveData.yDepthRenderFormat, characterBaseData.scale, originOffset);
        frames[0] = new RenderFrameDataComponent { uvMap = new Godot.Color(0, 0, 0, 0) };
        transforms[0] = new RenderTransformComponent(transform);

        // Capa del arma 
        //var weaponBase =AccesoryManager.Instance.GetAccesory(2);
        int idWeapon = 2; // Aquí deberías obtener el ID del arma que deseas equipar
        var weaponBase = AccesoryAvatarManager.Instance.ChangueAccesory(AccesoryAvatarType.WEAPON, idWeapon, characterBaseData.scale);

        animations[1] = new AnimationComponent(weaponBase.idTileSprite, EntityType.ACCESORIO, AnimationType.ARMA_ATACANDO, AnimationType.NINGUNA, 0, 0, 0, false, true,false);
        GPUData[1] = new RenderGPUComponent(weaponBase.gpu.rid, weaponBase.gpu.instance, 0, 0, layer, MoveData.yDepthRenderFormat, characterBaseData.scale, originOffset);
        frames[1] = new RenderFrameDataComponent { uvMap = new Godot.Color(0, 0, 0, 0) };
        transforms[1] = new RenderTransformComponent(transform);

        entity.Set(weaponBase.weaponComponent);
        
        entity.Set(new RenderLayerListComponent
            {
            Animations = animations,
            GPUData = GPUData,
            Frames = frames,
            Transforms = transforms
        });

        // Aquí puedes agregar la lógica para crear las armas del personaje principal
        // Por ejemplo, podrías agregar componentes relacionados con las armas
    }
    private Entity CreateEnemy(Entity entity, CharacterModelBaseData characterBaseData, Godot.Vector2 position)
    {
        var tileData = MasterDataManager.GetData<TileSpriteData>(characterBaseData.idTileSpriteData);

        var MoveData = tileData.spriteMultipleAnimationDirection.animationsTypes[AnimationType.CAMINANDO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];


        int idMaterial = MoveData.idMaterial;
        var instance = MultimeshManager.Instance.CreateInstance(idMaterial);
        GeometricShape2D colliderMove = MoveData.collisionBodyDictionary["Base"].Multiplicity(characterBaseData.scale);
        Circle circle = (Circle)colliderMove;
        GeometricShape2D colliderBody = MoveData.collisionBodyDictionary["Cuerpo"].Multiplicity(characterBaseData.scale);
 
        Godot.Vector2 originOffset = new Vector2(MoveData.offsetInternal.X * characterBaseData.scale, MoveData.offsetInternal.Y* characterBaseData.scale);

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X, position.Y, (position.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        entity.Set(new GodotFlecs.sources.Flecs.Components.CharacterComponent
        {

            characterStateType = CharacterStateType.IDLE, 
            characterBehaviorType = CharacterBehaviorType.GENERICO
            
        });
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new TeamComponent(2));
        entity.Set(new IdGenericComponent(characterBaseData.id, EntityType.PERSONAJE));
        entity.Set(new RenderGPUComponent( instance.rid, instance.instance, instance.material, instance.layerTexture, layer,MoveData.yDepthRenderFormat, characterBaseData.scale, originOffset));
        entity.Set(new AnimationComponent(characterBaseData.idTileSpriteData, EntityType.PERSONAJE, AnimationType.PARADO, AnimationType.NINGUNA, 0,0,0,false,true, true));
        entity.Set(new RenderFrameDataComponent{uvMap = new Godot.Color(0,0,0,0)});
        entity.Set(new PositionComponent(position,Vector2I.Zero,1));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero, DirectionAnimationType.DOS, GodotEcsArch.sources.components.AnimationDirection.LEFT));
        
        entity.Set(new VelocityComponent(new Vector2(0,0),3f,new Vector2(0,0)));
        entity.Set(new MoveResolutorComponent(false,0, position,0  ,0 ));
        

     //   int idCollider = CollisionManager.Instance.characterEntitiesFlecs.AddColliderObject(entity, colliderBody, position,2, colliderMove);
        //entity.Set(new ColliderComponent(idCollider, new Rect2(), colliderBody.OriginCurrent,new Rect2((colliderMove.GetSizeQuad() / 2) , colliderMove.GetSizeQuad()), colliderMove.OriginCurrent, 0));

        // circle.Radius
        
        entity.Add<UseBoidTag>();

        entity.Set(new HealthComponent(10000));
        //entity.Set(new RangedAttackComponent(1,20,5f,0.5f,0,true,6));
        entity.Set(new MeleeAttackComponent(20, MeshCreator.PixelsToUnits(colliderBody.widthPixel*4), 1f, 0));
        entity.Set(new AttackPendingComponent(false, default));
        entity.Set(new EnemySearchComponent(5, 2, 0));
        float dd = MeshCreator.PixelsToUnits(12);
        entity.Set(new SteeringComponent(dd,2, Vector2.Zero));
        //entity.Set(new StuckComponent(position,0,false));

        AddCollider(entity, position, colliderBody, colliderMove, out int idDebugMove, out int idDebugBody);
        if (DEBUG_COLLIDERS)
        {
            entity.Set(new RvoAgentDebugComponent(idDebugMove, idDebugBody, 0));
        }
        return entity;
    }



}

