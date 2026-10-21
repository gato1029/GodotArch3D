using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyDecorationWorldData: BlackyWorldDataMap<SerializerCellGeneric>
{
    private readonly ChunkManagerBase _chunkManager;
    private readonly BlackySimpleTileTextureMap _simpleTiles;

    public BlackyDecorationWorldData(int chunkSize, BlackySimpleTileTextureMap simpleTiles, BlackyWorldRegions regions, ChunkManagerBase chunkManager, BlackyWorld blackyWorld)
        : base(chunkSize, BlackyRenderLayer.Adornos, true, regions, blackyWorld)
    {
        _simpleTiles = simpleTiles ?? throw new ArgumentNullException(nameof(simpleTiles));
        _chunkManager = chunkManager;       
    }
    

    /// <summary>
    /// Pinta todos los adornos guardados en el mapa usando la caché de texturas.
    /// Equivalente a <c>PlotAllMap</c> de terreno/superficie, pero para tiles simples.
    /// </summary>
    public void PlotAllMap()
    {
        _simpleTiles.ApplyWorldBatchSimpleTiles(GetAllCells(), (int)RenderLayer);
    }

    public ushort GetDecorationPalettePersistenceId(DecorationData data)
    {
        return BlackyPalletesPersistence.decorationsPalette.GetIdPersistence(data.nameMod, data.id, out _);
    }

    /// <summary>Obtiene o crea un chunk para una escritura por lote, sin marcarlo sucio por celda.</summary>
    public BlackyChunkData<SerializerCellGeneric> GetOrCreateChunkDataForGeneration(BlackyChunkCoord coord)
    {
        if (_chunks.TryGetValue(coord, out var chunk))
            return chunk;

        chunk = new BlackyChunkData<SerializerCellGeneric>(ChunkSize);
        _chunks.Add(coord, chunk);
        return chunk;
    }

    /// <summary>Marca una sola vez el chunk y su región después de completar la escritura por lote.</summary>
    public void CompleteGenerationChunk(BlackyChunkCoord coord, BlackyChunkData<SerializerCellGeneric> chunk)
    {
        MarkChunkDirty(coord, chunk);
    }

    // =====================================================
    // SET TILE
    // =====================================================

    public void SetDataDirectNoRenderLocal(BlackyChunkCoord coord, int lx, int ly, int h, ushort id)
    {
        ref var cell = ref ResolveOrCreateCellLocal(coord, lx, ly, h);
        cell.id = id;
    }

    public void SetDecoration(int worldX, int worldY, int height, DecorationData data, Brush brush)
    {
        ushort id = BlackyPalletesPersistence.decorationsPalette.GetIdPersistence(data.nameMod, data.id, out var decorationPersist);
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell = ref ResolveOrCreateCell(x, y, height);

            cell.id = id;
            _simpleTiles.SetTileSprite(x, y, height, (int)RenderLayer, decorationPersist.idTileSprite);
        }        
    }

    public void SetTile(int worldX, int worldY, int height, string modName, ushort textureIndex)
    {
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.id = _simpleTiles.SetTile(worldX, worldY, height, (int)RenderLayer, modName, textureIndex, true);

    }

    // =====================================================
    // REMOVE TILE
    // =====================================================

    public void RemoveAdorno(
        int worldX,
        int worldY,
        int height)
    {
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.id = 0;

        _simpleTiles.RemoveTileSprite(
            worldX,
            worldY,
            height,
            (int)RenderLayer);
    }

    // =====================================================
    // GET
    // =====================================================

    public bool HasTile(
        int worldX,
        int worldY,
        int height)
    {
        if (!TryGetCell(
            worldX,
            worldY,
            height,
            out var cell))
        {
            return false;
        }

        return cell.id != 0;
    }

    public ushort GetTile(
        int worldX,
        int worldY,
        int height)
    {
        if (!TryGetCell(
            worldX,
            worldY,
            height,
            out var cell))
        {
            return 0;
        }
        return cell.id;
    }
}
