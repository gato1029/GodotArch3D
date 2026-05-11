using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Helpers;

public static class NodeHelper
{
    /// <summary>
    /// Elimina todos los nodos hijos de este nodo.
    /// </summary>
    /// <param name="node">
    /// Nodo cuyos hijos serán eliminados.
    /// </param>

    public static void ClearChildrens(this Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            child.QueueFree();
        }
    }
}
