using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Tilemap
{
    // SOLO APLICARA PARA TILES DE DIMENCIONES IGUALES
    public class TileMapRender<TData> where TData : TileDataGame
    {
        Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
        public Vector2I ChunkSize { get; private set; }
        public int TileSize { get; private set; }
        public Vector2I ViewDistance { get; private set; }
        public int layer { get; set; }
        public int maxPool {  get;  set; }

        private Dictionary<Vector2, ChunkData<TData>> dataChunks = new Dictionary<Vector2, ChunkData<TData>>();
        private Dictionary<Vector2, ChunkRender> loadedChunks = new Dictionary<Vector2, ChunkRender>();
        private Queue<ChunkRender> chunkPool = new Queue<ChunkRender>();
        private Vector2 playerChunkPosCurrent;

        public TileMapRender(Vector2I chunkSize, int tileSize, Vector2I viewDistance,  int layer)
        {
            maxPool = 15;
            ChunkSize = chunkSize;
            TileSize = tileSize;
            ViewDistance = viewDistance;
            this.layer = layer;
            multimeshMaterialDict = new Dictionary<int, MultimeshMaterial>();
        }

        public void AddUpdatedTile(Vector2I tilePositionGlobal, Vector2 PositionReal, TileData tileData, TData dataGame) 
        {
            var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, ChunkSize);
            var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition, tilePositionGlobal, ChunkSize);

            if (loadedChunks.ContainsKey(chunkPosition))
            {
                CreateTile(loadedChunks[chunkPosition], dataChunks[chunkPosition], tilePositionChunk, PositionReal, tileData, dataGame);
                
            }
            else
            {
                var tilemapChunk = new ChunkRender(chunkPosition, ChunkSize);                
                loadedChunks.Add(chunkPosition, tilemapChunk);

                var tilemapDataChunk = new ChunkData<TData>(chunkPosition, ChunkSize);
                dataChunks.Add(chunkPosition, tilemapDataChunk);    

                CreateTile(loadedChunks[chunkPosition], dataChunks[chunkPosition], tilePositionChunk, PositionReal, tileData, dataGame);
              
            }
        }

      

        private void CreateTile(ChunkRender tileMapChunkRender, ChunkData<TData> tileMapChunkData, Vector2I tilePositionChunk, Vector2 positionReal, TileData tileData, TData dataGame)
        {
            MultimeshMaterial multimeshMaterial = null;
            int idInternalPosition = 0;
            int idMaterial = 0;
 

            TileAnimateData tileAnimateData =null;
            switch (tileData.type)
            {
                case "TileSimpleData":
                    TileSimpleData tileSimpleData = (TileSimpleData)tileData;
                  
                    if (!multimeshMaterialDict.ContainsKey(tileSimpleData.idMaterial))
                    {
                        multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(tileSimpleData.idMaterial));
                        multimeshMaterialDict.Add(tileSimpleData.idMaterial, multimeshMaterial);
                    }
                    idMaterial = tileSimpleData.idMaterial;
                    multimeshMaterial = multimeshMaterialDict[tileSimpleData.idMaterial];
                    idInternalPosition = tileSimpleData.idInternalPosition;
                    break;
                case "TileAnimateData":
                     tileAnimateData = (TileAnimateData)tileData;
                    if (!multimeshMaterialDict.ContainsKey(tileAnimateData.idMaterial))
                    {
                        MaterialData materialData = MaterialManager.Instance.GetMaterial(tileAnimateData.idMaterial);
                        multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(tileAnimateData.idMaterial));
                        multimeshMaterialDict.Add(tileAnimateData.idMaterial, multimeshMaterial);
                    }
  
                    idMaterial = tileAnimateData.idMaterial;
                    multimeshMaterial = multimeshMaterialDict[tileAnimateData.idMaterial];                    
                    break;
                default:
                    break;
            }

            float x = MeshCreator.PixelsToUnits(multimeshMaterial.materialData.divisionPixelX) / 2f;
            float y = MeshCreator.PixelsToUnits(multimeshMaterial.materialData.divisionPixelY) / 2f;
            positionReal =positionReal + new Vector2(x, y);

            Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
                      
            if (!tileMapChunkRender.ExistTile(tilePositionChunk))
            {
                dataGame.idCollider = TileNumeratorManager.Instance.getNumerator();
                       
                xform = xform.Translated(new Vector3(positionReal.X, positionReal.Y, (positionReal.Y * -0.05f) + layer));                                 
            }
            else
            {
                Tile tile = tileMapChunkRender.GetTileAt(tilePositionChunk);
                dataGame = tileMapChunkData.GetTileAt(tilePositionChunk);

                if (tile.idMaterial == idMaterial)
                {                   
                    xform.Origin = new Vector3(positionReal.X, positionReal.Y, (positionReal.Y * -0.05f) + layer);                                          
                }
                else
                {
                    xform.Origin = new Vector3(positionReal.X, positionReal.Y, (positionReal.Y * -0.05f) + layer);                   
                }                   
            }

            
            dataGame.positionReal =positionReal;
            dataGame.tilePositionChunk =tilePositionChunk;
            dataGame.idMaterial =idMaterial;
            dataGame.transform3d = xform;
            dataGame.idTile =tileData.id;
            dataGame.collisionBody = tileData.collisionBody;
            dataGame.idInternal = idInternalPosition;
            tileMapChunkData.CreateUpdateTile(tilePositionChunk, dataGame);

            CollisionManager.Instance.tileColliders.AddUpdateItem(new Vector2(positionReal.X, positionReal.Y), dataGame);

            

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

        public void UnloadAll()
        {
            foreach (var item in loadedChunks)
            {
                UnloadChunk(item.Key);
            }
            
        }
        private Vector2 WorldToChunkCoords(Vector2 worldPos)
        {
            return new Vector2(
                (int)Math.Floor(worldPos.X / (ChunkSize.X * TileSize)),
                (int)Math.Floor(worldPos.Y / (ChunkSize.Y * TileSize))
            );
        }

        public void UpdateChunks()
        {
            HashSet<Vector2> requiredChunks = new HashSet<Vector2>();

            for (int x = -ViewDistance.X; x <= ViewDistance.X; x++)
            {
                for (int y = -ViewDistance.Y; y <= ViewDistance.Y; y++)
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
            if (dataChunks.ContainsKey(chunkPos))
            {
                ChunkRender chunk;
                if (chunkPool.Count > 0)
                {
                    chunk = chunkPool.Dequeue();
                }
                else
                {
                    chunk = new ChunkRender(chunkPos, ChunkSize);
                }

                ChunkData<TData> tileMapChunkData = dataChunks[chunkPos];

                for (int i = 0; i < tileMapChunkData.size.X; i++)
                {
                    for (int j = 0; j < tileMapChunkData.size.Y; j++)
                    {
                        var dataGame = tileMapChunkData.tiles[i, j];
                        if (dataGame!=null)
                        {
                            (Rid, int) instance = multimeshMaterialDict[dataGame.idMaterial].CreateInstance();
                            chunk.CreateUpdate(i, j, dataGame.idMaterial, instance.Item1, instance.Item2, dataGame.idInternal, dataGame.transform3d,dataGame.idTile);
                        }                        
                    }
                }
                loadedChunks.Add(chunkPos, chunk);
              
            }
          
        }

        private void UnloadChunk(Vector2 chunkPos)
        {
            if (loadedChunks.ContainsKey(chunkPos))
            {
                ChunkRender tileMapChunkRender = loadedChunks[chunkPos];
            
                    for (int i = 0; i < tileMapChunkRender.size.X; i++)
                    {
                        for (int j = 0; j < tileMapChunkRender.size.Y; j++)
                        {
                            var datatile = tileMapChunkRender.tiles[i, j];
                            if (datatile!=null && datatile.idMaterial!=0)
                            {
                                multimeshMaterialDict[datatile.idMaterial].FreeInstance(datatile.rid, datatile.instance);
                                datatile.FreeTile();
                            }                            
                        }
                    }
                if (chunkPool.Count<maxPool)
                {
                    chunkPool.Enqueue(tileMapChunkRender);
                }                
                loadedChunks.Remove(chunkPos);                              
            }
        }
    }
}
