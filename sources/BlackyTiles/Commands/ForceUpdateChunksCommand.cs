using Godot;
using GodotEcsArch.sources.managers.Chunks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;
public struct ForceUpdateChunksCommand : IRenderCommand
{
    private readonly ChunkManagerBase chunkManager;
    private readonly Vector2I position;

    public ForceUpdateChunksCommand(ChunkManagerBase chunkManager, Vector2I position)
    {
        this.chunkManager = chunkManager;
        this.position = position;
    }

    public void Execute()
    {
        chunkManager.ForcedUpdateChunks(position);
    }
}