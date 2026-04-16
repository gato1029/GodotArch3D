using Godot;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;
using Vector2 = Godot.Vector2;

namespace GodotEcsArch.sources.managers.Collision
{
    public class Polygon : GeometricShape2D
    { // === Datos base ===
        public List<Vector2> LocalPoints { get; set; } = new();   // puntos en espacio local
        [BsonIgnore]
        public List<Vector2> WorldPoints { get; set; } = new();   // puntos transformados
        [BsonIgnore]
        public Rect2 AABB { get; set; }              // broad phase               
        public List<Vector2> Axes { get; set; } = new();          // normales (SAT)                
        public int Count { get; set; }
        public Vector2 LocalCenter { get; set; }
        public List<Vector2> VerticesPixels { get; set; } = new();
       
        public List<Polygon> Triangles { get; private set; } = new();
        public Polygon()
        {

        }

        

        private void ComputeLocalCenter()
        {
            Vector2 sum = Vector2.Zero;
            for (int i = 0; i < LocalPoints.Count; i++)
                sum += LocalPoints[i];

            LocalCenter = sum / LocalPoints.Count;
        }
        public Polygon(List<Vector2> verticesPixel, bool invertSign=false)
        {
            List<Vector2> verts = new List<Vector2>();
            foreach (var item in verticesPixel)
            {
                Vector2 vc = item;
                if (invertSign)
                {
                    vc = new Vector2( item.X, item.Y * (-1));
                }
                verts.Add(vc);
                AddVertice(vc);
            }
            VerticesPixels = verts;
           
            Count = LocalPoints.Count;
            ComputeAxes();
            ComputeLocalCenter();
            AABB = ComputeAABB(LocalPoints.ToArray());
        }


        private void AddVertice(Vector2 vertice)
        {
            Vector2 nVertice = new Vector2(MeshCreator.PixelsToUnits(vertice.X), MeshCreator.PixelsToUnits(vertice.Y));
            LocalPoints.Add(nVertice);
        }

        // === Calcula ejes (SOLO UNA VEZ) ===
        private void ComputeAxes()
        {
            Axes.Clear();

            for (int i = 0; i < Count; i++)
            {
                Vector2 p1 = LocalPoints[i];
                Vector2 p2 = LocalPoints[(i + 1) % Count];

                Vector2 edge = p2 - p1;
                Vector2 axis = new Vector2(-edge.Y, edge.X).Normalized();

                Axes.Add(axis);
            }
        }

        public void UpdatePosition(Vector2 position)
        {
            
            for (int i = 0; i < Count; i++)
                WorldPoints.Add( LocalPoints[i] + position);

            AABB = ComputeAABB(WorldPoints.ToArray());
        }
        public Polygon GetWorldPolygon(Vector2 position)
        {
            Polygon world = new Polygon();

            world.LocalPoints = LocalPoints;
            world.Axes = Axes;
            world.LocalCenter = LocalCenter;

            world.WorldPoints = new List<Vector2>(Count);
            for (int i = 0; i < Count; i++)
                world.WorldPoints.Add((LocalPoints[i] - LocalCenter) + position);

            world.AABB = ComputeAABB(world.WorldPoints.ToArray());

            return world;
        }


        private Rect2 ComputeAABB(Vector2[] points)
        {
            float minX = points[0].X;
            float maxX = points[0].X;
            float minY = points[0].Y;
            float maxY = points[0].Y;

            for (int i = 1; i < points.Length; i++)
            {
                Vector2 p = points[i];

                if (p.X < minX) minX = p.X;
                else if (p.X > maxX) maxX = p.X;

                if (p.Y < minY) minY = p.Y;
                else if (p.Y > maxY) maxY = p.Y;
            }

            return new Rect2(
                new Vector2(minX, minY),
                new Vector2(maxX - minX, maxY - minY)
            );
        }

        // === Proyección SAT ===
        public void Project(Vector2 axis, out float min, out float max)
        {
            float dot = WorldPoints[0].Dot(axis);
            min = max = dot;

            for (int i = 1; i < WorldPoints.Count; i++)
            {
                dot = WorldPoints[i].Dot(axis);
                if (dot < min) min = dot;
                else if (dot > max) max = dot;
            }
        }

        // === SAT vs otro polígono ===
        public bool Intersects(Polygon other)
        {
            // Broad phase
            if (!AABB.Intersects(other.AABB))
                return false;

            // Mis ejes
            for (int i = 0; i < Axes.Count; i++)
            {
                if (IsSeparated(Axes[i], other))
                    return false;
            }

            // Ejes del otro
            for (int i = 0; i < other.Axes.Count; i++)
            {
                if (IsSeparated(other.Axes[i], other))
                    return false;
            }

            return true;
        }

