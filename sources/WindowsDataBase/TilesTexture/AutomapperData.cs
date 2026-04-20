using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotEcsArch.sources.WindowsDataBase.Materials;

namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;

public class AutomapperData : IdDataLong
{
    // El pipeline de fases (ej: 0:Bordes, 1:Acantilados, 2:Decoración)
    public List<AutoTilePhase> Phases { get; set; } = new List<AutoTilePhase>();

    /// <summary>
    /// Ejecuta todas las fases del automapper sobre una región específica.
    /// </summary>
    public void UpdateRegion(IReadOnlyTileMap map, ITileMapActions actions, Godot.Rect2I region)
    {
        foreach (var phase in Phases)
        {
            //phase.Execute(map, actions, region);
        }
    }
}
