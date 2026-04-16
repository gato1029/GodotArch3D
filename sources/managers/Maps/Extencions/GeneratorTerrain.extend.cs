using Godot;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;
public enum SpotShape
{
    Cluster,
    Circle,
    // Futuro:
    // Ellipse,
    // NoisePatch
}
public class SpotOptions
{
    public SpotShape Shape = SpotShape.Cluster;

    public int CountSpots = 10;   // cuántas manchas
    public int MinSize = 5;       // usado en cluster
    public int MaxSize = 20;

    public int MinRadius = 2;     // usado en circle
    public int MaxRadius = 6;

    public bool Mark = true;      // marcar o desmarcar

    public TerrainTileType? UnderType = null; // solo si está sobre otro tipo
}

public partial class GeneratorTerrain
{

    public List<Vector2I> CreateSpots(TerrainTileType type, SpotOptions opt)
    {
        return opt.Shape switch
        {
            SpotShape.Circle => CreateSpotsCircular(type, opt),
            SpotShape.Cluster => CreateSpotsCluster(type, opt),
            _ => throw new NotImplementedException()
        };

    }
    private List<Vector2I> CreateSpotsCluster(TerrainTileType type, SpotOptions opt)
    {
        List<Vector2I> affected = new List<Vector2I>();
        Random rng = new Random();

        // offsets del bloque 2x2
        Vector2I[] blockOffsets = new Vector2I[]
        {
        new Vector2I(0, 0),
        new Vector2I(1, 0),
        new Vector2I(0, 1),
        new Vector2I(1, 1)
        };

        for (int s = 0; s < opt.CountSpots; s++)
        {
            Vector2I center = GetRandomTilePosition();
            int targetSize = rng.Next(opt.MinSize, opt.MaxSize + 1);

            HashSet<Vector2I> spot = new HashSet<Vector2I>();
            Queue<Vector2I> frontier = new Queue<Vector2I>();

            frontier.Enqueue(center);
            spot.Add(center);

            while (frontier.Count > 0 && spot.Count < targetSize)
            {
                var pos = frontier.Dequeue();

                foreach (var dir in NeighborDirs)
                {
                    Vector2I next = pos + dir;

                    if (spot.Contains(next))
                        continue;

                    spot.Add(next);
                    frontier.Enqueue(next);

                    if (spot.Count >= targetSize)
                        break;
                }
            }

            // Aplicar cambios con bloque 2x2
            foreach (var basePos in spot)
            {
                if (opt.UnderType != null && !IsTileMarked(opt.UnderType.Value, basePos))
                    continue;

                foreach (var off in blockOffsets)
                {
                    Vector2I pos = basePos + off;

                    if (opt.Mark)
                        MarkTile(type, pos);
                    else
                        UnmarkTile(type, pos);

                    affected.Add(pos);
                }
            }
        }

        return affected;
    }

    private static readonly Vector2I[] NeighborDirs =
{
    new Vector2I(1,0), new Vector2I(-1,0),
    new Vector2I(0,1), new Vector2I(0,-1),
    new Vector2I(1,1), new Vector2I(1,-1),
    new Vector2I(-1,1), new Vector2I(-1,-1)
};

