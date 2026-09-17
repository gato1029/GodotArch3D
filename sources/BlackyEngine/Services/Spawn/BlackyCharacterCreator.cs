using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.Services.Spawn;

public struct CreateCharacterCommand
{
    public int height;
    public long Id;
    public Vector2 Position;
}

public class BlackyCharacterCreator
{
    private int layer = (int)BlackyRenderLayer.Personajes_Arboles_Edificios;
    private readonly FlecsManager flecsManager;
    private readonly FastSpatialHash dynamicHash;
    private readonly Core.BlackyWorld world;
    private bool DEBUG_COLLIDERS = false;

    // Cola thread-safe y presupuesto por frame
    private readonly ConcurrentQueue<CreateCharacterCommand> _createQueue = new();
    private const int MaxOpsPerFrame = 5;

    public BlackyCharacterCreator(FlecsManager flecsManager, FastSpatialHash dynamicHash, Core.BlackyWorld world)
    {
        this.flecsManager = flecsManager;
        this.dynamicHash = dynamicHash;
        this.world = world;
    }

    public void RequestCreate(long id,int height, Vector2 position)
    {
        _createQueue.Enqueue(new CreateCharacterCommand
        {
            Id = id,
            height = height,
            Position = position
        });
    }

    public void ProcessPendingCommands()
    {
        int created = 0;
        while (created < MaxOpsPerFrame && _createQueue.TryDequeue(out var cmd))
        {
            InternalExecuteCreation(cmd.Id,cmd.height, cmd.Position);
            created++;
        }
    }
    public void CreateRenderDirect(SavedUnitData unitData)
    {
        world.Tick.TotalUnits++;
        CharacterModelBaseData charModel= BlackyPalletesPersistence.characterPalette.GetData(unitData.TemplateId);
        
        if (charModel == null) return;

        var entity = flecsManager.WorldFlecs.Entity();

        switch (charModel.characterType)
        {
            case CharacterType.MAIN:
                CreateGeneric(unitData.TemplateId, entity, charModel, new Vector2(unitData.PosX,unitData.PosY), unitData.Height,unitData.Health);                
                break;
            case CharacterType.NPC:
                break;
            case CharacterType.ENEMIGO:
                CreateEnemy(unitData.TemplateId, entity, charModel, new Vector2(unitData.PosX, unitData.PosY), unitData.Height,unitData.Health);
                break;
            default:
                break;
        }

    }
    public Entity InternalExecuteCreation(long id, int height, Godot.Vector2 position)
    {
        world.Tick.TotalUnits++;
        ushort characterId = BlackyPalletesPersistence.characterPalette.GetIdPersistence("Base", id, out CharacterModelBaseData charModel);

        if (charModel == null) return default;

        var entity = flecsManager.WorldFlecs.Entity();

        return charModel.characterType switch
        {
            CharacterType.MAIN => CreateGeneric(characterId, entity, charModel, position, height),
            CharacterType.ENEMIGO => CreateEnemy(characterId, entity, charModel, position, height),
            _ => entity,
        };
    }

    private void AddCollider(Entity entity, Vector2 position, List<FastCollider> collisionBodys, GeometricShape2D collisionFeet, out int idDebugMove, out int idDebugBody, ushort team)
    {
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
        }

        var spatialIDComponent = new SpatialIDComponent
        {
            Layer = CollisionConfig.TypePlayer,
            Mask = CollisionConfig.TypeBullet | CollisionConfig.TypePlayer,
            Value = dynamicHash.GetNewNodeIndex()
        };
        var moveComponent = new MoveColliderComponent(widthPies, new Vector2(offsetXPies, offsetYPies), widthPies * 1.5f);

        entity.Set(spatialIDComponent);
        entity.Set(moveComponent);
        entity.Add<UnitTag>();

        dynamicHash.Register(spatialIDComponent.Value, position.X, position.Y, entity,team);

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

