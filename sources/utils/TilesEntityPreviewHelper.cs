using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.BlackyTiles.Tiles;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;

namespace GodotEcsArch.sources.utils;

public struct TilePreviewData
{
    public bool isEmpty;
    public int idMaterial;
    public int index;

    public TilePreviewData(int idMaterial, int index, bool isEmpty = false)
    {
        this.idMaterial = idMaterial;
        this.index = index;
        this.isEmpty = isEmpty;
    }

    public static TilePreviewData Empty()
    {
        return new TilePreviewData()
        {
            isEmpty = true,
            idMaterial = -1,
            index = -1
        };
    }
}

public static class TilesEntityPreviewHelper
{
    private static TilePreviewData[,] _previewMatrixData;
    private static Entity[,] _previewEntities;
    private static Vector2I _pivot;
    private static FlecsManager _flecsManager;
    private static Vector2I _size;
    private static Vector2I _positionLast;

    public static void Initialize(FlecsManager flecsManager)
    {
        _flecsManager = flecsManager;
    }

    // =========================================================
    // CREATE SIMPLE RECTANGLE (MISMO TILE)
    // =========================================================
    public static void Create(Vector2I size, int idMaterial, int index)
    {
        Clear();
        TilePreviewData[,] matrixData = new TilePreviewData[size.X, size.Y];

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                matrixData[x, y] = new TilePreviewData(idMaterial, index);
            }
        }

        Create(matrixData);
    }

    // =========================================================
    // CREATE RECTANGLE (MISMO MATERIAL, DIFERENTES INDEX)
    // =========================================================
    public static void Create(int idMaterial, int[,] matrixIndexes)
    {
        Clear();
        int width = matrixIndexes.GetLength(0);
        int height = matrixIndexes.GetLength(1);

        TilePreviewData[,] matrixData = new TilePreviewData[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                matrixData[x, y] = new TilePreviewData(idMaterial, matrixIndexes[x, y]);
            }
        }

        Create(matrixData);
    }

    // =========================================================
    // CREATE UNIVERSAL
    // =========================================================
    public static void Create(TilePreviewData[,] matrixData)
    {
        

        //_pivot = Vector2I.Zero; top left
        _previewMatrixData = matrixData;

        int width = matrixData.GetLength(0);
        int height = matrixData.GetLength(1);

        _size = new Vector2I(width, height);
        _pivot = new Vector2I(_size.X / 2, _size.Y / 2);
        _positionLast = new Vector2I(int.MinValue, int.MinValue);

        _previewEntities = new Entity[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TilePreviewData tileData = _previewMatrixData[x, y];

                if ( tileData.isEmpty)
                    continue;

                Vector2I localOffset = new Vector2I(x, y);

                var entity = TilesEntityTextureCreatorHelper.CreateSingle(
                    _flecsManager,
                    tileData.idMaterial,
                    tileData.index,
                    localOffset
                );

                _previewEntities[x, y] = entity;
            }
        }
    }
    internal static void Create(int idMaterial, GodotFlecs.sources.KuroTiles.TileSelectionMatrixData matrix)
    {
        Clear();

        TilePreviewData[,] matrixData = new TilePreviewData[matrix.width, matrix.height];

        for (int y = 0; y < matrix.height; y++)
        {
            for (int x = 0; x < matrix.width; x++)
            {
                int index = matrix.matrix[x, y].index - 1;
                bool isEmpty = matrix.matrix[x, y].isEmpty;
                int idMaterialIN = matrix.matrix[x,y].idMaterial;
                matrixData[x, y] = new TilePreviewData(idMaterialIN, index,isEmpty);
            }
        }

        Create(matrixData);
    }
    // =========================================================
    // MOVE PREVIEW CENTRADO
    // =========================================================
    public static void Move(Vector2I newPositionCenter)
    {
        if (_previewMatrixData == null || _previewEntities == null)
            return;

        if (_positionLast == newPositionCenter)
            return;

        _positionLast = newPositionCenter;

        for (int y = 0; y < _size.Y; y++)
        {
            for (int x = 0; x < _size.X; x++)
            {
                TilePreviewData tileData = _previewMatrixData[x, y];

                if ( tileData.isEmpty)
                    continue;

                Entity entity = _previewEntities[x, y];

                if (!entity.IsAlive())
                    continue;

                ref PositionComponent pos = ref entity.GetMut<PositionComponent>();

                Vector2I offset = new Vector2I(x - _pivot.X, (_size.Y - 1 - y) - _pivot.Y);

                pos.tilePosition = newPositionCenter + offset;

                entity.Add<DirtyTransformTag>();
            }
        }
    }

    private static Vector2I CalculateOffset(int x, int y, int centerX, int centerY)
    {
        return new Vector2I(x - centerX, y - centerY);
    }

    // =========================================================
    // CLEAR
    // =========================================================
    public static void Clear()
    {
        if (_previewEntities != null)
        {
            for (int x = 0; x < _size.X; x++)
            {
                for (int y = 0; y < _size.Y; y++)
                {
                    TilePreviewData tileData = _previewMatrixData[x, y];

                    if (tileData.isEmpty)
                        continue;

                    Entity entity = _previewEntities[x, y];

                    if (!entity.IsAlive())
                        continue;

                    var renderComp = entity.Get<RenderGPUComponent>();
                    BlackyTileRenderInstance instance = new BlackyTileRenderInstance(renderComp.rid, renderComp.instance);

                    RenderCommandQueue.Enqueue(new RemoveInstanceCommand(instance));
                    entity.Destruct();
                }
            }
        }

        _previewMatrixData = null;
        _previewEntities = null;
        _size = Vector2I.Zero;
        _positionLast = Vector2I.Zero;
    }


}