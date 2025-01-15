using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap
{
    internal class TileMapManager:SingletonBase<TileMapManager>
    {

        Vector2I chunkDimencion;

        TileMap tilemapTerrain;
        public void AddUpdateTile(Vector2I tilePositionGlobal, int idTile)
        {
            var chunkPosition = PositionsManager.Instance.ChunkPosition(tilePositionGlobal, chunkDimencion );
            var tilePositionChunk = PositionsManager.Instance.TilePositionInChunk(chunkPosition,tilePositionGlobal,chunkDimencion);

            tilemapTerrain.AdupdatedTile(chunkPosition,tilePositionChunk,idTile);
        }
        protected override void Initialize()
        {
            chunkDimencion = PositionsManager.Instance.chunkDimencion;
            tilemapTerrain = new TileMap(chunkDimencion.X, 32, 6);
        }

        protected override void Destroy()
        {
            throw new NotImplementedException();
        }

        //Entity CreateTile(int id)
        //{
        //    Entity tileEntity = new Entity();
        //    tileEntity.Add(new Tile { id = id});
        //    return tileEntity;
        //}
    }
}
