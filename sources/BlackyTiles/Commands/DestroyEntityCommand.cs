using Flecs.NET.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands
{
    internal class DestroyEntityCommand : IRenderCommand
    {
        private readonly Entity entity;
        public DestroyEntityCommand(Entity entity)
        {
            this.entity = entity;
        }

        public void Execute()
        {
            entity.Destruct();
        }
    }
}
