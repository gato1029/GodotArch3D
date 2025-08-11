using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;

public interface IDataSprite
{
    public int idData { get; set; }
    public int idUnique { get; set; }
    public bool isAnimation { get; set; }
    public Vector2 positionCollider { get; set; }
    public Vector2I positionTileChunk { get; set; }
    public Vector2I positionTileWorld { get; set; }
    public Vector3 positionWorld { get; set; }
    public AnimationStateData GetAnimationStateData();

    public SpriteData GetSpriteData();
    public bool IsAnimation();
}

public enum FormatSave
{
    JSON,
    BINARIO
}
public interface ISpriteMapChunk
{
    public event Action<int> OnRealInstanceCountChanged;
    public event Action<int> OnRenderingInstanceCountChanged;
    public string GetName();

    public int GetRealInstanceCount();
    public int GetRenderingInstanceCount();
    public void SetRenderEnabled(bool enabled);
    public bool GetRenderEnable();
}
public class SpriteMapChunk<TData>: ISpriteMapChunk where TData: DataItem, new() 
{
    public Dictionary<Vector2, ChunkData<TData>> dataChunks = new Dictionary<Vector2, ChunkData<TData>>();

    private ChunkManagerBase chunkManager;

    private Queue<ChunkRenderGPU> chunkPoolRender = new Queue<ChunkRenderGPU>();

    private int idMaterialLastUsed = 0;
    private Dictionary<Vector2, ChunkRenderGPU> loadedChunksRender = new();
    private string name;
    private int realInstanceCount = 0;
    private bool renderEnabled = true;
    private int renderingInstanceCount = 0;
    private bool serializableUnload;
    private string carpetSave;
    private FormatSave formatSave;
    public SpriteMapChunk(string name, string carpetSave,int layer, ChunkManagerBase ChunkManager, bool SerializableUnload = false, bool render = true, FormatSave formatSave = FormatSave.BINARIO)
    {
        this.formatSave = formatSave;
        this.carpetSave = carpetSave + "/" + name;
        this.name = name;
        serializableUnload = SerializableUnload;
        this.chunkManager = ChunkManager;
        chunkManager.OnChunkLoad += Instance_OnChunkLoad;
        chunkManager.OnChunkUnload += Instance_OnChunkUnload;
        chunkManager.OnChunkProcessingCompleted += ChunkManager_OnChunkProcessingCompleted;
        maxPool = 25;
        ChunkSize = chunkManager.chunkDimencion;
        tileSize = chunkManager.tileSize;

        this.layer = layer;
        renderEnabled = render;

    }

    public event Action<Vector2, ChunkData<TData>> OnChunSerialize;

    public event Action<int> OnRealInstanceCountChanged;

    public event Action<int> OnRenderingInstanceCountChanged;

    public Vector2I ChunkSize { get; private set; }

    public int IdMaterialLastUsed { get => idMaterialLastUsed; set => idMaterialLastUsed = value; }

    public int layer { get; set; }

    public int maxPool { get; set; }

    public Vector2I tileSize { get; set; }
    public string CarpetSave { get => carpetSave; set => carpetSave = value; }
    public bool GetRenderEnable()
    {
        return renderEnabled;
    }
    public void LoadAll()
    {
        List<string> listFiles = FileHelper.GetAllFiles(carpetSave);
        foreach (var item in listFiles)
        {
            string fullPath = item;
            Serializer.Data.ChunkDataSerializable<TData> data = SerializerManager.LoadFromFileBin<Serializer.Data.ChunkDataSerializable<TData>>(fullPath);
            CreateChunkData(data);
        }
    }

