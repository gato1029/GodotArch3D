using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Chunks;
public class ChunkManagerBase
{
    

    public event Action<Vector2> OnChunkLoad;
    public event Action<Vector2> OnChunkUnload;

    private ConcurrentQueue<(Vector2, bool)> chunkQueue = new ConcurrentQueue<(Vector2, bool)>();
    private Thread workerThread;
    private bool isRunning = true;

    public Vector2I ViewDistance { get; private set; }
    private Vector2 playerChunkPosCurrent;

    public Vector2I chunkDimencion { get; }
    public Vector2I tileSize { get; }

    private HashSet<Vector2> activeChunks = new HashSet<Vector2>();



    public ChunkManagerBase(Vector2I viewDistance,Vector2I chunkSize, Vector2I tileSize)
    {
        chunkDimencion = chunkSize;
        ViewDistance = viewDistance;
        workerThread = new Thread(ProcessChunkQueue);
        workerThread.Start();
        this.tileSize = tileSize;
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
        var dd = MeshCreator.PixelsToUnits(tileSize.X);
        return new Vector2(
            (int)Math.Floor(worldPos.X / (chunkDimencion.X* dd)), // Suponiendo que cada chunk tiene 8 tiles de ancho
            (int)Math.Floor(worldPos.Y / (chunkDimencion.Y *dd))
        );
    }

    public void Stop()
    {
        isRunning = false;
        workerThread.Join();
    }
}
