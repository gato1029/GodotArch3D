using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
public  class CommonAtributes
{
    public const float LAYER_MULTIPLICATOR = -0.05f;
    public const float HEIGHT_OFFSET = 32f;
    public const float LAYER_OFFSET = 0.0005f;
    //public static float LAYER_MULTIPLICATOR = -0.05F; // -0.05F
    //public const float HEIGHT_OFFSET = 0.01f;
    //public const float LAYER_OFFSET = 0.003f;
    public static Vector2I VIEW_DISTANCE_CHUNK_32 = new Vector2I(5, 2);
    public static Vector2I VIEW_DISTANCE_CHUNK_16 = new Vector2I(20, 4);
    public static string pathMaps = "AssetExternals/Mapas";
}
