using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Globals;
internal class EntityChunkMap : SingletonBase<EntityChunkMap>
{
    // Diccionarios por capa
    private readonly Dictionary<int, EntityChunkRender> rendersByLayer = new();
    private readonly Dictionary<int, OccupancyGrid> occupancyByLayer = new();

    private ChunkManager chunkManager;

    protected override void Initialize()
    {
        chunkManager = ChunkManager.Instance;
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

    public bool IsPosibleAddEntity(Vector2I positionTileWorld, int sizeX, int sizeY, int layer)
    {
        var grid = GetGrid(layer);
        if (!grid.CanPlace(positionTileWorld,sizeX,sizeY))
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
    public void AddEntityToChunk(Entity entity, Vector2I positionTileWorld, int layer)
    {
        // 1) Grid
        var grid = GetGrid(layer);
        grid.SetOccupied(positionTileWorld, occupied: true);

        // 2) Guardar layer en la entidad
        entity.Set(new LayerRenderComponent(layer));

        // 3) Render
        var render = GetRender(layer);
        render.AddEntity(positionTileWorld, entity);
    }

    public void RemoveEntityFromChunk(Entity entity, Vector2I positionTileWorld)
    {        
        int layer = entity.Get<LayerRenderComponent>().layerRender;
        // 1) Grid
        var grid = GetGrid(layer);
        grid.SetOccupied(positionTileWorld, occupied: false);

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
            var idGenericComponent = entity.Get<IdGenericComponent>();
            if (idGenericComponent.entityType == entityType)
            {
                return entity;
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

