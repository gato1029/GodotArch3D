

using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Metrics;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.Flecs.Globals;

public class ChunkEntityContainer
{
    private Dictionary<Vector2I, List<Entity>> tiles = new();
    public List<Entity> flat = new();

    public  List<Entity> GetEntitiesChunk(Vector2I TilePos)
    {
        if (tiles.TryGetValue(TilePos, out var list))
        {
            return list;
        }
        return new List<Entity>();
    }
    public void AddEntity(Vector2I tilePos, Entity e)
    {
        if (!tiles.TryGetValue(tilePos, out var list))
        {
            list = new List<Entity>();
            tiles[tilePos] = list;
        }

        list.Add(e);
        flat.Add(e);
    }  
    // Remover entidad
    public bool RemoveEntity(Vector2I tilePos, Entity e)
    {
        if (tiles.TryGetValue(tilePos, out var list))
        {
            bool removedFromTile = list.Remove(e);
            bool removedFromFlat = flat.Remove(e);

            // Si la lista del tile quedó vacía, la eliminamos
            if (list.Count == 0)
                tiles.Remove(tilePos);

            return removedFromTile && removedFromFlat;
        }

        return false;
    }
}
public class ChunkEntityRender
{
    public Vector2 ChunkPosition { get; private set; }

    public bool IsActive { get; private set; }

    public ChunkEntityRender()
    {
        
    }

    public void Initialize(Vector2 chunkPos)
    {
        ChunkPosition = chunkPos;
        IsActive = true;
      
    }
    public void RenderEntityForced(Entity entity)
    {
        TileSpriteComponent ts = entity.Get<TileSpriteComponent>();
        PositionComponent pc = entity.Get<PositionComponent>();
        var typeSprite = MasterDataManager.GetData<TileSpriteData>(ts.idTileSprite);
        switch (typeSprite.tileSpriteType)
        {
            case TileSpriteType.Static:
                CreateRenderStatic(entity, typeSprite, pc);
                break;
            case TileSpriteType.Animated:
                CreateRenderAnimate(entity, typeSprite, pc);
                break;
            default:
                break;
        }
    }

    public void RenderEntities(List<Entity> entities)
    {
        foreach (var e in entities)
        {
            RenderEntityForced(e);    
        }        
    }

    private void CreateRenderAnimate(Entity entity, TileSpriteData data, PositionComponent position)
    {
        Vector2 positionTexture = position.position + new Vector2(data.animationData.offsetInternal.X * data.animationData.scale, data.animationData.offsetInternal.Y * data.animationData.scale);
        int renderLayer = entity.Get<LayerRenderComponent>().layerRender;
        float z = ((position.position.Y + data.animationData.yDepthRender) * CommonAtributes.LAYER_MULTIPLICATOR) + renderLayer;
        Transform3D transform3D = new Transform3D(Basis.Identity, new Vector3(positionTexture.X, positionTexture.Y, z));
        transform3D = transform3D.ScaledLocal(new Vector3(data.animationData.scale, data.animationData.scale, 1));

        EntityChunkMap.Instance.AddPendingInstance(new PendingInstance
        {
            entity = entity,
            layer = renderLayer,
            tileId = data.id,
            transform = transform3D
        });
    }

    private void CreateRenderStatic(Entity entity, TileSpriteData data, PositionComponent position)
    {      
        Vector2 positionTexture = position.position + new Vector2(data.spriteData.offsetInternal.X * data.spriteData.scale, data.spriteData.offsetInternal.Y * data.spriteData.scale);
        int renderLayer = entity.Get<LayerRenderComponent>().layerRender;
        float z = ((position.position.Y + data.spriteData.yDepthRender) * CommonAtributes.LAYER_MULTIPLICATOR) + renderLayer;
        Transform3D transform3D = new Transform3D(Basis.Identity, new Vector3(positionTexture.X, positionTexture.Y, z));
        transform3D = transform3D.ScaledLocal(new Vector3(data.spriteData.scale, data.spriteData.scale, 1));

        EntityChunkMap.Instance.AddPendingInstance(new PendingInstance { entity = entity, 
            layer = renderLayer, 
            tileId =data.id, 
            transform = transform3D});
             
        
    }

    public void Release(List<Entity> entities)
    {
        foreach (var e in entities)
        {
            RenderReleaseForced(e);
        }
        

    }

    internal void RenderReleaseForced(Entity entity)
    {
        if (!entity.IsAlive())
        {
            return;
        }

        EntityChunkMap.Instance.AddPendingRemoveInstance(new PendingInstance
        {
            entity = entity
        });

       
    }
}
// Gestor de renderizado de entidades por chunks
public class EntityChunkRender 
{
    public Dictionary<Vector2, ChunkEntityContainer> dataChunks = new();

    private ChunkManagerBase chunkManager;

    private Queue<ChunkEntityRender> chunkPoolRender = new Queue<ChunkEntityRender>();
    
    private Dictionary<Vector2, ChunkEntityRender> loadedChunksRender = new();
        
    private bool renderEnabled = true;
    private int renderingInstanceCount = 0;
    public int maxPool { get; set; }
    public Vector2I ChunkSize { get; private set; }
    public Vector2I tileSize { get; set; }


