
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Creators;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;


namespace GodotEcsArch.sources.utils;
public class PlacementPreview : SingletonBase<PlacementPreview>
{
    private Entity[,] previewEntities;
    private long previewTileId;
    private int renderLayer;

    public Vector2I Size { get; private set; }

    protected override void Initialize() { }
    protected override void Destroy() { Clear(); }

    public void Configure(long tileId, int layer)
    {
        previewTileId = tileId;
        renderLayer = layer;
        Size = new Vector2I(0, 0);
    }

    public void Create(Vector2I size, Vector2I centerTile, TileSpriteData tileSpriteData)
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

                var entity = TileSpriteCreator.Instance.CreateSingleSprite(tileSpriteData, worldPos,tilePos,renderLayer);
                // Crear la entidad del preview usando SpriteData

                previewEntities[i, j] = entity;

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
                var spriteData = entity.Get<RenderGPUComponent>();
                Vector2I offset = new(x - halfSize.X, y - halfSize.Y);
                Vector2I newTilePos = targetTilePosition + offset;
                Vector2 newWorldPos = TilesHelper.WorldPositionTile(newTilePos);

                Vector2 positionNormalize = newTilePos * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
                Vector2 positionCenter = positionNormalize + new Vector2(xx, yy);
                Vector2 positionReal = positionNormalize + new Vector2(xx, yy) + new Vector2(spriteData.originOffset.X * spriteData.scale, spriteData.originOffset.Y * spriteData.scale);


                ref var position = ref entity.GetMut<PositionComponent>();
                position.position = positionReal;
                position.tilePosition = newTilePos;                
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
                {
                    var entity = previewEntities[i, j];
                    var renderComp = entity.Get<RenderGPUComponent>();  
                    MultimeshManager.Instance.FreeInstance(renderComp.rid,renderComp.instance,renderComp.idMaterial);
                    entity.Destruct();
                }
            }
        }
        previewEntities = null;
    }
}