    public void ClearAllFiles()
    {
        List<string> listFiles = FileHelper.GetAllFiles(carpetSave);
        foreach (var item in listFiles)
        {
            string fullPath = item;            
            FileHelper.DeleteFile(fullPath);
        }
    }
    public void SaveAll()
    {
        foreach (var item in dataChunks)
        {
            var chunkData = item.Value;
            SaveChunk(chunkData);
        }
    }
    public void SaveChunk(ChunkData<TData> chunkData)
    {
        if (chunkData.changue) // solo se guarda si hubo algun cambio
        {
            string name = chunkData.positionChunk.X + "_" + chunkData.positionChunk.Y;
            var dataSer = chunkData.ToSerializable();
            switch (formatSave)
            {
                case FormatSave.BINARIO:
                    SerializerManager.SaveToFileBin(dataSer, carpetSave, name);
                    break;
                case FormatSave.JSON:
                    SerializerManager.SaveToFileJson(dataSer, carpetSave, name);
                    break;
                default:
                    break;
            }
        }
        
        
    }
    public ChunkData<TData> FromSerializable(ChunkDataSerializable<TData> serializable)
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
    public void CreateChunkData(ChunkDataSerializable<TData> serializable)
    {

        ChunkData<TData> data = FromSerializable(serializable);
        if (!dataChunks.ContainsKey(data.positionChunk))
        {
            dataChunks.Add(data.positionChunk, data);

            foreach (var item in data.tiles)
            {
                if (item != null && item.idData!=0)
                {
                    item.SetDataGame();
                }
            }
        }
        else
        {
            dataChunks[data.positionChunk] = data;
        }

    }
    public void AddUpdatedTile(Vector2I tilePositionGlobal, int idData)
    {
        if (idData == 0) // caso vacio delete
        {
            Remove(tilePositionGlobal);
            return;
        }
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, idData);

        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.Add(chunkPosition, tilemapDataChunk);
            CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, idData);


        }


    }

    public void AddUpdatedTileRule(Vector2I tilePositionGlobal, RuleData[] ruleDataArray)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);
        if (dataChunks.ContainsKey(chunkPosition))
        {

            CreateTileRule(dataChunks[chunkPosition], tilePositionGlobal, tilePositionChunk, ruleDataArray);

        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.Add(chunkPosition, tilemapDataChunk);
            CreateTileRule(dataChunks[chunkPosition], tilePositionGlobal, tilePositionChunk, ruleDataArray);
        }
    }

    public void ClearAllChunks()
    {
        // Liberar instancias visuales y render de los chunks cargados
        foreach (var kvp in loadedChunksRender)
        {
            ChunkRenderGPU chunkRender = kvp.Value;

            for (int i = 0; i < chunkRender.size.X; i++)
            {
                for (int j = 0; j < chunkRender.size.Y; j++)
                {
                    SpriteRender renderTile = chunkRender.tiles[i, j];
                    if (renderTile != null && renderTile.instance != -1)
                    {
                        MultimeshManager.Instance.FreeInstance(renderTile.rid, renderTile.instance, renderTile.idMaterial);
                        renderTile.FreeRidRender();
                    }
                }
            }
        }

        loadedChunksRender.Clear();

        // Limpiar datos lógicos de los tiles
        foreach (var chunk in dataChunks.Values)
        {
            for (int x = 0; x < chunk.size.X; x++)
            {
                for (int y = 0; y < chunk.size.Y; y++)
                {
                    TData tile = chunk.tiles[x, y];
                    if (tile != null)
                    {
                        if (tile.GetSpriteData().haveCollider)
                        {
                            tile.ClearDataGame();
                            //CollisionManager.Instance.spriteColliders.RemoveItem(tile.positionCollider, tile);
                        }
                    }
                }
            }
        }

        dataChunks.Clear();

        // Opcional: limpiar el pool de chunks renderizados
        chunkPoolRender.Clear();
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

    string ISpriteMapChunk.GetName()
    {
        return name;
    }

    public List<TData> GetOccupiedTiles()
    {
        var occupiedTiles = new List<TData>();

        foreach (var chunkPair in dataChunks)
        {
            ChunkData<TData> chunkData = chunkPair.Value;

            for (int x = 0; x < chunkData.size.X; x++)
            {
                for (int y = 0; y < chunkData.size.Y; y++)
                {
                    TData tile = chunkData.tiles[x, y];
                    if (tile != null && tile.idData != 0)
                    {
                        occupiedTiles.Add(tile);
                    }
                }
            }
        }

        return occupiedTiles;
    }

    int ISpriteMapChunk.GetRealInstanceCount()
    {
        return realInstanceCount;
    }

    int ISpriteMapChunk.GetRenderingInstanceCount()
    {
        return renderingInstanceCount;
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

    public ChunkData<TData> GetTilesByChunk(Vector2I chunkPosition)
    {

        if (dataChunks.ContainsKey(chunkPosition))
        {
            return dataChunks[chunkPosition];
        }

        return null;
    }

    public void LoadMaterials(List<int> materialsUsed)
    {

    }

    public void RecalculateRealInstanceCount()
    {
        int total = 0;
        foreach (var chunk in dataChunks.Values)
        {
            for (int x = 0; x < chunk.size.X; x++)
            {
                for (int y = 0; y < chunk.size.Y; y++)
                {
                    var tile = chunk.tiles[x, y];
                    if (tile != null && tile.idData != 0)
                    {
                        total++;
                    }
                }
            }
        }
        realInstanceCount = total;
    }

    public void RecalculateRenderingInstanceCounts()
    {
        renderingInstanceCount = 0;

        foreach (var chunkRender in loadedChunksRender.Values)
        {
            for (int x = 0; x < chunkRender.size.X; x++)
            {
                for (int y = 0; y < chunkRender.size.Y; y++)
                {
                    var renderTile = chunkRender.tiles[x, y];
                    if (renderTile != null && renderTile.instance != -1)
                    {
                        renderingInstanceCount++;
                    }
                }
            }
        }


    }

    public void Remove(Vector2I tilePositionGlobal, RuleData[] autoTileData = null)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            if (dataChunks[chunkPosition].GetTileAt(tilePositionChunk) != null)
            {
                var data = dataChunks[chunkPosition].GetTileAt(tilePositionChunk);

                dataChunks[chunkPosition].RemoveTile(tilePositionChunk);

                if (loadedChunksRender.ContainsKey(chunkPosition))
                {
                    ChunkRenderGPU tileMapChunkRender = loadedChunksRender[chunkPosition];
                    var datatile = tileMapChunkRender.tiles[tilePositionChunk.X, tilePositionChunk.Y];
                    if (datatile != null && datatile.instance != -1)
                    {
                        //  var dataTileInfo = TilesManager.Instance.GetTileData(datatile.);
                        MultimeshManager.Instance.FreeInstance(datatile.rid, datatile.instance, datatile.idMaterial);
                        datatile.FreeRidRender();
                        if (autoTileData != null)
                        {
                            UpdateNeighbors(tilePositionGlobal, autoTileData);
                        }
                        data.ClearDataGame();
                        //CollisionManager.Instance.spriteColliders.RemoveItem(data.positionCollider, data);
                        realInstanceCount--;
                        OnRealInstanceCountChanged?.Invoke(realInstanceCount);

                    }
                }


            }


        }
    }

    public void SetRenderEnabled(bool enabled)
    {
        renderEnabled = enabled;

        if (!renderEnabled)
        {
            // Desactivamos: limpiamos todo el render
            foreach (var kvp in loadedChunksRender)
            {
                Instance_OnChunkUnload(kvp.Key);
            }
            loadedChunksRender.Clear();
        }
        else
        {
            var chunksRenderView = chunkManager.GetVisibleChunks(PositionsManager.Instance.positionCamera);
            HashSet<Vector2> visibleSet = new HashSet<Vector2>(chunksRenderView);

            int cantChunk = 0;
            // Paso 1: cargar los visibles que no están cargados
            foreach (var chunkPos in visibleSet)
            {
                if (dataChunks.ContainsKey(chunkPos) && !loadedChunksRender.ContainsKey(chunkPos))
                {
                    Instance_OnChunkLoad(chunkPos);
                    cantChunk++;
                }
            }
            GD.Print(name + " ChunkRenderizados:" + cantChunk);
            // Paso 2: eliminar los que ya no son visibles
            var toRemove = loadedChunksRender.Keys.Where(pos => !visibleSet.Contains(pos)).ToList();
            foreach (var chunkPos in toRemove)
            {
                Instance_OnChunkUnload(chunkPos);
                loadedChunksRender.Remove(chunkPos);
            }
        }
    }

    internal void Refresh(Vector2I tilePositionGlobal)
    {
        if (!renderEnabled) return; // 🚫 No cargar render si está deshabilitado
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

        TData spriteData = tileMapChunkData.tiles[tilePositionChunk.X, tilePositionChunk.Y];
        if (spriteData == null)
            return;

        // liberar el tile Renderizado     
        SpriteRender tileRender = chunk.GetTileAt(tilePositionChunk);
        if (tileRender != null && tileRender.instance != -1)
        {
            MultimeshManager.Instance.FreeInstance(tileRender.rid, tileRender.instance, tileRender.idMaterial);
            tileRender.FreeRidRender();
        }

        //   var tileInfo = TilesManager.Instance.GetTileData(tileData.idData);
        var instance = MultimeshManager.Instance.CreateInstance(spriteData.GetSpriteData().idMaterial);
        if (spriteData.IsAnimation())
        {
            chunk.CreateUpdate(
            tilePositionChunk.X,
            tilePositionChunk.Y,
            instance.rid,
            instance.instance,
            instance.materialBatchPosition,
            spriteData.positionWorld,
            spriteData.GetSpriteData(), spriteData.GetAnimationStateData()
            );
        }
        else
        {
            chunk.CreateUpdate(
          tilePositionChunk.X,
          tilePositionChunk.Y,
          instance.rid,
          instance.instance,
          instance.materialBatchPosition,
          spriteData.positionWorld,
          spriteData.GetSpriteData()
      );
        }

    }

    private void ChunkManager_OnChunkProcessingCompleted()
    {
        RecalculateRenderingInstanceCounts();
         OnRenderingInstanceCountChanged?.Invoke(renderingInstanceCount);
    }
    private void CreateTile(ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2I tilePositionGlobal, int idData)
    {
        if (idData == 0)
        {
            return;
        }

        TData dataGame = default;
        if (!tileMapChunkData.ExistTile(tilePositionChunk))
        {
            dataGame = new TData();
            //dataGame.idUnique = TileNumeratorManager.Instance.getNumerator();
            realInstanceCount++; // nuevo tile real
            OnRealInstanceCountChanged?.Invoke(realInstanceCount);
        }
        else
        {
            dataGame = tileMapChunkData.GetTileAt(tilePositionChunk);
            if (dataGame.idData == idData)
            {
                return;
            }
        }
        dataGame.idData = idData;

        SpriteData spriteData = dataGame.GetSpriteData();
        int idMaterial = spriteData.idMaterial;
        IdMaterialLastUsed = idMaterial;

        float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;


        Vector2 positionNormalize = tilePositionGlobal * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
        Vector2 positionCenter = positionNormalize + new Vector2(x, y);
        Vector2 positionReal = positionNormalize + new Vector2(x, y) + new Vector2(spriteData.offsetInternal.X * spriteData.scale, spriteData.offsetInternal.Y * spriteData.scale);


        Vector2 posicionCollider = positionReal;
        dataGame.positionWorld = new Vector3(positionReal.X, positionReal.Y, (positionCenter.Y * CommonAtributes.LAYER_MULTIPLICATOR) + layer);
        //dataGame.Scale = spriteData.scale;        
        dataGame.positionTileChunk = tilePositionChunk;
        dataGame.positionTileWorld = tilePositionGlobal;
        dataGame.positionCollider = positionReal;

        tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataGame);


        //if (spriteData.haveCollider)
        //{

        //    posicionCollider = positionNormalize + new Vector2(x, y) + spriteData.collisionBody.Multiplicity(spriteData.scale).OriginCurrent;//+ new Vector2(x, y);
        //    dataGame.positionCollider = posicionCollider;
            
        //    CollisionManager.Instance.spriteColliders.AddUpdateItem(positionReal,);            
            
        //}
        //else
        //{
        //    dataGame.positionCollider = posicionCollider;
        //    CollisionManager.Instance.spriteColliders.RemoveItem(posicionCollider, dataGame);
        //}
        dataGame.SetDataGame();
        Refresh(tilePositionGlobal);
    }

    private void CreateTileRule(ChunkData<TData> chunkData, Vector2I tilePositionGlobal, Vector2I tilePositionChunk, RuleData[] ruleDataArray)
    {
        //byte neighborMask = CalculateNeighborMask(tilePositionGlobal); //
        
        var neighborTileMask = GetNeighborTileIdsAndMask(tilePositionGlobal);
        RuleData bestRule = TilesHelper.FindBestMatchingRule(ruleDataArray, neighborTileMask.mask, neighborTileMask.neighborTileIds);

        if (bestRule == null)
        {
           // GD.PrintErr($"❌ No se encontró una regla válida para mask {neighborTileMask.mask}");
            return;
        }
        // 🟢 Colocar el tile en la posición dada
        CreateTile(chunkData, tilePositionChunk, tilePositionGlobal, bestRule.idDataCentral);
        UpdateNeighbors(tilePositionGlobal, ruleDataArray);
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

            if (neighborData != null && neighborData.idData != 0) //
            {

                neighborTileIds[i] = neighborData.idData;
                mask |= (byte)direction;

            }
            else
            {
                neighborTileIds[i] = 0; // 0 indica que no hay tile
            }
        }

        return (neighborTileIds, mask);
    }

    private void Instance_OnChunkLoad(Godot.Vector2 chunkPos)
    {
        if (!renderEnabled) return; // 🚫 No cargar render si está deshabilitado
        // Si ya esta cargado, no hagas nada
        if (loadedChunksRender.ContainsKey(chunkPos))
            return;
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
                        var instanceComplex = MultimeshManager.Instance.CreateInstance(idMaterial); // multimeshMaterialDict[idMaterial].CreateInstance();
                        if (dataGame.IsAnimation())
                        {
                            chunkRender.CreateUpdate(i, j, instanceComplex.rid, instanceComplex.instance, instanceComplex.materialBatchPosition, dataGame.positionWorld, dataGame.GetSpriteData(), dataGame.GetAnimationStateData());
                        }
                        else
                        {
                            chunkRender.CreateUpdate(i, j, instanceComplex.rid, instanceComplex.instance, instanceComplex.materialBatchPosition, dataGame.positionWorld, dataGame.GetSpriteData());
                        }
                        renderingInstanceCount++;
                      
                    }
                }
            }
            loadedChunksRender.Add(chunkPos, chunkRender);

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

                    if (datatile != null && datatile.instance != -1) 
                    {
                        MultimeshManager.Instance.FreeInstance(datatile.rid, datatile.instance, datatile.idMaterial);                        
                        datatile.FreeRidRender();
                        renderingInstanceCount--;
                       // OnRenderingInstanceCountChanged?.Invoke(renderingInstanceCount);
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
                SaveChunk(dataChunks[chunkPos]);
            }

        }

    }

    private void UpdateNeighbors(Vector2I tilePosGlobal, RuleData[] ruleDataArray)
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

                foreach (var kvp in ruleDataArray)
                {
                    //TileRuleData bestRule = autoTileData.FindBestMatchingRule(newMask);
                    RuleData bestRule = TilesHelper.FindBestMatchingRule(ruleDataArray, neighborTileMask.mask, neighborTileMask.neighborTileIds);
                    
                    if (bestRule != null)
                    {
                        AddUpdatedTile(neighborPos, bestRule.idDataCentral);
                        Refresh(neighborPos);
                        break;
                    }
                }
            }
        }
    }

    
}
