using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands
{
    public class DisableEntityRenderCommand : IRenderCommand
    {
        private readonly Entity entity;
        private readonly Rid rid;
        private readonly int instance;
        private readonly int materialId;

        public DisableEntityRenderCommand(
            Entity entity,
            Rid rid,
            int instance,
            int materialId)
        {
            this.entity = entity;
            this.rid = rid;
            this.instance = instance;
            this.materialId = materialId;
        }

        public void Execute()
        {
            AtlasTexturesModsManager.Instance.FreeInstance(
                rid,
                instance
            );
            //MultimeshManager.Instance.FreeInstance(
            //    rid,
            //    instance,
            //    materialId
            //);

            ref var render = ref entity.Ensure<RenderInstanceComponent>();
            render.isActive = false;

            entity.Add<RenderDisabledTag>();
        }
    }
}