using Godot;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision
{
    public abstract class GeometricShape2D
    {
        public float originPixelX { get; set; }
        public float originPixelY { get; set; }

        public float widthPixel { get; set; }
        public float heightPixel { get; set; }

        [BsonIgnore]
        public Vector2 OriginRelative { get;  set; }
        [BsonIgnore]
        public Vector2 OriginCurrent { get;  set; }
        public abstract Vector2 GetSizeQuad();
    }
}
