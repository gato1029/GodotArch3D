using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using MessagePack;
using MessagePackGodot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;




public static class Array2DConverter
{
    public static T[][] ToJagged<T>(T[,] array2D)
    {
        int rows = array2D.GetLength(0);
        int cols = array2D.GetLength(1);

        T[][] jagged = new T[rows][];

        for (int i = 0; i < rows; i++)
        {
            jagged[i] = new T[cols];
            for (int j = 0; j < cols; j++)
                jagged[i][j] = array2D[i, j];
        }

        return jagged;
    }
}
public class SpriteChunkSerializer : SingletonBase<SpriteChunkSerializer>
{
    protected override void Initialize()
    {
        SetUp();
    }
    private readonly object _writeLock = new();

    // --------------------------------------------------------------
    // SERIALIZAR: SpriteMapChunk por capas (con escritura segura)
    // --------------------------------------------------------------
    public async Task SerializarSpriteMapChunk<T>(SpriteMapChunk<T> chunkMap, string pakPath)
        where T : DataItem, new()
    {
        SpritePakSystem pak = new SpritePakSystem();

        // 1) Cargar metadatos
        PakMetadata meta = File.Exists(pakPath)
            ? pak.CargarMetadatos(pakPath)
            : new PakMetadata
            {
                ChunkIndex = new PackIndexDto(),
                LayerIndex = new LayerIndex()
            };

        // Registrar capa
        if (!meta.LayerIndex.Layers.ContainsKey(chunkMap.layer))
            meta.LayerIndex.Layers[chunkMap.layer] = chunkMap.layerName;

        // ----------------------------------------------------------
        // 2) PROCESO PARALELO: Serializar data → byte[]
        // ----------------------------------------------------------
        var trabajos = new List<(Vector2 pos, byte[] bytes)>();

        Parallel.ForEach(chunkMap.dataChunks, kvp =>
        {
            var pos = kvp.Key;
            var chunk = kvp.Value;

            if (!chunk.changue)
                return;

            var jagged = Array2DConverter.ToJagged(chunk.renderTiles);

            var serialized = new ChunkDataSerialized
            {
                renderTiles = jagged
            };

            byte[] bytes = MessagePackSerializer.Serialize(serialized);

            lock (trabajos)
                trabajos.Add((pos, bytes));

            chunk.changue = false;
        });

        if (trabajos.Count == 0)
            return;

        // -----------------------------------------------
        // 3) Escribir todos los chunks al .pak
        // -----------------------------------------------
        foreach (var trabajo in trabajos)
        {
            pak.GuardarChunkIncremental(
                pakPath,
                trabajo.pos,
                chunkMap.layer,
                trabajo.bytes
            );
        }

        // -----------------------------------------------
        // 4) Guardar metadatos actualizados
        // -----------------------------------------------
        pak.GuardarMetadatos(pakPath);
    }

    // --------------------------------------------------------------
    // SERIALIZAR TODAS LAS CAPAS
    // --------------------------------------------------------------
    public void SerializarLayers<T>(LayerChunksMaps<T> layersChunks, string pakPath)
        where T : DataItem, new()
    {
        SpritePakSystem pak = new SpritePakSystem();

        // 1) Crear el pak si no existe
        if (!File.Exists(pakPath))
            pak.CrearPak(pakPath);

        // 2) Cargar metadata
        PakMetadata meta = pak.CargarMetadatos(pakPath);

        List<(Vector2 pos, int layer, byte[] bytes)> trabajos = new List<(Vector2 pos, int layer, byte[] bytes)>();

        foreach (var item in layersChunks.Layers)
        {
            string layerName = item.Key;
            SpriteMapChunk<T> chunkMap = item.Value;

            if (!meta.LayerIndex.Layers.ContainsKey(chunkMap.layer))
                meta.LayerIndex.Layers[chunkMap.layer] = layerName;

            Parallel.ForEach(chunkMap.dataChunks, kvp =>
            {
                var pos = kvp.Key;
                var chunk = kvp.Value;

                if (!chunk.changue)
                    return;

                var jagged = Array2DConverter.ToJagged(chunk.renderTiles);

                var serialized = new ChunkDataSerialized
                {
                    renderTiles = jagged
                };

                byte[] bytes = MessagePackSerializer.Serialize(serialized);

                lock (trabajos)
                    trabajos.Add((pos, chunkMap.layer, bytes));

                chunk.changue = false;
            });
        }
        // -------------------------
        // 3) ESCRITURA AL .PAK
        // -------------------------
        foreach (var trabajo in trabajos)
        {
            pak.GuardarChunkIncremental(
                pakPath,
                trabajo.pos,
                trabajo.layer,
                trabajo.bytes
            );
        }

        // -------------------------
        // 4) GUARDAR METADATOS
        // -------------------------
        pak.GuardarMetadatos(pakPath);
        GD.Print("Todas las capas serializadas en el .pak (seguro).");
    }

    // -----------------------------------------------
    // CARGAR UN CHUNK
    // -----------------------------------------------
    public ChunkDataSerialized CargarChunk(string pakPath, Vector2 pos, int layer)
    {
        SpritePakSystem pak = new SpritePakSystem();
        return pak.CargarChunk(pakPath, pos, layer);
    }

    public  Dictionary<ChunkId, ChunkDataSerialized> CargarMapaCompleto(string pakPath)
    {
        SpritePakSystem pak = new SpritePakSystem();
        return  pak.CargarMapaConThreads(pakPath);
    }

    public void SetUp()
    {
        // initialize MessagePack resolvers
        var resolver = MessagePack.Resolvers.CompositeResolver.Create(
            // enable extension packages first
            GodotResolver.Instance,
            // finally use standard (default) resolver
            MessagePack.Resolvers.StandardResolver.Instance
        );
        var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

        // pass options every time to set as default
        MessagePackSerializer.DefaultOptions = options;
    }
}
