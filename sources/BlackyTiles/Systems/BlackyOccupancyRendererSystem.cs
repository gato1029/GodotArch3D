using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Systems
{
    public class BlackyOccupancyRendererSystem
    {
        private readonly ChunkManagerBase chunkManager;
        private readonly BlackyChunkOccupancyMap occupancy;
        private readonly FlecsManager flecsManager;

        private readonly Dictionary<Vector2I, Dictionary<Vector2I, int>> chunksLoad = new();
        private bool _active = false;
        private World flecsWorld;

        public BlackyOccupancyRendererSystem(FlecsManager flecsManager, ChunkManagerBase chunkManager, BlackyChunkOccupancyMap occupancy)
        {
            this.chunkManager = chunkManager;
            this.occupancy = occupancy;

            chunkManager.OnChunkLoad += OnChunkLoad;
            chunkManager.OnChunkUnload += OnChunkUnload;

            occupancy.OnTileUpdated += OnTileUpdated;
            this.flecsWorld = flecsManager.WorldFlecs;
            this.flecsManager = flecsManager;
        }

        // ----------------------------------------
        // ---------- Control de render ------------
        // ----------------------------------------
        public void SetActive(bool active)
        {
            if (_active == active)
                return;

            _active = active;

            if (!_active)
            {
                foreach (var shapes in chunksLoad.Values)
                    foreach (var id in shapes.Values)
                        WireShape.Instance.FreeShape(id);

                chunksLoad.Clear();
            }
            else
            {
                foreach (var chunkPos in chunkManager.GetActiveChunks())
                    OnChunkLoad(chunkPos);
            }
        }

        public bool IsActive() => _active;

        // ----------------------------------------
        // ---------- Eventos de chunking ----------
        // ----------------------------------------
        private void OnChunkLoad(Vector2I chunkPos)
        {
            if (!_active || chunksLoad.ContainsKey(chunkPos))
                return;

            var shapes = new Dictionary<Vector2I, int>();
            int sizeX = chunkManager.chunkDimencion.X;
            int sizeY = chunkManager.chunkDimencion.Y;

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Vector2I local = new(x, y);
                    Vector2I global = chunkManager.TilePositionGlobal(chunkPos, local);
                    ulong entityId = occupancy.Get(0, global.X, global.Y);

                    if (entityId != 0)
                    {
                        Vector2 posWorld = TilesHelper.WorldPositionTile(global);
                        int shapeId = WireShape.Instance.DrawFilledSquare(
                            new Vector2(16, 16),
                            posWorld,
                            40,
                            GetColorFromEntity(entityId),
                            0.2f
                        );
                        shapes[global] = shapeId;
                    }
                }
            }

            chunksLoad.Add(chunkPos, shapes);
        }

        private void OnChunkUnload(Vector2I chunkPos)
        {
            if (!_active)
                return;

            if (chunksLoad.TryGetValue(chunkPos, out var shapes))
            {
                foreach (var id in shapes.Values)
                    WireShape.Instance.FreeShape(id);

                chunksLoad.Remove(chunkPos);
            }
        }

        // ----------------------------------------
        // ---------- Sincronización automática ----------
        // ----------------------------------------
        private void OnTileUpdated(int layer, int worldX, int worldY, ulong entityId)
        {
            if (!_active)
                return;

            Vector2I tilePosGlobal = new(worldX, worldY);
            Vector2I chunkPos = chunkManager.ChunkPosition(tilePosGlobal);

            if (!chunksLoad.TryGetValue(chunkPos, out var shapes))
                chunksLoad[chunkPos] = shapes = new Dictionary<Vector2I, int>();

            if (entityId != 0)
            {
                if (!shapes.ContainsKey(tilePosGlobal))
                {
                    Vector2 posWorld = TilesHelper.WorldPositionTile(tilePosGlobal);
                    int shapeId = WireShape.Instance.DrawFilledSquare(
                        new Vector2(16, 16),
                        posWorld,
                        40,
                        GetColorFromEntity(entityId),
                        0.2f
                    );
                    shapes[tilePosGlobal] = shapeId;
                }
            }
            else
            {
                if (shapes.TryGetValue(tilePosGlobal, out int shapeId))
                {
                    WireShape.Instance.FreeShape(shapeId);
                    shapes.Remove(tilePosGlobal);
                }
            }
        }

        // ----------------------------------------
        // ---------- Métodos manuales opcionales ----------
        // ----------------------------------------
        public void UpdateArea(Vector2I startGlobal, int width, int height)
        {
            if (!_active) return;

            for (int dx = 0; dx < width; dx++)
                for (int dy = 0; dy < height; dy++)
                    OnTileUpdated(0, startGlobal.X + dx, startGlobal.Y + dy,
                        occupancy.Get(0, startGlobal.X + dx, startGlobal.Y + dy));
        }

        public void UpdateTiles(Vector2I basePos, List<KuroTile> tiles)
        {
            if (!_active) return;

            foreach (var tile in tiles)
            {
                int x = basePos.X + tile.x;
                int y = basePos.Y + tile.y;
                OnTileUpdated(0, x, y, occupancy.Get(0, x, y));
            }
        }

        public void ClearChunk(Vector2I chunkPos)
        {
            if (!_active) return;

            if (chunksLoad.TryGetValue(chunkPos, out var shapes))
            {
                foreach (var id in shapes.Values)
                    WireShape.Instance.FreeShape(id);

                chunksLoad.Remove(chunkPos);
            }
        }

        // ----------------------------------------
        // ---------- Helper: Color por entidad ----------
        // ----------------------------------------
        private Godot.Color GetColorFromEntity(ulong entityId)
        {
            if (entityId == 0)
                return Godot.Colors.Transparent;

            // Recuperamos el componente IdGenericComponent desde Flecs
            try
            {
                // Obtenemos el componente IdGenericComponent de la entidad



                Entity ent = flecsWorld.Entity(entityId);
                if (ent.Has<IdGenericComponent>())
                {
                    var cmp = ent.Get<IdGenericComponent>();

                    return cmp.entityType switch
                    {
                        EntityType.PERSONAJE => Godot.Colors.Blue,
                        EntityType.EDIFICIO => Godot.Colors.Red,
                        EntityType.RECURSO => Godot.Colors.Pink,
                        EntityType.ACCESORIO => Godot.Colors.Orange,
                        EntityType.TERRENO => Godot.Colors.Brown,
                        EntityType.TILESPRITE => Godot.Colors.Purple,
                        _ => Godot.Colors.Gray
                    };
                }
                return Godot.Colors.Gray;
            }
            catch
            {
                // Si la entidad no existe o no tiene componente, fallback
                return Godot.Colors.Gray;
            }
        }
    }
}
