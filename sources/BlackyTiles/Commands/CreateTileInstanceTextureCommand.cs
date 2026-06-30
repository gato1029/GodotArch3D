using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
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
    public readonly TileSpriteData tileDataMod;
    private readonly BlackyChunkRenderTiles chunkRender;
    private readonly bool dualOffset;
    private readonly int idSprite;
    private readonly FlecsManager flecsManager;
    public CreateTileInstanceTextureCommand(int idSprite,int height, int layer, int x, int y, bool dualOffset, TileSpriteData tileDataMod, BlackyChunkRenderTiles chunkRender, FlecsManager flecsManager)
    {
        this.idSprite = idSprite;
        this.dualOffset = dualOffset;
        this.height = height;
        this.layer = layer;
        this.x = x;
        this.y = y;
        this.tileDataMod = tileDataMod;
        this.chunkRender = chunkRender;
        this.flecsManager = flecsManager;
    }

    public void Execute()
    {
        if (chunkRender.IsDestroyed)
            return;
        //cuando es solo tile simple visual, no hay entidad
        switch (tileDataMod.tileSpriteType)
        {         
            case TileSpriteType.Static:
                CreateSpriteSingle(idSprite,tileDataMod.spriteData);
                break;
            case TileSpriteType.Animated:
                CreateSpriteAnimated(idSprite,tileDataMod.animationData);
                break;
            default:
                break;
        }

        //var RenderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(tileDataMod.ModName);
        //Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(x, y);
        //Vector2 offset = new Vector2(0, 0);
        //if (dualOffset)
        //{
        //    offset = new Vector2(0.25f, 0.25f);
        //}
        ////else
        ////{
        ////    offset = new Vector2(0.25f, 0.25f);
        ////}

        //float depthOffset = 0;
        //float depthValue = positionCenter.Y + depthOffset - height * CommonAtributes.HEIGHT_OFFSET ;
        ////float z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layer * CommonAtributes.LAYER_OFFSET;

        //float z = CommonAtributes.Calculate(depthOffset, height, layer, positionCenter); // debemos usar esto apartir de ahora
        ////GD.Print("depthValue:  " + depthValue);
        ////GD.Print("Z:  " + z);

        //Vector3 worldPosition = new(positionCenter.X+offset.X, positionCenter.Y+offset.Y, z);

        //Transform3D transform = new(Basis.Identity, worldPosition);
        //transform = transform.ScaledLocal(new Vector3(1,1,1));

        //RenderingServer.MultimeshInstanceSetTransform(
        //    RenderInstance.rid,
        //    RenderInstance.instance,
        //    transform
        //);

        //RenderingServer.MultimeshInstanceSetCustomData(
        //    RenderInstance.rid,
        //    RenderInstance.instance,
        //    tileDataMod.BaseUV
        //);

        //RenderingServer.MultimeshInstanceSetColor(
        //    RenderInstance.rid,
        //    RenderInstance.instance,
        //    new Godot.Color(0, 0, 0, RenderInstance.layerTexture)
        //);



        //var tileRender = new TileRenderTextureInstance
        //{
        //    Rid = RenderInstance.rid,
        //    InstanceId = RenderInstance.instance,
        //    SubTextureId = tileDataMod.SubTextureId,
        //    Index = tileDataMod.Index
        //};
        //chunkRender.AddOrReplace((height, layer, x, y), tileRender);

        //if (tileDataMod.IsAnimated)
        //{

        //}
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

    private void CreateSpriteAnimated(int id, SpriteAnimationData animationData)
    {
        var RenderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(animationData.idModMaterial);
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(x, y);
        Vector2 offset = animationData.offsetInternal;
        if (dualOffset)
        {
            offset = new Vector2(0.25f, 0.25f);
        }
        float depthOffset = animationData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, height, layer, positionCenter); // debemos usar esto apartir de ahora
        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(animationData.scale, animationData.scale, 1));

        RenderingServer.MultimeshInstanceSetTransform(
            RenderInstance.rid,
            RenderInstance.instance,
            transform
        );

        RenderingServer.MultimeshInstanceSetCustomData(
            RenderInstance.rid,
            RenderInstance.instance,
            animationData.uvFramesArray[0]
        );

        RenderingServer.MultimeshInstanceSetColor(
            RenderInstance.rid,
            RenderInstance.instance,
            new Godot.Color(0, 0, 0, RenderInstance.layerTexture)
        );

        var world = flecsManager.WorldFlecs;
        var entity = world.Entity();

        entity.Set(new RenderTransformComponent(transform));

        // ya no requieres material
        entity.Set(new RenderGPUComponent(
            RenderInstance.rid,
            RenderInstance.instance,            
            0,
            RenderInstance.layerTexture,
            layer,
            depthOffset,
            animationData.scale,
            offset));

        entity.Set(new AnimationComponent(
            idSprite,
            EntityType.TILESPRITE,
            AnimationType.PARADO,
            AnimationType.NINGUNA,
            1,
            0,
            animationData.frameDuration,
            false,
            true,
            true));

        entity.Set(new RenderFrameDataComponent
        {
            uvMap = animationData.uvFramesArray[0]
        });

        entity.Set(new PositionComponent
        {
            position = new Vector2(worldPosition.X, worldPosition.Y),
            tilePosition = new Vector2I(x, y)
        });

        entity.Add<TileSpriteAnimationTag>();

        //renderData.SetEntityReference(entity);


        var tileRender = new TileRenderTextureInstance
        {
            Rid = RenderInstance.rid,
            InstanceId = RenderInstance.instance,
            idSprite = id,
            Entity = entity
        };
        chunkRender.AddOrReplace((height, layer, x, y), tileRender);
    }

    private void CreateSpriteSingle(int id, SpriteData spriteData)
    {        
        var RenderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(spriteData.idModMaterial);
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(x, y);
        Vector2 offset = spriteData.offsetInternal;
        if (dualOffset)
        {
            offset = offset + new Vector2(0.25f, 0.25f);
        }
        float depthOffset = spriteData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, height, layer, positionCenter); // debemos usar esto apartir de ahora
        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(spriteData.scale, spriteData.scale, 1));

        RenderingServer.MultimeshInstanceSetTransform(
            RenderInstance.rid,
            RenderInstance.instance,
            transform
        );

        RenderingServer.MultimeshInstanceSetCustomData(
            RenderInstance.rid,
            RenderInstance.instance,
            spriteData.uv
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
            idSprite = id,            
        };
        chunkRender.AddOrReplace((height, layer, x, y), tileRender);
    }
}
