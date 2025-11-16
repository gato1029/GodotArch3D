
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Globals;

public struct PendingInstance
{
    public PendingInstance(Entity entity, long tileId, Transform3D transform, int layer)
    {
        this.entity = entity;
        this.tileId = tileId;
        this.transform = transform;
        this.layer = layer;
    }

    public Entity entity {  get; set; }
    public long tileId { get; set; }
    public Transform3D transform { get; set; }
    public int layer {  get; set; } 
    public Vector2I tilePosition { get; set; }  
    public Vector2 position { get; set; }
    public bool isTileSprite { get; set; } = false;
}
internal class EntityChunkMap : SingletonBase<EntityChunkMap>
{
    // Diccionarios por capa
    private readonly Dictionary<int, EntityChunkRender> rendersByLayer = new();
    private readonly Dictionary<int, OccupancyGrid> occupancyByLayer = new();

    private ChunkManager chunkManager;

    private readonly ConcurrentQueue<PendingInstance> pendingInstances
        = new ConcurrentQueue<PendingInstance>();

    private readonly ConcurrentQueue<PendingInstance> pendingInstancesRemove
        = new ConcurrentQueue<PendingInstance>();

    protected override void Initialize()
    {
        chunkManager = ChunkManager.Instance;
    }

    public void AddPendingInstance(PendingInstance pending)
    {
        pendingInstances.Enqueue(pending);
    }
    public void AddPendingRemoveInstance(PendingInstance pending)
    {
        pendingInstancesRemove.Enqueue(pending);
    }
    public void ProcessPendingRemoveInstaces()
    {
        while (pendingInstancesRemove.TryDequeue(out var item))
        {
            if (item.entity.IsAlive())
            {
                RenderGPUComponent rgp = item.entity.Get<RenderGPUComponent>();                
                TileSpriteComponent ts = item.entity.Get<TileSpriteComponent>();                
                TileSpriteData data = MasterDataManager.GetData<TileSpriteData>(ts.idTileSprite);
                MultimeshManager.Instance.FreeInstance(rgp.rid, rgp.instance, rgp.idMaterial);

                if (item.entity.Has<TileSpriteAnimationTag>())
                {
                    item.entity.Remove<TileSpriteAnimationTag>();
                }
                if (item.entity.Has<AnimationComponent>())
                {
                    item.entity.Remove<AnimationComponent>();
                }
                if (item.entity.Has<RenderFrameDataComponent>())
                {
                    item.entity.Remove<RenderFrameDataComponent>();
                }
                if (item.entity.Has<RenderTransformComponent>())
                {
                    item.entity.Remove<RenderTransformComponent>();
                }

            
            }
            else
            {
                GD.Print("Huerfano");
            }
        }
    }
    public void ProcessPendingInstaces()
    {
        while (pendingInstances.TryDequeue(out var item))
        {
            if (item.entity.IsAlive())
            {
                TileSpriteData data = MasterDataManager.GetData<TileSpriteData>(item.tileId);
                switch (data.tileSpriteType)
                {
                    case TileSpriteType.Static:
                        CreateStatic(item, data);
                        break;
                    case TileSpriteType.Animated:
                        CreateAnimated(item, data);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                GD.Print("Huerfano");
            }
        }
    }

    private void CreateAnimated(PendingInstance item, TileSpriteData data)
    {
        if (!item.entity.IsAlive())
        {
            item.entity = FlecsManager.Instance.WorldFlecs.Entity();
        }
        var instanceComplex = MultimeshManager.Instance.CreateInstance(data.animationData.idMaterial);

        // 1. LIMPIEZA (buffer seguro)
        RenderingServer.MultimeshInstanceSetTransform(instanceComplex.rid, instanceComplex.instance, Transform3D.Identity);
        RenderingServer.MultimeshInstanceSetColor(instanceComplex.rid, instanceComplex.instance, Godot.Colors.White);
        RenderingServer.MultimeshInstanceSetCustomData(instanceComplex.rid, instanceComplex.instance, new Godot.Color(0, 0, 0, 0));

        RenderingServer.MultimeshInstanceSetTransform(instanceComplex.rid, instanceComplex.instance, item.transform);
        var uvds = data.animationData.uvFramesArray[0]; 
        RenderingServer.MultimeshInstanceSetCustomData(instanceComplex.rid, instanceComplex.instance, uvds);
        RenderingServer.MultimeshInstanceSetColor(instanceComplex.rid, instanceComplex.instance, new Godot.Color(0, 0, 0, instanceComplex.layerTexture));


        item.entity.Set(new RenderTransformComponent(item.transform));
        item.entity.Set(new RenderGPUComponent(
            instanceComplex.rid,
            instanceComplex.instance,
            instanceComplex.material,
            instanceComplex.layerTexture,
            item.layer,
            data.animationData.yDepthRender,
            data.animationData.scale,
            data.animationData.offsetInternal));

        item.entity.Set(new AnimationComponent(data.id, EntityType.TILESPRITE, 1,
            -1,
            1,
            0,
            data.animationData.frameDuration,
            false,
            true,
            true));

        var uv = data.animationData.uvFramesArray[0];
        item.entity.Set(new RenderFrameDataComponent { uvMap = uv });
        item.entity.Add<TileSpriteAnimationTag>();
        if (item.isTileSprite)
        {
            item.entity.Set(new PositionComponent { position = item.position, tilePosition = item.tilePosition });
        }
    }

    private void CreateStatic(PendingInstance pending, TileSpriteData data)
    {
        if (pending.entity.Has<TileSpriteAnimationTag>())
        {
            pending.entity.Remove<TileSpriteAnimationTag>();    
        }
        if (pending.entity.Has<AnimationComponent>())
        {
            pending.entity.Remove<AnimationComponent>();
        }
        if (pending.entity.Has<RenderFrameDataComponent>())
        {
            pending.entity.Remove<RenderFrameDataComponent>();
        }
        if (pending.entity.Has<RenderTransformComponent>())
        {
            pending.entity.Remove<RenderTransformComponent>();
        }

        var instanceComplex = MultimeshManager.Instance.CreateInstance(data.spriteData.idMaterial);
        // 1. LIMPIEZA (buffer seguro)
        RenderingServer.MultimeshInstanceSetTransform(instanceComplex.rid, instanceComplex.instance, Transform3D.Identity);
        RenderingServer.MultimeshInstanceSetColor(instanceComplex.rid, instanceComplex.instance, Godot.Colors.White);
        RenderingServer.MultimeshInstanceSetCustomData(instanceComplex.rid, instanceComplex.instance, new Godot.Color(0, 0, 0, 0));


        RenderingServer.MultimeshInstanceSetTransform(instanceComplex.rid, instanceComplex.instance, pending.transform);
        var uv = data.spriteData.GetUv();        
        RenderingServer.MultimeshInstanceSetCustomData(instanceComplex.rid, instanceComplex.instance, uv);
        RenderingServer.MultimeshInstanceSetColor(instanceComplex.rid, instanceComplex.instance, new Godot.Color(0, 0, 0, instanceComplex.layerTexture));
        
        pending.entity.Set(new RenderGPUComponent(
            instanceComplex.rid,
            instanceComplex.instance,
            instanceComplex.material,
            instanceComplex.layerTexture,
            pending.layer,
            data.spriteData.yDepthRender,
            data.spriteData.scale,
            data.spriteData.offsetInternal));
        //pending.entity.Set(new RenderFrameDataComponent { uvMap = data.spriteData.GetUv() });
        //pending.entity.Set(new RenderTransformComponent { transform = pending.transform });
    }
    // -------------------------------------------------------------
    // -------------------- MANEJO DE LAYERS -----------------------
    // -------------------------------------------------------------

    /// <summary>
    /// Obtiene el OccupancyGrid de un layer, creándolo si no existe.
    /// </summary>
    private OccupancyGrid GetGrid(int layer)
    {
        if (!occupancyByLayer.TryGetValue(layer, out var grid))
        {
            grid = new OccupancyGrid(chunkManager.tiles16X16);
            occupancyByLayer[layer] = grid;
        }
        return grid;
    }

    /// <summary>
    /// Obtiene el render handler de un layer, creándolo si no existe.
    /// </summary>
    private EntityChunkRender GetRender(int layer)
    {
        if (!rendersByLayer.TryGetValue(layer, out var render))
        {
            render = new EntityChunkRender(chunkManager.tiles16X16);
            rendersByLayer[layer] = render;
        }
        return render;
    }

    // -------------------------------------------------------------
    // ------------------- AGREGAR / REMOVER -----------------------
    // -------------------------------------------------------------
    public bool IsPosibleAddEntity(Vector2I positionTileWorld, List<KuroTile> tiles,int layer)
    {
        var grid = GetGrid(layer);
        if (grid.CanPlace(positionTileWorld, tiles))
        {
            return true;
        }
        return false;
    }
    public bool IsPosibleAddEntity(Vector2I positionTileWorld, int sizeX, int sizeY, int layer)
    {
        var grid = GetGrid(layer);
        if (grid.CanPlace(positionTileWorld,sizeX,sizeY))
        {
            return true;
        }
        return false;
    }

    public bool IsPosibleAddEntity(Vector2I positionTileWorld, int layer)
    {
        var grid = GetGrid(layer);
        if (!grid.IsOccupied(positionTileWorld))
        {
            return true;
        }
        
        return false;
    }
    public void AddEntityToChunk(Entity entity, Vector2I positionTileWorld, List<KuroTile> tiles, int layer)
    {
        // 1) Grid
        var grid = GetGrid(layer);
        foreach (var item in tiles)
        {
            Vector2I pos = new(positionTileWorld.X + item.x, positionTileWorld.Y + item.y);
            grid.SetOccupied(pos, occupied: true);
        }
        

        // 2) Guardar layer en la entidad
        entity.Set(new LayerRenderComponent(layer));

        // 3) Render
        var render = GetRender(layer);
        render.AddEntity(positionTileWorld, entity);
    }

    public void RemoveEntityFromChunk(Entity entity, Vector2I positionTileWorld)
    {
       
        int layer = entity.Get<LayerRenderComponent>().layerRender;
        var data = MasterDataManager.GetData<TileSpriteData>(entity.Get<TileSpriteComponent>().idTileSprite);
        // 1) Grid
        var grid = GetGrid(layer);

        foreach (var item in data.tilesOcupancy)
        {
            Vector2I pos = new(positionTileWorld.X + item.x, positionTileWorld.Y + item.y);
            grid.SetOccupied(pos, occupied: false);
        }        
        // 2) Render
        var render = GetRender(layer);
        render.RemoveEntity(positionTileWorld, entity);

        entity.Destruct();
    }
    public Entity GetEntityInChunk(Vector2I positionTileWorld, EntityType entityType, int layer)
    {
        var render = GetRender(layer);
        var dataEntities = render.GetEntities(positionTileWorld);
        foreach (var entity in dataEntities.ToList())
        {
            if (entity.IsAlive())
            {
                var idGenericComponent = entity.Get<IdGenericComponent>();
                if (idGenericComponent.entityType == entityType)
                {
                    return entity;
                }
            }            
        }        
        return Entity.Null();
    }

    // -------------------------------------------------------------
    protected override void Destroy()
    {
        rendersByLayer.Clear();
        occupancyByLayer.Clear();
    }
}

