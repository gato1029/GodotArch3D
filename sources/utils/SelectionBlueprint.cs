
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Creators;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System.Collections.Generic;

public class SelectionBlueprint:SingletonBase<SelectionBlueprint>
{
    private Entity[,] gridEntities;
    private long tileId = 1762385049549000;
    private int renderLayer;
    private  FlecsManager flecsManager;

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
    public void Configure(long tileId, int renderLayer)
    {
        this.renderLayer = renderLayer;
        this.tileId = tileId;        
    }
    public void ConfigureFlecs(FlecsManager flecsManager)
    {
        this.flecsManager = flecsManager;
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
                TileSpriteData tileData = MasterDataManager.GetData<TileSpriteData>(tileId);
                if (tileData!=null)
                {
                    var entity = TileSpriteCreator.Instance.CreateSingleSprite(flecsManager,tileData.spriteData, worldPos, tilePos, renderLayer);

                    gridEntities[i, j] = entity;
                }
              
                //Entity entity = TilesManager.Instance.CreateTileDinamic(tileId, 1, worldPos, renderLayer, Vector2.Zero);
                

                //ref var tilePosition = ref entity.Get<TilePositionComponent>();
                //tilePosition.x = tilePos.X;
                //tilePosition.y = tilePos.Y;

                //ref var position = ref entity.Get<PositionComponent>();
                //position.position = worldPos;
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

                ref var position = ref entity.GetMut<PositionComponent>();
                position.position = newWorldPos;
                position.tilePosition = newTilePos;
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
                {
                    var entity = gridEntities[i, j];
                    var renderComp = entity.Get<RenderGPUComponent>();
                    MultimeshManager.Instance.FreeInstance(renderComp.rid, renderComp.instance, renderComp.idMaterial);
                    entity.Destruct();
                }
                  
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

                var tile = e.Get<PositionComponent>();
                yield return (e, tile.tilePosition);
            }
        }
    }

}
