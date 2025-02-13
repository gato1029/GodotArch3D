using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Godot;
namespace GodotEcsArch.sources.managers
{
    internal class PositionsManager : SingletonBase<PositionsManager>
    {
        public Vector2I chunkDimencion { get; set; }
        public Vector2 positionCamera { get; set; }
        public Vector2 positionMouseCamera { get; set; }
        public Vector2 positionMouseCameraPixel { get; set; }
        public Vector2 positionMouseTileGlobal { get; set; }
        public Vector2 positionMouseChunk { get; set; }
        public Vector2 positionMouseTileChunk { get; set; }


        public Vector2I TilePositionInChunk(Vector2I chunkPositon, Vector2I TilePositionGlobal, Vector2I ChunkDimencion)
        {
            var calc = chunkPositon * ChunkDimencion;
            return TilePositionGlobal - calc;                    
        }

        public Vector2I ChunkPosition(Vector2I TilePositionGlobal, Vector2 ChunkDimencion)
        {
            return new Vector2I(
                (int)Mathf.Floor(TilePositionGlobal.X / ChunkDimencion.X),
                (int)MathF.Floor(TilePositionGlobal.Y / ChunkDimencion.Y));
        }
        protected override void Initialize()
        {         
            chunkDimencion = new Vector2I(8,8);
        }

        protected override void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