    private List<Vector2I> CreateSpotsCircular(TerrainTileType type, SpotOptions opt)
    {
        List<Vector2I> affected = new List<Vector2I>();
        Random rng = new Random();

        // offsets del bloque 2x2
        Vector2I[] blockOffsets = new Vector2I[]
        {
        new Vector2I(0, 0),
        new Vector2I(1, 0),
        new Vector2I(0, 1),
        new Vector2I(1, 1)
        };
        for (int i = 0; i < opt.CountSpots; i++)
        {
            Vector2I center = GetRandomTilePosition();
            int radius = rng.Next(opt.MinRadius, opt.MaxRadius + 1);
            int r2 = radius * radius;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy > r2)
                        continue;

                    Vector2I basePos = center + new Vector2I(dx, dy);
                    
                    if (opt.UnderType != null && !IsTileMarked(opt.UnderType.Value, basePos))
                        continue;

                    // aplicar bloque 2x2
                    foreach (var off in blockOffsets)
                    {
                        Vector2I pos = basePos + off;

                        if (opt.Mark)
                            MarkTile(type, pos);
                        else
                            UnmarkTile(type, pos);

                        affected.Add(pos);
                    }
                }
            }
        }

        return affected;
    }
    private Vector2I GetRandomTilePosition()
    {
        int x = (int)(GD.Randi() % sizeMap.X - sizeMap.X / 2);
        int y = (int)(GD.Randi() % sizeMap.Y - sizeMap.Y / 2);
        return new Vector2I(x, y);
    }
    private bool IsTileMarked(TerrainTileType type, Vector2I pos)
    {
        var (ix, iy) = Normalize(pos);

        if (ix < 0 || ix >= sizeMap.X ||
            iy < 0 || iy >= sizeMap.Y)
            return false;

        return layerTileMask[type][ix, iy];
    }
    private void MarkTile(TerrainTileType type, Vector2I pos)
    {
        var (ix, iy) = Normalize(pos);

        if (ix >= 0 && ix < sizeMap.X &&
            iy >= 0 && iy < sizeMap.Y)
        {
            layerTileMask[type][ix, iy] = true;
        }
    }
    private void UnmarkTile(TerrainTileType type, Vector2I pos)
    {
        var (ix, iy) = Normalize(pos);

        if (ix >= 0 && ix < sizeMap.X &&
            iy >= 0 && iy < sizeMap.Y)
        {
            layerTileMask[type][ix, iy] = false;
        }
    }
    private (int ix, int iy) Normalize(Vector2I pos)
    {
        int initX = sizeMap.X / 2;
        int initY = sizeMap.Y / 2;

        return (pos.X + initX, pos.Y + initY);
    }

    private Vector2I NormalizeInverse(int ix, int iy)
    {
        int initX = sizeMap.X / 2;
        int initY = sizeMap.Y / 2;

        int worldX = ix - initX;
        int worldY = iy - initY;

        return new Vector2I(worldX, worldY);
    }

   

    private int MX(int x) => x + offsetX;
    private int MY(int y) => y + offsetY;

    private int MX(Vector2I p) => p.X + offsetX;
    private int MY(Vector2I p) => p.Y + offsetY;
    private int[,] distToBorder;
    private int sizeX, sizeY;
    private int offsetX, offsetY;

    private Dictionary<(int, int), HashSet<Vector2I>> blobGrid = new();
    private int gridCellSize = 8;
    private List<Vector2I> CreateSmallMediumBlobsLayer(
    TerrainTileType type,
    TerrainTileType underType,
    int minBlobSize,
    int maxBlobSize,
    float chancePercent,
    int minBorderDistance,
    int minBlobSeparation)
    {
        List<Vector2I> tiles = new();
        Random rng = new Random();
        double chance = chancePercent / 100.0;

        sizeX = sizeMap.X;
        sizeY = sizeMap.Y;

        offsetX = sizeX / 2;
        offsetY = sizeY / 2;

        PrecalcBorderDistances(underType);
        blobGrid.Clear();

        ForEachTilePosition(pos =>
        {
            int mx = MX(pos);
            int my = MY(pos);

            if (distToBorder[mx, my] < minBorderDistance)
                return true;

            if (IsNearOtherBlobFast(pos, minBlobSeparation))
                return true;

            if (rng.NextDouble() > chance)
                return true;

            int blobSize = rng.Next(minBlobSize, maxBlobSize + 1);
            FloodBlobNoiseFast(pos, blobSize, type, underType, tiles, minBorderDistance);

            return true;
        });

        return tiles;
    }
    private void PrecalcBorderDistances(TerrainTileType underType)
    {
        distToBorder = new int[sizeX, sizeY];
        Queue<Vector2I> q = new();

        for (int x = -offsetX; x < sizeX - offsetX; x++)
            for (int y = -offsetY; y < sizeY - offsetY; y++)
            {
                int mx = MX(x);
                int my = MY(y);

                if (!IsTileMarked(underType, new Vector2I(x, y)))
                {
                    distToBorder[mx, my] = 0;
                    q.Enqueue(new(x, y));
                }
                else distToBorder[mx, my] = int.MaxValue;
            }

        int[,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            int mx = MX(p);
            int my = MY(p);
            int d = distToBorder[mx, my];

            for (int i = 0; i < 4; i++)
            {
                Vector2I np = new(p.X + dirs[i, 0], p.Y + dirs[i, 1]);

                int nx = MX(np);
                int ny = MY(np);

                if (nx < 0 || ny < 0 || nx >= sizeX || ny >= sizeY)
                    continue;

                if (distToBorder[nx, ny] > d + 1)
                {
                    distToBorder[nx, ny] = d + 1;
                    q.Enqueue(np);
                }
            }
        }
    }
    private void FloodBlobNoiseFast(
    Vector2I start,
    int size,
    TerrainTileType type,
    TerrainTileType underType,
    List<Vector2I> output,
    int minBorderDistance)
    {
        if (distToBorder[MX(start), MY(start)] < minBorderDistance)
            return;

        int radius = Mathf.CeilToInt(Mathf.Sqrt(size)) + 2;

        List<(float val, Vector2I pos)> candidates = new();

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                Vector2I p = new(start.X + dx, start.Y + dy);

                int mx = MX(p);
                int my = MY(p);

                if (mx < 0 || my < 0 || mx >= sizeX || my >= sizeY)
                    continue;

                if (!IsTileMarked(underType, p)) continue;
                if (distToBorder[mx, my] < minBorderDistance) continue;

                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > radius) continue;

                float nv = noise.GetNoise(p.X, p.Y) - dist * 0.05f;
                candidates.Add((nv, p));
            }
        }

        var selected = TopK(candidates, size);

        foreach (var (_, p) in selected)
        {
            MarkTile(type, p);
            output.Add(p);
            AddToBlobGrid(p);
        }
    }

    private static List<(float value, Vector2I pos)> TopK(
    List<(float value, Vector2I pos)> data, int k)
    {
        // Min-heap donde la prioridad es el valor
        var pq = new PriorityQueue<(float value, Vector2I pos), float>();

        foreach (var item in data)
        {
            // Encolamos usando el ruido como prioridad
            pq.Enqueue(item, item.value);

            // Si supera K elementos, quitamos el menor
            if (pq.Count > k)
                pq.Dequeue();
        }

        // Convertimos la cola en lista
        List<(float value, Vector2I pos)> result = new(pq.Count);
        while (pq.Count > 0)
            result.Add(pq.Dequeue());

        return result;
    }

    private (int gx, int gy) GetGridCell(Vector2I p)
    {
        return (p.X / gridCellSize, p.Y / gridCellSize);
    }

    private void AddToBlobGrid(Vector2I p)
    {
        var cell = GetGridCell(p);
        if (!blobGrid.TryGetValue(cell, out var set))
        {
            set = new HashSet<Vector2I>();
            blobGrid[cell] = set;
        }
        set.Add(p);
    }

    private bool IsNearOtherBlobFast(Vector2I pos, int minDist)
    {
        int range = (minDist / gridCellSize) + 2;
        var (gx, gy) = GetGridCell(pos);

        for (int dx = -range; dx <= range; dx++)
            for (int dy = -range; dy <= range; dy++)
            {
                var key = (gx + dx, gy + dy);

                if (!blobGrid.TryGetValue(key, out var set)) continue;

                foreach (var p in set)
                    if (pos.DistanceSquaredTo(p) <= minDist * minDist)
                        return true;
            }

        return false;
    }

}