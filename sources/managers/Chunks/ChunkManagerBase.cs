using Arch.Core;
using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Chunks;

public class ChunkManagerBase
{
    public enum ChunkTaskType
    {
        Load,
        Unload,
        GeneratorPreload
    }

    public event Action<Vector2I> OnChunkLoad;
    public event Action<Vector2I> OnChunkPreLoad;
    public event Action<Vector2I> OnChunkUnload;
    public event Action<Vector2I> OnChunkPreLoadGenerator;
    public event Action OnChunkProcessingCompleted;

    private ConcurrentQueue<(Vector2I, ChunkTaskType)> chunkQueue =
        new ConcurrentQueue<(Vector2I, ChunkTaskType)>();

    private Thread workerThread;
    private bool isRunning = true;

    public Vector2I ViewDistance { get; private set; }

    public int GeneratorPreloadDistance { get; set; } = 2;

    private Vector2I playerChunkPosCurrent;
    private Vector2I playerChunkPosPrevious;

    public Vector2I chunkDimencion { get; }
    public Vector2I tileSize { get; }

    private HashSet<Vector2I> activeChunks = new HashSet<Vector2I>();
    private HashSet<Vector2I> generatorChunks = new HashSet<Vector2I>();

    public Vector2I MinChunk { get; private set; }
    public Vector2I MaxChunk { get; private set; }

