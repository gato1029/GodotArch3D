using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.Services.Paint;

public struct CreateBuildingCommand
{
    public ushort IdBuilding;
    public Vector2I TilePosition;
    public int Height;
}

public struct RemoveBuildingCommand
{
    public Vector2I TilePosition;
}

public class BlackyBuildingCreator
{
    private readonly BlackyChunkOccupancyMap occupancyMap;
    private readonly BlackySpatialEntityMap spatialEntityMap;
    private readonly StaticSpatialGridOptimizedGeneric<Entity> staticHash;
    private readonly BlackyTerrainWorldData terrain;
    private readonly FlecsManager flecsManager;

    // Colas thread-safe para peticiones desde cualquier hilo
    private readonly ConcurrentQueue<CreateBuildingCommand> _createQueue = new();
    private readonly ConcurrentQueue<RemoveBuildingCommand> _removeQueue = new();
    private const int MaxOpsPerFrame = 5; // Presupuesto por frame para evitar tirones

    private const bool DEBUG_COLLIDERS = false;
    private readonly Dictionary<int, List<int>> _colliderDebugMap = new();
    private readonly int layer = (int)BlackyRenderLayer.Personajes_Arboles_Edificios;

    public BlackyBuildingCreator(
        FlecsManager flecsManager,
        BlackyChunkOccupancyMap occupancyMap,
        BlackySpatialEntityMap spatialEntityMap,
        StaticSpatialGridOptimizedGeneric<Entity> staticHash,
        BlackyTerrainWorldData terrain)
    {
        this.flecsManager = flecsManager;
        this.occupancyMap = occupancyMap;
        this.spatialEntityMap = spatialEntityMap;
        this.staticHash = staticHash;
        this.terrain = terrain;
    }

    // --- MÉTODOS PÚBLICOS DE ENCOLADO (Seguros para hilos secundarios) ---

    public void RequestCreate(ushort idBuilding, Vector2I tilePosition, int height)
    {
        _createQueue.Enqueue(new CreateBuildingCommand
        {
            IdBuilding = idBuilding,
            TilePosition = tilePosition,
            Height = height
        });
    }

    public void RequestRemove(Vector2I tilePosition)
    {
        _removeQueue.Enqueue(new RemoveBuildingCommand
        {
            TilePosition = tilePosition
        });
    }

    // --- PROCESAMIENTO EN EL HILO PRINCIPAL (Llamado por Flecs) ---

    public void ProcessPendingCommands()
    {
        int created = 0;
        while (created < MaxOpsPerFrame && _createQueue.TryDequeue(out var cmd))
        {
            InternalExecuteCreation(cmd.IdBuilding, cmd.TilePosition, cmd.Height);
            created++;
        }

        int removed = 0;
        while (removed < MaxOpsPerFrame && _removeQueue.TryDequeue(out var cmd))
        {
            InternalExecuteRemoval(cmd.TilePosition);
            removed++;
        }
    }

