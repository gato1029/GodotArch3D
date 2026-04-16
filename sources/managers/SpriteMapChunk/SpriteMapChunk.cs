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
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;

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
    public bool GetRenderEnable();

    public int GetRenderingInstanceCount();
    public void SetRenderEnabledGlobal(bool enabled);
}
public class SpriteMapChunk<TData>: ISpriteMapChunk where TData: DataItem, new() 
{
    public ConcurrentDictionary<Vector2, ChunkData<TData>> dataChunks = new ConcurrentDictionary<Vector2, ChunkData<TData>>();

    private string carpetSave;
    private ChunkManagerBase chunkManager;

    private Queue<ChunkRenderGPU> chunkPoolRender = new Queue<ChunkRenderGPU>();

    private FormatSave formatSave;
    private Dictionary<Vector2I, ChunkRenderGPU> loadedChunksRender = new();
    public string layerName;
    private int realInstanceCount = 0;
    private bool renderEnabled = true;
    private int renderingInstanceCount = 0;
    private bool serializableUnload;
    public SpriteMapChunk(string name, string carpetSave, int layer, ChunkManagerBase ChunkManager, bool SerializableUnload = false, bool render = true, FormatSave formatSave = FormatSave.BINARIO)
    {
        this.formatSave = formatSave;
        this.carpetSave = carpetSave + "/" + name;
        this.layerName = name;
        serializableUnload = SerializableUnload;
        this.chunkManager = ChunkManager;
        chunkManager.OnChunkLoad += Instance_OnChunkLoad;
        chunkManager.OnChunkUnload += Instance_OnChunkUnload;
        chunkManager.OnChunkProcessingCompleted += ChunkManager_OnChunkProcessingCompleted;
        maxPool = 25;
        ChunkSize = chunkManager.chunkDimencion;
        tileSize = chunkManager.tileSize;

        this.layer = layer;
        renderEnabled = false;
    }

    public event Action<Vector2, ChunkData<TData>> OnChunSerialize;

    public event Action<int> OnRealInstanceCountChanged;

    public event Action<int> OnRenderingInstanceCountChanged;

    public string CarpetSave { get => carpetSave; set => carpetSave = value; }
    public Vector2I ChunkSize { get; private set; }
    public int layer { get; set; }
    public int maxPool { get; set; }

