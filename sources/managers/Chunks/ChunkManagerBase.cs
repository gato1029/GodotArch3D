using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Chunks;

public class ChunkManagerBase
{
    public enum ChunkTaskType { DataPreload, Load, Unload, DataUnload }

    public event Action<Vector2I> OnChunkLoad;
    public event Action<Vector2I> OnChunkUnload;
    public event Action<Vector2I> OnChunkDataPreload;
    public event Action<Vector2I> OnChunkDataUnload;
    public event Action OnChunkProcessingCompleted;

    // Diccionario de estado que almacena el tipo de tarea exacto y la generación a la que pertenece
    private readonly ConcurrentDictionary<Vector2I, (ChunkTaskType Type, int Generation)> _chunkStates = new();

    // Cola que incluye la posición, el tipo y la generación
    private readonly ConcurrentQueue<(Vector2I Pos, ChunkTaskType Type, int Generation)> _chunkQueue = new();

    private Thread _workerThread;
    private bool _isRunning = true;
    private int _currentGeneration = 0; // Incrementa en cada Teleport / Carga inicial

    public bool UseParallelProcessing { get; set; } = false;
    public Vector2I ViewDistance { get; private set; }
    public int DataPreloadDistance { get; set; } = 2;
    public Vector2I chunkDimencion { get; }
    public Vector2I tileSize { get; }

    private Vector2I _playerChunkPosCurrent;
    private HashSet<Vector2I> _activeChunks = new();
    private HashSet<Vector2I> _dataChunks = new();

    public Vector2I MinChunk { get; private set; }
    public Vector2I MaxChunk { get; private set; }

    public ChunkManagerBase(Vector2I totalViewChunks, Vector2I chunkSize, Vector2I tileSize)
    {
        chunkDimencion = chunkSize;
        this.tileSize = tileSize;
        ViewDistance = new Vector2I(totalViewChunks.X / 2, totalViewChunks.Y / 2);

        _workerThread = new Thread(ProcessChunkQueue) { IsBackground = true };
        _workerThread.Start();
    }

    public void SetBounds(Vector2I min, Vector2I max)
    {
        MinChunk = min;
        MaxChunk = max;
    }

    public IReadOnlyCollection<Vector2I> GetActiveChunks() => _activeChunks.ToList().AsReadOnly();

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

    public IReadOnlyCollection<Vector2I> GetGeneratorChunks() => _dataChunks.ToList().AsReadOnly();

    public IReadOnlyCollection<Vector2I> GetAllTrackedChunks()
    {
        HashSet<Vector2I> all = new HashSet<Vector2I>(_activeChunks);
        all.UnionWith(_dataChunks);
        return all.ToList().AsReadOnly();
    }

    public void UpdatePlayerPosition(Vector2 playerPosition)
    {
        Vector2I currentChunkPos = WorldToChunkCoords(playerPosition);
        if (currentChunkPos == _playerChunkPosCurrent && _activeChunks.Count > 0) return;

        _playerChunkPosCurrent = currentChunkPos;
        UpdateChunks(); // Movimiento normal del jugador
    }

    public void Teleport(Vector2 newPosition)
    {
        // 1. Invalidamos instantáneamente todo lo anterior incrementando la generación
        Interlocked.Increment(ref _currentGeneration);

        // 2. Limpiamos cola y estados previos
        while (_chunkQueue.TryDequeue(out _)) { }
        _chunkStates.Clear();

        _activeChunks.Clear();
        _dataChunks.Clear();

        // 3. Forzamos la recarga total para la nueva posición
        _playerChunkPosCurrent = WorldToChunkCoords(newPosition);
        UpdateChunksFullReload();
    }

    private void UpdateChunksFullReload()
    {
        int extX = ViewDistance.X + DataPreloadDistance;
        int extY = ViewDistance.Y + DataPreloadDistance;

        HashSet<Vector2I> targetVisible = new();
        HashSet<Vector2I> targetData = new();

        for (int x = -extX; x <= extX; x++)
        {
            for (int y = -extY; y <= extY; y++)
            {
                Vector2I chunk = _playerChunkPosCurrent + new Vector2I(x, y);
                if (chunk.X < MinChunk.X || chunk.X > MaxChunk.X || chunk.Y < MinChunk.Y || chunk.Y > MaxChunk.Y) continue;

                if (Math.Abs(x) <= ViewDistance.X && Math.Abs(y) <= ViewDistance.Y)
                    targetVisible.Add(chunk);
                else
                    targetData.Add(chunk);
            }
        }

        int gen = _currentGeneration;

        // FASE 1: Encolamos y registramos las precargas de datos
        foreach (var chunk in targetData)
        {
            EnqueueTask(chunk, ChunkTaskType.DataPreload, gen);
        }

        foreach (var chunk in targetVisible)
        {
            // OJO: Aquí registramos temporalmente DataPreload en el diccionario para la fase 1
            EnqueueTask(chunk, ChunkTaskType.DataPreload, gen);
        }

        // FASE 2: Inmediatamente después, actualizamos el diccionario al estado final "Load" 
        // y encolamos la tarea Load para los visibles. 
        // Como el hilo procesa la cola en orden FIFO, primero sacará el DataPreload y luego el Load.
        foreach (var chunk in targetVisible)
        {
            EnqueueTask(chunk, ChunkTaskType.Load, gen);
        }

        _activeChunks = targetVisible;
        _dataChunks = targetData;
    }