    // --- EJECUCIÓN REAL (Hilo Principal) ---
    public void CreationDataNoRender(SavedBuildingData itemBuild)
    {
        InternalExecuteCreation(itemBuild.TemplateId, new Vector2I(itemBuild.WorldTileX, itemBuild.WorldTileY), itemBuild.Height, itemBuild.Health, false);
    }
    private void InternalExecuteCreation(ushort idBuilding, Vector2I tilePosition, int height, int health=0, bool render=true)
    {
        ushort team = 1;
        var templateBuilding = BlackyPalletesPersistence.buildingPalette.GetData(idBuilding);
        if (templateBuilding == null) return;

        int spriteIdNormal = AtlasModsManager.GetSpriteUniqueId(templateBuilding.IdTileSpriteNormal);
        AtlasModsManager.TryGetTileSprite(spriteIdNormal, out var spriteNormal);

        if (spriteNormal == null) return;

        if (occupancyMap.IsOccupiedTiles(0, tilePosition.X, tilePosition.Y, spriteNormal.tilesOcupancy))
        {
            return;
        }

        int spriteIdConstruccion = 0;
        int spriteIdDestruccion = 0;
        if (templateBuilding.IdTileSpriteConstruccion!=0)
        {
            spriteIdConstruccion = AtlasModsManager.GetSpriteUniqueId(templateBuilding.IdTileSpriteConstruccion);
            AtlasModsManager.TryGetTileSprite(spriteIdConstruccion, out var spriteConstruccion);
        }
        if (templateBuilding.IdTileSpriteDestruccion!=0)
        {
            spriteIdDestruccion = AtlasModsManager.GetSpriteUniqueId(templateBuilding.IdTileSpriteDestruccion);
            AtlasModsManager.TryGetTileSprite(spriteIdDestruccion, out var spriteDestruccion);
        }
        


        Entity entity = flecsManager.WorldFlecs.Entity();
        Vector2 position = TilesHelper.TilePositionToWorldPosition(tilePosition);

        entity.Set(new PositionComponent { position = position, tilePosition = tilePosition, height = height });
        entity.Set(new TeamComponent(team));
        entity.Set(new BuildingDefinitionComponent(idBuilding, spriteIdNormal, spriteIdConstruccion, spriteIdDestruccion));

        if (health!=0)
        {
            entity.Set(new HealthComponent(health));
        }
        else
        {
            entity.Set(new HealthComponent(templateBuilding.MaxHealth));
        }
        
        
        spatialEntityMap.Add(entity, ChunkHelper.WorldToChunkCoord(tilePosition),true);

        AsignarCollider(tilePosition.X, tilePosition.Y, entity, spriteNormal,out int idDebugBody,team);
        occupancyMap.SetTiles(0, tilePosition.X, tilePosition.Y, spriteNormal.tilesOcupancy, entity.Id.Value);
        entity.Set(new RvoAgentDebugComponent(0, idDebugBody, 0, 0));
        // Renderizado
        switch (spriteNormal.tileSpriteType)
        {
            case TileSpriteType.Static:
                CreateSprite(entity, spriteNormal.spriteData, height, tilePosition,render);
                break;
            case TileSpriteType.Animated:
                CreateAnimation(entity, spriteNormal.animationData, spriteIdNormal, height, tilePosition,render);
                break;
        }

        // Particularidades por tipo de edificio
        if (templateBuilding.BuildingType == BuildingType.Torres)
        {
            CreateTorre(entity, templateBuilding);
        }
    }

    private void InternalExecuteRemoval(Vector2I tilePosition)
    {
        ulong idEntity = occupancyMap.Get(0, tilePosition.X, tilePosition.Y);
        if (idEntity == 0) return;

        Entity entity = flecsManager.WorldFlecs.Entity(idEntity);
        if (!entity.IsAlive()) return;

        if (entity.Has<SpatialIDComponent>())
        {
            SpatialIDComponent spatial = entity.Get<SpatialIDComponent>();
            staticHash.FreeCollider(spatial.Value);

            if (_colliderDebugMap.TryGetValue(spatial.Value, out var debugShapes))
            {
                foreach (int item in debugShapes)
                {
                    CollisionShapeDraw.Instance.FreeDraw(item);
                }
                _colliderDebugMap.Remove(spatial.Value);
            }
        }

        if (entity.Has<RenderGPUComponent>())
        {
            RenderGPUComponent gpu = entity.Get<RenderGPUComponent>();
            AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
        }
        if (entity.Has<RangedAttackComponent>())
        {
            var rangedAttack = entity.Get<RangedAttackComponent>();
            ContadoresHelper.Liberar(TipoContador.EdificiosUnidadesRango, rangedAttack.NumberUnitRange);
        }
        spatialEntityMap.Remove(entity);
        occupancyMap.ClearByEntity(0, tilePosition.X, tilePosition.Y);
        entity.Destruct();
    }

    // --- MÉTODOS AUXILIARES ---

    private void CreateTorre(Entity entity, BuildingData templateBuilding)
    {
        int damage = 0;
        if (templateBuilding.AttackPowers != null)
        {
            foreach (var attackPower in templateBuilding.AttackPowers)
            {
                if (attackPower.type == ElementType.BASE)
                {
                    damage = (int)attackPower.value;
                    break;
                }
            }
        }

        entity.Set(new AttackPendingComponent(false, default,false,Vector2.Zero,0));
        entity.Set(new RangedAttackComponent
        {
            idMod = templateBuilding.idMod,
            Cooldown = templateBuilding.AttackCooldown,
            Range = templateBuilding.AttackRange,
            Damage = damage,
            idProjectile = templateBuilding.IdProjectile,
            Timer = 0,
            Homing = false,
            SpeedProjectile = 10,
            NumberUnitRange = ContadoresHelper.Obtener(TipoContador.EdificiosUnidadesRango)
        });
    }

