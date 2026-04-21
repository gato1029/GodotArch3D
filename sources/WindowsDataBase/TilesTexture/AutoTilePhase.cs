using System.Collections.Generic;
using System.Xml.Linq;
using GodotEcsArch.sources.WindowsDataBase.Materials;

namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;

public class AutoTilePhase
{
    public string name { get; set; }
    public int materialId { get; set; }
    public int order {  get; set; }
    public List<TileRuleTextureData> rules { get; set; } = new List<TileRuleTextureData>();

    // Determina si esta fase lee del mapa lógico o de lo que puso la fase anterior
    public bool ReadsFromVisualLayer { get; set; } = false;

    public AutoTilePhase() { }
}
