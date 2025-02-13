using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Tilemap;
public class TileMapChunkRender<TData> where TData : TileDataGame
{
    Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
    public Vector2I ChunkSize { get; private set; }

    public int layer { get; set; }
    public int maxPool { get; set; }

    private Dictionary<Vector2, ChunkData<TData>> dataChunks = new Dictionary<Vector2, ChunkData<TData>>();
    private Dictionary<Vector2, ChunkRender> loadedChunks = new Dictionary<Vector2, ChunkRender>();
    private Queue<ChunkRender> chunkPool = new Queue<ChunkRender>();

    public TileMapChunkRender(int layer)
    {
        ChunkManager.Instance.OnChunkLoad += Instance_OnChunkLoad;
        ChunkManager.Instance.OnChunkUnload += Instance_OnChunkUnload;
        maxPool = 15;
        ChunkSize = PositionsManager.Instance.chunkDimencion;
        this.layer = layer;
        multimeshMaterialDict = new Dictionary<int, MultimeshMaterial>();
    }
    public void AddUpdatedTile(Vector2I tilePositionGlobal, Vector2 PositionReal, TileData tileData, TData dataGame)
    {
        var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
        var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            CreateTile( dataChunks[chunkPosition], tilePositionChunk, PositionReal, tileData, dataGame);

        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.Add(chunkPosition, tilemapDataChunk);
            CreateTile( dataChunks[chunkPosition], tilePositionChunk, PositionReal, tileData, dataGame);
        }
    }

    private void CreateTile( ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2 positionReal, TileData tileData, TData dataGame)
    {
        MultimeshMaterial multimeshMaterial = null;
        int idInternalPosition = 0;
        int idMaterial = 0;


        TileAnimateData tileAnimateData = null;
        switch (tileData.type)
        {
            case "TileSimpleData":
                TileSimpleData tileSimpleData = (TileSimpleData)tileData;

                if (!multimeshMaterialDict.ContainsKey(tileSimpleData.idMaterial))
                {
                    multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(tileSimpleData.idMaterial));
                    multimeshMaterialDict.Add(tileSimpleData.idMaterial, multimeshMaterial);
                }
                idMaterial = tileSimpleData.idMaterial;
                multimeshMaterial = multimeshMaterialDict[tileSimpleData.idMaterial];
                idInternalPosition = tileSimpleData.idInternalPosition;
                break;
            case "TileAnimateData":
                tileAnimateData = (TileAnimateData)tileData;
                if (!multimeshMaterialDict.ContainsKey(tileAnimateData.idMaterial))
                {
                    MaterialData materialData = MaterialManager.Instance.GetMaterial(tileAnimateData.idMaterial);
                    multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(tileAnimateData.idMaterial));
                    multimeshMaterialDict.Add(tileAnimateData.idMaterial, multimeshMaterial);
                }

                idMaterial = tileAnimateData.idMaterial;
                multimeshMaterial = multimeshMaterialDict[tileAnimateData.idMaterial];
                break;
            default:
                break;
        }

        float x = MeshCreator.PixelsToUnits(multimeshMaterial.materialData.divisionPixelX) / 2f;
        float y = MeshCreator.PixelsToUnits(multimeshMaterial.materialData.divisionPixelY) / 2f;
        positionReal = positionReal + new Vector2(x, y);

        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
        xform.Origin = new Vector3(positionReal.X, positionReal.Y, (positionReal.Y * CommonAtributes.LAYER_MULTIPLICATOR) + layer);

        if (!tileMapChunkData.ExistTile(tilePositionChunk))
        {
            dataGame.idCollider = TileNumeratorManager.Instance.getNumerator();
        }
        else 
        { 
            dataGame = tileMapChunkData.GetTileAt(tilePositionChunk);
        }
        
        
        dataGame.positionReal = positionReal;
        dataGame.tilePositionChunk = tilePositionChunk;
        dataGame.idMaterial = idMaterial;
        dataGame.transform3d = xform;
        dataGame.idTile = tileData.id;
        dataGame.collisionBody = tileData.collisionBody;
        dataGame.idInternal = idInternalPosition;
        tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataGame);
        if (tileData.haveCollider)
        {
            CollisionManager.Instance.tileColliders.AddUpdateItem(new Vector2(positionReal.X, positionReal.Y), dataGame);
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
                        chunk.CreateUpdate(i, j, dataGame.idMaterial, instance.Item1, instance.Item2, dataGame.idInternal, dataGame.transform3d, dataGame.idTile);
                    }
                }
            }
            loadedChunks.Add(chunkPos, chunk);

        }
    }
}
