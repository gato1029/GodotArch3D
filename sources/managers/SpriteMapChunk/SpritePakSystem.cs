using Godot; // asumes Godot.Vector2 y Vector3I disponibles
using GodotEcsArch.sources.managers.Tilemap;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileAccess = System.IO.FileAccess;

#region HELPERS / DTOs

[MessagePackObject]
public struct ChunkPakEntry
{
    [Key(0)]
    public long Offset;
    [Key(1)]
    public int Size;
}

[MessagePackObject]
public class PackIndexDto
{
    [Key(0)]
    public Dictionary<ChunkId, ChunkPakEntry> Index { get; set; } = new();
}

[MessagePackObject]
public class LayerIndex
{
    [Key(0)]
    public Dictionary<int, string> Layers { get; set; } = new();
}

[MessagePackObject]
public class PakMetadata
{
    [Key(0)]
    public PackIndexDto ChunkIndex { get; set; } = new();

    [Key(1)]
    public LayerIndex LayerIndex { get; set; } = new();
}

/// <summary>
/// Key para index — enteros, estable y serializable por MessagePack.
/// Implementa equality / hashcode para usarlo como clave de Dictionary.
/// </summary>
[MessagePackObject]
public readonly struct ChunkId : IEquatable<ChunkId>
{
    [Key(0)] public int X { get; }
    [Key(1)] public int Y { get; }
    [Key(2)] public int Layer { get; }

    [SerializationConstructor]
    public ChunkId(int x, int y, int layer) { X = x; Y = y; Layer = layer; }

    public static ChunkId FromVector2(Vector2 v, int layer) => new((int)v.X, (int)v.Y, layer);
    public bool Equals(ChunkId other) => X == other.X && Y == other.Y && Layer == other.Layer;
    public override bool Equals(object obj) => obj is ChunkId c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(X, Y, Layer);
    public override string ToString() => $"ChunkId({X},{Y},L{Layer})";
}

#endregion

#region CHUNK DATA (tu clase)
[MessagePackObject]
public class ChunkDataSerialized
{
    [Key(0)]
    public DataRender[][] renderTiles;
}
#endregion

public class SpritePakSystem
{
    // header layout: [4 bytes magic "PAK1"][4 bytes chunkCount][8 bytes metadataOffset]
    private const string Magic = "PAK1";
    private const int HeaderMagicSize = 4;
    private const int HeaderCountSize = 4;
    private const int HeaderOffsetSize = 8;
    private const int HeaderTotalSize = HeaderMagicSize + HeaderCountSize + HeaderOffsetSize;

    // Caches y locks por archivo .pak
    private readonly ConcurrentDictionary<string, PakMetadata> _metadataCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _fileLocks = new(StringComparer.OrdinalIgnoreCase);

