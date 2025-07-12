using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
public  class CommonAtributes
{
    public static float LAYER_MULTIPLICATOR = -0.005F; // 0.005F
    public static Vector2I VIEW_DISTANCE_CHUNK_32 = new Vector2I(5, 2);
    public static Vector2I VIEW_DISTANCE_CHUNK_16 = new Vector2I(20, 4);
    public static string pathMaps = "AssetExternals/Mapas";
}