    public void SetBounds(Vector2I min, Vector2I max)
    {
        MinChunk = min;
        MaxChunk = max;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsInsideBounds(Vector2I chunk)
    {
        return chunk.X >= MinChunk.X &&
               chunk.X <= MaxChunk.X &&
               chunk.Y >= MinChunk.Y &&
               chunk.Y <= MaxChunk.Y;
    }
    public ChunkManagerBase(Vector2I totalViewChunks, Vector2I chunkSize, Vector2I tileSize)
    {
        chunkDimencion = chunkSize;

        ViewDistance = new Vector2I(
            totalViewChunks.X / 2,
            totalViewChunks.Y / 2
        );

        workerThread = new Thread(ProcessChunkQueue);
        workerThread.Start();

        this.tileSize = tileSize;
    }
    private bool initialized = false;
    public void UpdatePlayerPosition(Vector2 playerPosition)
    {
        Vector2I currentChunkPos = WorldToChunkCoords(playerPosition);

        if (!initialized)
        {
            initialized = true;

            playerChunkPosCurrent = currentChunkPos;
            playerChunkPosPrevious = currentChunkPos;

            UpdateChunksFull();
            return;
        }

        if (currentChunkPos != playerChunkPosCurrent)
        {
            playerChunkPosPrevious = playerChunkPosCurrent;
            playerChunkPosCurrent = currentChunkPos;

            UpdateChunksOptimized();
        }
    }
    public IReadOnlyCollection<Vector2I> GetActiveChunks()
    {
        // Devuelve una copia para evitar que el HashSet interno sea modificado desde afuera
        return activeChunks.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Vector2I> GetGeneratorChunks()
    {
        return generatorChunks.ToList().AsReadOnly();
    }
    public IReadOnlyCollection<Vector2I> GetAllTrackedChunks()
    {
        HashSet<Vector2I> all = new HashSet<Vector2I>(activeChunks);
        all.UnionWith(generatorChunks);
        return all.ToList().AsReadOnly();
    }
    public void ForcedUpdateChunks(Vector2I position)
    {
        playerChunkPosPrevious = position;
        playerChunkPosCurrent = position;

        UpdateChunksFull();
    }

    // ================================
    // OPTIMIZED UPDATE
    // ================================

    private void UpdateChunksOptimized()
    {
        int extX = ViewDistance.X + GeneratorPreloadDistance;
        int extY = ViewDistance.Y + GeneratorPreloadDistance;

        HashSet<Vector2I> newVisible = new HashSet<Vector2I>();
        HashSet<Vector2I> newGenerator = new HashSet<Vector2I>();

        for (int x = -extX; x <= extX; x++)
        {
            for (int y = -extY; y <= extY; y++)
            {
                Vector2I chunk = playerChunkPosCurrent + new Vector2I(x, y);
                if (!IsInsideBounds(chunk))
                    continue;
                int dx = Math.Abs(x);
                int dy = Math.Abs(y);

                bool insideVisible =
                    dx <= ViewDistance.X &&
                    dy <= ViewDistance.Y;

                if (insideVisible)
                    newVisible.Add(chunk);
                else
                    newGenerator.Add(chunk);
            }
        }

        // ======================
        // CARGAR VISIBLES NUEVOS
        // ======================

        foreach (var chunk in newVisible)
        {
            if (!activeChunks.Contains(chunk))
            {
                activeChunks.Add(chunk);
                RequestLoadChunk(chunk);
            }
        }

        // ======================
        // DESCARGAR VISIBLES
        // ======================

        foreach (var chunk in new List<Vector2I>(activeChunks))
        {
            if (!newVisible.Contains(chunk))
            {
                activeChunks.Remove(chunk);
                RequestUnloadChunk(chunk);
            }
        }

        // ======================
        // GENERADOR
        // ======================

        foreach (var chunk in newGenerator)
        {
            if (!generatorChunks.Contains(chunk))
            {
                generatorChunks.Add(chunk);
                RequestGeneratorPreload(chunk);
            }
        }

        foreach (var chunk in new List<Vector2I>(generatorChunks))
        {
            if (!newGenerator.Contains(chunk))
            {
                generatorChunks.Remove(chunk);
            }
        }
    }

    // ================================
    // FULL UPDATE (inicio o teleport)
    // ================================

    private void UpdateChunksFull()
    {
        int extX = ViewDistance.X + GeneratorPreloadDistance;
        int extY = ViewDistance.Y + GeneratorPreloadDistance;

        activeChunks.Clear();
        generatorChunks.Clear();

        for (int x = -extX; x <= extX; x++)
        {
            for (int y = -extY; y <= extY; y++)
            {
                Vector2I chunk = playerChunkPosCurrent + new Vector2I(x, y);
                if (!IsInsideBounds(chunk))
                    continue;
                bool insideVisible =
                    Math.Abs(x) <= ViewDistance.X &&
                    Math.Abs(y) <= ViewDistance.Y;

                if (insideVisible)
                {
                    activeChunks.Add(chunk);
                    RequestLoadChunk(chunk);
                }
                else
                {
                    generatorChunks.Add(chunk);
                    RequestGeneratorPreload(chunk);
                }
            }
        }
    }

    // ================================
    // CHUNK HANDLING
    // ================================

    private void HandleChunkAddition(Vector2I chunk)
    {
        Vector2I diff = chunk - playerChunkPosCurrent;

        bool insideVisible =
            Math.Abs(diff.X) <= ViewDistance.X &&
            Math.Abs(diff.Y) <= ViewDistance.Y;

        if (insideVisible)
        {
            if (!activeChunks.Contains(chunk))
            {
                activeChunks.Add(chunk);
                RequestLoadChunk(chunk);
            }
        }
        else
        {
            if (!generatorChunks.Contains(chunk))
            {
                generatorChunks.Add(chunk);
                RequestGeneratorPreload(chunk);
            }
        }
    }

    private void HandleChunkRemoval(Vector2I chunk)
    {
        if (activeChunks.Contains(chunk))
        {
            activeChunks.Remove(chunk);
            RequestUnloadChunk(chunk);
        }

        if (generatorChunks.Contains(chunk))
        {
            generatorChunks.Remove(chunk);
        }
    }

    // ================================
    // QUEUE REQUESTS
    // ================================

    public void RequestLoadChunk(Vector2I chunkPos)
    {
        chunkQueue.Enqueue((chunkPos, ChunkTaskType.Load));
    }

    public void RequestUnloadChunk(Vector2I chunkPos)
    {
        chunkQueue.Enqueue((chunkPos, ChunkTaskType.Unload));
    }

    public void RequestGeneratorPreload(Vector2I chunkPos)
    {
        chunkQueue.Enqueue((chunkPos, ChunkTaskType.GeneratorPreload));
    }

    // ================================
    // WORKER THREAD
    // ================================

    private void ProcessChunkQueue()
    {
        bool hasPendingNotification = false;

        while (isRunning)
        {
            if (chunkQueue.TryDequeue(out var chunkTask))
            {
                Vector2I chunkPos = chunkTask.Item1;
                ChunkTaskType taskType = chunkTask.Item2;

                switch (taskType)
                {
                    case ChunkTaskType.GeneratorPreload:
                        OnChunkPreLoadGenerator?.Invoke(chunkPos);
                        break;

                    case ChunkTaskType.Load:
                        OnChunkPreLoad?.Invoke(chunkPos);
                        OnChunkLoad?.Invoke(chunkPos);
                        break;

                    case ChunkTaskType.Unload:
                        OnChunkUnload?.Invoke(chunkPos);
                        break;
                }

                hasPendingNotification = true;
            }
            else
            {
                if (hasPendingNotification)
                {
                    hasPendingNotification = false;

                    MainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        OnChunkProcessingCompleted?.Invoke();
                    });
                }

                Thread.Sleep(10);
            }
        }
    }

    // ================================
    // UTILS
    // ================================

