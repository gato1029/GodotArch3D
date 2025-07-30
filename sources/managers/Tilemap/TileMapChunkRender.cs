using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Tilemap;

public interface IDataTile
{
    public int IdTile { get; set; }
    public Vector3 PositionWorld { get; set; }
    public float Scale { get; set; }
    public int IdCollider { get; set; }
    public Vector2 PositionCollider { get; set; }
    public Vector2I PositionTileWorld { get; set; }
    public Vector2I PositionTileChunk { get; set; }
}

public class TileMapChunkRender<TData> where TData : IDataTile
{
    public event Action<Vector2, ChunkData<TData>> OnChunSerialize;
    

    Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
    public Vector2I ChunkSize { get; private set; }
    public Vector2I tileSize { get; set; }
    public int layer { get; set; }
    public int maxPool { get; set; }
    public int IdMaterialLastUsed { get => idMaterialLastUsed; set => idMaterialLastUsed = value; }

    private bool serializableUnload;
    public Dictionary<Vector2, ChunkData<TData>> dataChunks = new Dictionary<Vector2, ChunkData<TData>>();
    private Dictionary<Vector2, ChunkRender> loadedChunksRender = new Dictionary<Vector2, ChunkRender>();
    private Queue<ChunkRender> chunkPool = new Queue<ChunkRender>();


    private int idMaterialLastUsed = 0;

    ChunkManagerBase chunkManager;
    public TileMapChunkRender(int layer, ChunkManagerBase ChunkManager, bool SerializableUnload=false)
    {
        serializableUnload = SerializableUnload;
        this.chunkManager = ChunkManager;
        chunkManager.OnChunkLoad += Instance_OnChunkLoad;
        chunkManager.OnChunkUnload += Instance_OnChunkUnload;
        
        maxPool = 25;
        ChunkSize = chunkManager.chunkDimencion;
        tileSize = chunkManager.tileSize;

        this.layer = layer;
        multimeshMaterialDict = new Dictionary<int, MultimeshMaterial>();
        
    }

