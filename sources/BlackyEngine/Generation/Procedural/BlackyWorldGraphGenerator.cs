using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Procedural;



    /// <summary>
    /// Genera el grafo "macro" del mundo: posiciones de nodos con distancia
    /// mínima entre sí, bioma asignado, radio de influencia para el
    /// NodeChunkIndex, y conexiones entre nodos (para caminos y para
    /// garantizar que el mundo sea navegable).
    /// </summary>
public class BlackyWorldGraphGenerator
{
        public int MapWidth { get; }
        public int MapHeight { get; }
        public int NodeCount { get; }
        public float MinDistanceBetweenNodes { get; }

        public BlackyWorldGraphGenerator(int mapWidth, int mapHeight, int nodeCount=0, float minDistanceBetweenNodes = 0)
        {
            // hago esta correcion de tamaño por que esto trabaja con posiciones de mundo, y el ancho y alto de mapa es en tiles
            MapWidth = (int)TilesHelper.TilePositionToWorldPosition(mapWidth);
            MapHeight = (int)TilesHelper.TilePositionToWorldPosition(mapHeight);
            if (nodeCount == 0)
            {
                NodeCount = CalculateProportionalNodeCount(mapWidth, mapHeight);
            }
            else
            {
                NodeCount = nodeCount;
            }            
            if (minDistanceBetweenNodes == 0)
            {
                MinDistanceBetweenNodes = CalculateAutomaticMinDistance(MapWidth, MapHeight, NodeCount);
            }
            else
            {
                MinDistanceBetweenNodes = minDistanceBetweenNodes;
            }
        }

    /// <summary>
    /// Calcula cuántos nodos corresponden al mapa en función de su área total.
    /// </summary>
    private int CalculateProportionalNodeCount(int width, int height)
    {
        float totalArea = (float)width * height;

        // Define cuántas unidades cuadradas abarca cada nodo en promedio.
        // A menor valor, más denso será el grafo de nodos; a mayor valor, más espaciado.
        float areaPerNode = 15000f;

        int proportionalCount = Mathf.RoundToInt(totalArea / areaPerNode);

        // Aseguramos un rango lógico (por ejemplo, nunca menos de 4 nodos y un límite según prefieras)
        return Math.Clamp(proportionalCount, 4, 200);
    }
    /// <summary>
    /// Calcula dinámicamente la distancia mínima entre nodos basada en el área total y la cantidad de nodos.
    /// </summary>
    private static float CalculateAutomaticMinDistance(int width, int height, int nodeCount)
        {
            if (nodeCount <= 0) return 10f;

            float totalArea = width * height;
            float areaPerNode = totalArea / nodeCount;

            // Obtenemos la distancia base aproximada usando la raíz cuadrada del área por nodo
            float baseDistance = Mathf.Sqrt(areaPerNode);

            // Aplicamos un factor de seguridad (0.8f) para garantizar que el muestreo por rechazo 
            // encuentre espacio suficiente sin volverse excesivamente lento o fallar.
            float safetyFactor = 0.8f;

            // Aseguramos un límite mínimo absoluto (por ejemplo, 10 unidades) para mapas diminutos
            return MathF.Max(10f, baseDistance * safetyFactor);
        }
        public List<BlackyWorldNode> Generate(BlackyWorldSeed seed)
        {
            var positionRng = seed.GetRng("graph_positions");
            var biomeRng = seed.GetRng("graph_biomes");

            var positions = GeneratePositions(positionRng);
            var nodes = AssignBiomesAndRadius(positions, biomeRng);

            ConnectMinimumSpanningTree(nodes);
            AddExtraConnections(nodes, seed.GetRng("graph_extra_edges"), extraEdgeChance: 0.15f);

            return nodes;
        }

