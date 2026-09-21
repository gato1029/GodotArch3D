using Godot;
using Godot.Collections;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyTiles;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Core;

public class BlackySubWorld
{
    // Struct unificado para reciclar el ID y el chunk de origen cuando se libera un mapa
    private readonly struct SubWorldSlot
    {
        public readonly int Id;
        public readonly Vector2I OriginChunk;

        public SubWorldSlot(int id, Vector2I originChunk)
        {
            Id = id;
            OriginChunk = originChunk;
        }
    }

    private readonly BlackyWorld _mainWorld;
    private readonly int _chunkSize; // Chunk size fijo del mapa principal

    // Diccionario de sub-mundos activos (ID 0 es siempre el principal)
    public System.Collections.Generic.Dictionary<int, BlackySubWorldData> SubWorlds { get; } = new();

    // Cola para reciclar slots (ID + Chunk de Origen) liberados
    private readonly Queue<SubWorldSlot> _availableSlots = new();
    private int _nextSubWorldId = 1;

    // Configuración del ancho máximo por mapa en tiles (parametrizable)
    public int MaxMapWidth { get; set; } = 8192;

    // Mínimo de chunks de separación (por defecto 3)
    public int MinChunkSeparation { get; set; } = 3;

    // Último chunk de origen asignado horizontalmente
    private Vector2I _lastAssignedOriginChunk = Vector2I.Zero;

    public BlackySubWorld(BlackyWorld world)
    {
        _mainWorld = world;
        _chunkSize = _mainWorld.Config != null ? _mainWorld.Config.ChunkSize : 16; // Fallback por seguridad
        RegisterMainWorld();
    }

    private void RegisterMainWorld()
    {
        BlackyWorldConfig config = _mainWorld.Config;
        if (config == null) return;

        Vector2I originChunk = Vector2I.Zero;
        Vector2I mapSize = config.MapSize;

        BlackySubWorldData mainSubWorldData = new BlackySubWorldData
        {
            Id = 0,
            NameMap = config.Name,
            TypeWorld = config.WorldTypeDetail,
            GlobalOffset = new Vector2(originChunk.X * _chunkSize, originChunk.Y * _chunkSize),
            SizeMap = mapSize,
            TilesByChunk = _chunkSize,
            Chunks = CalculateSubWorldChunks(originChunk, mapSize.X, mapSize.Y, _chunkSize)
        };

        SubWorlds[0] = mainSubWorldData;
        _lastAssignedOriginChunk = originChunk;
    }

    public void LoadMiniWorld(string nameMap, bool isMainWorld=false)
    {
        BlackyWorldLoader loader = new BlackyWorldLoader(nameMap);
        SavedMapData infoSubMap = loader.LoadInfoMap();
        Vector2 offset = Vector2.Zero;
        if (!isMainWorld) // si es el mundo principal el offset sigue siendo 0,0
        {
            int id =LoadSubMap(infoSubMap); // aqui cargamos y calculamos el offset
            offset = SubWorlds[id].GlobalOffset;
        }        
    }


