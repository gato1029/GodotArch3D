using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public class TilePaletteCount
{
    public int Count { get; private set; }

    public TilePaletteCount(int count)
    {
        Count = count;
    }
}
