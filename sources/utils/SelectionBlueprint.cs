using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using System.Collections.Generic;

public class SelectionBlueprint:SingletonBase<SelectionBlueprint>
{
    private Entity[,] gridEntities;
    private int tileId;
    private int renderLayer;

    public Vector2I Size { get; private set; }
    protected override void Initialize()
    {

    }

    protected override void Destroy()
    {

    }
    public SelectionBlueprint()
    {
     
    }
    public void Configure(int tileId, int renderLayer)
    {
        this.renderLayer = renderLayer;
        this.tileId = tileId;
    }

    public void Create(Vector2I size, Vector2I centerTile)
    {
        Clear();
        Size = size;
        gridEntities = new Entity[size.X, size.Y];

        Vector2I halfSize = size / 2;

        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                Vector2I offset = new(i - halfSize.X, j - halfSize.Y);
                Vector2I tilePos = centerTile + offset;

                Vector2 worldPos = TilesHelper.WorldPositionTile(tilePos);

                Entity entity = TilesManager.Instance.CreateTileDinamic(tileId, 1, worldPos, renderLayer, Vector2.Zero);
                gridEntities[i, j] = entity;

                ref var tilePosition = ref entity.Get<TilePositionComponent>();
                tilePosition.x = tilePos.X;
                tilePosition.y = tilePos.Y;

                ref var position = ref entity.Get<PositionComponent>();
                position.position = worldPos;
            }
        }
    }

    public void Move(Vector2I targetTilePosition)
    {
        if (gridEntities == null) return;

        Vector2I halfSize = Size / 2;

        for (int x = 0; x < Size.X; x++)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                Entity entity = gridEntities[x, y];
                if (!entity.IsAlive()) continue;

                Vector2I offset = new(x - halfSize.X, y - halfSize.Y);
                Vector2I newTilePos = targetTilePosition + offset;
                Vector2 newWorldPos = TilesHelper.WorldPositionTile(newTilePos);

                ref var position = ref entity.Get<PositionComponent>();
                position.position = newWorldPos;

                ref var tilePosition = ref entity.Get<TilePositionComponent>();
                tilePosition.x = newTilePos.X;
                tilePosition.y = newTilePos.Y;
            }
        }
    }

    public void Clear()
    {
        if (gridEntities == null) return;

        for (int i = 0; i < Size.X; i++)
        {
            for (int j = 0; j < Size.Y; j++)
            {
                if (gridEntities[i, j].IsAlive())
                    TilesManager.Instance.FreeTileEntity(gridEntities[i, j]);
            }
        }
        gridEntities = null;
    }

    public IEnumerable<(Entity entity, Vector2I tilePos)> IterateWithTilePositions()
    {
        if (gridEntities == null) yield break;

        for (int x = 0; x < Size.X; x++)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                var e = gridEntities[x, y];
                if (!e.IsAlive()) continue;

                var tile = e.Get<TilePositionComponent>();
                yield return (e, new Vector2I(tile.x, tile.y));
            }
        }
    }

}
