using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;

namespace GodotEcsArch.sources.managers.Collision
{
    public static class Collision2D
    {
        public static bool Collides(GeometricShape2D shape1, GeometricShape2D shape2, Vector2 position1, Vector2 position2)
        {
            switch (shape1)
            {
                case Rectangle rect1 when shape2 is Rectangle rect2:
                    return Collides(rect1, rect2, position1, position2);
                case Rectangle rect when shape2 is Circle circle:
                    return Collides(rect, circle, position1, position2);
                case Rectangle rect when shape2 is Polygon polygon:
                    return Collides(rect, polygon, position1, position2);
                case Circle circle when shape2 is Rectangle rect:
                    return Collides(rect, circle, position2, position1);
                case Circle circle1 when shape2 is Circle circle2:
                    return Collides(circle1, circle2, position1, position2);
                case Circle circle when shape2 is Polygon polygon:
                    return Collides(circle, polygon, position1, position2);
                case Polygon polygon when shape2 is Rectangle rect:
                    return Collides(rect, polygon, position2, position1);
                case Polygon polygon when shape2 is Circle circle:
                    return Collides(circle, polygon, position2, position1);
                case Polygon polygon1 when shape2 is Polygon polygon2:
                    return Collides(polygon1, polygon2, position1, position2);
                default:
                    return false;
            }
        }

        private static bool Collides(Rectangle rect1, Rectangle rect2, Vector2 position1, Vector2 position2)
        {
            // Calcular los puntos mínimo y máximo para rect1
            float rect1MinX = position1.X - rect1.Width / 2f;
            float rect1MaxX = position1.X + rect1.Width / 2f;
            float rect1MinY = position1.Y - rect1.Height / 2f;
            float rect1MaxY = position1.Y + rect1.Height / 2f;

            // Calcular los puntos mínimo y máximo para rect2
            float rect2MinX = position2.X - rect2.Width / 2f;
            float rect2MaxX = position2.X + rect2.Width / 2f;
            float rect2MinY = position2.Y - rect2.Height / 2f;
            float rect2MaxY = position2.Y + rect2.Height / 2f;

            // Comprobar la colisión
            return !(rect1MaxX < rect2MinX ||
                     rect1MinX > rect2MaxX ||
                     rect1MaxY < rect2MinY ||
                     rect1MinY > rect2MaxY);
        }

        private static bool Collides(Rectangle rect, Circle circle, Vector2 rectPosition, Vector2 circlePosition)
        {
            float rectLeft = rectPosition.X - rect.Width / 2f;
            float rectRight = rectPosition.X + rect.Width / 2f;
            float rectTop = rectPosition.Y - rect.Height / 2f;
            float rectBottom = rectPosition.Y + rect.Height / 2f;

          
            float nearestX = Math.Clamp(circlePosition.X, rectLeft, rectRight);
            float nearestY = Math.Clamp(circlePosition.Y, rectTop, rectBottom);

          
            float deltaX = circlePosition.X - nearestX;
            float deltaY = circlePosition.Y - nearestY;    
            return (deltaX * deltaX + deltaY * deltaY) < (circle.Radius * circle.Radius);
        }

        private static bool Collides(Rectangle rect, Polygon polygon, Vector2 rectPosition, Vector2 polygonPosition)
        {
            return Collides(polygon, rect, polygonPosition, rectPosition); // Delegar en la lógica de polígono
        }

        private static bool Collides(Circle circle1, Circle circle2, Vector2 position1, Vector2 position2)
        {
            float distance = position1.DistanceTo(position2);
            return distance < (circle1.Radius + circle2.Radius);
        }

        private static bool Collides(Circle circle, Polygon polygon, Vector2 circlePosition, Vector2 polygonPosition)
        {
            return Collides(polygon, circle, polygonPosition, circlePosition); // Delegar en la lógica de polígono
        }

        private static bool Collides(Polygon polygon, Rectangle rect, Vector2 polygonPosition, Vector2 rectPosition)
        {
            // Implementación de colisión polígono-rectángulo
            // A implementar
            return false;
        }

        private static bool Collides(Polygon polygon, Circle circle, Vector2 polygonPosition, Vector2 circlePosition)
        {
            // Implementación de colisión polígono-círculo
            // A implementar
            return false;
        }

        private static bool Collides(Polygon polygon1, Polygon polygon2, Vector2 position1, Vector2 position2)
        {
            // Implementación de colisión polígono-polígono
            // A implementar
            return false;
        }

        public static Vector2 RotatePosition(Vector2 position, Vector2 direction)
        {

          return Vector2.Zero + (position*direction);
        }

 
    }
}
