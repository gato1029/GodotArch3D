using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Systems;
public class ChunkGenerationState
{
    private readonly HashSet<Vector2I> generatedChunks = new();

    public bool IsGenerated(Vector2I chunk)
    {
        return generatedChunks.Contains(chunk);
    }

    public void MarkGenerated(Vector2I chunk)
    {
        generatedChunks.Add(chunk);
    }

    public void Clear()
    {
        generatedChunks.Clear();
    }
}