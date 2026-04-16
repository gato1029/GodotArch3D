using Godot;
using GodotFlecs.sources.Flecs.Components;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision;
internal class CollisionShapeDraw:SingletonBase<CollisionShapeDraw> 
{
    private readonly int LayerRender = 30;
    public int DrawCircleShape(float radius, Vector2 position, Godot.Color color)
    {
        return WireShape.Instance.DrawCircle(radius, position, LayerRender, color, 32, WireShape.TypeDraw.NORMAL);     
    }
    public int DrawCollisionShapes(GeometricShape2D geometricShape2D, Vector2 position, Godot.Color color)
    {
        switch (geometricShape2D)
        {
            case Rectangle rectangle:
                return WireShape.Instance.DrawSquare(rectangle.widthPixel, rectangle.heightPixel, position + rectangle.OriginCurrent, LayerRender, color, WireShape.TypeDraw.PIXEL);
                break;
            case Circle circle:
                return WireShape.Instance.DrawCircle(circle.widthPixel, position + circle.OriginCurrent, LayerRender, color, 32, WireShape.TypeDraw.PIXEL);
                break;
            case Polygon polygon:
                return WireShape.Instance.DrawPolygon(polygon.VerticesPixels, position, LayerRender, color, WireShape.TypeDraw.PIXEL);
                break;
            default:
                break;
        }
        return 0;
        // Lógica para dibujar las formas de colisión
    }
    public int DrawCollisionShapes(GeometricShape2D geometricShape2D, Vector2 position)
    {
        switch (geometricShape2D)
        {
            case Rectangle rectangle:
                return WireShape.Instance.DrawSquare(rectangle.widthPixel, rectangle.heightPixel, position + rectangle.OriginCurrent, LayerRender, Colors.OrangeRed, WireShape.TypeDraw.PIXEL);
                break;
            case Circle circle:
                return WireShape.Instance.DrawCircle(circle.widthPixel, position + circle.OriginCurrent, LayerRender, Colors.OrangeRed, 32, WireShape.TypeDraw.PIXEL);
                break;
            case Polygon polygon:
                return WireShape.Instance.DrawPolygon(polygon.VerticesPixels,position, LayerRender, Colors.OrangeRed, WireShape.TypeDraw.PIXEL);
                break;
            default:
                break;
        }
        return 0;
        // Lógica para dibujar las formas de colisión
    }
    public List<int> DrawCollisionShapes(List<GeometricShape2D> shapes, Vector2 position, Godot.Color color)
    {
        List<int> listaids = new List<int>();
        foreach (var item in shapes)
        {
            listaids.Add(DrawCollisionShapes(item, position,color));
        }
        return listaids;
    }
    public List<int> DrawCollisionShapes(List<GeometricShape2D> shapes, Vector2 position)
    {
        List<int> listaids = new List<int>();
        foreach (var item in shapes)
        {
            listaids.Add( DrawCollisionShapes(item, position));
        }
        return listaids;
    }
}
