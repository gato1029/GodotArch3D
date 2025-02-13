using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Chunks;
public class ChunkManager
{
    public static ChunkManager Instance { get; private set; }

    public event Action<Vector2> OnChunkLoad;
    public event Action<Vector2> OnChunkUnload;

    private ConcurrentQueue<(Vector2, bool)> chunkQueue = new ConcurrentQueue<(Vector2, bool)>();
    private Thread workerThread;
    private bool isRunning = true;

    public Vector2I ViewDistance { get; private set; }
    private Vector2 playerChunkPosCurrent;
    private Vector2 chunkDimencion;
    private HashSet<Vector2> activeChunks = new HashSet<Vector2>();

    public static void Initialize(Vector2I viewDistance)
    {
        if (Instance == null)
        {
            Instance = new ChunkManager(viewDistance);
        }
    }

    private ChunkManager(Vector2I viewDistance)
    {
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        ViewDistance = viewDistance;
        workerThread = new Thread(ProcessChunkQueue);
        workerThread.Start();
    }

    public void UpdatePlayerPosition(Vector2 playerPosition)
    {
        Vector2 currentChunkPos = WorldToChunkCoords(playerPosition);

        if (currentChunkPos != playerChunkPosCurrent)
        {
            playerChunkPosCurrent = currentChunkPos;
            UpdateChunks();
        }
    }

    public void ForcedUpdateChunks(Vector2 Position)
    {
        playerChunkPosCurrent = Position;
        UpdateChunks();
    }

    private void UpdateChunks()
    {
        HashSet<Vector2> requiredChunks = new HashSet<Vector2>();

        for (int x = -ViewDistance.X; x <= ViewDistance.X; x++)
        {
            for (int y = -ViewDistance.Y; y <= ViewDistance.Y; y++)
            {
                Vector2 chunkPos = playerChunkPosCurrent + new Vector2(x, y);
                requiredChunks.Add(chunkPos);

                if (!activeChunks.Contains(chunkPos))
                {
                    RequestLoadChunk(chunkPos);
                    activeChunks.Add(chunkPos);
                }
            }
        }

        foreach (var chunkPos in new List<Vector2>(activeChunks))
        {
            if (!requiredChunks.Contains(chunkPos))
            {
                RequestUnloadChunk(chunkPos);
                activeChunks.Remove(chunkPos);
            }
        }
    }

    public void RequestLoadChunk(Vector2 chunkPos)
    {
        chunkQueue.Enqueue((chunkPos, true));
    }

    public void RequestUnloadChunk(Vector2 chunkPos)
    {
        chunkQueue.Enqueue((chunkPos, false));
    }

    private void ProcessChunkQueue()
    {
        while (isRunning)
        {
            if (chunkQueue.TryDequeue(out var chunkTask))
            {
                Vector2 chunkPos = chunkTask.Item1;
                bool isLoad = chunkTask.Item2;

                if (isLoad)
                    OnChunkLoad?.Invoke(chunkPos);
                else
                    OnChunkUnload?.Invoke(chunkPos);
            }
            else
            {
                Thread.Sleep(10); // Pequenia pausa para evitar consumo alto de CPU
            }
        }
    }

    private Vector2 WorldToChunkCoords(Vector2 worldPos)
    {
        return new Vector2(
            (int)Math.Floor(worldPos.X / chunkDimencion.X), // Suponiendo que cada chunk tiene 8 tiles de ancho
            (int)Math.Floor(worldPos.Y / chunkDimencion.Y)
        );
    }

    public void Stop()
    {
        isRunning = false;
        workerThread.Join();
    }
}
