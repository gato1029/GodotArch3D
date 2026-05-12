using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;

public enum BrushType
{
    Square,
    Circle,
    Ring,
    Cross
}
public static class Brushes
{
    public static readonly Brush Single = new(
        (0, 0)
    );

    public static Brush Cross(int size)
    {
        var set = new HashSet<(int x, int y)>();

        for (int i = -size; i <= size; i++)
        {
            set.Add((i, 0));
            set.Add((0, i));
        }

        return new Brush(set.ToArray());
    }
    public static Brush Square(int size)
    {
        var list = new List<(int x, int y)>();

        int start = -(size / 2);
        int end = start + size - 1;

        for (int y = start; y <= end; y++)
            for (int x = start; x <= end; x++)
                list.Add((x, y));

        return new Brush(list.ToArray());
    }
    public static Brush Circle(int radius)
    {
        var list = new List<(int x, int y)>();

        int r2 = radius * radius;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= r2)
                    list.Add((x, y));
            }
        }

        return new Brush(list.ToArray());
    }
    public static Brush Ring(int radius, int thickness = 1)
    {
        var list = new List<(int x, int y)>();

        int rOuter = radius * radius;
        int rInner = (radius - thickness) * (radius - thickness);

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int dist = x * x + y * y;

                if (dist <= rOuter && dist >= rInner)
                    list.Add((x, y));
            }
        }

        return new Brush(list.ToArray());
    }
}
