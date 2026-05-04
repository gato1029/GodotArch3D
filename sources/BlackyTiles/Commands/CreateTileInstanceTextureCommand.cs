using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Metrics;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyTiles.Commands;

public class CreateTileInstanceTextureCommand : IRenderCommand
{
    public readonly int height;
    public readonly int layer;
    public readonly int x;
    public readonly int y;
    public readonly TileDataMod tileDataMod;
    private readonly BlackyChunkRenderTiles chunkRender;
    public CreateTileInstanceTextureCommand(int height, int layer, int x, int y, TileDataMod tileDataMod, BlackyChunkRenderTiles chunkRender)
    {
        this.height = height;
        this.layer = layer;
        this.x = x;
        this.y = y;
        this.tileDataMod = tileDataMod;
        this.chunkRender = chunkRender;
    }

    public void Execute()
    {
        if (chunkRender.IsDestroyed)
            return;

        //cuando es solo tile simple visual, no hay entidad

        var RenderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(tileDataMod.ModName);
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(x, y);


        float depthOffset = 0;
        float depthValue = positionCenter.Y + depthOffset - height * CommonAtributes.HEIGHT_OFFSET ;
        float z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layer * CommonAtributes.LAYER_OFFSET;

        Vector3 worldPosition = new(positionCenter.X, positionCenter.Y, z);

        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(1,1,1));

        RenderingServer.MultimeshInstanceSetTransform(
            RenderInstance.rid,
            RenderInstance.instance,
            transform
        );

        RenderingServer.MultimeshInstanceSetCustomData(
            RenderInstance.rid,
            RenderInstance.instance,
            tileDataMod.BaseUV
        );

        RenderingServer.MultimeshInstanceSetColor(
            RenderInstance.rid,
            RenderInstance.instance,
            new Godot.Color(0, 0, 0, RenderInstance.layerTexture)
        );



        var tileRender = new TileRenderTextureInstance
        {
            Rid = RenderInstance.rid,
            InstanceId = RenderInstance.instance,
            SubTextureId = tileDataMod.SubTextureId,
            Index = tileDataMod.Index
        };
        chunkRender.AddOrReplace((height, layer, x, y), tileRender);

        if (tileDataMod.IsAnimated)
        {

        }
        //var renderData = new BlackyTileRenderInstance(
        //    dataInstance.rid,
        //    dataInstance.instance,
        //    data.idMaterial, height, layer, localX, localY
        //);

        //var world = flecsManager.WorldFlecs;
        //var entity = world.Entity();

        //entity.Set(new RenderTransformComponent(transform));

        //entity.Set(new RenderGPUComponent(
        //    renderData.rid,
        //    renderData.Instance,
        //    renderData.MaterialId,
        //    dataInstance.layerTexture,
        //    layer,
        //    depth,
        //    scale,
        //    offset));

        //entity.Set(new AnimationComponent(
        //    animationId,
        //    EntityType.TILESPRITE,
        //    AnimationType.PARADO,
        //    AnimationType.NINGUNA,
        //    1,
        //    0,
        //    frameDuration,
        //    false,
        //    true,
        //    true));

        //entity.Set(new RenderFrameDataComponent
        //{
        //    uvMap = data.uvFramesArray[0]
        //});

        //entity.Set(new PositionComponent
        //{
        //    position = new Vector2(worldPosition.X, worldPosition.Y),
        //    tilePosition = new Vector2I(localX, localY)
        //});

        //entity.Add<TileSpriteAnimationTag>();


    }
}
