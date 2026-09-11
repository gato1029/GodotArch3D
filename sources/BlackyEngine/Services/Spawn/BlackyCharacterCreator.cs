
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
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

namespace GodotEcsArch.sources.BlackyEngine.Services.Spawn;

public class BlackyCharacterCreator 
{
    private int _characterCount = 0;
    private int layer =4;
    private readonly FlecsManager flecsManager;
    private readonly FastSpatialHash dynamicHash;
    private readonly Core.BlackyWorld world;
    private bool DEBUG_COLLIDERS = false;
    public BlackyCharacterCreator(FlecsManager flecsManager, FastSpatialHash dynamicHash, Core.BlackyWorld world)
    {
        this.flecsManager = flecsManager;
        this.dynamicHash = dynamicHash;
        this.world = world;
    }

    public Entity Create(long id, Godot.Vector2 position)
    {
        world.Tick.TotalUnits++;
        ushort characterId = BlackyPalletesPersistence.characterPalette.GetIdPersistence("Base", id, out CharacterModelBaseData charModel);
      
        
        //CharacterModelBaseData characterBaseData = CharacterLocalBase.Instance.GetCharacterBaseData(id);
        var entity = flecsManager.WorldFlecs.Entity();



        switch (charModel.characterType)
        {
            case CharacterType.MAIN:
                return CreateGeneric( characterId, entity, charModel, position);
                break;

            case CharacterType.NPC:
                break;

            case CharacterType.ENEMIGO:
                return CreateEnemy(characterId, entity, charModel, position);
                break;

            default:
                break;
        }
        return entity;
    }

