using Godot;
using GodotEcsArch.sources.BlackyTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;

public class ChunkHelper
{
    public const int ChunkSize = 32;

    public static Vector2I WorldToChunkCoord(Vector2I position)
    {
        return new Vector2I(
            FloorDiv(position.X, ChunkSize),
            FloorDiv(position.Y, ChunkSize)
        );
    }


    public static Vector2I WorldToChunkCoord(int worldX, int worldY)
    {
        return new Vector2I(
            FloorDiv(worldX, ChunkSize),
            FloorDiv(worldY, ChunkSize)
        );
    }

    public static (int localX, int localY) WorldToLocal(int worldX, int worldY)
    {
        int localX = Mod(worldX, ChunkSize);
        int localY = Mod(worldY, ChunkSize);

        return (localX, localY);
    }

    private static int FloorDiv(int a, int b)
    {
        int result = a / b;
        if ((a ^ b) < 0 && (result * b != a))
            result--;
        return result;
    }

    private static int Mod(int a, int b)
    {
        int result = a % b;
        if (result < 0)
            result += b;
        return result;
    }
}
