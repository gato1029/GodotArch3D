using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;
using Godot;
namespace GodotEcsArch.sources.BlackyTiles.Systems;
public enum BlackyTerrainLayer
{
    Terrain = 0,   // suelo base
    Overlay = 1,   // caminos, cesped, nieve
    Decor = 2      // rocas, flores, objetos pequeños
}

[StructLayout(LayoutKind.Sequential)]
public struct BlackyTerrainLayerTile
{

    public ushort RuleType;   // tipo de regla autotile
    public ushort RuleGrouping;  // id del agrupador en la base de datos


    public ushort IdBiome; // id del bioma
    public ushort TileGroup;  // grupo de conexión
    public ushort TileSetId;  // id del tile en la base de datos

    // 0 = Sin colisión.
    public ushort CollisionId;
    public bool IsEmpty => TileSetId == 0;

    public void Clear()
    {
        CollisionId = 0;
        RuleType = 0;
        TileGroup = 0;
        TileSetId = 0;
        RuleGrouping = 0;
        IdBiome = 0;
    }
}
public struct BlackyTerrainLevel
{
    public const int LAYER_COUNT = 3;
    private BlackyTerrainLayerTile[] _layers;
    public void Init()
    {
        if (_layers == null)
            _layers = new BlackyTerrainLayerTile[LAYER_COUNT];
    }

    public BlackyTerrainLevel()
    {
        Init();
    }

    public void Clear()
    {
        for (int i = 0; i < 3; i++)
            _layers[i].Clear();
    }

    public void ClearLayer(int layer)
    {
        Init();
        _layers[layer].Clear();
    }

    public void ClearLayer(BlackyTerrainLayer layer)
    {
        Init();
        _layers[(int)layer].Clear();
        
    }

    public ref BlackyTerrainLayerTile GetLayer(int layer)
    {
        Init();
        return ref _layers[layer];
    }

    public ref BlackyTerrainLayerTile GetLayer(BlackyTerrainLayer layer)
    {
        Init();
        return ref _layers[(int)layer];
    }

    public void SetLayer(BlackyTerrainLayer layer, BlackyTerrainLayerTile tile)
    {
        Init();
        _layers[(int)layer] = tile;
    }

    public BlackyTerrainLayerTile GetTopOccupiedTile()
    {
        Init();

        if (!_layers[2].IsEmpty) return _layers[2];
        if (!_layers[1].IsEmpty) return _layers[1];
        if (!_layers[0].IsEmpty) return _layers[0];

        return default;
    }
    public BlackyTerrainLayer? GetTopOccupiedLayerEnum()
    {
        Init();

        for (int i = LAYER_COUNT - 1; i >= 0; i--)
        {
            if (!_layers[i].IsEmpty)
                return (BlackyTerrainLayer)i;
        }

        return null; // Ninguna capa ocupada
    }
    public bool IsEmpty =>
        _layers[0].IsEmpty &&
        _layers[1].IsEmpty &&
        _layers[2].IsEmpty;
}

public struct BlackyTerrainColumn
{
    public const int MAX_HEIGHT = 4;

    private BlackyTerrainLevel[] _levels;

    public int TopHeight;

    public int GetTopHeight()
    {
        return TopHeight;
    }
    public BlackyTerrainColumn()
    {
        _levels = new BlackyTerrainLevel[MAX_HEIGHT];
        TopHeight = -1;
    }

    public ref BlackyTerrainLevel GetLevel(int height)
    {
        return ref _levels[height];
    }

    public void SetLayer(
        int height,
        BlackyTerrainLayer layer,
        BlackyTerrainLayerTile tile)
    {
        if (height < 0 || height >= MAX_HEIGHT)
            return;

        _levels[height].SetLayer(layer, tile);

        if (!_levels[height].IsEmpty && height > TopHeight)
            TopHeight = height;
    }

    public void RemoveLayer(
        int height,
        BlackyTerrainLayer layer)
    {
        if (height < 0 || height >= MAX_HEIGHT)
            return;

        _levels[height].SetLayer(layer, default);

        if (height == TopHeight && _levels[height].IsEmpty)
        {
            for (int i = height - 1; i >= 0; i--)
            {
                if (!_levels[i].IsEmpty)
                {
                    TopHeight = i;
                    return;
                }
            }

            TopHeight = -1;
        }
    }
}
public class BlackyTerrainChunkData
{
    private readonly int _chunkSize;
    private readonly BlackyTerrainColumn[] _columns;

    private ushort _biomeId;

    public int Size => _chunkSize;

    public BlackyTerrainChunkData(int chunkSize, ushort biomeId)
    {
        _chunkSize = chunkSize;
        _biomeId = biomeId;

        _columns = new BlackyTerrainColumn[_chunkSize * _chunkSize];

        for (int i = 0; i < _columns.Length; i++)
            _columns[i] = new BlackyTerrainColumn();
    }

    private int GetIndex(int x, int y)
    {
        return y * _chunkSize + x;
    }

    public ref BlackyTerrainColumn GetColumn(int x, int y)
    {
        int index = GetIndex(x, y);
        return ref _columns[index];
    }

    public void ClearColumn(int x, int y)
    {
        int index = GetIndex(x, y);
        _columns[index] = new BlackyTerrainColumn();
    }

    public void ClearAll()
    {
        for (int i = 0; i < _columns.Length; i++)
            _columns[i] = new BlackyTerrainColumn();
    }

    #region ===== BIOME =====

    public ushort GetBiomeId()
    {
        return _biomeId;
    }

    public void SetBiomeId(ushort biomeId)
    {
        _biomeId = biomeId;
    }

    #endregion

    #region ===== UTILITIES =====

    public bool IsChunkEmpty()
    {
        for (int i = 0; i < _columns.Length; i++)
        {
            if (_columns[i].TopHeight >= 0)
                return false;
        }

        return true;
    }

    #endregion
}