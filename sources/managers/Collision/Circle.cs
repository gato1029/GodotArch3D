using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision
{
    public class Circle : GeometricShape2D
    {
        public float Radius { get; set; }
        public Circle(float radius)
        {
            Radius = radius;
            OriginRelative = Godot.Vector2.Zero;
            OriginCurrent = Godot.Vector2.Zero;
        }
        public Circle(float radius, Godot.Vector2 originRelative)
        {
            Radius = radius;
            OriginRelative = originRelative;
            OriginCurrent = originRelative;

        }

        public override Vector2 GetSizeQuad()
        {
            return new Vector2(Radius, Radius);
        }
    }
}
