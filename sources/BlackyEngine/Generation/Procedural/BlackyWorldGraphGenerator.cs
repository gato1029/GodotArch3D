using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.managers.Mods;
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

        public BlackyWorldGraphGenerator(int mapWidth, int mapHeight, int nodeCount, float minDistanceBetweenNodes)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            NodeCount = nodeCount;
            MinDistanceBetweenNodes = minDistanceBetweenNodes;
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

            while (positions.Count < NodeCount && attempts < maxAttempts)
            {
                attempts++;

                var candidate = new Vector2(
                    (float)(rng.NextDouble() * MapWidth),
                    (float)(rng.NextDouble() * MapHeight));

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
                    positions.Add(candidate);
            }

            if (positions.Count < NodeCount)
            {
                throw new InvalidOperationException(
                    $"Solo se pudieron colocar {positions.Count}/{NodeCount} nodos. " +
                    "Reducí MinDistanceBetweenNodes o aumentá el tamaño del mapa.");
            }

            return positions;
        }

        private List<BlackyWorldNode> AssignBiomesAndRadius(List<Vector2> positions, Random rng)
        {
            var biomes = BlackyPalletesPersistence.biomePalette.GetAllPallete();            
            var nodes = new List<BlackyWorldNode>(positions.Count);

            // Radio generoso: bastante más que la distancia mínima entre nodos,
            // así el NodeChunkIndex no descarta por error un chunk que sí le
            // corresponde. Es solo optimización, no el borde real (ver BlackyWorldNode).
            float influenceRadius = MinDistanceBetweenNodes * 1.8f;

            for (int i = 0; i < positions.Count; i++)
            {
                var biome = biomes[(ushort)rng.Next(biomes.Count)];
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