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
    private readonly BlackyWorld _world;
    private BlackyWorldSeed _worldSeed;
    private BlackyWorldGraphGenerator _graph;
    private BlackyWorldConfig _config;
    private List<BlackyWorldNode> _nodes;
    private BlackyWorldChunkNode _chunksNodes;
    public BlackyWorldProceduralGeneration(BlackyWorld world, BlackyWorldConfig worldConfig)
    {        
        _world = world;
        _config = worldConfig;
        _worldSeed = new BlackyWorldSeed(_config.WorldSeed);
        _graph = new BlackyWorldGraphGenerator(_config.MapSize.X, _config.MapSize.Y);
        _nodes = _graph.Generate(_worldSeed);
        _chunksNodes = new BlackyWorldChunkNode(_config);
        _chunksNodes.Build(_nodes);

        // llena todo el mapa de terreno
        GenerateMapTerrain();
        GenerateMapBordersOptimized();
        
        //_world.Services.TerrainDataLienzo.DrawAllMapTxt(); // debug: genera un txt con el mapa de terreno generado

        world.Streaming.chunkManagerLocal.Teleport(new Vector2(0,0)); // esto hace que se renderice el chunk central y se carguen los chunks alrededor
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
            //GD.Print($"Coordenada {chunkCandidate.X}, {chunkCandidate.Y} procesada. Biomas asignados.");
        }        
    }
    private void ProcessCandidate(BlackyChunkCoord coord, IReadOnlyList<BlackyWorldNode> candidates)
    {
        if (candidates.Count == 0)
        {
            GD.PrintErr($"No candidates found for chunk {coord}. This chunk will remain unassigned.");
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
                
                Vector2I worldPos = _world.Services.TerrainDataLienzo.LocalToWorld(coord, lx, ly);
                var bioma = FindClosestBiome(candidates, worldPos);
                var terreno = AtlasModsManager.GetDirect<TerrainBaseData>(bioma.idTerreno);
                _world.Services.TerrainDataLienzo.SetTerrainDirectNoRenderLocal(coord, lx,ly, 1, false, terreno);
                
               
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
                
                _world.Services.TerrainDataLienzo.SetTerrainDirectNoRenderLocal(coord, lx,ly, 1, false, terreno);
            }
        }
    }
    public void GenerateMapBordersOptimized()
    {
        int minX = _config.MinChunk.X;
        int maxX = _config.MaxChunk.X;
        int minY = _config.MinChunk.Y;
        int maxY = _config.MaxChunk.Y;

        // Recorremos únicamente los chunks que están en los bordes extremos del mapa global
        for (int cx = minX; cx <= maxX; cx++)
        {
            for (int cy = minY; cy <= maxY; cy++)
            {
                // Verificamos qué caras de este chunk dan hacia el exterior absoluto del mapa
                bool isLeftBorderChunk = (cx == minX);
                bool isRightBorderChunk = (cx == maxX);
                bool isTopBorderChunk = (cy == minY);
                bool isBottomBorderChunk = (cy == maxY);

                // Si el chunk toca al menos un borde exterior, procesamos solo sus caras externas
                if (isLeftBorderChunk || isRightBorderChunk || isTopBorderChunk || isBottomBorderChunk)
                {
                    ProcessChunkOuterBorderOnly(new BlackyChunkCoord(cx, cy), isLeftBorderChunk, isRightBorderChunk, isTopBorderChunk, isBottomBorderChunk);
                }
            }
        }
    }

    private void ProcessChunkOuterBorderOnly(BlackyChunkCoord coord, bool isLeft, bool isRight, bool isTop, bool isBottom)
    {
        int size = _config.ChunkSize;
        int last = size - 1;

        for (int i = 0; i < size; i++)
        {
            // Borde superior absoluto del mapa
            if (isTop)
            {
                _world.Services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, i, 0, 1, true);
            }

            // Borde inferior absoluto del mapa
            if (isBottom)
            {
                _world.Services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, i, last, 1, true);
            }

            // Borde izquierdo absoluto del mapa
            if (isLeft)
            {
                _world.Services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, 0, i, 1, true);
            }

            // Borde derecho absoluto del mapa
            if (isRight)
            {
                _world.Services.TerrainDataLienzo.SetTerrainBorderDirectNoRenderLocal(coord, last, i, 1, true);
            }
        }
    }
}
