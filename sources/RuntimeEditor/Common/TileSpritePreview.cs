using Godot;
using System;

public partial class TileSpritePreview : TextureRect
{
    TileSpriteData tileSpriteData;
    private object _dataInternal;

    [Export]
    public bool isSelectorEnabled = false;

    public delegate void OnItemSelected(TileSpriteData objectControl, object dataInternal);
    public event OnItemSelected OnItemSelectedChanged;

    // Colores originales y de efecto
    private Color normalColor = new Color(1, 1, 1, 1);
    private Color hoverColor = new Color(1.2f, 1.2f, 1.2f, 1); // Más brillante
    private Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1); // Más oscuro al presionar

    public override void _Ready()
    {
        InitializeUI();
        MouseFilter = MouseFilterEnum.Stop;
        MouseEntered += TileSpritePreview_MouseEntered;
        MouseExited += TileSpritePreview_MouseExited;
        // Asegurar que el pivote esté en el centro si deseas escalar
        PivotOffset = Size / 2;
    }

    private void TileSpritePreview_MouseExited()
    {
        // Efecto cuando el mouse entra al control
        Modulate = hoverColor;
        Scale = new Vector2(1.05f, 1.05f); // Pequeño zoom opcional
    }

    private void TileSpritePreview_MouseEntered()
    {
        // Regresa al estado normal cuando el mouse sale
        Modulate = normalColor;
        Scale = Vector2.One;
    }

    public override void _Process(double delta)
    {
    }

    public void EnableSelector()
    {
        isSelectorEnabled = true;
    }

    public void LoadData(TileSpriteData data, object dataInternal = null)
    {
        tileSpriteData = data;
        SpriteTexture.Texture = data.textureVisual;
        _dataInternal = dataInternal;
    }

    public object GetDataInternal()
    {
        return _dataInternal;
    }

    public TileSpriteData GetTileSpriteData()
    {
        return tileSpriteData;
    }



    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    // Efecto visual al presionar
                    Modulate = pressedColor;

                    if (isSelectorEnabled)
                    {
                        OnSelectionClick();
                    }
                    else
                    {
                        OnSingleClick();
                    }
                }
                else
                {
                    // Al soltar el click, regresa al color de hover si el mouse sigue dentro
                    Modulate = hoverColor;
                }
            }
        }
    }

    private void OnSelectionClick()
    {
        WindowMaterialTiles wm = RuntimeServices.NodeRegistry.Create<WindowMaterialTiles>();
        AddChild(wm);
        wm.OnItemSelected += (TileSpriteData obj) =>
        {
            tileSpriteData = obj;
            SpriteTexture.Texture = obj.textureVisual;
            OnNotifyChangued?.Invoke(this);
            OnItemSelectedChanged?.Invoke(tileSpriteData, _dataInternal);
        };
        wm.Show();
    }

    private void OnSingleClick()
    {
        OnNotifyChangued?.Invoke(this);
        OnItemSelectedChanged?.Invoke(tileSpriteData, _dataInternal);
    }
}