using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
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
public class TileMapChunkRender<TData> where TData : TileDataGame
{
    Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
    public Vector2I ChunkSize { get; private set; }
    public Vector2I tileSize { get; set; }
    public int layer { get; set; }
    public int maxPool { get; set; }

    private Dictionary<Vector2, ChunkData<TData>> dataChunks = new Dictionary<Vector2, ChunkData<TData>>();
    private Dictionary<Vector2, ChunkRender> loadedChunks = new Dictionary<Vector2, ChunkRender>();
    private Queue<ChunkRender> chunkPool = new Queue<ChunkRender>();

    ChunkManagerBase chunkManager;
    public TileMapChunkRender(int layer, ChunkManagerBase ChunkManager)
    {
        this.chunkManager = ChunkManager;
        chunkManager.OnChunkLoad += Instance_OnChunkLoad;
        chunkManager.OnChunkUnload += Instance_OnChunkUnload;
        
        maxPool = 25;
        ChunkSize = chunkManager.chunkDimencion;
        tileSize = chunkManager.tileSize;

        this.layer = layer;
        multimeshMaterialDict = new Dictionary<int, MultimeshMaterial>();
        
    }

    (Vector2I chunk, Vector2I tile) GetTilePositionInChunk(Vector2 tilePositionGlobalPixel)
    {
      
        int tileX = (int)MathF.Floor(tilePositionGlobalPixel.X / tileSize.X);
        int tileY = (int)MathF.Floor(tilePositionGlobalPixel.Y / tileSize.Y);
        var tilePosition = new Vector2(tileX, tileY);

        Vector2I chunkPosition = new Vector2I((int)MathF.Floor(tilePositionGlobalPixel.X/ ChunkSize.X),
                                            (int)MathF.Floor(tilePositionGlobalPixel.Y / ChunkSize.Y));        
        return (chunkPosition,chunkPosition);
    }

    public TData GetTileGlobalPosition(Vector2I tilePositionGlobal)
    {
        var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
        var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);
        if (dataChunks.ContainsKey(chunkPosition))
        {
            return dataChunks[chunkPosition].GetTileAt(tilePositionChunk);
        }
        return null;
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
        byte neighborMask = CalculateNeighborMask(tilePositionGlobal); //
        TileRuleData bestRule = autoTileData.FindBestMatchingRule(neighborMask);
        
