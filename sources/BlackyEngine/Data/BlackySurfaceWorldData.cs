using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackySurfaceWorldData: BlackyWorldDataMap<SerializerCellGeneric>
{
    private readonly ChunkManagerBase _chunkManager;
    private DualTileTemplate _dualTemplate;

    // Cache para optimizar la lógica de "DiferentPosition"
    private struct BrushState { public int x, y, altura, capa; public long id; }
    private BrushState _lastState = new() { altura = -1, capa = -1, id = -1 };

    public BlackySurfaceWorldData(int chunkSize, BlackyChunkCacheTextureMap textureMap, BlackyWorldRegions regions, ChunkManagerBase chunkManager, BlackyWorld blackyWorld)
        : base(chunkSize, BlackyRenderLayer.Superficie, textureMap, true, regions,blackyWorld)
    {
        _chunkManager = chunkManager;
        _chunkManager.OnChunkDataPreload += OnChunkDataPreload;
    }

    #region Event Handlers
    private void OnChunkDataPreload(Vector2I pos)
    {
        var coord = new BlackyChunkCoord(pos.X, pos.Y);
        if (!_chunks.TryGetValue(coord, out var chunk)) return;

        foreach (var (height, heightData) in chunk.GetHeights())
        {
            _textureMap.ApplyChunkBatchDualTiles(coord.X, coord.Y, height, (int)RenderLayer, heightData.GetCells().ToArray());
        }
    }
    #endregion

    #region Public API: Set & Remove
    public void SetDualTemplate(DualTileTemplate dualTemplate) => _dualTemplate = dualTemplate;

    public void SetSuperficieDirectNoRender(int x, int y, int h, bool isBorder, SuperficieData data)
    {
        ref var cell = ref ResolveOrCreateCell(x, y, h);
        cell.id = BlackyPalletesPersistence.surfacesPalette.GetIdPersistence(data.nameMod, data.id, out _);
        cell.isBorder = isBorder;
    }

    public void RemoveSuperficieDirectNoRender(int x, int y, int h)
    {
        ref var cell = ref ResolveOrCreateCell(x, y, h);
        cell.id = 0;
        cell.isBorder = false;
    }

    public void SetSuperficie(int x, int y, int h, SuperficieData data, Brush brush)
    {
        if (!IsNewPosition(x, y, h, (int)RenderLayer, brush, data.id)) return;

        // 1. Aplicar cambios
        foreach (var offset in brush.Cells)
        {
            ref var cell = ref ResolveOrCreateCell(x + offset.x, y + offset.y, h);
            cell.id = BlackyPalletesPersistence.surfacesPalette.GetIdPersistence(data.nameMod, data.id, out _);
        }

        // 2. Actualizar estado de bordes y Render
        UpdateBorderStatesForBrush(x, y, h, brush);
        _dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);
        _textureMap.ApplyBrushCreateDual(x, y, h, (int)RenderLayer, brush, _dualTemplate);
    }

    public void RemoveSuperficie(int x, int y, int h, Brush brush)
    {
        if (!IsNewPosition(x, y, h, (int)RenderLayer, brush, 0)) return;

        ushort lastId = 0;
        foreach (var offset in brush.Cells)
        {
            ref var cell = ref ResolveOrCreateCell(x + offset.x, y + offset.y, h);
            if (cell.id != 0) lastId = cell.id;
            cell.id = 0;
        }

        if (lastId == 0) return;

        UpdateBorderStatesForBrush(x, y, h, brush);
        var data = BlackyPalletesPersistence.surfacesPalette.GetData(lastId);
        _dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);
        _textureMap.ApplyBrushRemoveDual(x, y, h, (int)RenderLayer, brush, _dualTemplate);
    }
    #endregion

    #region Internal Logic
    private void UpdateBorderStatesForBrush(int worldX, int worldY, int h, Brush brush)
    {
        // Actualiza el área afectada + 1 de margen para detectar vecinos
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                foreach (var offset in brush.Cells)
                {
                    int x = worldX + offset.x + ox;
                    int y = worldY + offset.y + oy;

                    // Si HasSuperficie devuelve true, significa que la celda existe y tiene ID != 0
                    if (HasSuperficie(x, y, h))
                    {
                        // Obtenemos la REFERENCIA real a la celda en memoria
                        ref var cell = ref ResolveOrCreateCell(x, y, h);

                        // Ahora sí, la modificación afectará a la data del chunk
                        cell.isBorder = CalculateIsBorder(x, y, h);
                    }
                }
            }
        }
    }
    public bool HasSuperficie(int worldX, int worldY, int height)
    {
        if (!TryGetCell(worldX, worldY, height, out var cell))
        {
            return false;
        }

        return cell.id != 0;
    }
    private bool CalculateIsBorder(int x, int y, int h)
    {
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                if (ox == 0 && oy == 0) continue;
                if (!HasSuperficie(x + ox, y + oy, h)) return true;
            }
        }
        return false;
    }

    private bool IsNewPosition(int x, int y, int h, int layer, Brush brush, long id)
    {
        bool isNew = (x != _lastState.x || y != _lastState.y || h != _lastState.altura || id != _lastState.id);
        if (isNew) _lastState = new BrushState { x = x, y = y, altura = h, capa = layer, id = id };
        return isNew;
    }
    #endregion
}
