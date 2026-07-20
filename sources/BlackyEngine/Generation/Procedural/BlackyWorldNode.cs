using Godot;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Procedural;



    /// <summary>
    /// Representa un nodo "macro" del mundo: un punto abstracto que define
    /// el centro de un bioma, antes de que exista la grilla de tiles.
    /// </summary>
    public class BlackyWorldNode
{
        public int Id { get; }
        public BiomaData Bioma { get; }

        /// <summary>Posición del nodo en el espacio del mundo (mismas unidades que la grilla de tiles).</summary>
        public Vector2 Position { get; set; }

        /// <summary>Bioma asignado a este nodo.</summary>
        

        /// <summary>
        /// Radio de influencia APROXIMADO, usado solo como optimización para
        /// descartar rápido qué chunks podrían pertenecer a este nodo.
        /// NO es el borde real del bioma — el borde real lo decide la
        /// comparación de distancia tile por tile (Voronoi). Este radio debe
        /// ser generoso (más grande que el territorio típico esperado) para
        /// no descartar por error un chunk que en realidad le corresponde.
        /// </summary>
        public float InfluenceRadius { get; set; }

        /// <summary>Nodos vecinos conectados por un camino. La conexión es bidireccional.</summary>
        public List<BlackyWorldNode> Neighbors { get; } = new();

        public BlackyWorldNode(int id, Vector2 position, BiomaData bioma, float influenceRadius)
        {
            Id = id;
            Bioma = bioma;
            Position = position;            
            InfluenceRadius = influenceRadius;
        }

        public void ConnectTo(BlackyWorldNode other)
        {
            if (other == null || other == this)
                return;

            if (!Neighbors.Contains(other))
                Neighbors.Add(other);

            if (!other.Neighbors.Contains(this))
                other.Neighbors.Add(this);
        }

        /// <summary>
        /// Distancia al cuadrado hacia un punto. Sin sqrt: para comparar
        /// "más cercano" no hace falta la distancia real, y evitar la raíz
        /// cuadrada es más barato en loops grandes.
        /// </summary>
        public float DistanceSquaredTo(Vector2 point)
        {
            return Position.DistanceSquaredTo(point);
        
        }

        public override string ToString()
        {
            return $"Node # [{Bioma.name}] at ({Position.X:F1}, {Position.Y:F1}), " +
                   $"radius {InfluenceRadius:F1}, {Neighbors.Count} connections";
        }
    }