        if (bestRule == null)
        {
            GD.PrintErr($"❌ No se encontró una regla válida para mask {neighborMask}");
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
                byte newMask = CalculateNeighborMask(neighborPos);

                foreach (var kvp in autoTileData.arrayTiles)
                {
                    TileRuleData bestRule = autoTileData.FindBestMatchingRule(newMask);

                    if (bestRule != null)
                    {
                        AddUpdatedTile(neighborPos, bestRule.tileDataCentral, dataValue);                      
                        break;
                    }
                }
            }
        }
    }
    private int GetDirectionIndex(NeighborDirection direction)
    {
        switch (direction)
        {
            case NeighborDirection.Up: return 0;
            case NeighborDirection.Right: return 1;
            case NeighborDirection.Down: return 2;
            case NeighborDirection.Left: return 3;
            case NeighborDirection.UpRight: return 4;
            case NeighborDirection.DownRight: return 5;
            case NeighborDirection.DownLeft: return 6;
            case NeighborDirection.UpLeft: return 7;
            default:
                throw new ArgumentException("Dirección no válida.");
        }
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



    public void AddUpdatedTile(Vector2I tilePositionGlobal, TileData tileData, TData dataGame)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        //var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
        //var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            CreateTile( dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, tileData, dataGame);

        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.Add(chunkPosition, tilemapDataChunk);
            CreateTile( dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, tileData, dataGame);
        }
    }

    private void CreateTile(ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2I tilePositionGlobal, TileData tileData, TData dataGame)
    {
        MultimeshMaterial multimeshMaterial = null;

        int idMaterial = tileData.idMaterial;

        if (!multimeshMaterialDict.ContainsKey(tileData.idMaterial))
        {
            multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(tileData.idMaterial));
            multimeshMaterialDict.Add(tileData.idMaterial, multimeshMaterial);
        }
        multimeshMaterial = multimeshMaterialDict[idMaterial];

        float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;
        //float x = 0;
        //float y = 0;

        Vector2 positionNormalize = tilePositionGlobal * new Vector2( MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
        Vector2 posicionCollider = positionNormalize + new Vector2(x, y);

        Vector2 positionReal = positionNormalize + new Vector2(x, y) +new Vector2(tileData.offsetInternal.X,tileData.offsetInternal.Y);

        
        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
        xform.Origin = new Vector3(positionReal.X, positionReal.Y, (positionReal.Y * CommonAtributes.LAYER_MULTIPLICATOR) + layer);
        xform = xform.ScaledLocal(new Vector3(tileData.scale, tileData.scale, 1));
        

        if (!tileMapChunkData.ExistTile(tilePositionChunk))
        {
            dataGame.idCollider = TileNumeratorManager.Instance.getNumerator();
        }
        else 
        { 
            dataGame = tileMapChunkData.GetTileAt(tilePositionChunk);
        }
        
        
        dataGame.positionCollider = posicionCollider;
        dataGame.tilePositionChunk = tilePositionChunk;
        dataGame.idMaterial = idMaterial;
        dataGame.transform3d = xform;
        dataGame.idTile = tileData.id;
        dataGame.collisionBody = tileData.collisionBody;    
        tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataGame);
        if (tileData.haveCollider)
        {
            CollisionManager.Instance.tileColliders.AddUpdateItem(posicionCollider, dataGame);
        }
        else
        {
            CollisionManager.Instance.tileColliders.RemoveItem(posicionCollider, dataGame);
        }
        
    }

    private void Instance_OnChunkUnload(Godot.Vector2 chunkPos)
    {
        if (loadedChunks.ContainsKey(chunkPos))
        {
            ChunkRender tileMapChunkRender = loadedChunks[chunkPos];

            for (int i = 0; i < tileMapChunkRender.size.X; i++)
            {
                for (int j = 0; j < tileMapChunkRender.size.Y; j++)
                {
                    var datatile = tileMapChunkRender.tiles[i, j];
                    if (datatile != null && datatile.idMaterial != 0)
                    {
                        multimeshMaterialDict[datatile.idMaterial].FreeInstance(datatile.rid, datatile.instance);
                        datatile.FreeTile();
                    }
                }
            }
            if (chunkPool.Count < maxPool)
            {
                chunkPool.Enqueue(tileMapChunkRender);
            }
            loadedChunks.Remove(chunkPos);
        }
    }

    private void Instance_OnChunkLoad(Godot.Vector2 chunkPos)
    {
        if (dataChunks.ContainsKey(chunkPos))
        {
            ChunkRender chunk;
            if (chunkPool.Count > 0)
            {
                chunk = chunkPool.Dequeue();
            }
            else
            {
                chunk = new ChunkRender(chunkPos, ChunkSize);
            }

            ChunkData<TData> tileMapChunkData = dataChunks[chunkPos];

            for (int i = 0; i < tileMapChunkData.size.X; i++)
            {
                for (int j = 0; j < tileMapChunkData.size.Y; j++)
                {
                    var dataGame = tileMapChunkData.tiles[i, j];
                    if (dataGame != null)
                    {
                        (Rid, int) instance = multimeshMaterialDict[dataGame.idMaterial].CreateInstance();
                        chunk.CreateUpdate(i, j, dataGame.idMaterial, instance.Item1, instance.Item2, dataGame.transform3d, dataGame.idTile);
                    }
                }
            }
            loadedChunks.Add(chunkPos, chunk);

        }
    }
}
