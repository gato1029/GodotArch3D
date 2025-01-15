using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision
{
    public abstract class GeometricShape2D
    {
        public Vector2 OriginRelative { get;  set; }
        public Vector2 OriginCurrent { get;  set; }
        public abstract Vector2 GetSizeQuad();
    }
}