    public EntityChunkRender(ChunkManagerBase ChunkManager)
    {
        this.chunkManager = ChunkManager;
        chunkManager.OnChunkLoad += Instance_OnChunkLoad;
        chunkManager.OnChunkUnload += Instance_OnChunkUnload;
        chunkManager.OnChunkProcessingCompleted += ChunkManager_OnChunkProcessingCompleted;
        maxPool = 25;
        ChunkSize = chunkManager.chunkDimencion;
        tileSize = chunkManager.tileSize;
    }

    private void CreateChunkRender(Godot.Vector2 chunkPos)
    {
        // Si ya esta cargado, no hagas nada
        if (loadedChunksRender.ContainsKey(chunkPos))
            return;
        if (dataChunks.ContainsKey(chunkPos))
        {

            ChunkEntityRender chunkRender = null;
            if (chunkPoolRender.Count > 0)
            {
                chunkRender = chunkPoolRender.Dequeue();
            }
            else
            {
                chunkRender = new ChunkEntityRender();
            }
            loadedChunksRender.Add(chunkPos, chunkRender);
        }
    }

    internal void Refresh(Vector2I tilePositionGlobal, Entity entity)
    {
        if (!renderEnabled) return; // 🚫 No cargar render si está deshabilitado
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        // Cargar el chunk si aún no está en render
        if (!loadedChunksRender.TryGetValue(chunkPosition, out var chunk))
        {
            // Crea el chunk render
            CreateChunkRender(chunkPosition);
            if (!loadedChunksRender.TryGetValue(chunkPosition, out chunk))
            {
                //caso cuando no se pudo crear el chunk render, que ocurre con vecindades a un no cargadas
                return;
            }     
        }

        if (!dataChunks.TryGetValue(chunkPosition, out var tileMapChunkData))
            return;

        // liberar el tile Renderizado     
      //  chunk.RenderReleaseForced(entity);
        // Forzar la actualización del render para la entidad específica
        chunk.RenderEntityForced(entity);

    }
    public void AddEntity(Vector2I global, Entity entity)
    {
        Vector2I chunkPos = chunkManager.ChunkPosition(global);
        Vector2I tilePos = chunkManager.TilePositionInChunk(chunkPos, global);

        if (!dataChunks.TryGetValue(chunkPos, out var container))
        {
            container = new ChunkEntityContainer();
            dataChunks.Add(chunkPos, container);
        }

            container.AddEntity(tilePos, entity);
        Refresh(global, entity);
    }
    public void RemoveEntity(Vector2I tileGlobalPosition, Entity entity)
    {
        Vector2I chunkPos = chunkManager.ChunkPosition(tileGlobalPosition);
        Vector2I tilePos = chunkManager.TilePositionInChunk(chunkPos, tileGlobalPosition);

        if (dataChunks.TryGetValue(chunkPos, out var container))
        {
            
            if (loadedChunksRender.TryGetValue(chunkPos, out var chunk))
            {
                chunk.RenderReleaseForced(entity);
            }

            container.RemoveEntity(tilePos, entity);
        }

    }

    public List<Entity> GetEntities(Vector2I tileGlobalPosition)
    {
        Vector2I chunkPos = chunkManager.ChunkPosition(tileGlobalPosition);
        Vector2I tilePos = chunkManager.TilePositionInChunk(chunkPos, tileGlobalPosition);
        if (dataChunks.TryGetValue(chunkPos, out var container))
        {
            
            if (container != null && container.flat != null)
            {
                return container.GetEntitiesChunk(tilePos);                
            }
        }
        return new List<Entity>();
    }
    public void MoveEntity(Vector2I oldTile, Vector2I newTile, Entity e)
    {
        RemoveEntity(oldTile, e);
        AddEntity(newTile, e);
    }

    private void ChunkManager_OnChunkProcessingCompleted()
    {
        //throw new NotImplementedException();
    }

    private void Instance_OnChunkUnload(Vector2 chunkPos)
    {
      
        if (loadedChunksRender.ContainsKey(chunkPos))
        {
            ChunkEntityRender tileMapChunkRender = loadedChunksRender[chunkPos];
            tileMapChunkRender.Release(dataChunks[chunkPos].flat);

            if (chunkPoolRender.Count < maxPool)
            {
                chunkPoolRender.Enqueue(tileMapChunkRender);
            }
            loadedChunksRender.Remove(chunkPos);
   

        }
    }

    private void Instance_OnChunkLoad(Vector2 chunkPos)
    {
      
        if (!renderEnabled) return; // 🚫 No cargar render si está deshabilitado
        // Si ya esta cargado, no hagas nada
        if (loadedChunksRender.ContainsKey(chunkPos))
            return;
        if (dataChunks.ContainsKey(chunkPos))
        {

            ChunkEntityRender chunkRender;
            if (chunkPoolRender.Count > 0)
            {
                chunkRender = chunkPoolRender.Dequeue();
            }
            else
            {
                chunkRender = new ChunkEntityRender();
            }

            ChunkEntityContainer tileMapChunkData = dataChunks[chunkPos];

            chunkRender.Initialize(chunkPos);
            chunkRender.RenderEntities(tileMapChunkData.flat);
            loadedChunksRender.Add(chunkPos, chunkRender);
        }
    }
}
