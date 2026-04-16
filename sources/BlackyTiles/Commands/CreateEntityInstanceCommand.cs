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
    public class CreateEntityInstanceCommand : IRenderCommand
    {
        private readonly Entity entity;
        private readonly Vector3 worldPosition;
        private readonly SpriteData data;

        public CreateEntityInstanceCommand(
            Entity entity,
            Vector3 worldPosition,
            SpriteData data)
        {
            this.entity = entity;
            this.worldPosition = worldPosition;
            this.data = data;
        }

        public void Execute()
        {
            Transform3D transform = new(Basis.Identity, worldPosition);
            transform = transform.ScaledLocal(new Vector3(data.scale, data.scale, 1));

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
                data.GetUv()
            );

            RenderingServer.MultimeshInstanceSetColor(
                dataInstance.rid,
                dataInstance.instance,
                new Color(0, 0, 0, dataInstance.layerTexture)
            );

            // 🔥 guardar en la entidad
            ref var render = ref entity.Ensure<RenderInstanceComponent>();

            render.rid = dataInstance.rid;
            render.instance = dataInstance.instance;
            render.materialId = data.idMaterial;
            render.isActive = true;
        }
    }
}
