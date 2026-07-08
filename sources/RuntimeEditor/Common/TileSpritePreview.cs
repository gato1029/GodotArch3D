using Godot;
using System;

public partial class TileSpritePreview : TextureRect
{   
    TileSpriteData tileSpriteData;
    [Export]
    public bool isSelectorEnabled = false;

    public delegate void OnItemSelected(TileSpriteData objectControl);
    public event OnItemSelected OnItemSelectedChanged;

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
    
    public void EnableSelector()
    {
        isSelectorEnabled = true;
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
                if (isSelectorEnabled)
                {
                    OnSelectionClick();
                }
                else
                {
                    OnSingleClick();
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
            OnItemSelectedChanged?.Invoke(tileSpriteData);
        };        
        wm.Show();
    }    

    private void OnSingleClick()
    {
        OnNotifyChangued?.Invoke(this);
        OnItemSelectedChanged?.Invoke(tileSpriteData);        
    }    
}
