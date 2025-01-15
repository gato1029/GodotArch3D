using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision
{
    public  class Polygon : GeometricShape2D
    {
        public List<Vector2> Vertices { get; set; }
        public Polygon(List<Vector2> vertices)
        {
            Vertices = vertices;
        }

        public override Godot.Vector2 GetSizeQuad()
        {
           return new Godot.Vector2(Vertices.Count, Vertices.Count);
        }
    }
}
