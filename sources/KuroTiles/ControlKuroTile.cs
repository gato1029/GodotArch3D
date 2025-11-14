using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotFlecs.sources.KuroTiles;
using System;
using static CustomButtonRule;

public partial class ControlKuroTile : MarginContainer
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        PanelBase.GuiInput += _GuiInputPanel;
    }

    public int idMaterial { get; set; } = -1;
    public int x { get; set; } = 0;
    public int y { get; set; } = 0;
    public int width { get; set; } = 0;
    public int height { get; set; } = 0;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    WindowSearchTileMaterial windowLocal=null;
    public void _GuiInputPanel(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    if (windowLocal==null)
                    {
                        windowLocal = GD.Load<PackedScene>("res://sources/KuroTiles/WindowSearchTileMaterial.tscn").Instantiate<WindowSearchTileMaterial>();
                        // Escuchar cuando se cierre
                      
                        AddChild(windowLocal);

                        windowLocal.TreeExited += () => windowLocal = null;
                        //windowLocal.PopupHide += () => windowLocal = null;
                        // 🔸 Colocar al costado derecho del Control
                        Vector2I globalPos = (Vector2I)GetScreenPosition();
                        Vector2I size = (Vector2I)GetGlobalRect().Size;

                        // Si la ventana tiene tamaño fijo:
                        windowLocal.Position = new Vector2I(globalPos.X + size.X + 10, globalPos.Y);
                        windowLocal.OnNotifySelection += WindowLocal_OnNotifySelection;
                        windowLocal.Popup();
                        if (idMaterial!=-1)
                        {
                            
                            windowLocal.SetSelection(idMaterial,x, y, width, height);
                        }
                    }
                    break;
                case MouseButton.Right:                    
                    break;
                case MouseButton.Middle:
                    
                    break;
            }                           
        }
    }

    

    private void WindowLocal_OnNotifySelection(MaterialData materialData, float x, float y, float width, float height)
    {
        idMaterial = materialData.id;
        this.x = (int)x;
        this.y = (int)y;
        this.width = (int)width;
        this.height = (int)height;
        TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(materialData.id, x, y, width, height);
        OnNotifyChangued?.Invoke(this);
    }

    internal void SetData(int idMaterial, int x, int y, int width, int height)
    {
        this.idMaterial = idMaterial;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
        TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, x, y, width, height);
    }
    internal void SetData(int idMaterial, float x, float y, float width, float height)
    {
        this.idMaterial = idMaterial;
        this.x = (int)x;
        this.y = (int)y;
        this.width = (int)width;
        this.height = (int)height;
        MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
        TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, x, y, width, height);
    }
    public TileInfoKuro GetTileData()
    {
        return new TileInfoKuro()
        {
            idMaterial = idMaterial,
            x = x,
            y = y,
            width = width,
            height = height,
            texture = TextureImage.Texture
        };
    }
}
