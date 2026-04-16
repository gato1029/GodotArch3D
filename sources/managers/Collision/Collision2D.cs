using Godot;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;
using static Godot.TextServer;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            return distance < (circle1.widthPixel + circle2.widthPixel);
        }

        private static bool Collides(Circle circle, Polygon polygon, Vector2 circlePosition, Vector2 polygonPosition)
        {
            return Collides(polygon, circle, polygonPosition, circlePosition); // Delegar en la lógica de polígono
        }
        private static bool Collides(Polygon polygon, Circle circle, Vector2 polygonPosition, Vector2 circlePosition)
        {
            return polygon.IntersectsCircle(circlePosition, circle.widthPixel);

        }
        private static bool Collides(
          Polygon polygon,
          Rectangle rect,
          Vector2 polygonPosition,
          Vector2 rectPosition)
        {
            
            var polygonWorld = polygon.GetWorldPolygon(polygonPosition);
            //List<Vector2> points = new List<Vector2>();
            //points.Add(new Vector2(0, 0) );
            //points.Add(new Vector2(0, 32) );
            //points.Add(new Vector2(32, 32) );
            //points.Add(new Vector2(32, 0) );
            bool value = polyRect(polygonWorld.WorldPoints, rectPosition.X - (rect.Width/2) , rectPosition.Y -( rect.Height/2 ), rect.Width, rect.Height);

            GameLog.LogCat("Collision2D" + "detectando collision"+ value    );

            //WireShape.Instance.DrawSquare(polygonWorld.GetSizeQuad(), polygonPosition, 30, Godot.Colors.DarkCyan, WireShape.TypeDraw.NORMAL);

            //bool value = polygonWorld.PolygonVsAARect(rectPosition, rect.widthPixel, rect.heightPixel);

            return value;
        }



        // POLYGON/RECTANGLE
        private static bool polyRect(List<Vector2> vertices, float rx, float ry, float rw, float rh)
        {

            // go through each of the vertices, plus the next
            // vertex in the list
            int next = 0;
            for (int current = 0; current < vertices.Count; current++)
            {

                // get next vertex in list
                // if we've hit the end, wrap around to 0
                next = current + 1;
                if (next == vertices.Count) next = 0;

                // get the PVectors at our current position
                // this makes our if statement a little cleaner
                Vector2 vc = vertices[current];    // c for "current"
                Vector2 vn = vertices[next];       // n for "next"

                // check against all four sides of the rectangle
                bool collision = lineRect(vc.X, vc.Y, vn.X, vn.Y, rx, ry, rw, rh);
                if (collision) return true;                
            }
            // optional: test if the rectangle is INSIDE the polygon
            // note that this iterates all sides of the polygon
            // again, so only use this if you need to
            bool inside = polygonPoint(vertices, rx, ry);
            if (inside) return true;

            return false;
        }

        // is INSIDE the polygon
        public static bool polygonPoint(List<Vector2> vertices, float px, float py)
        {
            bool collision = false;

            // go through each of the vertices, plus the next
            // vertex in the list
            int next = 0;
            for (int current = 0; current < vertices.Count; current++)
            {

                // get next vertex in list
                // if we've hit the end, wrap around to 0
                next = current + 1;
                if (next == vertices.Count) next = 0;

                // get the PVectors at our current position
                // this makes our if statement a little cleaner
                Vector2 vc = vertices[current];    // c for "current"
                Vector2 vn = vertices[next];       // n for "next"

                // compare position, flip 'collision' variable
                // back and forth
                if (((vc.Y > py && vn.Y < py) || (vc.Y < py && vn.Y > py)) &&
                     (px < (vn.X - vc.X) * (py - vc.Y) / (vn.Y - vc.Y) + vc.X))
                {
                    collision = !collision;
                }
            }
            return collision;
        }
        // LINE/RECTANGLE
        private static bool lineRect(float x1, float y1, float x2, float y2, float rx, float ry, float rw, float rh)
        {

            // check if the line has hit any of the rectangle's sides
            // uses the Line/Line function below
            bool left = lineLine(x1, y1, x2, y2, rx, ry, rx, ry + rh);
            bool right = lineLine(x1, y1, x2, y2, rx + rw, ry, rx + rw, ry + rh);
            bool top = lineLine(x1, y1, x2, y2, rx, ry, rx + rw, ry);
            bool bottom = lineLine(x1, y1, x2, y2, rx, ry + rh, rx + rw, ry + rh);

            // if ANY of the above are true,
            // the line has hit the rectangle
            if (left || right || top || bottom)
            {
                return true;
            }
            return false;
        }


        // LINE/LINE
        private static bool lineLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {

            // calculate the direction of the lines
            float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
            float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {
                return true;
            }
            return false;
        }

    }
}
