using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
public class PlacementPreview : SingletonBase<PlacementPreview>
{
    private Entity[,] previewEntities;
    private int previewTileId;
    private int renderLayer;

    public Vector2I Size { get; private set; }

    protected override void Initialize() { }
    protected override void Destroy() { Clear(); }

    public void Configure(int tileId, int layer)
    {
        previewTileId = tileId;
        renderLayer = layer;
        Size = new Vector2I(0, 0);
    }

    public void Create(Vector2I size, Vector2I centerTile, SpriteData spriteData)
    {
        Clear();
        Size = size;
        previewEntities = new Entity[size.X, size.Y];

        Vector2I halfSize = size / 2;

        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                Vector2I offset = new(i - halfSize.X, j - halfSize.Y);
                Vector2I tilePos = centerTile + offset;

                Vector2 worldPos = TilesHelper.WorldPositionTile(tilePos);

                // Crear la entidad del preview usando SpriteData
                Entity entity = SpriteHelper.CreateEntity(
                    spriteData,
                    spriteData.scale,
                    worldPos,
                    renderLayer,
                    spriteData.offsetInternal
                );

                previewEntities[i, j] = entity;

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
        if (previewEntities == null) return;
        Vector2I tileSize = new Vector2I(16, 16);
        float xx = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float yy = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;
        Vector2I halfSize = Size / 2;

        for (int x = 0; x < Size.X; x++)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                Entity entity = previewEntities[x, y];
                if (!entity.IsAlive()) continue;
                var spriteData = entity.Get<SpriteRenderGPUComponent>();
                Vector2I offset = new(x - halfSize.X, y - halfSize.Y);
                Vector2I newTilePos = targetTilePosition + offset;
                Vector2 newWorldPos = TilesHelper.WorldPositionTile(newTilePos);

                Vector2 positionNormalize = newTilePos * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
                Vector2 positionCenter = positionNormalize + new Vector2(xx, yy);
                Vector2 positionReal = positionNormalize + new Vector2(xx, yy) + new Vector2(spriteData.originOffset.X * spriteData.scale, spriteData.originOffset.Y * spriteData.scale);


                ref var position = ref entity.Get<PositionComponent>();
                position.position = positionReal;

                ref var tilePosition = ref entity.Get<TilePositionComponent>();
                tilePosition.x = newTilePos.X;
                tilePosition.y = newTilePos.Y;
            }
        }
    }

    public void Clear()
    {
        if (previewEntities == null) return;

        for (int i = 0; i < Size.X; i++)
        {
            for (int j = 0; j < Size.Y; j++)
            {
                if (previewEntities[i, j].IsAlive())
                    TilesManager.Instance.FreeTileEntity(previewEntities[i, j]);
            }
        }
        previewEntities = null;
    }
}
