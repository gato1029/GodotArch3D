using Arch.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap
{
    internal class TileMap
    {
        public int ChunkSize { get; private set; }
        public int TileSize { get; private set; }
        public int ViewDistance { get; private set; }

        private Dictionary<Vector2, TileMapChunk> loadedChunks = new Dictionary<Vector2, TileMapChunk>();
        private Queue<TileMapChunk> chunkPool = new Queue<TileMapChunk>();
        private Vector2 playerChunkPosCurrent;

        public TileMap(int chunkSize, int tileSize, int viewDistance)
        {
            ChunkSize = chunkSize;
            TileSize = tileSize;
            ViewDistance = viewDistance;
        }

        public void AdupdatedTile(Vector2I chunkPosition, Vector2I tilePosition, int idTile)
        {
            if (loadedChunks.ContainsKey(chunkPosition))
            {
                loadedChunks[chunkPosition].CreateUpdateTile(tilePosition, idTile);
            }
            else
            {
                var tilemapChunk = new TileMapChunk(chunkPosition, ChunkSize);
                tilemapChunk.CreateUpdateTile(tilePosition, idTile);
                loadedChunks.Add(chunkPosition, tilemapChunk);
            }
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

        private Vector2 WorldToChunkCoords(Vector2 worldPos)
        {
            return new Vector2(
                (int)Math.Floor(worldPos.X / (ChunkSize * TileSize)),
                (int)Math.Floor(worldPos.Y / (ChunkSize * TileSize))
            );
        }

        private void UpdateChunks()
        {
            HashSet<Vector2> requiredChunks = new HashSet<Vector2>();

            for (int x = -ViewDistance; x <= ViewDistance; x++)
            {
                for (int y = -ViewDistance; y <= ViewDistance; y++)
                {
                    Vector2 chunkPos = playerChunkPosCurrent + new Vector2(x, y);
                    requiredChunks.Add(chunkPos);

                    if (!loadedChunks.ContainsKey(chunkPos))
                        LoadChunk(chunkPos);
                }
            }

            // Descargar chunks fuera del rango
            foreach (var chunkPos in new List<Vector2>(loadedChunks.Keys))
            {
                if (!requiredChunks.Contains(chunkPos))
                    UnloadChunk(chunkPos);
            }
        }
        private void LoadChunk(Vector2 chunkPos)
        {
            TileMapChunk chunk = new TileMapChunk(chunkPos, ChunkSize);
           
            loadedChunks[chunkPos] = chunk;

            Console.WriteLine($"Chunk cargado en posición: {chunkPos}");
        }

        private void UnloadChunk(Vector2 chunkPos)
        {
            if (loadedChunks.ContainsKey(chunkPos))
            {
                loadedChunks.Remove(chunkPos);
                Console.WriteLine($"Chunk descargado en posición: {chunkPos}");
            }
        }
    }
}