    private void CreateSprite(Entity entity, WindowsDataBase.Accesories.DataBase.SpriteData spriteData, int heightRender, Vector2I positionTile, bool render)
    {
        
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(positionTile.X, positionTile.Y);
        Vector2 offset = spriteData.offsetInternal;

        float depthOffset = spriteData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, heightRender, layer, positionCenter);

        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(spriteData.scale, spriteData.scale, 1));

        if (render)
        {
            var renderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(spriteData.idModMaterial);
            RenderingServer.MultimeshInstanceSetTransform(renderInstance.rid, renderInstance.instance, transform);
            RenderingServer.MultimeshInstanceSetCustomData(renderInstance.rid, renderInstance.instance, spriteData.uv);
            RenderingServer.MultimeshInstanceSetColor(renderInstance.rid, renderInstance.instance, new Godot.Color(0, 0, 0, renderInstance.layerTexture));
            entity.Set(new RenderGPUComponent(renderInstance.rid, renderInstance.instance, 0, renderInstance.layerTexture,
                     layer, depthOffset, spriteData.scale, offset));
        }
        else
        {
            entity.Set(new RenderGPUComponent(default, -1, 0, -1,layer, depthOffset, spriteData.scale, offset));
        }
        
    }

    private void CreateAnimation(Entity entity, WindowsDataBase.Accesories.DataBase.SpriteAnimationData animationData, int idSprite, int heightRender, Vector2I positionTile, bool render)
    {
        
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(positionTile);
        Vector2 offset = animationData.offsetInternal;

        float depthOffset = animationData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, heightRender, layer, positionCenter);
        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(animationData.scale, animationData.scale, 1));
        
        entity.Set(new RenderTransformComponent(transform));
        if (render)
        {
            var renderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(animationData.idModMaterial);
            RenderingServer.MultimeshInstanceSetTransform(renderInstance.rid, renderInstance.instance, transform);
            RenderingServer.MultimeshInstanceSetCustomData(renderInstance.rid, renderInstance.instance, animationData.uvFramesArray[0]);
            RenderingServer.MultimeshInstanceSetColor(renderInstance.rid, renderInstance.instance, new Godot.Color(0, 0, 0, renderInstance.layerTexture));
            entity.Set(new RenderGPUComponent(renderInstance.rid, renderInstance.instance, 0, renderInstance.layerTexture,
                 layer, depthOffset, animationData.scale, offset));

            entity.Set(new AnimationSimpleComponent(idSprite, 1, 0, animationData.frameDuration, false, true, true));
            entity.Set(new RenderFrameDataComponent { uvMap = animationData.uvFramesArray[0] });
            entity.Add<SpriteSimpleAnimationTag>();
        }
        else
        {
            entity.Set(new RenderGPUComponent(default, -1, 0, -1,layer, depthOffset, animationData.scale, offset));
        }
        
        
    }

    private int AsignarCollider(int mundoX, int mundoY, Entity entity, TileSpriteData tileSpriteData, out  int idDebugBody, ushort team)
    {
        idDebugBody = -1;
        if (tileSpriteData.fastCollidersBody.Count == 0) return 0;

        int idCollider = staticHash.GetNewEntityId();
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(mundoX, mundoY);

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

        if (DEBUG_COLLIDERS)
        {
            _colliderDebugMap[idCollider] = new List<int>();
        }

        foreach (var fast in tileSpriteData.fastCollidersBody)
        {
            float actualX = positionCenter.X + fast.Offset.X;
            float actualY = positionCenter.Y + fast.Offset.Y;
            float width = fast.Shape == ShapeType.Circle ? fast.Width * 2 : fast.Width;
            float height = fast.Shape == ShapeType.Circle ? fast.Height * 2 : fast.Height;

            float currentMinX = actualX - (width * 0.5f) - 0.01f;
            float currentMinY = actualY - (height * 0.5f) - 0.01f;
            float currentMaxX = actualX + (width * 0.5f) + 0.01f;
            float currentMaxY = actualY + (height * 0.5f) + 0.01f;

            if (currentMinX < minX) minX = currentMinX;
            if (currentMinY < minY) minY = currentMinY;
            if (currentMaxX > maxX) maxX = currentMaxX;
            if (currentMaxY > maxY) maxY = currentMaxY;

            if (DEBUG_COLLIDERS)
            {
                 idDebugBody = CollisionShapeDraw.Instance.DrawCollisionShapes(fast, positionCenter, Godot.Colors.OrangeRed);
                _colliderDebugMap[idCollider].Add(idDebugBody);
            }
            else
            {
                idDebugBody = -1;
            }
        }

        staticHash.RegisterStatic(idCollider, entity, minX, minY, maxX, maxY,team);

        entity.Set(new SpatialIDComponent
        {
            Layer = CollisionConfig.TypeResource,
            Mask = CollisionConfig.None,
            Value = idCollider
        });

        return idCollider;
    }


}