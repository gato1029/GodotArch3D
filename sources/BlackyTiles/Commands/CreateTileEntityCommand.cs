using Godot;
using GodotEcsArch.sources.BlackyTiles.Tiles;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;
public class CreateTileEntityCommand : IRenderCommand
{
    private readonly Vector3 worldPosition;
    private readonly int localX;
    private readonly int localY;
    private readonly int height;
    private readonly int layer;
    private readonly float depth;
    private readonly float scale;
    private readonly Vector2 offset;

    private readonly long animationId;
    private readonly float frameDuration;

    private readonly SpriteAnimationData data;

    private readonly Action<BlackyTileRenderInstance> onCreated;
    private readonly FlecsManager flecsManager;

    public CreateTileEntityCommand(
        Vector3 worldPosition,
        int height,
        int localX,
        int localY,
        int layer,
        float depth,
        float scale,
        Vector2 offset,
        long animationId,
        float frameDuration,
        SpriteAnimationData data,
        Action<BlackyTileRenderInstance> onCreated, FlecsManager flecsManager)
    {
        this.height = height;
        this.worldPosition = worldPosition;
        this.localX = localX;
        this.localY = localY;

        this.layer = layer;
        this.depth = depth;
        this.scale = scale;
        this.offset = offset;

        this.animationId = animationId;
        this.frameDuration = frameDuration;

        this.data = data;
        this.onCreated = onCreated;
        this.flecsManager = flecsManager;
    }

    public void Execute()
    {
        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(data.scale, data.scale, 1));        
        var dataInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(data.idModMaterial);

        //var dataInstance = MultimeshManager.Instance.CreateInstance(data.idMaterial);

        RenderingServer.MultimeshInstanceSetTransform(
            dataInstance.rid,
            dataInstance.instance,
            transform
        );

        RenderingServer.MultimeshInstanceSetCustomData(
            dataInstance.rid,
            dataInstance.instance,
            data.uvFramesArray[0]
        );

        RenderingServer.MultimeshInstanceSetColor(
            dataInstance.rid,
            dataInstance.instance,
            new Color(0, 0, 0, dataInstance.layerTexture)
        );

        var renderData = new BlackyTileRenderInstance(
            dataInstance.rid,
            dataInstance.instance,
            data.idMaterial,height,layer,localX, localY 
        );

        var world = flecsManager.WorldFlecs;
        var entity = world.Entity();

        entity.Set(new RenderTransformComponent(transform));

        entity.Set(new RenderGPUComponent(
            renderData.rid,
            renderData.Instance,
            renderData.MaterialId,
            dataInstance.layerTexture,
            layer,
            depth,
            scale,
            offset));

        entity.Set(new AnimationComponent(
            animationId,
            EntityType.TILESPRITE,
            AnimationType.PARADO,
            AnimationType.NINGUNA,
            1,
            0,
            frameDuration,
            false,
            true,
            true));

        entity.Set(new RenderFrameDataComponent
        {
            uvMap = data.uvFramesArray[0]
        });

        entity.Set(new PositionComponent
        {
            position = new Vector2(worldPosition.X, worldPosition.Y),
            tilePosition = new Vector2I(localX, localY)
        });

        entity.Add<TileSpriteAnimationTag>();

        renderData.SetEntityReference(entity);

        // ← aquí devolvemos el renderData
        onCreated?.Invoke(renderData);
    }
}