    private Entity CreateGeneric(ushort characterId, Entity entity, CharacterModelBaseData characterBaseData, Vector2 position, int height,int health=0)
    {        
        var idTileSprite = characterBaseData.idTileSpriteData;
        int spriteId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);
        AtlasModsManager.TryGetTileSprite(spriteId, out var sprite);
        var animationDir = sprite.spriteMultipleAnimationDirection;

        var moveData = animationDir.animationsTypes[AnimationType.PARADO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];
        GeometricShape2D colliderMove = moveData.collisionDictionary[CollisionUseType.BASE_PIES].Multiplicity(characterBaseData.scale);

        var attackData = animationDir.animationsTypes[AnimationType.ATACANDO_CUERPO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];
        Circle colliderAtackMelle = (Circle)attackData.collisionDictionary[CollisionUseType.RADIO_ATAQUE_CUERPO].Multiplicity(characterBaseData.scale);

        var instance = AtlasTexturesModsManager.Instance.CreateInstanceRender(moveData.idModMaterial);
        Godot.Vector2 originOffset = new Vector2(moveData.offsetInternal.X * characterBaseData.scale, moveData.offsetInternal.Y * characterBaseData.scale);

        float depthOffset = moveData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, height, layer, position);

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X + originOffset.X, position.Y + originOffset.Y, z);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instance.rid, instance.instance, 0, instance.layerTexture, layer, depthOffset, characterBaseData.scale, originOffset));
        entity.Set(new AnimationComponent(characterBaseData.idTileSpriteData, EntityType.PERSONAJE, AnimationType.PARADO, AnimationType.NINGUNA, 0, 0, 0, false, true, true));
        entity.Set(new RenderFrameDataComponent { uvMap = moveData.uvFramesArray[0] });

        entity.Set(new GodotFlecs.sources.Flecs.Components.StateComponent
        {
            stateType = StateType.IDLE,
            behaviorType = BehaviorType.PERSONAJE_PRINCIPAL,
            lastStateType = StateType.BLOCKED
        });
        if (characterBaseData.isPersist)
        {
            entity.Add<PersistEntityTag>(); // O un componente con datos si lo prefieres
        }
        entity.Set(new TeamComponent(1));
        entity.Set(new UnitDefinitionComponent(characterId));
        entity.Set(new PositionComponent(position, Vector2I.Zero, height));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero, DirectionAnimationType.OCHO, GodotEcsArch.sources.components.AnimationDirection.LEFT));
        entity.Set(new VelocityComponent(new Vector2(0, 0), 3, new Vector2(0, 0)));
        entity.Set(new MoveResolutorComponent(false, 0, position, 0, 0));
        entity.Set(new PlayerInputComponent());
        if (health!=0)
        {
            entity.Set(new HealthComponent(health));
        }
        else
        {
            entity.Set(new HealthComponent(100));
        }
        

        float rvoRadius = MeshCreator.PixelsToUnits(16);

        entity.Set(new MeleeAttackComponent(20, colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, 0f, 0,0));
        entity.Set(new SteeringComponent(rvoRadius, 4, Vector2.Zero));
        entity.Set(new WeaponComponent(1, false));

        AddCollider(entity, position, characterBaseData.bodyColliders, colliderMove, out int idDebugMove, out int idDebugBody,1);

        int idDebugRangeMelle = CollisionShapeDraw.Instance.DrawCircleShape(colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, Colors.Blue);

        if (DEBUG_COLLIDERS)
        {
            entity.Set(new RvoAgentDebugComponent(idDebugMove, idDebugBody, idDebugRangeMelle, 0));
        }

        entity.Add<UseBoidTag>();

        return entity;
    }

    private Entity CreateEnemy(ushort characterId, Entity entity, CharacterModelBaseData characterBaseData, Godot.Vector2 position, int height, int health=0)
    {
        characterBaseData.isPersist = true;
        var idTileSprite = characterBaseData.idTileSpriteData;
        int spriteId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);
        AtlasModsManager.TryGetTileSprite(spriteId, out var sprite);
        var animationDir = sprite.spriteMultipleAnimationDirection;

        var moveData = animationDir.animationsTypes[AnimationType.PARADO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];
        GeometricShape2D colliderMove = moveData.collisionDictionary[CollisionUseType.BASE_PIES].Multiplicity(characterBaseData.scale);

        var attackData = animationDir.animationsTypes[AnimationType.ATACANDO_CUERPO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];
        Circle colliderAtackMelle = (Circle)attackData.collisionDictionary[CollisionUseType.RADIO_ATAQUE_CUERPO].Multiplicity(characterBaseData.scale);

        var instance = AtlasTexturesModsManager.Instance.CreateInstanceRender(moveData.idModMaterial);
        Godot.Vector2 originOffset = new Vector2(moveData.offsetInternal.X * characterBaseData.scale, moveData.offsetInternal.Y * characterBaseData.scale);

        float depthOffset = moveData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, height, layer, position);
        
        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X+ originOffset.X, position.Y+ originOffset.Y, z);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instance.rid, instance.instance, 0, instance.layerTexture, layer, depthOffset, characterBaseData.scale, originOffset));
        entity.Set(new AnimationComponent(characterBaseData.idTileSpriteData, EntityType.PERSONAJE, AnimationType.PARADO, AnimationType.NINGUNA, 0, 0, 0, false, true, true));
        entity.Set(new RenderFrameDataComponent { uvMap = moveData.uvFramesArray[0] });

        entity.Set(new StateComponent
        {
            stateType = StateType.IDLE,
            behaviorType = BehaviorType.GENERICO,
            lastStateType = StateType.BLOCKED
        });
        if (characterBaseData.isPersist)
        {
            entity.Add<PersistEntityTag>(); // O un componente con datos si lo prefieres
        }
        entity.Set(new TeamComponent(2));
        entity.Set(new UnitDefinitionComponent(characterId));
        entity.Set(new PositionComponent(position, Vector2I.Zero, 1));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero, animationDir.directionAnimationType, GodotEcsArch.sources.components.AnimationDirection.LEFT));
        entity.Set(new VelocityComponent(new Vector2(0, 0), 3f, new Vector2(0, 0)));
        entity.Set(new MoveResolutorComponent(false, 0, position, 0, 0));
        entity.Add<StoppedTag>();
        entity.Add<UseBoidTag>();
        if (health != 0)
        {
            entity.Set(new HealthComponent(health));
        }
        else
        {
            entity.Set(new HealthComponent(100));
        }

        float rvoRadius = MeshCreator.PixelsToUnits(16);
        float radiusSearchEnemy = MeshCreator.PixelsToUnits(256);

        if (characterBaseData.unitAttackType == UnitAttackType.CUERPO)
        {
            entity.Set(new MeleeAttackComponent(20, colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, 1f, 0,ContadoresHelper.Obtener(TipoContador.UnidadesMelle)));
            entity.Set(new EnemySearchComponent(radiusSearchEnemy, 0, 0));
      
        }
        else
        {
            entity.Set(new RangedAttackComponent(characterBaseData.idMod, characterBaseData.idProjectile, 20, 10f, 1f, 0, true, 6, ContadoresHelper.Obtener(TipoContador.EdificiosUnidadesRango)));
        }

        entity.Set(new AttackPendingComponent(false, default,false,Vector2.Zero,0));
        entity.Set(new SteeringComponent(rvoRadius, 2, Vector2.Zero));

        AddCollider(entity, position, characterBaseData.bodyColliders, colliderMove, out int idDebugMove, out int idDebugBody,2);

        if (DEBUG_COLLIDERS)
        {
            int idDebugRangeMelle = CollisionShapeDraw.Instance.DrawCircleShape(colliderAtackMelle.Radius, colliderAtackMelle.OriginCurrent, Colors.Blue);
            int idDebugRangeSearch = CollisionShapeDraw.Instance.DrawCircleShape(radiusSearchEnemy, position, Colors.Yellow);
            entity.Set(new RvoAgentDebugComponent(idDebugMove, idDebugBody, idDebugRangeMelle, idDebugRangeSearch));
        }

        return entity;
    }
}