    private Vector2I WorldToChunkCoords(Vector2 worldPos)
    {
        var dd = MeshCreator.PixelsToUnits(tileSize.X);

        return new Vector2I(
            (int)Math.Floor(worldPos.X / (chunkDimencion.X * dd)),
            (int)Math.Floor(worldPos.Y / (chunkDimencion.Y * dd))
        );
    }

    public List<Vector2I> GetVisibleChunks(Vector2 position)
    {
        var temp = WorldToChunkCoords(position);
        Vector2I centerChunk = new Vector2I(temp.X, temp.Y);

        List<Vector2I> visibleChunks = new();

        for (int x = -ViewDistance.X; x <= ViewDistance.X; x++)
        {
            for (int y = -ViewDistance.Y; y <= ViewDistance.Y; y++)
            {
                visibleChunks.Add(centerChunk + new Vector2I(x, y));
            }
        }

        return visibleChunks;
    }
    public List<Vector2I> GetGeneratorChunks(Vector2 position)
    {
        var temp = WorldToChunkCoords(position);
        Vector2I centerChunk = new Vector2I(temp.X, temp.Y);

        List<Vector2I> chunks = new();

        int extX = ViewDistance.X + GeneratorPreloadDistance;
        int extY = ViewDistance.Y + GeneratorPreloadDistance;

        for (int x = -extX; x <= extX; x++)
        {
            for (int y = -extY; y <= extY; y++)
            {
                bool insideVisible =
                    Math.Abs(x) <= ViewDistance.X &&
                    Math.Abs(y) <= ViewDistance.Y;

                if (!insideVisible)
                {
                    chunks.Add(centerChunk + new Vector2I(x, y));
                }
            }
        }

        return chunks;
    }
    public List<Vector2I> GetExtendedChunksWithContext(Vector2 position, int extraBorder = 1)
    {
        var temp = WorldToChunkCoords(position);
        Vector2I centerChunk = new Vector2I(temp.X, temp.Y);

        List<Vector2I> chunks = new();

        int extX = ViewDistance.X + GeneratorPreloadDistance + extraBorder;
        int extY = ViewDistance.Y + GeneratorPreloadDistance + extraBorder;

        for (int x = -extX; x <= extX; x++)
        {
            for (int y = -extY; y <= extY; y++)
            {
                chunks.Add(centerChunk + new Vector2I(x, y));
            }
        }

        return chunks;
    }

    public List<Vector2I> GetExtendedChunks(Vector2 position)
    {
        var temp = WorldToChunkCoords(position);
        Vector2I centerChunk = new Vector2I(temp.X, temp.Y);

        List<Vector2I> chunks = new();

        int extX = ViewDistance.X + GeneratorPreloadDistance;
        int extY = ViewDistance.Y + GeneratorPreloadDistance;

        for (int x = -extX; x <= extX; x++)
        {
            for (int y = -extY; y <= extY; y++)
            {
                var chunk = centerChunk + new Vector2I(x, y);   
                if (!IsInsideBounds(chunk))
                    continue;
                chunks.Add(chunk);                

            }
        }

        return chunks;
    }

    public Vector2I TilePositionInChunk(Vector2I chunkPositon, Vector2I TilePositionGlobal)
    {
        Vector2I basePosition = chunkPositon * chunkDimencion;
        Vector2I local = TilePositionGlobal - basePosition;

        // Asegurar que el resultado sea siempre positivo con módulo corregido
        int x = ((local.X % chunkDimencion.X) + chunkDimencion.X) % chunkDimencion.X;
        int y = ((local.Y % chunkDimencion.Y) + chunkDimencion.Y) % chunkDimencion.Y;

        return new Vector2I(x, y);
    }
    public Vector2I TilePositionGlobal(Vector2I chunkPosition, Vector2I tilePositionLocal)
    {
        // La posición base del chunk en coordenadas globales
        Vector2I basePosition = chunkPosition * chunkDimencion;

        // La posición global es simplemente la base más el offset local
        return basePosition + tilePositionLocal;
    }
    private int FloorDiv(int a, int b)
    {
        return (a >= 0) ? (a / b) : ((a - b + 1) / b);
    }
    public Vector2I ChunkPosition(Vector2I TilePositionGlobal)
    {
        //return new Vector2I(
        //    (int)Mathf.Floor(TilePositionGlobal.X / chunkDimencion.X),
        //    (int)MathF.Floor(TilePositionGlobal.Y / chunkDimencion.Y));
        return new Vector2I(
        FloorDiv(TilePositionGlobal.X, chunkDimencion.X),
        FloorDiv(TilePositionGlobal.Y, chunkDimencion.Y)
);
    }

    public void Stop()
    {
        isRunning = false;
        workerThread.Join();
    }
}