using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;

public partial class ImageSelectionControl : CenterContainer
{
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private bool _isSelecting = false;
    private ColorRect _selectionRect;
    private TextureRect _image;
    private const int GRID_SIZE = 16; // Tamaño de la cuadrícula
    [Export] Sprite2D _selectionSprite;
    [Export] GridDrawUI _gridUI;
    MaterialData materialData;
    
    public override void _Ready()
    {
        _selectionRect = GetNode<ColorRect>("ColorRect");
        _image = GetNode<TextureRect>("TextureRect");
        
        _selectionRect.Modulate = new Color(1, 1, 1, 0.5f); // Semi-transparente
        _selectionRect.Visible = false;
    }
    public void SetMaterial(int idMaterial)
    {
        materialData = MaterialManager.Instance.GetMaterial(idMaterial);
        _image.Texture = (Texture2D) materialData.textureMaterial;
        _gridUI.Redraw(new Vector2(materialData.widhtTexture, materialData.heightTexture));
    }
    public override void _Input(InputEvent @event)
    {      
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed && IsMouseInsideTexture(mouseEvent.Position))
                {
                    _startPosition = MouseToTextureRect(mouseEvent.Position);
                    _isSelecting = true;
                    _selectionRect.Visible = true;
                }
                else if(!mouseEvent.Pressed && IsMouseInsideTexture(mouseEvent.Position))
                {
                    _isSelecting = false;
                    ProcessSelection();
                }
            }
        }
        else if (@event is InputEventMouseMotion motionEvent && _isSelecting)
        {
            if (IsMouseInsideTexture(motionEvent.Position))
            {
                _endPosition = MouseToTextureRect(motionEvent.Position);
                UpdateSelectionRect();
            }
        }
    }
    private bool IsMouseInsideTexture(Vector2 mousePos)
    {
        Vector2 imagePos = _image.GlobalPosition;
        Vector2 imageSize = _image.Size * _image.Scale; // Ajuste por zoom

        return mousePos.X >= imagePos.X && mousePos.X <= imagePos.X + imageSize.X &&
               mousePos.Y >= imagePos.Y && mousePos.Y <= imagePos.Y + imageSize.Y;
    }
    private void UpdateSelectionRect()
    {
        var minX = Mathf.Min(_startPosition.X, _endPosition.X);
        var minY = Mathf.Min(_startPosition.Y, _endPosition.Y);
        var maxX = Mathf.Max(_startPosition.X, _endPosition.X);
        var maxY = Mathf.Max(_startPosition.Y, _endPosition.Y);

        _selectionRect.Position = new Vector2(minX, minY);
        _selectionRect.Size = new Vector2(maxX - minX, maxY - minY);
    }

    private void ProcessSelection()
    {
        if (materialData!=null && _selectionRect.Size.X>0 && _selectionRect.Size.Y > 0)
        {
            _selectionSprite.Texture = MaterialManager.Instance.GetAtlasTexture(materialData.id, (int)_selectionRect.Position.X, (int)_selectionRect.Position.Y, _selectionRect.Size.X, _selectionRect.Size.Y);
        }
    
    }

    private Vector2 MouseToTextureRect(Vector2 mousePosition)
    {
        // Restar la posición global del TextureRect para obtener coordenadas relativas
        Vector2 localPos = mousePosition - _image.GlobalPosition;

        // Ajustar a la cuadrícula de 16x16
        return SnapToGrid(ClampToTextureRect(localPos));
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Round(position.X / GRID_SIZE) * GRID_SIZE,
            Mathf.Round(position.Y / GRID_SIZE) * GRID_SIZE
        );
    }

    private Vector2 ClampToTextureRect(Vector2 position)
    {
        Vector2 textureSize = _image.Size;

        float clampedX = Mathf.Clamp(position.X, 0, textureSize.X - GRID_SIZE);
        float clampedY = Mathf.Clamp(position.Y, 0, textureSize.Y - GRID_SIZE);

        // Si la imagen es múltiplo de GRID_SIZE, permitir la última celda
        if (textureSize.Y % GRID_SIZE == 0 && position.Y > textureSize.Y - GRID_SIZE)
        {
            clampedY = textureSize.Y;
        }
        if (textureSize.X % GRID_SIZE == 0 && position.X > textureSize.X - GRID_SIZE)
        {
            clampedX = textureSize.X;
        }
        return new Vector2(clampedX, clampedY);
    }
}

