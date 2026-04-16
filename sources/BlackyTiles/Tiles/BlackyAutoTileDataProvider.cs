using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Tiles;
public interface BlackyAutoTileDataProvider
{
    /// <summary>
    /// Devuelve true si existe tile válido en esa posición.
    /// También devuelve los datos necesarios para reglas.
    /// </summary>
    bool TryGetTileData(
        int altura,
        int layer,
        int worldX,
        int worldY,
        out long idTile,
        out int idAgrupador);

    /// <summary>
    /// Aplica el template resultante de la regla.
    /// </summary>
    void ApplyTileTemplate(int altura,
        int layer,
        int worldX,
        int worldY,
        TileTemplate template, bool clearData = false);
}