    public Vector2I tileSize { get; set; }
    public void AddUpdateBordersSprite(List<Vector2I> tilesBorder, long idAutoTileSprite, List<SpriteMapChunk<TerrainDataGame>> underLayers = null)
    {
        var dataAuto = MasterDataManager.GetData<AutoTileSpriteData>(idAutoTileSprite);
        foreach (var item in tilesBorder)
        {
            var chunkPosition = chunkManager.ChunkPosition(item);
            var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, item);

            if (dataChunks.ContainsKey(chunkPosition))
            {
                CreateTileRuleBulk(dataChunks[chunkPosition], item, tilePositionChunk, dataAuto, underLayers);
            }
            else
            {
                var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
                dataChunks.TryAdd(chunkPosition, tilemapDataChunk);
                CreateTileRuleBulk(dataChunks[chunkPosition], item, tilePositionChunk, dataAuto, underLayers);
            }
        }

    }

    public void AddUpdatedAutoTileSprite(Vector2I tilePositionGlobal, long idAutoTileSprite, List<SpriteMapChunk<TerrainDataGame>> underLayers = null, bool bulkmode = false)
    {
        var dataAuto = MasterDataManager.GetData<AutoTileSpriteData>(idAutoTileSprite);

        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);
        if (dataChunks.ContainsKey(chunkPosition))
        {
            if (bulkmode)
            {
                CreateTileRuleBulk(dataChunks[chunkPosition], tilePositionGlobal, tilePositionChunk, dataAuto, underLayers);
            }
            else
            {
                CreateTileRule(dataChunks[chunkPosition], tilePositionGlobal, tilePositionChunk, dataAuto, underLayers);
            }


        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.TryAdd(chunkPosition, tilemapDataChunk);
            if (bulkmode)
            {
                CreateTileRuleBulk(dataChunks[chunkPosition], tilePositionGlobal, tilePositionChunk, dataAuto, underLayers);
            }
            else
            {
                CreateTileRule(dataChunks[chunkPosition], tilePositionGlobal, tilePositionChunk, dataAuto, underLayers);
            }

        }
    }

    public void AddUpdatedTileSprite(Vector2I tilePositionGlobal, TileTemplate idData)
    {
        if (idData.idTileSprite == 0) // caso vacio delete
        {
            RemoveTileSprite(tilePositionGlobal);
            Refresh(tilePositionGlobal);
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
            dataChunks.TryAdd(chunkPosition, tilemapDataChunk);
            CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, idData);
        }
    }
    public void AddUpdatedTileSpriteInChunk(Vector2I chunkPosition, Vector2I tilePositionChunk, TileTemplate idData, bool updateObject = true)
    {

        var tilePositionGlobal = chunkManager.TilePositionGlobal(chunkPosition,tilePositionChunk);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, idData,updateObject);

        }
        else
        {
            var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
            dataChunks.TryAdd(chunkPosition, tilemapDataChunk);
            CreateTile(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, idData, updateObject);
        }
    }
    public void AddUpdateMatrixSprite(List<Vector2I> matrizTiles, long idAutoTileSprite)
    {
        TileTemplate temp = null;
        var dataAuto = MasterDataManager.GetData<AutoTileSpriteData>(idAutoTileSprite);

        var data = dataAuto.tileRuleTemplates.Last();
        if (data.RandomTiles.Count > 1)
        {
            int randomIndex = (int)(GD.Randi() % data.RandomTiles.Count);
            temp = data.RandomTiles[randomIndex];
        }
        else
        {
            temp = dataAuto.tileRuleTemplates.Last().TileCentral;
        }




        foreach (var item in matrizTiles)
        {
            if (data.IsRandomTiles)
            {
                int randomIndex = (int)(GD.Randi() % data.RandomTiles.Count);
                temp = data.RandomTiles[randomIndex];
            }
            AddUpdatedTileSprite(item, temp);
        }

    }

    public void ApplyRulesBulk(List<Vector2I> tiles, AutoTileSpriteData autoData, List<SpriteMapChunk<TerrainDataGame>> underLayers)
    {
        foreach (var tilePositionGlobal in tiles)
        {
            var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
            var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

            var neighborTileMask = CreateTileEnvironment(tilePositionGlobal);
            TileRuleTemplate bestRule = null;

            foreach (var ruleTemplate in autoData.tileRuleTemplates)
            {
                if (ruleTemplate.MatchesEnvironment(neighborTileMask))
                {
                    //bestRule = ruleTemplate;
                    //break;
                    if (underLayers == null)
                    {
                        bestRule = ruleTemplate;
                        break;
                    }
                    else
                    {

                        int grupoActual = ruleTemplate.neighborConditionTemplateCenter.groupId;
                        var UnderRender = ruleTemplate.neighborConditionTemplateCenter.UnderNeighborType;
                        if (grupoActual == 0)
                        {
                            bestRule = ruleTemplate;

                            foreach (var item in underLayers)
                            {
                                var dataTemp = item.GetTileGlobalPosition(tilePositionGlobal).render;
                                if (dataTemp != null)
                                {
                                    switch (UnderRender)
                                    {
                                        case UnderNeighborType.NoHacerNada:
                                            item.SetTileRenderGlobalPosition(tilePositionGlobal, true);
                                            break;
                                        case UnderNeighborType.Ocultar:
                                            item.SetTileRenderGlobalPosition(tilePositionGlobal, false);
                                            break;
                                        case UnderNeighborType.Eliminar:
                                            item.RemoveTileSprite(tilePositionGlobal, true);
                                            break;
                                        default:
                                            break;
                                    }

                                }
                            }
                            break;
                        }
                        else
                        {
                            bool underRuleExist = false;
                            foreach (var item in underLayers)
                            {
                                var dataTemp = item.GetTileGlobalPosition(tilePositionGlobal).render;
                                if (dataTemp != null)
                                {
                                    int grupoInferior = dataTemp.idGroup;
                                    if (grupoActual == grupoInferior)
                                    {
                                        switch (UnderRender)
                                        {
                                            case UnderNeighborType.NoHacerNada:
                                                item.SetTileRenderGlobalPosition(tilePositionGlobal, true);
                                                break;
                                            case UnderNeighborType.Ocultar:
                                                item.SetTileRenderGlobalPosition(tilePositionGlobal, false);
                                                break;
                                            case UnderNeighborType.Eliminar:
                                                item.RemoveTileSprite(tilePositionGlobal, true);
                                                break;
                                            default:
                                                break;
                                        }

                                        underRuleExist = true;
                                    }
                                }
                            }
                            if (underRuleExist == true)
                            {
                                bestRule = ruleTemplate;
                                break;
                            }
                        }

                    }
                }
            }
            if (bestRule != null)
            {
                TileTemplate tileTemplate;
                if (bestRule.IsRandomTiles)
                {
                    int randomIndex = (int)(GD.Randi() % bestRule.RandomTiles.Count);
                    tileTemplate = bestRule.RandomTiles[randomIndex];
                }
                else
                {
                    tileTemplate = bestRule.RandomTiles[0];
                }
                CreateTileBulk(dataChunks[chunkPosition], tilePositionChunk, tilePositionGlobal, tileTemplate);
            }

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
                    var tile = chunk.GetTileAt(x, y);
                    if (tile.render != null)
                    {
                        bool haveCollider = false;
                        switch (tile.render.GetSpriteData().tileSpriteType)
                        {
                            case TileSpriteType.Static:
                                haveCollider = tile.render.GetSpriteData().spriteData.haveCollider;
                                break;
                            case TileSpriteType.Animated:
                                haveCollider = tile.render.GetSpriteData().animationData.haveCollider;
                                break;
                            default:
                                break;
                        }

                        if (haveCollider)
                        {
                            tile.data.ClearDataGame();
                            tile.render = null;
                        }
                    }
                }
            }
        }

        dataChunks.Clear();

        // Opcional: limpiar el pool de chunks renderizados
        chunkPoolRender.Clear();
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

    public void CreateChunkData(ChunkDataSerializable<TData> serializable)
    {

        //ChunkData<TData> data = FromSerializable(serializable);
        //if (!dataChunks.ContainsKey(data.positionChunk))
        //{
        //    dataChunks.Add(data.positionChunk, data);

        //    foreach (var item in data.tiles)
        //    {
        //        if (item != null && item.idDataTileSprite!=0)
        //        {
        //            item.SetDataGame();
        //        }
        //    }
        //}
        //else
        //{
        //    dataChunks[data.positionChunk] = data;
        //}

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
        return layerName;
    }

    public Vector2I GetNeighborPositionTileSprite(Vector2I tilePos, NeighborPosition direction)
    {
        switch (direction)
        {
            case NeighborPosition.Arriba: return new Vector2I(tilePos.X, tilePos.Y + 1);
            case NeighborPosition.Derecha: return new Vector2I(tilePos.X + 1, tilePos.Y);
            case NeighborPosition.Abajo: return new Vector2I(tilePos.X, tilePos.Y - 1);
            case NeighborPosition.Izquierda: return new Vector2I(tilePos.X - 1, tilePos.Y);
            case NeighborPosition.ArribaDerecha: return new Vector2I(tilePos.X + 1, tilePos.Y + 1);
            case NeighborPosition.AbajoDerecha: return new Vector2I(tilePos.X + 1, tilePos.Y - 1);
            case NeighborPosition.AbajoIzquierda: return new Vector2I(tilePos.X - 1, tilePos.Y - 1);
            case NeighborPosition.ArribaIzquierda: return new Vector2I(tilePos.X - 1, tilePos.Y + 1);
            default: return tilePos;
        }
    }

    public List<(TData, DataRender)> GetOccupiedTiles()
    {
        var occupiedTiles = new List<(TData, DataRender)>();

        foreach (var chunkPair in dataChunks)
        {
            ChunkData<TData> chunkData = chunkPair.Value;

            for (int x = 0; x < chunkData.size.X; x++)
            {
                for (int y = 0; y < chunkData.size.Y; y++)
                {
                    DataRender tile = chunkData.GetTileAt(x, y).render;
                    if (tile != null && tile.idDataTileSprite != 0)
                    {
                        occupiedTiles.Add(chunkData.GetTileAt(x, y));
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

    public bool GetRenderEnable()
    {
        return renderEnabled;
    }
    int ISpriteMapChunk.GetRenderingInstanceCount()
    {
        return renderingInstanceCount;
    }

    public (TData data, DataRender render) GetTileGlobalPosition(Vector2I tilePositionGlobal)
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
                    var tile = chunk.renderTiles[x, y];
                    if (tile != null && tile.idDataTileSprite != 0)
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

    public void RemoveTileSprite(Vector2I tilePositionGlobal, bool forced = false, long idAutoTileSprite = 0, List<SpriteMapChunk<TerrainDataGame>> layerUnder = null)
    {
        var chunkPosition = chunkManager.ChunkPosition(tilePositionGlobal);
        var tilePositionChunk = chunkManager.TilePositionInChunk(chunkPosition, tilePositionGlobal);

        if (dataChunks.ContainsKey(chunkPosition))
        {
            if (dataChunks[chunkPosition].GetTileAt(tilePositionChunk).render != null)
            {
                var data = dataChunks[chunkPosition].GetTileAt(tilePositionChunk);

                dataChunks[chunkPosition].RemoveTile(tilePositionChunk);

                if (loadedChunksRender.ContainsKey(chunkPosition))
                {
                    ChunkRenderGPU tileMapChunkRender = loadedChunksRender[chunkPosition];
                    var datatile = tileMapChunkRender.tiles[tilePositionChunk.X, tilePositionChunk.Y];
                    if (datatile != null && datatile.instance != -1)
                    {

                        if (forced)
                        {
                            datatile.FreeRidRenderForced();
                        }
                        else
                        {
                            datatile.FreeRidRender();
                        }
                        if (idAutoTileSprite != 0)
                        {
                            var autoData = MasterDataManager.GetData<AutoTileSpriteData>(idAutoTileSprite);
                            UpdateNeighborsTileSprite(tilePositionGlobal, autoData, layerUnder);
                        }
                        data.data.ClearDataGame();
                        data.render = null;

                        realInstanceCount--;

                        if (layerUnder != null)
                        {
                            foreach (var item in layerUnder)
                            {
                                var daGame = item.GetTileGlobalPosition(tilePositionGlobal).render;
                                if (daGame != null && daGame.idDataTileSprite != 0)
                                {
                                    item.SetTileRenderGlobalPosition(tilePositionGlobal, true);
                                    item.Refresh(tilePositionGlobal);
                                }
                            }
                        }
                        OnRealInstanceCountChanged?.Invoke(realInstanceCount);
                    }
                }
            }
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
    public void SetRenderEnabledGlobal(bool enabled)
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
            HashSet<Vector2I> visibleSet = new HashSet<Vector2I>(chunksRenderView);

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

            // Paso 2: eliminar los que ya no son visibles
            var toRemove = loadedChunksRender.Keys.Where(pos => !visibleSet.Contains(pos)).ToList();
            foreach (var chunkPos in toRemove)
            {
                Instance_OnChunkUnload(chunkPos);
                loadedChunksRender.Remove(chunkPos);
            }
        }
    }

    public void SetRenderEnableManual(bool enabled)
    {
        renderEnabled = enabled;
    }

    public void SetTileRenderGlobalPosition(Vector2I tilePositionGlobal, bool isRender)
    {
        if (!renderEnabled) return; // 🚫 No cargar render si está deshabilitado

        var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
        var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);
        if (dataChunks.ContainsKey(chunkPosition))
        {
            var data = dataChunks[chunkPosition].GetTileAt(tilePositionChunk).render;
            if (data != null && data.idDataTileSprite != 0)
            {
                data.render = isRender;
                if (data.render == false)
                {
                    if (loadedChunksRender.TryGetValue(chunkPosition, out var chunk))
                    {
                        // liberar el tile Renderizado     
                        SpriteRender tileRender = chunk.GetTileAt(tilePositionChunk);
                        if (tileRender != null && tileRender.instance != -1)
                        {
                            tileRender.FreeRidRenderForced();
                        }
                    }
                }
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
            // Crea el chunk render
            CreateChunkRender(chunkPosition);
            if (!loadedChunksRender.TryGetValue(chunkPosition, out chunk))
            {
                //caso cuando no se pudo crear el chunk render, que ocurre con vecindades a un no cargadas
                return;
            }
            //loadedChunksRender[chunkPosition];

        }

        if (!dataChunks.TryGetValue(chunkPosition, out var tileMapChunkData))
            return;

        DataRender spriteData = tileMapChunkData.renderTiles[tilePositionChunk.X, tilePositionChunk.Y];
        if (spriteData == null)
            return;

        if (!spriteData.render)
        {
            return; // si no se debe renderizar, salir
        }

        // liberar el tile Renderizado     
        SpriteRender tileRender = chunk.GetTileAt(tilePositionChunk);
        if (tileRender != null && tileRender.instance != -1)
        {
            tileRender.FreeRidRender();
        }



        if (spriteData.GetSpriteData().tileSpriteType == TileSpriteType.Animated)
        {

            chunk.CreateUpdate(
            spriteData.idDataTileSprite,
            tilePositionChunk.X,
            tilePositionChunk.Y,
            spriteData.positionWorld,
            spriteData.layer,
            spriteData.GetSpriteData().animationData
            );
        }
        else
        {

            chunk.CreateUpdate(
            spriteData.idDataTileSprite,
            tilePositionChunk.X,
            tilePositionChunk.Y,
            spriteData.positionWorld,
            spriteData.layer,
            spriteData.GetSpriteData().spriteData
        );
        }

    }

    private void ChunkManager_OnChunkProcessingCompleted()
    {
        RecalculateRenderingInstanceCounts();
        OnRenderingInstanceCountChanged?.Invoke(renderingInstanceCount);
    }

    private void CreateChunkRender(Godot.Vector2I chunkPos)
    {
        // Si ya esta cargado, no hagas nada
        if (loadedChunksRender.ContainsKey(chunkPos))
            return;
        if (dataChunks.ContainsKey(chunkPos))
        {

            ChunkRenderGPU chunkRender = null;
            if (chunkPoolRender.Count > 0)
            {
                chunkRender = chunkPoolRender.Dequeue();
            }
            else
            {
                chunkRender = new ChunkRenderGPU(chunkPos, ChunkSize);
            }
            loadedChunksRender.Add(chunkPos, chunkRender);
        }
    }

    private void CreateTile(ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2I tilePositionGlobal, TileTemplate tileTemplate, bool updateObject = true)
    {
        if (tileTemplate.idTileSprite == 0)
        {
            return;
        }

        DataRender dataRender = null;

        if (!tileMapChunkData.ExistTile(tilePositionChunk))
        {
            dataRender = new DataRender();
            //dataGame.idUnique = TileNumeratorManager.Instance.getNumerator();
            realInstanceCount++; // nuevo tile real
            OnRealInstanceCountChanged?.Invoke(realInstanceCount);
        }
        else
        {
            dataRender = tileMapChunkData.GetTileAt(tilePositionChunk).render;
            if (dataRender.idDataTileSprite == tileTemplate.idTileSprite)
            {
                return;
            }
        }
        dataRender.idDataTileSprite = tileTemplate.idTileSprite;
        dataRender.idGroup = tileTemplate.idGroup;
        dataRender.layer = layer;
        dataRender.render = true;
        float scale = 1f;
        Vector2 offset = Vector2.Zero;
        int idMaterial = 0;
        float yDepthRender = 0f;

        switch (dataRender.GetSpriteData().tileSpriteType)
        {
            case TileSpriteType.Static:
                SpriteData spriteData = dataRender.GetSpriteData().spriteData;
                idMaterial = spriteData.idMaterial;
                offset = spriteData.offsetInternal;
                scale = spriteData.scale;
                yDepthRender = spriteData.yDepthRender;
                break;
            case TileSpriteType.Animated:
                SpriteAnimationData animationData = dataRender.GetSpriteData().animationData;
                idMaterial = animationData.idMaterial;
                offset = animationData.offsetInternal;
                scale = animationData.scale;
                yDepthRender = animationData.yDepthRender;
                break;
            default:
                break;
        }



        float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;


        Vector2 positionNormalize = tilePositionGlobal * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
        Vector2 positionCenter = positionNormalize + new Vector2(x, y);
        Vector2 positionReal = positionCenter + new Vector2(offset.X * scale, offset.Y * scale);


        Vector2 posicionCollider = positionCenter;
        dataRender.positionReal = positionCenter;
        dataRender.positionWorld = new Vector3(positionReal.X, positionReal.Y, ((positionReal.Y + yDepthRender) * CommonAtributes.LAYER_MULTIPLICATOR) + layer);
        //dataGame.Scale = spriteData.scale;        
        dataRender.positionTileChunk = tilePositionChunk;
        dataRender.positionTileWorld = tilePositionGlobal;
        dataRender.positionCollider = posicionCollider;

        TData dataGame = new TData();       
        tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataRender, dataGame);
        if (updateObject)
        {
            dataGame.SetDataGame(dataRender);
            Refresh(tilePositionGlobal);
        }
        
    }

    public void RecalculateAllColliders()
    {
        foreach (var item in dataChunks)
        {
            var data =item.Value;
            for (int x = 0; x< data.renderTiles.GetLength(0); x++)
            {
                for (int y = 0; y < data.renderTiles.GetLength(1); y++)
                {
                    if (data.renderTiles[x, y]!=null)
                    {
                        TData dataGame = data.tiles[x, y];
                        dataGame.SetDataGame(data.renderTiles[x, y]);
                    }
                    
                }
            }
        }
    }

    private void CreateTileBulk(ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2I tilePositionGlobal, TileTemplate tileTemplate)
    {
        var dataTile = tileMapChunkData.GetTileAt(tilePositionChunk);
        DataRender dataRender = dataTile.render;

        dataRender.idDataTileSprite = tileTemplate.idTileSprite;
        dataRender.idGroup = tileTemplate.idGroup;
        dataRender.layer = layer;
        dataRender.render = true;
        float scale = 1f;
        Vector2 offset = Vector2.Zero;
        float yDepthRender = 0f;

        switch (dataRender.GetSpriteData().tileSpriteType)
        {
            case TileSpriteType.Static:
                SpriteData spriteData = dataRender.GetSpriteData().spriteData;
                offset = spriteData.offsetInternal;
                scale = spriteData.scale;
                yDepthRender = spriteData.yDepthRender;
                break;
            case TileSpriteType.Animated:
                SpriteAnimationData animationData = dataRender.GetSpriteData().animationData;
                offset = animationData.offsetInternal;
                scale = animationData.scale;
                yDepthRender = animationData.yDepthRender;
                break;
            default:
                break;
        }

        float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;

        Vector2 positionNormalize = tilePositionGlobal * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
        Vector2 positionCenter = positionNormalize + new Vector2(x, y);
        Vector2 positionReal = positionCenter + new Vector2(offset.X * scale, offset.Y * scale);

        Vector2 posicionCollider = positionCenter;
        dataRender.positionReal = positionCenter;
        dataRender.positionWorld = new Vector3(positionReal.X, positionReal.Y, ((positionReal.Y + yDepthRender) * CommonAtributes.LAYER_MULTIPLICATOR) + layer);
        //dataGame.Scale = spriteData.scale;        
        dataRender.positionTileChunk = tilePositionChunk;
        dataRender.positionTileWorld = tilePositionGlobal;
        dataRender.positionCollider = posicionCollider;

        TData dataGame = new TData();
        dataGame.SetDataGame(dataRender);
        tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataRender, dataGame);
    }

    private TileEnvironment CreateTileEnvironment(Vector2I tilePositionGlobal)
    {
        TileEnvironment tileEnvironment = new TileEnvironment();
        for (int i = 0; i < 8; i++)
        {
            NeighborPosition neighborPosition = (NeighborPosition)i;
            Vector2I neighborPos = GetNeighborPositionTileSprite(tilePositionGlobal, neighborPosition);
            var neighborData = GetTileGlobalPosition(neighborPos).render;

            if (neighborData != null && neighborData.idDataTileSprite != 0) //
            {
                tileEnvironment.Set(neighborPosition, neighborData.idDataTileSprite, neighborData.idGroup);
                // neighborTileIds[i] = neighborData.idDataTileSprite; // aqui borre
                // mask |= (byte)direction;

            }
            else
            {
                tileEnvironment.Set(neighborPosition, 0, 0); // 0 indica que no hay tile
            }
        }
        return tileEnvironment;
    }

    private void CreateTileRule(ChunkData<TData> chunkData, Vector2I tilePositionGlobal, Vector2I tilePositionChunk, AutoTileSpriteData rule, List<SpriteMapChunk<TerrainDataGame>> underLayers = null)
    {
        //byte neighborMask = CalculateNeighborMask(tilePositionGlobal); //

        var neighborTileMask = CreateTileEnvironment(tilePositionGlobal);

        TileRuleTemplate bestRule = null;

        foreach (var ruleTemplate in rule.tileRuleTemplates)
        {
            if (ruleTemplate.MatchesEnvironment(neighborTileMask))
            {
                //bestRule = ruleTemplate;
                //break;
                if (underLayers == null)
                {
                    bestRule = ruleTemplate;
                    break;
                }
                else
                {

                    int grupoActual = ruleTemplate.neighborConditionTemplateCenter.groupId;
                    var UnderRender = ruleTemplate.neighborConditionTemplateCenter.UnderNeighborType;
                    if (grupoActual == 0)
                    {
                        bestRule = ruleTemplate;

                        foreach (var item in underLayers)
                        {
                            var dataTemp = item.GetTileGlobalPosition(tilePositionGlobal).render;
                            if (dataTemp != null)
                            {
                                //switch (UnderRender)
                                //{
                                //    case UnderNeighborType.NoHacerNada:
                                //        item.SetTileRenderGlobalPosition(tilePositionGlobal, true);
                                //        item.Refresh(tilePositionGlobal);
                                //        break;
                                //    case UnderNeighborType.Ocultar:
                                //        item.SetTileRenderGlobalPosition(tilePositionGlobal, false);
                                //        break;
                                //    case UnderNeighborType.Eliminar:
                                //        item.RemoveTileSprite(tilePositionGlobal, true);
                                //        break;
                                //    default:
                                //        break;
                                //}

                            }
                        }
                        break;
                    }
                    else
                    {
                        bool underRuleExist = false;
                        foreach (var item in underLayers)
                        {
                            var dataTemp = item.GetTileGlobalPosition(tilePositionGlobal).render;
                            if (dataTemp != null)
                            {
                                int grupoInferior = dataTemp.idGroup;
                                if (grupoActual == grupoInferior)
                                {
                                    //switch (UnderRender)
                                    //{
                                    //    case UnderNeighborType.NoHacerNada:
                                    //        item.SetTileRenderGlobalPosition(tilePositionGlobal, true);
                                    //        item.Refresh(tilePositionGlobal);
                                    //        break;
                                    //    case UnderNeighborType.Ocultar:
                                    //        item.SetTileRenderGlobalPosition(tilePositionGlobal, false);
                                    //        break;
                                    //    case UnderNeighborType.Eliminar:
                                    //        item.RemoveTileSprite(tilePositionGlobal, true);
                                    //        break;
                                    //    default:
                                    //        break;
                                    //}

                                    underRuleExist = true;
                                }
                            }
                        }
                        if (underRuleExist == true)
                        {
                            bestRule = ruleTemplate;
                            break;
                        }
                    }

                }
            }
        }

        if (bestRule == null)
        {
            return;
        }

        // 🟢 Colocar el tile en la posición dada
        TileTemplate tileTemplate;
        if (bestRule.IsRandomTiles)
        {
            int randomIndex = (int)(GD.Randi() % bestRule.RandomTiles.Count);
            tileTemplate = bestRule.RandomTiles[randomIndex];
        }
        else
        {
            tileTemplate = bestRule.TileCentral;
        }

        CreateTile(chunkData, tilePositionChunk, tilePositionGlobal, tileTemplate);
        if (rule.tileRuleTemplates.Count > 1)
        {
            UpdateNeighborsTileSprite(tilePositionGlobal, rule, underLayers);
        }

    }

    private void CreateTileRuleBulk(ChunkData<TData> chunkData, Vector2I tilePositionGlobal, Vector2I tilePositionChunk, AutoTileSpriteData rule, List<SpriteMapChunk<TerrainDataGame>> underLayers = null)
    {
        var neighborTileMask = CreateTileEnvironment(tilePositionGlobal);

        TileRuleTemplate bestRule = null;

        foreach (var ruleTemplate in rule.tileRuleTemplates)
        {
            if (ruleTemplate.MatchesEnvironment(neighborTileMask))
            {
                //bestRule = ruleTemplate;
                //break;
                if (underLayers == null)
                {
                    bestRule = ruleTemplate;
                    break;
                }
                else
                {

                    int grupoActual = ruleTemplate.neighborConditionTemplateCenter.groupId;
                    var UnderRender = ruleTemplate.neighborConditionTemplateCenter.UnderNeighborType;
                    if (grupoActual == 0)
                    {
                        bestRule = ruleTemplate;
                        break;
                    }
                    else
                    {
                        bool underRuleExist = false;
                        foreach (var item in underLayers)
                        {
                            var dataTemp = item.GetTileGlobalPosition(tilePositionGlobal).render;
                            if (dataTemp != null)
                            {
                                int grupoInferior = dataTemp.idGroup;
                                if (grupoActual == grupoInferior)
                                {
                                    underRuleExist = true;
                                    break;
                                }
                            }
                        }
                        if (underRuleExist == true)
                        {
                            bestRule = ruleTemplate;
                            break;
                        }
                    }

                }
            }
        }
        if (bestRule == null)
        {
            TileTemplate tileTemplateInter = new TileTemplate();
            tileTemplateInter.idTileSprite = 1763600186389000;
            tileTemplateInter.idGroup = 0;
            CreateTile(chunkData, tilePositionChunk, tilePositionGlobal, tileTemplateInter);
            return;
        }


        // 🟢 Colocar el tile en la posición dada
        TileTemplate tileTemplate;
        if (bestRule.IsRandomTiles)
        {
            int randomIndex = (int)(GD.Randi() % bestRule.RandomTiles.Count);
            tileTemplate = bestRule.RandomTiles[randomIndex];
        }
        else
        {
            tileTemplate = bestRule.TileCentral;
        }

        CreateTile(chunkData, tilePositionChunk, tilePositionGlobal, tileTemplate);
        if (rule.tileRuleTemplates.Count > 1)
        {
            UpdateNeighborsTileSprite(tilePositionGlobal, rule, underLayers);
        }
    }

    private void Instance_OnChunkLoad(Godot.Vector2I chunkPos)
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
                    DataRender dataGame = tileMapChunkData.renderTiles[i, j];
                    if (dataGame != null && dataGame.render)
                    {


                        if (dataGame.GetSpriteData().tileSpriteType == TileSpriteType.Animated)
                        {

                            chunkRender.CreateUpdate(dataGame.idDataTileSprite, i, j, dataGame.positionWorld, dataGame.layer, dataGame.GetSpriteData().animationData);
                        }
                        else
                        {
                            chunkRender.CreateUpdate(dataGame.idDataTileSprite, i, j, dataGame.positionWorld, dataGame.layer, dataGame.GetSpriteData().spriteData);
                        }
                        renderingInstanceCount++;
                    }
                }
            }
            loadedChunksRender.Add(chunkPos, chunkRender);
        }
    }

    private void Instance_OnChunkUnload(Godot.Vector2I chunkPos)
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

    private Vector2I Normalize(int x, int y, Vector2I sizeMap)
    {
        int initX = sizeMap.X / 2;
        int initY = sizeMap.Y / 2;

        return new Vector2I(x + initX, y + initY);
    }
    private void UpdateNeighborsTileSprite(Vector2I tilePositionGlobal, AutoTileSpriteData rule, List<SpriteMapChunk<TerrainDataGame>> underLayers = null)
    {
        for (int i = 0; i < 8; i++)
        {
            NeighborPosition neighborPosition = (NeighborPosition)i;

            //  NeighborDirection direction = GetDirectionFromIndex(i);
            Vector2I neighborPos = GetNeighborPositionTileSprite(tilePositionGlobal, neighborPosition);

            //Vector2I neighborPos = TilesHelper.GetNeighborPosition(tilePosGlobal, direction);

            var dataValue = GetTileGlobalPosition(neighborPos).render;
            if (dataValue != null) // Si hay un tile vecino
            {
                //byte newMask = CalculateNeighborMask(neighborPos);
                var tileEnviroment = CreateTileEnvironment(neighborPos);
                // var neighborTileMask = GetNeighborTileIdsAndMask(neighborPos);

                foreach (var kvp in rule.tileRuleTemplates)
                {
                    //TileRuleData bestRule = autoTileData.FindBestMatchingRule(newMask);                   
                    if (kvp.MatchesEnvironment(tileEnviroment))
                    {
                        TileTemplate tileTemplate;
                        if (kvp.IsRandomTiles)
                        {
                            int randomIndex = (int)(GD.Randi() % kvp.RandomTiles.Count);
                            tileTemplate = kvp.RandomTiles[randomIndex];
                        }
                        else
                        {
                            tileTemplate = kvp.TileCentral;
                        }
                        ///
                        if (underLayers == null)
                        {
                            AddUpdatedTileSprite(neighborPos, tileTemplate);
                            break;
                        }
                        else
                        {
                            int grupoActual = kvp.neighborConditionTemplateCenter.groupId;
                            var UnderRender = kvp.neighborConditionTemplateCenter.UnderNeighborType;
                            if (grupoActual == 0)
                            {
                                AddUpdatedTileSprite(neighborPos, tileTemplate);
                                if (underLayers != null)
                                {
                                    foreach (var item in underLayers)
                                    {
                                        var dataTemp = item.GetTileGlobalPosition(neighborPos).render;
                                        if (dataTemp != null)
                                        {
                                            //switch (UnderRender)
                                            //{
                                            //    case UnderNeighborType.NoHacerNada:
                                            //        item.SetTileRenderGlobalPosition(neighborPos, true);
                                            //        item.Refresh(neighborPos);
                                            //        break;
                                            //    case UnderNeighborType.Ocultar:
                                            //        item.SetTileRenderGlobalPosition(neighborPos, false);
                                            //        break;
                                            //    case UnderNeighborType.Eliminar:
                                            //        item.RemoveTileSprite(neighborPos, true);
                                            //        break;
                                            //    default:
                                            //        break;
                                            //}                                              

                                        }
                                    }
                                }
                                break;
                            }
                            else
                            {
                                bool underRuleExist = false;
                                foreach (var item in underLayers)
                                {
                                    var dataTemp = item.GetTileGlobalPosition(neighborPos).render;
                                    if (dataTemp != null)
                                    {
                                        int grupoInferior = dataTemp.idGroup;
                                        if (grupoActual == grupoInferior)
                                        {
                                            //switch (UnderRender)
                                            //{
                                            //    case UnderNeighborType.NoHacerNada:
                                            //        item.SetTileRenderGlobalPosition(neighborPos, true);
                                            //        item.Refresh(neighborPos);
                                            //        break;
                                            //    case UnderNeighborType.Ocultar:
                                            //        item.SetTileRenderGlobalPosition(neighborPos, false);
                                            //        break;
                                            //    case UnderNeighborType.Eliminar:
                                            //        item.RemoveTileSprite(neighborPos, true);
                                            //        break;
                                            //    default:
                                            //        break;
                                            //}
                                            underRuleExist = true;
                                        }
                                    }
                                }
                                if (underRuleExist == true)
                                {
                                    AddUpdatedTileSprite(neighborPos, tileTemplate);
                                    break;
                                }
                            }
                        }
                        //AddUpdatedTileSprite(neighborPos, tileTemplate);
                        //break;
                    }
                }
            }
        }
    }

    private void UpdateNeighborsTileSpriteBulk(Vector2I tilePositionGlobal, AutoTileSpriteData rule)
    {
        for (int i = 0; i < 8; i++)
        {
            NeighborPosition neighborPosition = (NeighborPosition)i;

            Vector2I neighborPos = GetNeighborPositionTileSprite(tilePositionGlobal, neighborPosition);

            var dataValue = GetTileGlobalPosition(neighborPos).render;
            if (dataValue != null) // Si hay un tile vecino
            {
                var tileEnviroment = CreateTileEnvironment(neighborPos);

                foreach (var kvp in rule.tileRuleTemplates)
                {
                    if (kvp.MatchesEnvironment(tileEnviroment))
                    {
                        TileTemplate tileTemplate;
                        if (kvp.IsRandomTiles)
                        {
                            int randomIndex = (int)(GD.Randi() % kvp.RandomTiles.Count);
                            tileTemplate = kvp.RandomTiles[randomIndex];
                        }
                        else
                        {
                            tileTemplate = kvp.TileCentral;
                        }

                        AddUpdatedTileSprite(neighborPos, tileTemplate);
                        break;
                    }
                }
            }
        }
    }
}
