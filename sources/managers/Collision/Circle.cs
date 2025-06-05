using Godot;
using LiteDB;
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
        public override Circle Multiplicity(float value)
        {
            return new Circle(Radius * value, OriginCurrent *value);
        }
        public Circle(float widthPixel, float originPixelX, float originPixelY) : base()
        {
            this.originPixelX = originPixelX;
            this.originPixelY = originPixelY;
            this.widthPixel = widthPixel;

            Radius = MeshCreator.PixelsToUnits(widthPixel);
            OriginRelative = new Godot.Vector2(MeshCreator.PixelsToUnits(originPixelX), MeshCreator.PixelsToUnits(originPixelY));
            OriginCurrent = new Godot.Vector2(MeshCreator.PixelsToUnits(originPixelX), MeshCreator.PixelsToUnits(originPixelY));

        }
        public override Vector2 GetSizeQuad()
        {
            return new Vector2(Radius, Radius);
        }
    }
}