    // Obtener (o crear) el lock para un pakPath
    private ReaderWriterLockSlim GetFileLock(string pakPath)
    {
        return _fileLocks.GetOrAdd(pakPath, _ => new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion));
    }

    // ----------------------------
    // Crear archivo PAK vacío
    // ----------------------------
    public void CrearPak(string pakPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(pakPath) ?? ".");

        using var fs = new FileStream(pakPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        // Magic
        fs.Write(System.Text.Encoding.ASCII.GetBytes(Magic));
        // chunk count
        fs.Write(BitConverter.GetBytes(0));
        // metadata offset
        fs.Write(BitConverter.GetBytes((long)0));
        fs.Flush();

        // Inicializar cache vacío
        _metadataCache[pakPath] = new PakMetadata
        {
            ChunkIndex = new PackIndexDto(),
            LayerIndex = new LayerIndex()
        };
    }

    // ----------------------------
    // Serializar chunk
    // ----------------------------
    public byte[] SerializarChunk(ChunkDataSerialized chunk)
    {
        return MessagePackSerializer.Serialize(chunk);
    }

    // ----------------------------
    // Escribir bytes al final del archivo (thread-safe por pak)
    // ----------------------------
    public ChunkPakEntry EscribirChunkAlFinal(string pakPath, byte[] bytes)
    {
        //var rwlock = GetFileLock(pakPath);
        //rwlock.EnterWriteLock();
        //try
        //{
            using var fs = new FileStream(pakPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            long offset = fs.Length;
            fs.Position = offset;
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();

            return new ChunkPakEntry
            {
                Offset = offset,
                Size = bytes.Length
            };
        //}
        //finally
        //{
        //    rwlock.ExitWriteLock();
        //}
    }

    // ----------------------------
    // Sobrescribir chunk existente (solo si cabe)
    // ----------------------------
    public void SobrescribirChunk(string pakPath, ChunkPakEntry entry, byte[] bytes)
    {
        //var rwlock = GetFileLock(pakPath);
        //rwlock.EnterWriteLock();
        //try
        //{
            using var fs = new FileStream(pakPath, FileMode.Open, FileAccess.Write, FileShare.Read);
            fs.Position = entry.Offset;
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
        //}
        //finally
        //{
        //    rwlock.ExitWriteLock();
        //}
    }

    // ----------------------------
    // Guardar chunk incremental (usa cache de metadata)
    // ----------------------------
    public void GuardarChunkIncremental(
        string pakPath,
        Vector2 pos,
        int layer,
        byte[] bytes)
    {
        var id = ChunkId.FromVector2(pos, layer);

        // load metadata (from cache or disk)
        var meta = CargarMetadatosCached(pakPath);

        var index = meta.ChunkIndex.Index;
        var rwlock = GetFileLock(pakPath);

        rwlock.EnterWriteLock();
        try
        {
            // ¿Ya existe?
            if (index.TryGetValue(id, out var entry))
            {
                // ¿Cabe en el mismo espacio?
                if (bytes.Length <= entry.Size)
                {
                    SobrescribirChunk(pakPath, entry, bytes);
                    return;
                }

                // No cabe → escribir al final
                var newEntry = EscribirChunkAlFinal(pakPath, bytes);
                index[id] = newEntry;
                return;
            }

            // Nuevo chunk
            var entryNew = EscribirChunkAlFinal(pakPath, bytes);
            index[id] = entryNew;
        }
        finally
        {
            rwlock.ExitWriteLock();
        }
    }


    // ----------------------------
    // Guardar metadatos (index + capas) — atómico y thread-safe
    // ----------------------------
    public void GuardarMetadatos(string pakPath)
    {
        if (!_metadataCache.TryGetValue(pakPath, out var meta))
            throw new InvalidOperationException("No hay metadata cargada en cache para este pak. Usa CargarMetadatosCached primero.");

        var rwlock = GetFileLock(pakPath);
        rwlock.EnterWriteLock();
        try
        {
            byte[] metaBytes = MessagePackSerializer.Serialize(meta);

            using var fs = new FileStream(pakPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            long offset = fs.Length;
            fs.Position = offset;
            fs.Write(metaBytes, 0, metaBytes.Length);
            fs.Flush();

            // Actualizar header (chunk count + offset)
            fs.Position = HeaderMagicSize; // 4
            fs.Write(BitConverter.GetBytes(meta.ChunkIndex.Index.Count));
            fs.Write(BitConverter.GetBytes(offset));
            fs.Flush();
        }
        finally
        {
            rwlock.ExitWriteLock();
        }
    }

    // ----------------------------
    // Cargar metadatos desde disco (sin usar cache)
    // ----------------------------
    public PakMetadata CargarMetadatos(string pakPath)
    {
        // Validaciones básicas
        if (!File.Exists(pakPath))
            return new PakMetadata { ChunkIndex = new PackIndexDto(), LayerIndex = new LayerIndex() };

        using var fs = new FileStream(pakPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        // leer header
        if (fs.Length < HeaderTotalSize)
            return new PakMetadata { ChunkIndex = new PackIndexDto(), LayerIndex = new LayerIndex() };

        // Magic
        var magicBytes = new byte[HeaderMagicSize];
        fs.ReadExactly(magicBytes, 0, HeaderMagicSize);
        string key = System.Text.Encoding.ASCII.GetString(magicBytes);
        if (System.Text.Encoding.ASCII.GetString(magicBytes) != Magic)
            throw new Exception("Archivo PAK inválido (magic mismatch)");

        // chunkCount (no lo usamos realmente aquí, pero lo leemos)
        var countBytes = new byte[HeaderCountSize];
        fs.ReadExactly(countBytes, 0, HeaderCountSize);
        int chunkCount = BitConverter.ToInt32(countBytes);

        // metadata offset
        var offsetBytes = new byte[HeaderOffsetSize];
        fs.ReadExactly(offsetBytes, 0, HeaderOffsetSize);
        long metaOffset = BitConverter.ToInt64(offsetBytes);

        if (metaOffset == 0 || metaOffset >= fs.Length)
        {
            return new PakMetadata { ChunkIndex = new PackIndexDto(), LayerIndex = new LayerIndex() };
        }

        fs.Position = metaOffset;
        var buffer = new byte[fs.Length - metaOffset];
        fs.ReadExactly(buffer, 0, buffer.Length);

        return MessagePackSerializer.Deserialize<PakMetadata>(buffer);
    }

    // ----------------------------
    // Cargar metadatos y guardarlos en cache (thread-safe)
    // ----------------------------
    public PakMetadata CargarMetadatosCached(string pakPath)
    {
        // Si ya está en cache, devolverlo (lectura concurrente permitida)
        if (_metadataCache.TryGetValue(pakPath, out var cached))
            return cached;

        var rwlock = GetFileLock(pakPath);
        rwlock.EnterWriteLock(); // bloqueo para inicializar cache y prevenir races al crear entry
        try
        {
            // chequeo doble por si otro hilo ya lo creó
            if (_metadataCache.TryGetValue(pakPath, out var already))
                return already;

            var meta = CargarMetadatos(pakPath);
            _metadataCache[pakPath] = meta;
            return meta;
        }
        finally
        {
            rwlock.ExitWriteLock();
        }
    }

    // ----------------------------
    // Cargar solo un chunk — usa metadata cache (lecturas concurrentes)
    // ----------------------------
    public ChunkDataSerialized CargarChunk(string pakPath, Vector2 pos, int layer)
    {
        var meta = CargarMetadatosCached(pakPath);
        var id = ChunkId.FromVector2(pos, layer);

        if (!meta.ChunkIndex.Index.TryGetValue(id, out var entry))
            return null;

        // Para lectura no necesitamos bloquear la metadata, pero necesitamos abrir el archivo en modo que permita escrituras simultáneas.
        using var fs = new FileStream(pakPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var buffer = new byte[entry.Size];
        fs.Position = entry.Offset;
        fs.ReadExactly(buffer, 0, buffer.Length);

        return MessagePackSerializer.Deserialize<ChunkDataSerialized>(buffer);
    }

    // ----------------------------
    // Forzar recarga de metadata desde disco (invalidar cache)
    // ----------------------------
    public PakMetadata RecargarMetadatosDesdeDisco(string pakPath)
    {
        var rwlock = GetFileLock(pakPath);
        rwlock.EnterWriteLock();
        try
        {
            var meta = CargarMetadatos(pakPath);
            _metadataCache[pakPath] = meta;
            return meta;
        }
        finally
        {
            rwlock.ExitWriteLock();
        }
    }

    // ----------------------------
    // Eliminación segura de cache (ej. al cerrar niveles)
    // ----------------------------
    public void InvalidateCache(string pakPath)
    {
        _metadataCache.TryRemove(pakPath, out _);
    }

    // ----------------------------
    // Util: comprobar existencia y formato
    // ----------------------------
    public bool EsPakValido(string pakPath)
    {
        if (!File.Exists(pakPath)) return false;
        using var fs = new FileStream(pakPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (fs.Length < HeaderTotalSize) return false;

        var magicBytes = new byte[HeaderMagicSize];
        fs.ReadExactly(magicBytes, 0, HeaderMagicSize);
        return System.Text.Encoding.ASCII.GetString(magicBytes) == Magic;
    }

    public Dictionary<ChunkId, ChunkDataSerialized> CargarMapaConThreads(string pakPath)
    {
        // 1) cargar metadata
        var metadata = CargarMetadatosCached(pakPath);
        var index = metadata.ChunkIndex.Index;

        var rwlock = GetFileLock(pakPath);

        var queue = new ConcurrentQueue<KeyValuePair<ChunkId, ChunkPakEntry>>(index);
        var results = new ConcurrentDictionary<ChunkId, ChunkDataSerialized>();

        int workerCount = Math.Max(1, System.Environment.ProcessorCount);

        Thread[] threads = new Thread[workerCount];

        // 2) crear threads workers
        for (int i = 0; i < workerCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                while (queue.TryDequeue(out var kv))
                {
                    var id = kv.Key;
                    var entry = kv.Value;

                    byte[] buffer;

                    rwlock.EnterReadLock();
                    try
                    {
                        buffer = LeerBytesEnPosicion(pakPath, entry.Offset, entry.Size);
                    }
                    finally
                    {
                        rwlock.ExitReadLock();
                    }

                    var data = DeserializarChunk(buffer);
                    results[id] = data;
                }
            });

            threads[i].IsBackground = true; // opcional
            threads[i].Start();
        }

        // 3) esperar todos los threads
        for (int i = 0; i < workerCount; i++)
            threads[i].Join();

        return results.ToDictionary(x => x.Key, x => x.Value);
    }



    private ChunkDataSerialized DeserializarChunk(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        return MessagePackSerializer.Deserialize<ChunkDataSerialized>(bytes);
    }
    private byte[] LeerBytesEnPosicion(string path, long offset, int size)
    {
        var buffer = new byte[size];

        using (var fs = new FileStream(
            path,
            FileMode.Open,
            System.IO.FileAccess.Read,
            FileShare.ReadWrite))
        {
            fs.Seek(offset, SeekOrigin.Begin);
            fs.Read(buffer, 0, size);
        }

        return buffer;
    }
}
