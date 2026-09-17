using Godot;
using GodotEcsArch.sources.BlackyEngine.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Godot.Image;

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

    public static string RutaGuardadoMapas = "D:\\GitKraken\\MapsGame";

    internal static float Calculate(float depthOffsetY, float height, float layer, Vector2 positionCenter)
    {
        return  (height * 10f) + (layer * 0.1f) - ((positionCenter.Y+ depthOffsetY) * 0.001f);
    }

    public static SavedMapData LoadInfoMap(string nameMap, SaveFormat _format)
    {
        string rootPathLocal = RutaGuardadoMapas + "\\" + nameMap;
        string saveFolder = Path.Combine(rootPathLocal, "world");
        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"world_info.{extension}"; // Nota: Si prefieres cambiarlo a "map_info.{extension}" en el save y load, sería ideal para no mezclarlo con las unidades.
        string fullPath = Path.Combine(saveFolder, fileName);

        if (_format == SaveFormat.Json)
        {
            string json = File.ReadAllText(fullPath);
            byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
            return MessagePackSerializer.Deserialize<SavedMapData>(bytes);
        }
        else
        {
            byte[] data = File.ReadAllBytes(fullPath);
            return MessagePackSerializer.Deserialize<SavedMapData>(data);
        }
    }
}
