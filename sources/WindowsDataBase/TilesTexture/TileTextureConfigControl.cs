using Godot;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.Flecs.Components;
using System;

public partial class TileTextureConfigControl : PanelContainer
{
    TileTextureData data;
    FastCollider fastCollider;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        data = new TileTextureData();
        fastCollider = new FastCollider();
        ComboBoxCollider.ItemSelected += ComboBoxCollider_ItemSelected;
        // Conectamos la señal de dibujo del TextureRect
        TextureRectTile.Draw += OnTextureRectDraw;
        //TextureRectTile en este rectecxture debemos dibujar los colliders segun el tipo
        fastCollider.Width = (float)TextureRectTile.Size.X;
        fastCollider.Height = (float)TextureRectTile.Size.Y;
    }

    private void ComboBoxCollider_ItemSelected(long index)
    {
        switch (index)
        {
            case 0:
                fastCollider.Shape = ShapeType.Rect;
                break;
            case 1: 
                fastCollider.Shape = ShapeType.Circle;
                break;
            case 2:
                fastCollider.Shape = ShapeType.Slope;
                break;
            default:
                break;
        }
        data.fastCollider = fastCollider;
        TextureRectTile.QueueRedraw();
    }

    private void OnTextureRectDraw()
    {
        var size = TextureRectTile.Size;
        var color = new Color(0, 1, 0, 0.5f); // Verde semi-transparente
        var thickness = 2.0f;

        switch (fastCollider.Shape)
        {
            case ShapeType.Rect:
                TextureRectTile.DrawRect(new Rect2(Vector2.Zero, size), color, false, thickness);
                break;

            case ShapeType.Circle:
                TextureRectTile.DrawArc(size / 2, size.X / 2, 0, Mathf.Tau, 32, color, thickness);
                break;

            case ShapeType.Slope:
                DrawSlopePreview(size, color, thickness);
                break;
        }
    }

    private void DrawSlopePreview(Vector2 size, Color color, float thickness)
    {
        Vector2 p1, p2, p3;

        // Dibujamos según el Enum SlopeType que definimos antes
        switch (fastCollider.Slope)
        {
            case SlopeType.BottomLeft: // Triángulo 1
                p1 = new Vector2(0, 0); p2 = new Vector2(0, size.Y); p3 = new Vector2(size.X, size.Y);
                break;
            case SlopeType.TopRight: // Triángulo 2
                p1 = new Vector2(0, 0); p2 = new Vector2(size.X, 0); p3 = new Vector2(size.X, size.Y);
                break;
            case SlopeType.TopLeft: // Triángulo 3
                p1 = new Vector2(0, 0); p2 = new Vector2(size.X, 0); p3 = new Vector2(0, size.Y);
                break;
            case SlopeType.BottomRight: // Triángulo 4
                p1 = new Vector2(size.X, 0); p2 = new Vector2(size.X, size.Y); p3 = new Vector2(0, size.Y);
                break;
            default: return;
        }

        // Dibujamos el triángulo
        Vector2[] points = { p1, p2, p3, p1 };
        TextureRectTile.DrawPolyline(points, color, thickness);
    }

}



