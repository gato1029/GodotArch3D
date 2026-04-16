using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles;
public struct BlackyChunkCoord : IEquatable<BlackyChunkCoord>
{
    public int X;
    public int Y;

    public BlackyChunkCoord(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(BlackyChunkCoord other)
        => X == other.X && Y == other.Y;

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }
}
