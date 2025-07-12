using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;

public interface IDataSprite
{
    public int idUnique { get; set; }
    public int idData { get; set; }
    public bool isAnimation { get; set; }
    public Vector3 positionWorld { get; set; }
    public Vector2 positionCollider { get; set; }
    public Vector2I positionTileWorld { get; set; }
    public Vector2I positionTileChunk { get; set; }
    public SpriteData GetSpriteData();
    public AnimationStateData GetAnimationStateData();
}
public class SpriteMapChunk<TData> where TData: IDataSprite, new() 
{
    public event Action<Vector2, ChunkData<TData>> OnChunSerialize;

    Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
    public Vector2I ChunkSize { get; private set; }
    public Vector2I tileSize { get; set; }
    public int layer { get; set; }
    public int maxPool { get; set; }
    public int IdMaterialLastUsed { get => idMaterialLastUsed; set => idMaterialLastUsed = value; }

    private bool serializableUnload;
    private int idMaterialLastUsed = 0;
    public Dictionary<Vector2, ChunkData<TData>> dataChunks = new Dictionary<Vector2, ChunkData<TData>>();
    private Dictionary<Vector2, ChunkRenderGPU> loadedChunksRender = new();
    private Queue<ChunkRenderGPU> chunkPoolRender = new Queue<ChunkRenderGPU>();    
    private ChunkManagerBase chunkManager;

    public SpriteMapChunk(int layer, ChunkManagerBase ChunkManager, bool SerializableUnload = false)
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

    public void AddUpdatedTile(Vector2I tilePositionGlobal, SpriteData spriteData,  int idDataBase, bool isAnimation = false)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, spriteData, idDataBase, isAnimation);

        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.Add(chunkPosition, tilemapDataChunk);
            CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, spriteData, idDataBase, isAnimation);
        }
    }



    private void CreateTile(ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2I tilePositionGlobal, SpriteData spriteData, int idDataBase, bool isAnimation)
    {
        //MultimeshMaterial multimeshMaterial = null;
        
        int idMaterial = spriteData.idMaterial;
        IdMaterialLastUsed = idMaterial;
        //if (!multimeshMaterialDict.ContainsKey(spriteData.idMaterial))
        //{
        //    multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(spriteData.idMaterial), ChunkManager.Instance.maxTileView);
        //    multimeshMaterialDict.Add(spriteData.idMaterial, multimeshMaterial);
        //}
        //multimeshMaterial = multimeshMaterialDict[idMaterial];

        float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;


        Vector2 positionNormalize = tilePositionGlobal * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
        Vector2 positionCenter = positionNormalize + new Vector2(x, y);
        Vector2 positionReal = positionNormalize + new Vector2(x, y) + new Vector2(spriteData.offsetInternal.X * spriteData.scale, spriteData.offsetInternal.Y * spriteData.scale);

        TData dataGame = default;
        if (!tileMapChunkData.ExistTile(tilePositionChunk))
        {
            dataGame = new TData();
            dataGame.idUnique = TileNumeratorManager.Instance.getNumerator();
        }
        else
        {
            dataGame = tileMapChunkData.GetTileAt(tilePositionChunk);
        }
        dataGame.idData = idDataBase;
        dataGame.isAnimation = isAnimation;
        Vector2 posicionCollider = positionReal;
        dataGame.positionWorld = new Vector3(positionReal.X, positionReal.Y, (positionCenter.Y * CommonAtributes.LAYER_MULTIPLICATOR) + layer);
        //dataGame.Scale = spriteData.scale;        
        dataGame.positionTileChunk = tilePositionChunk;
        dataGame.positionTileWorld = tilePositionGlobal;


        tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataGame);
        if (spriteData.haveCollider)
        {

            posicionCollider = positionNormalize + new Vector2(x, y) + spriteData.collisionBody.Multiplicity(spriteData.scale).OriginCurrent;//+ new Vector2(x, y);
            dataGame.positionCollider = posicionCollider;
            CollisionManager.Instance.spriteColliders.AddUpdateItem(posicionCollider, dataGame);
        }
        else
        {
            dataGame.positionCollider = posicionCollider;
            CollisionManager.Instance.spriteColliders.RemoveItem(posicionCollider, dataGame);
        }

    }

   

    private void Instance_OnChunkUnload(Godot.Vector2 chunkPos)
    {
        if (loadedChunksRender.ContainsKey(chunkPos))
        {
            ChunkRenderGPU tileMapChunkRender = loadedChunksRender[chunkPos];

            for (int i = 0; i < tileMapChunkRender.size.X; i++)
            {
                for (int j = 0; j < tileMapChunkRender.size.Y; j++)
                {
                    SpriteRender datatile = tileMapChunkRender.tiles[i, j];
                    
                    if (datatile != null && datatile.instance != -1) //&& datatile.idMaterial != 0)
                    {
                        MultimeshManager.Instance.FreeInstance(datatile.rid, datatile.instance);
                        //multimeshMaterialDict[datatile.idMaterial].FreeInstance(datatile.rid, datatile.instance);
                        datatile.FreeRidRender();
                    }
                }
            }
            if (chunkPoolRender.Count < maxPool)
            {
                chunkPoolRender.Enqueue(tileMapChunkRender);
            }
            loadedChunksRender.Remove(chunkPos);
            if (serializableUnload && dataChunks[chunkPos].changue)
            {
                OnChunSerialize?.Invoke(chunkPos, dataChunks[chunkPos]);// SaveOnDisk(chunkPos);
            }

        }

    }
    private void Instance_OnChunkLoad(Godot.Vector2 chunkPos)
    {
        if (dataChunks.ContainsKey(chunkPos))
        {
            ChunkRenderGPU chunkRender;
            if (chunkPoolRender.Count > 0)
            {
                chunkRender = chunkPoolRender.Dequeue();
            }
            else
            {
                chunkRender = new ChunkRenderGPU(chunkPos, ChunkSize);
            }

            ChunkData<TData> tileMapChunkData = dataChunks[chunkPos];


            for (int i = 0; i < tileMapChunkData.size.X; i++)
            {
                for (int j = 0; j < tileMapChunkData.size.Y; j++)
                {
                    TData dataGame = tileMapChunkData.tiles[i, j];
                    if (dataGame != null)
                    {
                        int idMaterial = dataGame.GetSpriteData().idMaterial;
                        (Rid, int) instance = MultimeshManager.Instance.CreateInstance(); // multimeshMaterialDict[idMaterial].CreateInstance();
                        if (dataGame.isAnimation)
                        {
                            chunkRender.CreateUpdate(i, j, instance.Item1, instance.Item2, dataGame.positionWorld, dataGame.GetSpriteData(), dataGame.GetAnimationStateData());
                        }
                        else
                        {
                            chunkRender.CreateUpdate(i, j, instance.Item1, instance.Item2, dataGame.positionWorld, dataGame.GetSpriteData());
                        }
                        
                    }
                }
            }
            loadedChunksRender.Add(chunkPos, chunkRender);

        }
    }
}
