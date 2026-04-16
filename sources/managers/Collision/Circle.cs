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
            scale = 1;
        }
        public Circle(float radius, Godot.Vector2 originRelative)
        {
            Radius = radius;
            OriginRelative = originRelative;
            OriginCurrent = originRelative;
            scale = 1;

        }
        public override Circle Multiplicity(float value)
        { 
            // Escalamos tanto ancho como alto en base a los originales
            float newWidth = Radius * value;
            

            // Escalamos también el origen relativo
            var newOriginRelative = OriginRelative * value;

            var nc = new Circle(newWidth)
            {
                OriginRelative = newOriginRelative,
                OriginCurrent = newOriginRelative,
                scale = this.scale * value,
                Radius = this.Radius * value,
                widthPixel = widthPixel * value,
                heightPixel = heightPixel * value,
                originPixelX = originPixelX * value,
                originPixelY = originPixelY * value,
            };            
            return nc;
        }
        [BsonCtor]
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
            return new Vector2(Radius*2, Radius * 2);
        }

        public override Circle MultiplicityInternal(float value)
        {
            this.Radius = this.Radius * value;  
            this.OriginRelative = OriginRelative * value;
            this.OriginCurrent = OriginCurrent * value;
            this.scale = value;
            return this;
        }

        public override Vector2[] GetVertices(Vector2 position)
        {
            float r = Radius;

            return new Vector2[]
            {
            position + new Vector2(-r, -r), // esquina superior izquierda
            position + new Vector2(r, -r),  // esquina superior derecha
            position + new Vector2(-r, r),  // esquina inferior izquierda
            position + new Vector2(r, r)    // esquina inferior derecha
             };
        }
    }
}
