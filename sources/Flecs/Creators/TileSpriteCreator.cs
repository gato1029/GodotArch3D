using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Creators;
public class TileSpriteCreator:SingletonBase<TileSpriteCreator>
{

    public Entity CreateSingleSprite(FlecsManager flecsManager, SpriteData spriteData, Vector2 WorldPosition, Vector2I tilePosition, int renderLayer = 0)
    {
        if (flecsManager==null)
        {
            return default;
        }
        var instance = MultimeshManager.Instance.CreateInstance(spriteData.idMaterial);

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(WorldPosition.X, WorldPosition.Y, (WorldPosition.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        transform = transform.ScaledLocal(new Godot.Vector3(spriteData.scale, spriteData.scale, 1));

        Godot.Vector2 originOffset = spriteData.offsetInternal;

        var entity = flecsManager.WorldFlecs.Entity();
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instance.rid, instance.instance, instance.material, instance.layerTexture, renderLayer, spriteData.yDepthRender, 1, originOffset));
        var uv = spriteData.GetUv();
        entity.Set(new RenderFrameDataComponent { uvMap = uv});
        entity.Set(new PositionComponent { position = WorldPosition, tilePosition = tilePosition, height=10 });

        return entity;
    }

    internal Entity CreateSingleSprite(FlecsManager flecsManager, TileSpriteData tileSpriteData, Vector2 WorldPosition, Vector2I tilePosition, int renderLayer)
    {
        if (flecsManager == null)
        {
            return default;
        }
        (Rid rid, int instance, int material, int layerTexture) instance = default;
        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        Godot.Vector2 originOffset = Vector2.Zero;
        Color uv = new Color();
        float yDepthRender = 0;
        switch (tileSpriteData.tileSpriteType)
        {
            case TileSpriteType.Static:
                SpriteData spriteData = tileSpriteData.spriteData;
                instance= MultimeshManager.Instance.CreateInstance(spriteData.idMaterial);
                
                transform.Origin = new Godot.Vector3(WorldPosition.X, WorldPosition.Y, (WorldPosition.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
                transform = transform.ScaledLocal(new Godot.Vector3(spriteData.scale, spriteData.scale, 1));

                originOffset = spriteData.offsetInternal;                
                uv = spriteData.GetUv();
                yDepthRender = spriteData.yDepthRender;
                break;
            case TileSpriteType.Animated:
                SpriteAnimationData AnimationData = tileSpriteData.animationData;
                instance = MultimeshManager.Instance.CreateInstance(AnimationData.idMaterial);

                transform.Origin = new Godot.Vector3(WorldPosition.X, WorldPosition.Y, (WorldPosition.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
                transform = transform.ScaledLocal(new Godot.Vector3(AnimationData.scale, AnimationData.scale, 1));

                originOffset = AnimationData.offsetInternal;
                uv =TextureHelper.GetUvFormatFromSpriteAnimation(AnimationData,0);
                yDepthRender = AnimationData.yDepthRender;


                break;
                
            default:
                break;
        }

        var entity = flecsManager.WorldFlecs.Entity();
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instance.rid, instance.instance, instance.material, instance.layerTexture, renderLayer, yDepthRender, 1, originOffset));
        entity.Set(new RenderFrameDataComponent { uvMap = uv });
        entity.Set(new PositionComponent { position = WorldPosition, tilePosition = tilePosition, height = 10 });
        return entity;
       

    }
}
