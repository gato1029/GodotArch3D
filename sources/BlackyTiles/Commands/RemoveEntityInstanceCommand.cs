using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Multimesh;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands
{
    public class RemoveEntityInstanceCommand : IRenderCommand
    {
        private readonly Rid rid;
        private readonly int instance;
        private readonly int materialId;
        private readonly Entity entity;

        public RemoveEntityInstanceCommand(
            Rid rid,
            int instance,
            int materialId, Entity entity)
        {
            this.entity = entity;
            this.rid = rid;
            this.instance = instance;
            this.materialId = materialId;
        }

        public void Execute()
        {          
            MultimeshManager.Instance.FreeInstance(
                rid,
                instance,
                materialId
            );

            entity.Remove<RenderInstanceComponent>();
        }
    }
}
