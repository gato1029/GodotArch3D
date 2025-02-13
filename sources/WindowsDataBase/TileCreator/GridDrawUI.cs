using Godot;
using System;

public partial class GridDrawUI : ColorRect
{
    private const int GRID_SIZE = 16;
    private Color gridColor = new Color(45, 45, 45, 0.2f); // Blanco con transparencia
    [Export] TextureRect _image;
    private ShaderMaterial _shaderMaterial;
    public override void _Ready()
    {
        //_shaderMaterial = new ShaderMaterial();
        //_shaderMaterial.Shader = (Shader)GD.Load("res://resources/shaders/gridUI.gdshader"); // Ruta al shader

        //// Asignar el material al GridDrawer
        //Material = _shaderMaterial;
      
        _shaderMaterial = (ShaderMaterial)Material;
        Redraw(_image.Size);
    }
    public void Redraw(Vector2 sizeImage)
    {
        CustomMinimumSize = sizeImage;
        Size = sizeImage;
        _shaderMaterial.SetShaderParameter("size", new Vector2(sizeImage.X, sizeImage.Y));
    }
    public override void _Draw()
    {
        //if (_image == null || _image.Texture == null) return;

        //Vector2 imagePos = _image.GlobalPosition - GlobalPosition; // Posición dentro del GridDrawer
        //Vector2 imageSize = _image.Size; // Tamaño ajustado con el zoom

        //// Dibujar líneas verticales
        //for (float x = imagePos.X; x <= imagePos.X + imageSize.X; x += GRID_SIZE )
        //{
        //    DrawLine(new Vector2(x, imagePos.Y), new Vector2(x, imagePos.Y + imageSize.Y), gridColor, 1);
        //}

        //// Dibujar líneas horizontales
        //for (float y = imagePos.Y; y <= imagePos.Y + imageSize.Y; y += GRID_SIZE )
        //{
        //    DrawLine(new Vector2(imagePos.X, y), new Vector2(imagePos.X + imageSize.X, y), gridColor, 1);
        //}
    }

    public override void _Process(double delta)
    {
        //if (_image != null)
        //{
        //    // Ajustar el tamaño de la cuadrícula basado en la escala de la imagen
        //    _shaderMaterial.SetShaderParameter("size", new Vector2(_image.Size.X, _image.Size.Y));
        //}
    }
}
