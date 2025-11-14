using Godot;
using System;

public partial class ShapesForm : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public Vector2 positionShape { get; set; }
    public float radiusShape { get; set; }
    public Vector2 sizeShape { get; set; }

    [Export] public Color shapeColor = Colors.Gray;
    public enum ShapeType
    { 
        Ninguno,
        Circulo,
        Cuadrado
    }
    public ShapeType shapeType { get; set; } = ShapeType.Circulo;
    public override void _Ready()
	{
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        QueueRedraw();
    }
    public override void _Draw()
    {
 
        switch (shapeType)
        {
            case ShapeType.Circulo:                
                DrawCircle(positionShape, radiusShape, shapeColor);
                break;
            case ShapeType.Cuadrado:
                Rect2 rect = new Rect2(positionShape - sizeShape / 2f, sizeShape);
                DrawRect(rect, shapeColor);
                break;
            default:
                break;
        }
     
    }

}
