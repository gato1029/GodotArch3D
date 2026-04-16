using Godot;
using GodotEcsArch.sources.BlackyTiles.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;

public struct CreateResourceSourceCommand : IRenderCommand
{
    public ushort id;
    public Vector2I position;
    public bool renderForce;
    public BlackyResourcesSourceSystem system;

    public CreateResourceSourceCommand(
        BlackyResourcesSourceSystem system,
        ushort id,
        Vector2I position,
        bool renderForce)
    {
        this.system = system;
        this.id = id;
        this.position = position;
        this.renderForce = renderForce;
    }

    public void Execute()
    {
        system.Create(id, position, renderForce);
    }
}