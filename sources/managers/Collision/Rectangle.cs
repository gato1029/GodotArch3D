using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace GodotEcsArch.sources.managers.Collision
{
    public class Rectangle : GeometricShape2D
    {
        [BsonIgnore]
        public float OriginalWidth { get; private set; }
        [BsonIgnore]
        public float OriginalHeight { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
        [BsonIgnore]
        public Vector2 Direction { get; private set; }

        public Rectangle(float width, float height, Godot.Vector2 originRelative)
        {
            OriginalWidth = width;
            OriginalHeight = height;
            Width = width;
            Height = height;
            Direction = new Vector2(1, 0); 
            this.OriginRelative = originRelative;
            this.OriginCurrent = originRelative;
        
        }
        [BsonCtor]
        public Rectangle(float widthPixel, float heightPixel, float originPixelX, float originPixelY) : base()
        {
            OriginalWidth = MeshCreator.PixelsToUnits(widthPixel);
            OriginalHeight = MeshCreator.PixelsToUnits(heightPixel);
            Width = MeshCreator.PixelsToUnits(widthPixel);
            Height = MeshCreator.PixelsToUnits(heightPixel);

            this.widthPixel = widthPixel;
            this.heightPixel = heightPixel;
            Direction = new Vector2(1, 0);
            OriginRelative = new Godot.Vector2(MeshCreator.PixelsToUnits(originPixelX), MeshCreator.PixelsToUnits(originPixelY));
            OriginCurrent = new Godot.Vector2(MeshCreator.PixelsToUnits(originPixelX), MeshCreator.PixelsToUnits(originPixelY));
            this.originPixelX = originPixelX;
            this.originPixelY = originPixelY;
        }


        public Rectangle(float width, float height) 
        {
            OriginalWidth = width;
            OriginalHeight = height;
            Width = width;
            Height = height;
            Direction = new Vector2(1, 0);
            OriginRelative = Godot.Vector2.Zero;
            OriginCurrent = Godot.Vector2.Zero;
        }
        public void DirectionTo(float x, float y)
        {
            Direction = Vector2.Normalize(new Vector2(x,y));
            UpdateAABB();
        }

        private void UpdateAABB()
        {
            
            Vector2[] vertices = new Vector2[4];
            vertices[0] = new Vector2(-OriginalWidth / 2, OriginalHeight / 2); // Arriba izquierda
            vertices[1] = new Vector2(OriginalWidth / 2, OriginalHeight / 2);  // Arriba derecha
            vertices[2] = new Vector2(OriginalWidth / 2, -OriginalHeight / 2); // Abajo derecha
            vertices[3] = new Vector2(-OriginalWidth / 2, -OriginalHeight / 2);// Abajo izquierda
            
            Matrix3x2 rotationMatrix = Matrix3x2.CreateRotation(MathF.Atan2(Direction.Y, Direction.X));
            for (int i = 0; i < 4; i++)
            {
                vertices[i] = Vector2.Transform(vertices[i], rotationMatrix);
            }

            
            float minX = vertices[0].X;
            float maxX = vertices[0].X;
            float minY = vertices[0].Y;
            float maxY = vertices[0].Y;

            for (int i = 1; i < 4; i++)
            {
                minX = MathF.Min(minX, vertices[i].X);
                maxX = MathF.Max(maxX, vertices[i].X);
                minY = MathF.Min(minY, vertices[i].Y);
                maxY = MathF.Max(maxY, vertices[i].Y);
            }

            // Calcular el nuevo ancho y alto del AABB
            Width = maxX - minX;
            Height = maxY - minY;
        }


        public override string ToString()
        {
            return $"Rectangle: Width = {Width}, Height = {Height}, Direction = {Direction}";
        }

        public override Godot.Vector2 GetSizeQuad()
        {
            return new Godot.Vector2(Width, Height);
        }
    }
}
