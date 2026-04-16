using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands
{
    public class CreateEntityAnimatedInstanceCommand : IRenderCommand
    {
        private readonly Entity entity;
        private readonly Vector3 worldPosition;
        private readonly SpriteAnimationData data;

        private readonly long animationId;
        private readonly float frameDuration;
        private readonly float depth;
        private readonly float scale;
        private readonly Vector2 offset;
        private readonly int layer;

        public CreateEntityAnimatedInstanceCommand(
            Entity entity,
            Vector3 worldPosition,
            SpriteAnimationData data,
            long animationId,
            float frameDuration,
            float depth,
            float scale,
            Vector2 offset,
            int layer)
        {
            this.entity = entity;
            this.worldPosition = worldPosition;
            this.data = data;
            this.animationId = animationId;
            this.frameDuration = frameDuration;
            this.depth = depth;
            this.scale = scale;
            this.offset = offset;
            this.layer = layer;
        }

        public void Execute()
        {
            Transform3D transform = new(Basis.Identity, worldPosition);
            transform = transform.ScaledLocal(new Vector3(scale, scale, 1));

            //var dataInstance = MultimeshManager.Instance.CreateInstance(data.idMaterial);
            var dataInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(data.idModMaterial);
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

            // 🔥 COMPONENTES EN LA MISMA ENTITY

            ref var render = ref entity.Ensure<RenderInstanceComponent>();
            render.rid = dataInstance.rid;
            render.instance = dataInstance.instance;
            render.materialId = data.idMaterial;
            render.isActive = true;

            entity.Set(new RenderTransformComponent(transform));

            entity.Set(new RenderGPUComponent(
                dataInstance.rid,
                dataInstance.instance,
                data.idMaterial,
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

            entity.Add<TileSpriteAnimationTag>();
        }
    }
}
