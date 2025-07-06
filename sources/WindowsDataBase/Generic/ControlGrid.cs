using Godot;
using System;

[Tool]
public partial class ControlGrid : CenterContainer
{
    [Export]
    public Vector2 CellSize = new Vector2(16, 16);

    [Export]
    public Vector2I GridResolution = new Vector2I(16, 9);

    private Vector2 _lastSize = Vector2.Zero;
 //   [Export] public PackedScene LabelScene; // Escena para los números, debe contener un Label

    private Node2D _labelsContainer;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        UpdateSize();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            UpdateSize();
        }
    }

    private void UpdateSize()
    {
        Vector2 targetSize = new Vector2(
            CellSize.X * GridResolution.X,
            CellSize.Y * GridResolution.Y
        );

        if (_lastSize != targetSize)
        {
            // Cambiar tamaño y centrar
            CustomMinimumSize = targetSize;
            Size = targetSize;
            ControlRect.CustomMinimumSize = targetSize;
            SetAnchorsPreset(LayoutPreset.Center);
            SetOffsetsPreset(LayoutPreset.Center);

            _lastSize = targetSize;

            // Pasar parámetros al shader
            if (Material is ShaderMaterial shaderMaterial)
            {
                shaderMaterial.SetShaderParameter("cell_size", CellSize);
                shaderMaterial.SetShaderParameter("node_size", targetSize);
            }

            QueueRedraw();
        }

        //// Crear contenedor para los números si no existe
        //if (_labelsContainer == null)
        //{
        //    _labelsContainer = new Node2D();
        //    AddChild(_labelsContainer);
        //}

        //foreach (Node child in _labelsContainer.GetChildren())
        //{
        //    child.QueueFree();
        //}

        //int midX = GridResolution.X / 2 - 2;
        //int midY = GridResolution.Y / 2 - 2;

        //// Eje X (horizontal): números en línea central horizontal
        //for (int x = 0; x < GridResolution.X; x++)
        //{
        //    int number = x - (GridResolution.X / 2);
        //    if (LabelScene != null)
        //    {
        //        Label label = LabelScene.Instantiate<Label>();
        //        label.Text = number.ToString();
        //        label.Position = new Vector2(
        //            x * CellSize.X + CellSize.X / 2,
        //            (midY + 1) * CellSize.Y + CellSize.Y / 2
        //        );
        //        label.HorizontalAlignment = HorizontalAlignment.Center;
        //        label.VerticalAlignment = VerticalAlignment.Center;
        //        _labelsContainer.AddChild(label);
        //    }
        //}

        //// Eje Y (vertical): números en línea central vertical
        //for (int y = 0; y < GridResolution.Y; y++)
        //{
        //    if (y == midY + 1) continue; // evitar doble 0

        //    int number = (GridResolution.Y / 2) - y;
        //    number = number - 1;
        //    if (LabelScene != null)
        //    {
        //        Label label = LabelScene.Instantiate<Label>();
        //        label.Text = number.ToString();
        //        label.Position = new Vector2(
        //            (midX + 1) * CellSize.X + CellSize.X / 2,
        //            y * CellSize.Y + CellSize.Y / 2
        //        );
        //        label.HorizontalAlignment = HorizontalAlignment.Center;
        //        label.VerticalAlignment = VerticalAlignment.Center;
        //        _labelsContainer.AddChild(label);
        //    }
        //}
    }
}