        // === Check de eje separador ===
        private bool IsSeparated(Vector2 axis, Polygon other)
        {
            Project(axis, out float minA, out float maxA);
            other.Project(axis, out float minB, out float maxB);

            return (minA > maxB || minB > maxA);
        }

        // === Círculo vs Polígono ===
        public bool IntersectsCircle(Vector2 center, float radius)
        {
            // Broad phase
            if (!AABB.Grow(radius).HasPoint(center))
                return false;

            // Ejes del polígono
            for (int i = 0; i < Axes.Count; i++)
            {
                if (IsSeparatedCircle(Axes[i], center, radius))
                    return false;
            }

            // Eje extra: centro → vértice más cercano
            Vector2 closest = WorldPoints[0];
            float minDist = center.DistanceSquaredTo(closest);

            for (int i = 1; i < Count; i++)
            {
                float dist = center.DistanceSquaredTo(WorldPoints[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = WorldPoints[i];
                }
            }

            Vector2 axisToVertex = closest - center;
            if (axisToVertex != Vector2.Zero)
            {
                if (IsSeparatedCircle(axisToVertex, center, radius))
                    return false;
            }

            return true;
        }

        private bool IsSeparatedCircle(Vector2 axis, Vector2 center, float radius)
        {
            Project(axis, out float minP, out float maxP);

            float projCenter = center.Dot(axis);
            float minC = projCenter - radius;
            float maxC = projCenter + radius;

            return (minP > maxC || minC > maxP);
        }
        private static readonly Vector2[] RectAxes =
 {
    new Vector2(1, 0),
    new Vector2(0, 1)
};

        public bool PolygonVsAARect(
            Vector2 rectPos,
            float width,
            float height)
        {
            float hw = width * 0.5f;
            float hh = height * 0.5f;

            Vector2[] rectPoints =
            {
        new Vector2(rectPos.X - hw, rectPos.Y - hh),
        new Vector2(rectPos.X + hw, rectPos.Y - hh),
        new Vector2(rectPos.X + hw, rectPos.Y + hh),
        new Vector2(rectPos.X - hw, rectPos.Y + hh),
    };

            // 1️⃣ Ejes del polígono
            for (int i = 0; i < Axes.Count; i++)
            {
                if (IsSeparatedPolygonVsRect(Axes[i], rectPoints))
                    return false;
            }

            // 2️⃣ Ejes del rectángulo
            for (int i = 0; i < RectAxes.Length; i++)
            {
                if (IsSeparatedPolygonVsRect(RectAxes[i], rectPoints))
                    return false;
            }

            return true;
        }
        private bool IsSeparatedPolygonVsRect(Vector2 axis, Vector2[] rect)
        {
            // Polígono
            Project(axis, out float minP, out float maxP);

            // Rectángulo
            float minR = rect[0].Dot(axis);
            float maxR = minR;

            for (int i = 1; i < 4; i++)
            {
                float d = rect[i].Dot(axis);
                if (d < minR) minR = d;
                else if (d > maxR) maxR = d;
            }

            return (minP > maxR || minR > maxP);
        }

        private bool SeparatedOnX(float rectMinX, float rectMaxX)
        {
            float minP = WorldPoints[0].X;
            float maxP = WorldPoints[0].X;

            for (int i = 1; i < WorldPoints.Count; i++)
            {
                float x = WorldPoints[i].X;
                if (x < minP) minP = x;
                else if (x > maxP) maxP = x;
            }

            return (minP > rectMaxX || rectMinX > maxP);
        }

        private bool SeparatedOnY(float rectMinY, float rectMaxY)
        {
            float minP = WorldPoints[0].Y;
            float maxP = WorldPoints[0].Y;

            for (int i = 1; i < WorldPoints.Count; i++)
            {
                float y = WorldPoints[i].Y;
                if (y < minP) minP = y;
                else if (y > maxP) maxP = y;
            }

            return (minP > rectMaxY || rectMinY > maxP);
        }

        private bool IsSeparatedOnAxis(
        Vector2 axis, 
        float rectMin,
        float rectMax)
        {
            Project(axis, out float minP, out float maxP);
            return (minP > rectMax || rectMin > maxP);
        }

        public override Godot.Vector2 GetSizeQuad()
        {
            return  AABB.Size;
        }

        public override GeometricShape2D Multiplicity(float value)
        {
            //no se necesita
            throw new NotImplementedException();
        }

        public override GeometricShape2D MultiplicityInternal(float value)
        {
            //no se necesita
            throw new NotImplementedException();
        }

        public override Godot.Vector2[] GetVertices(Godot.Vector2 position)
        {
            throw new NotImplementedException();
        }

       
    }
}
