using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.AOT.SourceGenerator;
using Godot;
namespace GodotEcsArch.sources.managers.Tilemap
{
    
    public class Tile
    {
        public int id { get; set; }
        public Vector2I tilePosition { get; set; }

        public Tile(Vector2I tilePosition, int id)
        {
            this.tilePosition = tilePosition;
            this.id = id;
        }
    }

    public class TileBase
    {
        public Rid idRid;
        public int idSpriteOrAnimation;
        public int idInstance;
        public byte spriteAnimation; // 0 - Sprite 1- Animation
        public Byte layer;
    }

    public class TileSimple : TileBase
    {
        
    }

    public class TileAnimation : TileBase
    {
        public int[] arrayFrames;
        public float timePerFrame;
        public int currentFrame;

        public float TimeSinceLastFrame;
        public float TimePerFrame;
        public bool loop;
        public bool complete;
        public int horizontalOrientation;
    }
}