    /// <summary>
    /// Carga un sub-mapa reutilizando un slot libre o calculando su posición de origen hacia la derecha.
    /// </summary>
    public int LoadSubMap(SavedMapData infoSubMap)
    {
        

        if (infoSubMap.Name == null)
        {
            return -1; // Error al cargar la info
        }

        int assignedId;
        Vector2I originChunk;
        Vector2I mapSize = new Vector2I(infoSubMap.TileSizeX, infoSubMap.TileSizeY);

        // 1. Reutilizar slot (ID + Chunk de Origen) si hay alguno disponible en la cola
        if (_availableSlots.Count > 0)
        {
            SubWorldSlot slot = _availableSlots.Dequeue();
            assignedId = slot.Id;
            originChunk = slot.OriginChunk;
        }
        else
        {
            // 2. Asignar nuevo ID y calcular el siguiente chunk de origen hacia la derecha
            assignedId = _nextSubWorldId++;

            int maxChunksInWidth = Mathf.CeilToInt((float)MaxMapWidth / _chunkSize);
            int stepChunks = maxChunksInWidth + MinChunkSeparation;

            if (SubWorlds.Count == 1 && SubWorlds.ContainsKey(0))
            {
                originChunk = new Vector2I(stepChunks, 0);
            }
            else
            {
                originChunk = new Vector2I(_lastAssignedOriginChunk.X + stepChunks, 0);
            }

            _lastAssignedOriginChunk = originChunk;
        }

        // Convertimos el chunk de origen a coordenadas globales (pixels)
        Vector2 globalOffset = new Vector2(originChunk.X * _chunkSize, originChunk.Y * _chunkSize);

        BlackySubWorldData subWorldData = new BlackySubWorldData
        {
            Id = assignedId,
            NameMap = infoSubMap.Name,
            TypeWorld = infoSubMap.MapType,
            OriginChunk = new BlackyChunkCoord(originChunk.X,originChunk.Y),
            GlobalOffset = globalOffset,
            SizeMap = mapSize,
            TilesByChunk = _chunkSize,
            Chunks = CalculateSubWorldChunks(originChunk, mapSize.X, mapSize.Y, _chunkSize)
        };

        SubWorlds[assignedId] = subWorldData;
        // aqui llamar al load de carga
    
        return assignedId;
    }

  
    /// <summary>
    /// Remueve un sub-mapa y libera su slot para futuros mapas.
    /// </summary>
    public bool UnloadSubMap(int id)
    {
        if (id == 0 || !SubWorlds.ContainsKey(id)) return false;

        var subWorld = SubWorlds[id];

        // Derivamos el chunk de origen guardando la posición en chunks
        Vector2I originChunk = new Vector2I(
            Mathf.RoundToInt(subWorld.GlobalOffset.X / _chunkSize),
            Mathf.RoundToInt(subWorld.GlobalOffset.Y / _chunkSize)
        );

        _availableSlots.Enqueue(new SubWorldSlot(subWorld.Id, originChunk));

        return SubWorlds.Remove(id);
    }

    /// <summary>
    /// Calcula los chunks que ocupa el submapa expandiéndose simétricamente a ambos lados desde el chunk de origen.
    /// Soporta valores negativos y positivos por igual.
    /// </summary>
    private List<BlackyChunkCoord> CalculateSubWorldChunks(Vector2I originChunk, int sizeX, int sizeY, int chunkSize)
    {
        List<BlackyChunkCoord> chunks = new();

        // Cantidad total de chunks que abarca el mapa en cada eje
        int totalChunksX = Mathf.CeilToInt((float)sizeX / chunkSize);
        int totalChunksY = Mathf.CeilToInt((float)sizeY / chunkSize);

        // Mitad de chunks para repartir a la izquierda/arriba y derecha/abajo
        int halfX = totalChunksX / 2;
        int halfY = totalChunksY / 2;

        // Rango de chunks simétrico (ej: si total es 4 y origin es 0 -> -2, -1, 0, 1)
        int startX = originChunk.X - halfX;
        int endX = originChunk.X + totalChunksX - halfX - 1;

        int startY = originChunk.Y - halfY;
        int endY = originChunk.Y + totalChunksY - halfY - 1;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                chunks.Add(new BlackyChunkCoord
                {
                    X = x,
                    Y = y
                });
            }
        }

        return chunks;
    }
}

public struct BlackySubWorldData
{
    public int Id { get; set; }
    public string NameMap { get; set; }
    public BlackyWorldTypeDetail TypeWorld { get; set; }
    public Vector2 GlobalOffset { get; set; } // su centro en el mundo
    public BlackyChunkCoord OriginChunk { get; set; }
    public Vector2I SizeMap { get; set; } // tamaño del mundo en tiles
    public int TilesByChunk { get; set; }
    public List<BlackyChunkCoord> Chunks { get; set; }

}
