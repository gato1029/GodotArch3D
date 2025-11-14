using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
internal class TileSpriteHelper
{
    /// <summary>
    /// Devuelve todas las posiciones vecinas (8 direcciones) de los tiles del borde de un conjunto,
    /// es decir, aquellas posiciones adyacentes que no pertenecen al conjunto original.
    /// </summary>
    /// <param name="tiles">Lista o conjunto de posiciones base (Vector2I)</param>
    /// <returns>Lista de posiciones vecinas externas (sin duplicados)</returns>
    public static List<Vector2I> GetBorderNeighbors(IEnumerable<Vector2I> tiles)
    {
        // Convertir a HashSet para búsqueda rápida
        var selectedTiles = new HashSet<Vector2I>(tiles);
        var borderNeighbors = new HashSet<Vector2I>();

        // 8 direcciones vecinas
        Vector2I[] directions =
        {
        new Vector2I(1, 0),   // Este
        new Vector2I(-1, 0),  // Oeste
        new Vector2I(0, 1),   // Norte
        new Vector2I(0, -1),  // Sur
        new Vector2I(1, 1),   // Noreste
        new Vector2I(-1, 1),  // Noroeste
        new Vector2I(1, -1),  // Sureste
        new Vector2I(-1, -1), // Suroeste
    };

        // Iterar sobre todos los tiles del conjunto
        foreach (var tile in selectedTiles)
        {
            foreach (var dir in directions)
            {
                var neighbor = tile + dir;

                // Si el vecino no está dentro del conjunto original, es un borde externo
                if (!selectedTiles.Contains(neighbor))
                {
                    borderNeighbors.Add(neighbor);
                }
            }
        }

        return borderNeighbors.ToList();
    }
}