    public void LoadMaterials(List<int> materialsUsed)
    {
        foreach (var item in materialsUsed)
        {
            if (!multimeshMaterialDict.ContainsKey(item))
            {
                MultimeshMaterial multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(item), ChunkManager.Instance.maxTileView);
                multimeshMaterialDict.Add(item, multimeshMaterial);
            }
        }
    }
    public void CreateChunkData(ChunkDataSerializable<TData> serializable)
    {
       
        ChunkData<TData> data = FromSerializable(serializable);
        if (!dataChunks.ContainsKey(data.positionChunk))
        {
            dataChunks.Add(data.positionChunk, data);

            foreach (var item in data.tiles)
            {
                if (item !=null)
                {
                    var tileData = TilesManager.Instance.GetTileData(item.IdTile);             
                    if (tileData.haveCollider)
                    {
                        CollisionManager.Instance.tileColliders.AddUpdateItem(item.PositionCollider, item);
                    }

                }
             
            }
            
        }
        else
        {
            dataChunks[data.positionChunk] = data;
        }
        
    }

    public TData GetTileGlobalPosition(Vector2I tilePositionGlobal)
    {
        var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
        var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);
        if (dataChunks.ContainsKey(chunkPosition))
        {
            return dataChunks[chunkPosition].GetTileAt(tilePositionChunk);
        }
        return default;
    }
    public void AddUpdatedTileRule(Vector2I tilePositionGlobal, AutoTileData autoTileData, TData dataGame)
    {        
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition,tilePositionGlobal);
        //var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
        //var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            
            CreateTile(dataChunks[chunkPosition],tilePositionGlobal, tilePositionChunk,  autoTileData, dataGame);

        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.Add(chunkPosition, tilemapDataChunk);
            CreateTile(dataChunks[chunkPosition], tilePositionGlobal, tilePositionChunk,  autoTileData, dataGame);
        }
    }

    private void CreateTile(ChunkData<TData> chunkData, Vector2I tilePositionGlobal, Vector2I tilePositionChunk,  AutoTileData autoTileData, TData dataGame)
    {
        //byte neighborMask = CalculateNeighborMask(tilePositionGlobal); //
        var neighborTileMask = GetNeighborTileIdsAndMask(tilePositionGlobal);
        TileRuleData bestRule = autoTileData.FindBestMatchingRule(neighborTileMask.mask, neighborTileMask.neighborTileIds);
       
        if (bestRule == null)
        {
            GD.PrintErr($"❌ No se encontró una regla válida para mask {neighborTileMask.mask}");
            return;
        }
        // 🟢 Colocar el tile en la posición dada
        CreateTile(chunkData, tilePositionChunk, tilePositionGlobal, bestRule.tileDataCentral, dataGame);
        UpdateNeighbors(tilePositionGlobal,autoTileData,dataGame);
    }
    private void UpdateNeighbors(Vector2I tilePosGlobal, AutoTileData autoTileData, TData dataGame)
    {
        for (int i = 0; i < 8; i++)
        {
            NeighborDirection direction = GetDirectionFromIndex(i);
            Vector2I neighborPos = TilesHelper.GetNeighborPosition(tilePosGlobal, direction);

            var dataValue = GetTileGlobalPosition(neighborPos);
            if (dataValue != null) // Si hay un tile vecino
            {
                //byte newMask = CalculateNeighborMask(neighborPos);
                var neighborTileMask = GetNeighborTileIdsAndMask(neighborPos);
                
                foreach (var kvp in autoTileData.arrayTiles)
                {
                    //TileRuleData bestRule = autoTileData.FindBestMatchingRule(newMask);
                    TileRuleData bestRule = autoTileData.FindBestMatchingRule(neighborTileMask.mask, neighborTileMask.neighborTileIds);
                    if (bestRule != null)
                    {
                        AddUpdatedTile(neighborPos, bestRule.tileDataCentral, dataValue);                      
                        Refresh(neighborPos);
                        break;
                    }
                }
            }
        }
    }
    private void UpdateNeighborsNotData(Vector2I tilePosGlobal, AutoTileData autoTileData)
    {
        for (int i = 0; i < 8; i++)
        {
            NeighborDirection direction = GetDirectionFromIndex(i);
            Vector2I neighborPos = TilesHelper.GetNeighborPosition(tilePosGlobal, direction);

            var dataValue = GetTileGlobalPosition(neighborPos);
            if (dataValue != null) // Si hay un tile vecino
            {
                //byte newMask = CalculateNeighborMask(neighborPos);
                var neighborTileMask = GetNeighborTileIdsAndMask(neighborPos);
                foreach (var kvp in autoTileData.arrayTiles)
                {
                    //TileRuleData bestRule = autoTileData.FindBestMatchingRule(newMask);
                    TileRuleData bestRule = autoTileData.FindBestMatchingRule(neighborTileMask.mask, neighborTileMask.neighborTileIds);
                    if (bestRule != null)
                    {
                        AddUpdatedTile(neighborPos, bestRule.tileDataCentral, dataValue);
                        Refresh(neighborPos);
                        break;
                    }
                }
            }
        }
    }
    private (int[] neighborTileIds, byte mask) GetNeighborTileIdsAndMask(Vector2I tilePosGlobal)
    {
        int[] neighborTileIds = new int[8];
        byte mask = 0;

        for (int i = 0; i < 8; i++)
        {
            NeighborDirection direction = (NeighborDirection)(1 << i);
            Vector2I neighborPos = TilesHelper.GetNeighborPosition(tilePosGlobal, direction);
            var neighborData = GetTileGlobalPosition(neighborPos);

            if (neighborData != null && neighborData.IdTile != 0)
            {
                neighborTileIds[i] = neighborData.IdTile;
                mask |= (byte)direction;
            }
            else
            {
                neighborTileIds[i] = 0; // 0 indica que no hay tile
            }
        }

        return (neighborTileIds, mask);
    }

    public NeighborDirection GetDirectionFromIndex(int index)
    {
        switch (index)
        {
            case 0: return NeighborDirection.Up;
            case 1: return NeighborDirection.Right;
            case 2: return NeighborDirection.Down;
            case 3: return NeighborDirection.Left;
            case 4: return NeighborDirection.UpRight;
            case 5: return NeighborDirection.DownRight;
            case 6: return NeighborDirection.DownLeft;
            case 7: return NeighborDirection.UpLeft;
            default:
                throw new ArgumentException("Índice no válido. Debe estar entre 0 y 7.");
        }
    }
    private byte CalculateNeighborMask(Vector2I tilePosGlobal)
    {
        byte mask = 0;

        for (int i = 0; i < 8; i++)
        {
            NeighborDirection direction = GetDirectionFromIndex(i);
            Vector2I neighborPos = TilesHelper.GetNeighborPosition(tilePosGlobal, direction);

            if (GetTileGlobalPosition(neighborPos) != null) // Si hay un tile vecino
            {
                mask |= (byte)direction;
            }
            else
            {
                mask &= (byte)~direction;
            }
        }

        return mask;
    }



    public bool  AddUpdatedTile(Vector2I tilePositionGlobal, TileData tileData, TData dataGame)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        if (dataGame ==null)   
        {
            return false;
        }
        //var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
        //var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            if (dataChunks[chunkPosition].GetTileAt(tilePositionChunk)!= null && dataChunks[chunkPosition].GetTileAt(tilePositionChunk).IdTile == tileData.id)
            {
                return false;
            }
            else
            {
                CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, tileData, dataGame);
            }
            
        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.Add(chunkPosition, tilemapDataChunk);
            CreateTile( dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, tileData, dataGame);
        }
        return true;
    }



    private void CreateTile(ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2I tilePositionGlobal, TileData tileData, TData dataGame)
    {
        //MultimeshMaterial multimeshMaterial = null;

        int idMaterial = tileData.idMaterial;
        IdMaterialLastUsed = idMaterial;
        //if (!multimeshMaterialDict.ContainsKey(tileData.idMaterial))
        //{
        //    multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(tileData.idMaterial), ChunkManager.Instance.maxTileView);
        //    multimeshMaterialDict.Add(tileData.idMaterial, multimeshMaterial);
        //}
        //multimeshMaterial = multimeshMaterialDict[idMaterial];

        float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;

        
        Vector2 positionNormalize = tilePositionGlobal * new Vector2( MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y) );        
        Vector2 positionReal = positionNormalize + new Vector2(x , y ) + new Vector2(tileData.offsetInternal.X * tileData.scale, tileData.offsetInternal.Y * tileData.scale);        
        if (!tileMapChunkData.ExistTile(tilePositionChunk))
        {
            dataGame.IdCollider = TileNumeratorManager.Instance.getNumerator();
        }
        else 
        { 
            dataGame = tileMapChunkData.GetTileAt(tilePositionChunk);
        }


        Vector2 posicionCollider = positionReal;
        dataGame.PositionWorld = new Vector3(positionReal.X, positionReal.Y, (positionReal.Y * CommonAtributes.LAYER_MULTIPLICATOR) + layer);
        dataGame.Scale = tileData.scale;      
        dataGame.IdTile = tileData.id;
        dataGame.PositionTileChunk = tilePositionChunk;
        dataGame.PositionTileWorld = tilePositionGlobal;


        tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataGame);
        if (tileData.haveCollider)
        {

            posicionCollider = positionNormalize + new Vector2(x, y) + tileData.collisionBody.Multiplicity(tileData.scale).OriginCurrent;//+ new Vector2(x, y);
            dataGame.PositionCollider = posicionCollider;
            CollisionManager.Instance.tileColliders.AddUpdateItem(posicionCollider, dataGame);
        }
        else
        {
            dataGame.PositionCollider = posicionCollider;
            CollisionManager.Instance.tileColliders.RemoveItem(posicionCollider, dataGame);
        }
        
    }

    private void Instance_OnChunkUnload(Godot.Vector2 chunkPos)
    {
        if (loadedChunksRender.ContainsKey(chunkPos))
        {
            ChunkRender tileMapChunkRender = loadedChunksRender[chunkPos];

            for (int i = 0; i < tileMapChunkRender.size.X; i++)
            {
                for (int j = 0; j < tileMapChunkRender.size.Y; j++)
                {
                    var datatile = tileMapChunkRender.tiles[i, j];
                    if (datatile != null && datatile.instance!=-1) //&& datatile.idMaterial != 0)
                    {
                        var dataTileInfo = TilesManager.Instance.GetTileData(datatile.idTile);
                        // multimeshMaterialDict[dataTileInfo.idMaterial].FreeInstance(datatile.rid, datatile.instance);
                        MultimeshManager.Instance.FreeInstance( datatile.rid, datatile.instance,dataTileInfo.idMaterial);
                        datatile.FreeTile();
                    }
                }
            }
            if (chunkPool.Count < maxPool)
            {
                chunkPool.Enqueue(tileMapChunkRender);
            }
            loadedChunksRender.Remove(chunkPos);
            if (serializableUnload && dataChunks[chunkPos].changue)
            {
                OnChunSerialize?.Invoke(chunkPos, dataChunks[chunkPos]);// SaveOnDisk(chunkPos);
            }
            
        }
        
    }

    private void SaveOnDisk(Godot.Vector2 chunkPos)
    {
        Serializer.Data.ChunkDataSerializable<TData> dataSer = dataChunks[chunkPos].ToSerializable();

    }
    public  ChunkData<TData> FromSerializable(ChunkDataSerializable<TData> serializable)
    {
        var chunk = new ChunkData<TData>(
            serializable.PositionChunk,  // Se convierte implícitamente a Vector2
            serializable.Size            // Se convierte implícitamente a Vector2I
        );

        foreach (var tile in serializable.Tiles)
        {
            var pos = tile.Position; // ProtoVector2I -> Vector2I (implícito)
            chunk.tiles[pos.X, pos.Y] = tile.Value;
        }

        return chunk;
    }
    
    private void Instance_OnChunkLoad(Godot.Vector2 chunkPos)
    {
        int instanciasCargadas = 0;
        // Si ya esta cargado, no hagas nada
        if (loadedChunksRender.ContainsKey(chunkPos))
            return;

        if (dataChunks.ContainsKey(chunkPos))
        {
            ChunkRender chunkRender;
            if (chunkPool.Count > 0)
            {
                chunkRender = chunkPool.Dequeue();
            }
            else
            {
                chunkRender = new ChunkRender(chunkPos, ChunkSize);
            }

            ChunkData<TData> tileMapChunkData = dataChunks[chunkPos];

            for (int i = 0; i < tileMapChunkData.size.X; i++)
            {
                for (int j = 0; j < tileMapChunkData.size.Y; j++)
                {
                    var dataGame = tileMapChunkData.tiles[i, j];
                    if (dataGame != null)
                    {
                        //
                        var tileData = TilesManager.Instance.GetTileData(dataGame.IdTile);
                        var instanceComplex = MultimeshManager.Instance.CreateInstance( tileData.idMaterial);// multimeshMaterialDict[tileData.idMaterial].CreateInstance();
                        chunkRender.CreateUpdate(i, j,  instanceComplex.rid, instanceComplex.instance, instanceComplex.materialBatchPosition, dataGame.PositionWorld,dataGame.Scale, dataGame.IdTile);
                        instanciasCargadas++;
                    }
                }
            }           
            loadedChunksRender.Add(chunkPos, chunkRender);                      
        }
    }

    public void Remove(Vector2I tilePositionGlobal, AutoTileData autoTileData)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            if (dataChunks[chunkPosition].GetTileAt(tilePositionChunk) != null )
            {
               var data = dataChunks[chunkPosition].GetTileAt(tilePositionChunk);

                dataChunks[chunkPosition].RemoveTile(tilePositionChunk);

                if (loadedChunksRender.ContainsKey(chunkPosition))
                {
                    ChunkRender tileMapChunkRender = loadedChunksRender[chunkPosition];
                    var datatile = tileMapChunkRender.tiles[tilePositionChunk.X, tilePositionChunk.Y];
                    if (datatile != null && datatile.instance != -1) 
                    {
                        var dataTileInfo = TilesManager.Instance.GetTileData(datatile.idTile);                        
                        MultimeshManager.Instance.FreeInstance(datatile.rid, datatile.instance, dataTileInfo.idMaterial);
                        datatile.FreeTile();
                       
                        UpdateNeighborsNotData(tilePositionGlobal, autoTileData);
                    }
                }
                
                
            }
           

        }
    }
    internal void Refresh(Vector2I tilePositionGlobal)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        // Cargar el chunk si aún no está en render
        if (!loadedChunksRender.TryGetValue(chunkPosition, out var chunk))
        {
            Instance_OnChunkLoad(chunkPosition);
            if (!loadedChunksRender.TryGetValue(chunkPosition, out chunk))
                return;
        }

        if (!dataChunks.TryGetValue(chunkPosition, out var tileMapChunkData))
            return;

        TData tileData = tileMapChunkData.tiles[tilePositionChunk.X, tilePositionChunk.Y];
        if (tileData == null)
            return;

        // liberar el tile Renderizado     
        Tile tileRender = chunk.GetTileAt(tilePositionChunk);
        if (tileRender != null && tileRender.instance!=-1)
        {
            MultimeshManager.Instance.FreeInstance(tileRender.rid, tileRender.instance, tileRender.idMaterial);
            tileRender.FreeTile();
        }

        var tileInfo = TilesManager.Instance.GetTileData(tileData.IdTile);
        var instance = MultimeshManager.Instance.CreateInstance(tileInfo.idMaterial);

        chunk.CreateUpdate(
            tilePositionChunk.X,
            tilePositionChunk.Y,
            instance.rid,
            instance.instance,
            instance.materialBatchPosition,
            tileData.PositionWorld,
            tileData.Scale,
            tileData.IdTile
        );
    }
}
