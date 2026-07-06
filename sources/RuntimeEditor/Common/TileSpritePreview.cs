using Godot;
using System;

public partial class TileSpritePreview : TextureRect
{
    public event Action<TileSpriteData> OnItemSelected;

    TileSpriteData tileSpriteData;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        // Aseguramos que el control pueda recibir eventos de mouse
        MouseFilter = MouseFilterEnum.Stop;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
    public void LoadData(TileSpriteData data)
    {
        tileSpriteData = data;
        SpriteTexture.Texture = data.textureVisual;                
    }
    public TileSpriteData GetData()
    {
        return tileSpriteData;
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {            
                OnSingleClick();                
            }
        }
    }

    private void OnSingleClick()
    {
        OnItemSelected?.Invoke(tileSpriteData);
    }    
}