    /// <summary>
    /// Rejection sampling: tira puntos random y descarta los que caen
    /// muy cerca de uno ya aceptado. Simple y suficiente para 10-40 nodos;
    /// si necesitaras cientos, conviene Poisson-disc real (más eficiente).
    /// </summary>
    private List<Vector2> GeneratePositions(Random rng)
    {
        var positions = new List<Vector2>();
        int maxAttempts = NodeCount * 200;
        int attempts = 0;

        // Calculamos los límites basados en que el mapa está centrado en (0,0)
        float halfWidth = MapWidth * 0.5f;
        float halfHeight = MapHeight * 0.5f;

        while (positions.Count < NodeCount && attempts < maxAttempts)
        {
            attempts++;

            // Generamos coordenadas centradas en (0,0): van desde -halfWidth hasta +halfWidth
            var candidate = new Vector2(
                (float)(rng.NextDouble() * MapWidth - halfWidth),
                (float)(rng.NextDouble() * MapHeight - halfHeight));

            bool tooClose = false;
            foreach (var existing in positions)
            {
                if (candidate.DistanceSquaredTo(existing) < MinDistanceBetweenNodes * MinDistanceBetweenNodes)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                positions.Add(candidate);
                GD.Print("Candidato", candidate);
            }
        }

        if (positions.Count < NodeCount)
        {
            throw new InvalidOperationException(
                $"Solo se pudieron colocar {positions.Count}/{NodeCount} nodos. " +
                $"El mapa de {MapWidth}x{MapHeight} (centrado en 0,0) está muy saturado para la distancia automática ({MinDistanceBetweenNodes:F2}).");
        }

        return positions;
    }

    private List<BlackyWorldNode> AssignBiomesAndRadius(List<Vector2> positions, Random rng)
        {
            Dictionary<ushort, BiomaData> biomes = BlackyPalletesPersistence.biomePalette.GetAllPallete();            
            var nodes = new List<BlackyWorldNode>(positions.Count);

            // Radio generoso: bastante más que la distancia mínima entre nodos,
            // así el NodeChunkIndex no descarta por error un chunk que sí le
            // corresponde. Es solo optimización, no el borde real (ver BlackyWorldNode).
            float influenceRadius = MinDistanceBetweenNodes * 1.8f;

            var biomeKeys = new List<ushort>(biomes.Keys);

            for (int i = 0; i < positions.Count; i++)
            {
                // Seleccionamos una clave aleatoria válida del diccionario
                ushort randomKey = biomeKeys[rng.Next(biomeKeys.Count)];
                var biome = biomes[randomKey];

                nodes.Add(new BlackyWorldNode(id: i, positions[i], biome, influenceRadius));
            }
            return nodes;
        }

        /// <summary>
        /// Conecta todos los nodos con el árbol de menor costo total (Prim's
        /// algorithm) para garantizar que el mundo entero sea navegable con
        /// el mínimo de caminos redundantes.
        /// </summary>
        private void ConnectMinimumSpanningTree(List<BlackyWorldNode> nodes)
        {
            if (nodes.Count <= 1) return;

            var inTree = new HashSet<BlackyWorldNode> { nodes[0] };
            while (inTree.Count < nodes.Count)
            {
                BlackyWorldNode bestOutside = null;
                BlackyWorldNode bestInside = null;
                float bestDist = float.MaxValue;

                foreach (var inside in inTree)
                {
                    foreach (var outside in nodes)
                    {
                        if (inTree.Contains(outside)) continue;

                        float dist = inside.DistanceSquaredTo(outside.Position);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestInside = inside;
                            bestOutside = outside;
                        }
                    }
                }

                bestInside.ConnectTo(bestOutside);
                inTree.Add(bestOutside);
            }
        }

        /// <summary>
        /// El MST conecta todo con el mínimo de aristas posible, lo cual deja
        /// el mundo en forma de árbol puro (un único camino entre dos puntos
        /// cualquiera). Agregar algunas conexiones extra crea loops, así el
        /// mapa no se siente como un pasillo lineal.
        /// </summary>
        private void AddExtraConnections(List<BlackyWorldNode> nodes, Random rng, float extraEdgeChance)
        {
            foreach (var a in nodes)
            {
                foreach (var b in nodes)
                {
                    if (a.Id >= b.Id) continue; // evita duplicados y auto-conexión
                    if (a.Neighbors.Contains(b)) continue; // ya conectados por el MST

                    float dist = MathF.Sqrt(a.DistanceSquaredTo(b.Position));
                    bool closeEnough = dist < MinDistanceBetweenNodes * 2.5f;

                    if (closeEnough && rng.NextDouble() < extraEdgeChance)
                        a.ConnectTo(b);
                }
            }
        }
    }