    private void UpdateChunks()
    {
        int extX = ViewDistance.X + DataPreloadDistance;
        int extY = ViewDistance.Y + DataPreloadDistance;

        HashSet<Vector2I> targetVisible = new();
        HashSet<Vector2I> targetData = new();

        for (int x = -extX; x <= extX; x++)
        {
            for (int y = -extY; y <= extY; y++)
            {
                Vector2I chunk = _playerChunkPosCurrent + new Vector2I(x, y);
                if (chunk.X < MinChunk.X || chunk.X > MaxChunk.X || chunk.Y < MinChunk.Y || chunk.Y > MaxChunk.Y) continue;

                if (Math.Abs(x) <= ViewDistance.X && Math.Abs(y) <= ViewDistance.Y)
                    targetVisible.Add(chunk);
                else
                    targetData.Add(chunk);
            }
        }

        int gen = _currentGeneration;

        // Movimiento normal: Cargas y descargas dinámicas
        foreach (var chunk in targetData.Where(c => !_dataChunks.Contains(c)))
            EnqueueTask(chunk, ChunkTaskType.DataPreload, gen);

        foreach (var chunk in targetVisible.Where(c => !_activeChunks.Contains(c)))
            EnqueueTask(chunk, ChunkTaskType.Load, gen);

        foreach (var chunk in _activeChunks.Where(c => !targetVisible.Contains(c)))
            EnqueueTask(chunk, ChunkTaskType.Unload, gen);

        foreach (var chunk in _dataChunks.Where(c => !targetData.Contains(c) && !targetVisible.Contains(c)))
            EnqueueTask(chunk, ChunkTaskType.DataUnload, gen);

        _activeChunks = targetVisible;
        _dataChunks = targetData;
    }

    private void EnqueueTask(Vector2I pos, ChunkTaskType type, int generation)
    {
        // Actualizamos el diccionario con el estado exacto que este chunk DEBE tener al final
        _chunkStates[pos] = (type, generation);
        _chunkQueue.Enqueue((pos, type, generation));
    }

    private void ProcessChunkQueue()
    {
        while (_isRunning)
        {
            if (_chunkQueue.TryDequeue(out var task))
            {
                // 1. Protección contra Teleport: Si la generación cambió, se descarta por completo
                if (task.Generation != _currentGeneration)
                    continue;

                // 2. PROTECCIÓN CONTRA MOVIMIENTO RÁPIDO (Validación de relevancia de estado):
                // Si el jugador se movió tan rápido que este chunk cambió de estado en el diccionario
                // (por ejemplo, pasó de Load a Unload porque el jugador ya se alejó), descartamos la tarea vieja.
                if (_chunkStates.TryGetValue(task.Pos, out var currentState))
                {
                    if (currentState.Generation == task.Generation && currentState.Type != task.Type)
                    {
                        // Excepción controlada para FullReload: Permitimos que pase DataPreload si el estado actual es Load,
                        // ya que forman parte de la misma secuencia lógica de carga inicial.
                        bool isAllowedSequence = (task.Type == ChunkTaskType.DataPreload && currentState.Type == ChunkTaskType.Load);

                        if (!isAllowedSequence)
                            continue; // Descartar tarea obsoleta por movimiento rápido
                    }
                }

                if (UseParallelProcessing) Task.Run(() => ExecuteTask(task.Pos, task.Type));
                else ExecuteTask(task.Pos, task.Type);
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }

    private void ExecuteTask(Vector2I pos, ChunkTaskType type)
    {
        int maxDistX = ViewDistance.X + DataPreloadDistance + 1;
        int maxDistY = ViewDistance.Y + DataPreloadDistance + 1;

        if (Math.Abs(pos.X - _playerChunkPosCurrent.X) > maxDistX ||
            Math.Abs(pos.Y - _playerChunkPosCurrent.Y) > maxDistY)
        {
            return;
        }

        switch (type)
        {
            case ChunkTaskType.DataPreload: OnChunkDataPreload?.Invoke(pos); break;
            case ChunkTaskType.Load: OnChunkLoad?.Invoke(pos); break;
            case ChunkTaskType.Unload: OnChunkUnload?.Invoke(pos); break;
            case ChunkTaskType.DataUnload: OnChunkDataUnload?.Invoke(pos); break;
        }
    }

    private Vector2I WorldToChunkCoords(Vector2 worldPos)
    {
        float dd = MeshCreator.PixelsToUnits(tileSize.X);
        float chunkWidth = chunkDimencion.X * dd;
        float chunkHeight = chunkDimencion.Y * dd;

        return new Vector2I(
            (int)MathF.Floor(worldPos.X / chunkWidth),
            (int)MathF.Floor(worldPos.Y / chunkHeight)
        );
    }

    public Vector2I TilePositionInChunk(Vector2I chunkPositon, Vector2I TilePositionGlobal)
    {
        Vector2I basePosition = chunkPositon * chunkDimencion;
        Vector2I local = TilePositionGlobal - basePosition;

        int x = ((local.X % chunkDimencion.X) + chunkDimencion.X) % chunkDimencion.X;
        int y = ((local.Y % chunkDimencion.Y) + chunkDimencion.Y) % chunkDimencion.Y;

        return new Vector2I(x, y);
    }

    public Vector2I TilePositionGlobal(Vector2I chunkPosition, Vector2I tilePositionLocal)
    {
        Vector2I basePosition = chunkPosition * chunkDimencion;
        return basePosition + tilePositionLocal;
    }

    private int FloorDiv(int a, int b)
    {
        return (a >= 0) ? (a / b) : ((a - b + 1) / b);
    }

    public Vector2I ChunkPosition(Vector2I TilePositionGlobal)
    {
        return new Vector2I(
            FloorDiv(TilePositionGlobal.X, chunkDimencion.X),
            FloorDiv(TilePositionGlobal.Y, chunkDimencion.Y)
        );
    }

    public void Stop()
    {
        _isRunning = false;
        _workerThread.Join();
    }
}
