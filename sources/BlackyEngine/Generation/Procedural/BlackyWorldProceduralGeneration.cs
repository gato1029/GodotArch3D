using Arch.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Procedural;

public class BlackyWorldProceduralGeneration
{
    private readonly BlackyWorldServices _services;
    private BlackyWorldSeed _worldSeed;
    private BlackyWorldGraphGenerator _graph;
    private BlackyWorldConfig _config;
    private List<BlackyWorldNode> _nodes;
    private BlackyWorldChunkNode _chunksNodes;
    public BlackyWorldProceduralGeneration(BlackyWorldServices services, BlackyWorldConfig worldConfig)
    {
        _services = services;
        _config = worldConfig;
        _worldSeed = new BlackyWorldSeed(_config.WorldSeed);
        _graph = new BlackyWorldGraphGenerator(_config.MapSize.X, _config.MapSize.Y, 20, 60f);
        _nodes = _graph.Generate(_worldSeed);
        _chunksNodes = new BlackyWorldChunkNode(_config.ChunkSize);
        _chunksNodes.Build(_nodes);

        // llena todo el mapa de terreno
        GenerateMapTerrain();
        GenerateMapBordersOptimized();
    }

    public BlackyWorldProceduralGeneration()
    {
    }

    public void GenerateMapTerrain()
    {
        var allChunks = _chunksNodes.GetAllChunksWithCandidates();

        for (int i = 0; i < allChunks.Count; i++)
        {
            BlackyChunkCoord chunkCandidate = allChunks[i];
            IReadOnlyList<BlackyWorldNode> candidates = _chunksNodes.GetCandidates(chunkCandidate);
            ProcessCandidate(chunkCandidate,candidates);
        }        
    }
    private void ProcessCandidate(BlackyChunkCoord coord, IReadOnlyList<BlackyWorldNode> candidates)
    {
        if (candidates.Count == 0)
        {
            // el chunk no tiene bioma asignado
            return;
        }
        if (candidates.Count == 1)
        {
            FillAll(coord, candidates[0].Bioma);
            return;
        }
        for (int lx = 0; lx < _config.ChunkSize; lx++)
        {
            for (int ly = 0; ly < _config.ChunkSize; ly++)
            {
                
                Vector2I worldPos = _services.TerrainDataLienzo.LocalToWorld(coord, lx, ly);
                var bioma = FindClosestBiome(candidates, worldPos);
                var terreno = AtlasModsManager.GetDirect<TerrainBaseData>(bioma.idTerreno);
                _services.TerrainDataLienzo.SetTerrainDirectNoRenderLocal(coord, lx,ly, 1, false, terreno);
                
               
            }
        }
    }
    private BiomaData FindClosestBiome(System.Collections.Generic.IReadOnlyList<BlackyWorldNode> candidates, Vector2 worldPos)
    {
        BlackyWorldNode closest = candidates[0];
        float minDist = closest.DistanceSquaredTo(worldPos);

        for (int i = 1; i < candidates.Count; i++)
        {
            float dist = candidates[i].DistanceSquaredTo(worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = candidates[i];
            }
        }

        return closest.Bioma;
    }
    private void FillAll(BlackyChunkCoord coord, BiomaData bioma)
    {
        var terreno = AtlasModsManager.GetDirect<TerrainBaseData>(bioma.idTerreno);

        for (int lx = 0; lx < _config.ChunkSize; lx++)
        {
            for (int ly = 0; ly < _config.ChunkSize; ly++)
            {
                
                _services.TerrainDataLienzo.SetTerrainDirectNoRenderLocal(coord, lx,ly, 1, false, terreno);
            }
        }
    }
    public void GenerateMapBordersOptimized()
    {
        int minX = _config.MinChunk.X;
        int maxX = _config.MaxChunk.X;
        int minY = _config.MinChunk.Y;
        int maxY = _config.MaxChunk.Y;

        // Recorremos el perímetro del rango de chunks
        for (int cx = minX; cx <= maxX; cx++)
        {
            for (int cy = minY; cy <= maxY; cy++)
            {
                // Solo procesamos si el chunk está en el perímetro (borde de mapa)
                bool isPerimeter = (cx == minX || cx == maxX || cy == minY || cy == maxY);

                if (isPerimeter)
                {
                    ProcessChunkAsBorder(new BlackyChunkCoord(cx, cy));
                }
            }
        }
    }


    private void ProcessChunkAsBorder(BlackyChunkCoord coord)
    {
        int size = _config.ChunkSize;
        int last = size - 1;

        for (int i = 0; i < size; i++)
        {
            // Borde superior (fila 0) e inferior (fila last)
            _services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, i, 0, 1, true);
            _services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, i, last, 1, true);

            // Borde izquierdo (columna 0) y derecho (columna last)
            // Evitamos repetir las esquinas si es necesario (0 y last ya se marcaron arriba)
            if (i > 0 && i < last)
            {
                _services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, 0, i, 1, true);
                _services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, last, i, 1, true);
            }
        }
    }
}