    private void AddCollider(Entity entity, Vector2 position, List<FastCollider> collisionBodys, GeometricShape2D collisionFeet,    out int idDebugMove, out int idDebugBody)
    {
    

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



        entity.Set(SpatialIDComponent);
        //entity.Set(BodyComponent);
        entity.Set(MoveComponent);
        entity.Add<UnitTag>();

        dynamicHash.Register(SpatialIDComponent.Value, position.X, position.Y, entity);

        if (DEBUG_COLLIDERS)
        {
            idDebugMove = CollisionShapeDraw.Instance.DrawCircleShape(widthPies, position, Colors.IndianRed);
            idDebugBody = CollisionShapeDraw.Instance.DrawCollisionShapes(collisionBodys[0], position, Colors.Green);
        }
        else
        {
            idDebugBody = -1;
            idDebugMove = -1;
        }


    }
    private Entity CreateGeneric(ushort characterId, Entity entity, CharacterModelBaseData characterBaseData, Vector2 position)
    {
        int height = 5; // altura en el mundo

        var idTileSprite = characterBaseData.idTileSpriteData; // información del sprite del personaje y colliders ID
        int spriteId = AtlasModsManager.GetSpriteUniqueId(idTileSprite); // Obtén el ID único del sprite
        AtlasModsManager.TryGetTileSprite(spriteId, out var sprite); // Obtén el sprite del personaje
        var animationDir = sprite.spriteMultipleAnimationDirection; // Obtén las animación de caminar del personaje
      

        var MoveData = animationDir.animationsTypes[AnimationType.PARADO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];

        GeometricShape2D colliderMove = MoveData.collisionDictionary[CollisionUseType.BASE_PIES].Multiplicity(characterBaseData.scale);        
        

        var AtackData = animationDir.animationsTypes[AnimationType.ATACANDO_CUERPO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];

        Circle colliderAtackMelle = (Circle)AtackData.collisionDictionary[CollisionUseType.RADIO_ATAQUE_CUERPO].Multiplicity(characterBaseData.scale);
        

        var instance = AtlasTexturesModsManager.Instance.CreateInstanceRender(MoveData.idModMaterial);

        Godot.Vector2 originOffset = new Vector2(MoveData.offsetInternal.X * characterBaseData.scale, MoveData.offsetInternal.Y * characterBaseData.scale);

        float depthOffset = MoveData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, height, layer, position); // debemos usar esto apartir de ahora

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X, position.Y, z);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        // render
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instance.rid, instance.instance, 0, instance.layerTexture, layer, depthOffset, characterBaseData.scale, originOffset));
        entity.Set(new AnimationComponent(characterBaseData.idTileSpriteData, EntityType.PERSONAJE, AnimationType.PARADO, AnimationType.NINGUNA, 0, 0, 0, false, true, true));
        entity.Set(new RenderFrameDataComponent { uvMap = MoveData.uvFramesArray[0] });

        // comportamiento
        entity.Set(new GodotFlecs.sources.Flecs.Components.CharacterComponent
        {
            characterStateType = CharacterStateType.IDLE,
            characterBehaviorType = CharacterBehaviorType.PERSONAJE_PRINCIPAL
        });
                
        entity.Set(new TeamComponent(1));
        entity.Set(new UnitDefinitionComponent(characterId));
        
        entity.Set(new PositionComponent(position, Vector2I.Zero, height));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero, DirectionAnimationType.OCHO, GodotEcsArch.sources.components.AnimationDirection.LEFT));      
        entity.Set(new VelocityComponent(new Vector2(0, 0), 3, new Vector2(0, 0)));
        entity.Set(new MoveResolutorComponent(false, 0, position, 0, 0));
        entity.Set(new PlayerInputComponent());

        // este collider component tiene que salir luego
        //int idCollider = CollisionManager.Instance.characterEntitiesFlecs.AddColliderObject(entity, colliderBody, position,1,colliderMove);
      //  entity.Set(new ColliderComponent(0, new Rect2(), colliderBody.OriginCurrent, new Rect2(position - (colliderMove.GetSizeQuad() / 2), colliderMove.GetSizeQuad()), colliderMove.OriginCurrent, 0));

      //  entity.Set(new HumanAttackComponent(10, 1f, 0.0f, 0.2f, 0));
        entity.Set(new HealthComponent(100));

        float rvoRadius = MeshCreator.PixelsToUnits(16);
 
        entity.Set(new MeleeAttackComponent(20, colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, 0f, 0));
        entity.Set(new SteeringComponent(rvoRadius, 4, Vector2.Zero));
        entity.Set(new WeaponComponent(1, false));
        //entity.Set(new StuckComponent(position, 0, false));

        AddCollider(entity, position, characterBaseData.bodyColliders, colliderMove, out int idDebugMove, out int idDebugBody);

        int idDebugRangeMelle = CollisionShapeDraw.Instance.DrawCircleShape(colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, Colors.Blue);

        if (DEBUG_COLLIDERS)
        {
            entity.Set(new RvoAgentDebugComponent(idDebugMove, idDebugBody, idDebugRangeMelle,0));
        }


        entity.Add<UseBoidTag>();


        return entity;
    }
    
    private Entity CreateEnemy(ushort characterId, Entity entity, CharacterModelBaseData characterBaseData, Godot.Vector2 position)
    {
        int height = 5; // altura en el mundo

        var idTileSprite = characterBaseData.idTileSpriteData; // información del sprite del personaje y colliders ID
        int spriteId = AtlasModsManager.GetSpriteUniqueId(idTileSprite); // Obtén el ID único del sprite
        AtlasModsManager.TryGetTileSprite(spriteId, out var sprite); // Obtén el sprite del personaje
        var animationDir = sprite.spriteMultipleAnimationDirection; // Obtén las animación de caminar del personaje
        

        var MoveData = animationDir.animationsTypes[AnimationType.PARADO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];

        GeometricShape2D colliderMove = MoveData.collisionDictionary[CollisionUseType.BASE_PIES].Multiplicity(characterBaseData.scale);
        //GeometricShape2D colliderBody = MoveData.collisionDictionary[CollisionUseType.CUERPO].Multiplicity(characterBaseData.scale);

        var AtackData = animationDir.animationsTypes[AnimationType.ATACANDO_CUERPO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];

        Circle colliderAtackMelle = (Circle)AtackData.collisionDictionary[CollisionUseType.RADIO_ATAQUE_CUERPO].Multiplicity(characterBaseData.scale);


        var instance = AtlasTexturesModsManager.Instance.CreateInstanceRender(MoveData.idModMaterial);

        Godot.Vector2 originOffset = new Vector2(MoveData.offsetInternal.X * characterBaseData.scale, MoveData.offsetInternal.Y * characterBaseData.scale);

        float depthOffset = MoveData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, height, layer, position); // debemos usar esto apartir de ahora

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X, position.Y, z);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        // render
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instance.rid, instance.instance, 0, instance.layerTexture, layer, depthOffset, characterBaseData.scale, originOffset));
        entity.Set(new AnimationComponent(characterBaseData.idTileSpriteData, EntityType.PERSONAJE, AnimationType.PARADO, AnimationType.NINGUNA, 0, 0, 0, false, true, true));
        entity.Set(new RenderFrameDataComponent { uvMap = MoveData.uvFramesArray[0] });

        entity.Set(new GodotFlecs.sources.Flecs.Components.CharacterComponent
        {

            characterStateType = CharacterStateType.IDLE, 
            characterBehaviorType = CharacterBehaviorType.GENERICO
            
        });
        
        entity.Set(new TeamComponent(2));                     
        entity.Set(new UnitDefinitionComponent(characterId));
        entity.Set(new PositionComponent(position,Vector2I.Zero,1));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero, animationDir.directionAnimationType, GodotEcsArch.sources.components.AnimationDirection.LEFT));        
        entity.Set(new VelocityComponent(new Vector2(0,0),3f,new Vector2(0,0)));
        entity.Set(new MoveResolutorComponent(false,0, position,0  ,0 ));
             
    
        
        
        entity.Add<UseBoidTag>();

        entity.Set(new HealthComponent(100));

       
        float rvoRadius = MeshCreator.PixelsToUnits(16);
        float radiusSearchEnemy = MeshCreator.PixelsToUnits(64);

      
        

        if (characterBaseData.unitAttackType== UnitAttackType.CUERPO)
        {
            entity.Set(new MeleeAttackComponent(20, colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, 1f, 0));
            entity.Set(new EnemySearchComponent(radiusSearchEnemy, 2, 0)); // solo debe aplicar cuando es ataque melle
        }
        else
        {
            
            entity.Set(new RangedAttackComponent(characterBaseData.idMod,characterBaseData.idProjectile,20,10f,1f,0,true,6));
        }
        entity.Set(new AttackPendingComponent(false, default));



        entity.Set(new SteeringComponent(rvoRadius, 2, Vector2.Zero));

        AddCollider(entity, position, characterBaseData.bodyColliders, colliderMove, out int idDebugMove, out int idDebugBody);
        if (DEBUG_COLLIDERS)
        {
            int idDebugRangeMelle = CollisionShapeDraw.Instance.DrawCircleShape(colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, Colors.Blue);
            int idDebugRangeSearch = CollisionShapeDraw.Instance.DrawCircleShape(radiusSearchEnemy, position, Colors.Yellow);
            entity.Set(new RvoAgentDebugComponent(idDebugMove, idDebugBody, idDebugRangeMelle,idDebugRangeSearch));
        }
        return entity;
    }



}

