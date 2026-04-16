using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.MapSeed;

internal class SemillaTerrenoCapa
{
    public Dictionary<Vector2I, SemillaIsla> islas;

}
internal class SemillaIsla
{
    public Vector2I centro;
    public Vector2I dimencion;
    public Vector2I posicionInicio;
    public bool[,]  baldosas;
    public List<Vector2I> bordeIsla; // posiciones Baldosas

    public SemillaIsla(Vector2I centro, Vector2I dimencion, Vector2I posicionInicio)
    {
        this.centro = centro;
        this.dimencion = dimencion;
        this.posicionInicio = posicionInicio;
    }
}
