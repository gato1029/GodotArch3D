using Godot;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ControlListTextures : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
     
		InitializeUI(); // Insertado por el generador de UI

        //AnimationStateData		
       PanelContainerTile.GuiInput += HBoxContainerTiles_GuiInput;
       // ButtonAdd.Pressed += ButtonAdd_Pressed;
    }

    public List<TileInfoKuro> dataInfo=null;
    public int idMaterial { get; set; } = -1;
    WindowSearchTileMaterial windowLocal = null;

    public List<TileInfoKuro> GetData()
    {    
        return dataInfo;
    }
    public void SetData(List<TileInfoKuro> tiles)
    {
        dataInfo = tiles;
        foreach (var item in HBoxContainerTiles.GetChildren())
        {
            HBoxContainerTiles.RemoveChild(item);
            item.QueueFree();
        }
        foreach (var item in tiles)
        {
            idMaterial = item.idMaterial;
            TextureRect textureRect = new TextureRect();
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
            textureRect.Texture = item.texture;
            // Aseguramos que pueda recibir eventos
            textureRect.MouseFilter = Control.MouseFilterEnum.Stop;
            // Asignamos el evento de click
            textureRect.GuiInput += (InputEvent @event) =>
            {
                if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
                {
                    if (mouseEvent.ButtonIndex == MouseButton.Right)
                    {
                        // Eliminamos el TextureRect del contenedor
                        HBoxContainerTiles.RemoveChild(textureRect);
                        textureRect.QueueFree();
                        OnNotifyChangued?.Invoke(this);
                    }
                }
            };
            HBoxContainerTiles.AddChild(textureRect);
        }
    }
    private void HBoxContainerTiles_GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    windowLocal = GD.Load<PackedScene>("res://sources/KuroTiles/WindowSearchTileMaterial.tscn").Instantiate<WindowSearchTileMaterial>();                    
                    // Escuchar cuando se cierre
                    AddChild(windowLocal);

                    windowLocal.SetMultipleSelectionMode(true);
                    windowLocal.TreeExited += () => windowLocal = null;
                    //windowLocal.PopupHide += () => windowLocal = null;
                    // 🔸 Colocar al costado derecho del Control
                    Vector2I globalPos = (Vector2I)GetScreenPosition();
                    Vector2I size = (Vector2I)GetGlobalRect().Size;

                    // Si la ventana tiene tamaño fijo:
                    windowLocal.Position = new Vector2I(globalPos.X + size.X + 10, globalPos.Y);
                    windowLocal.OnNotifyMultiSelection += WindowLocal_OnNotifyMultiSelection;
                    windowLocal.Popup();
                    if (idMaterial != -1)
                    {
                        windowLocal.SetSelectionMultiple(idMaterial, dataInfo);
                    }
                    break;
            }
        }
    }

    private void WindowLocal_OnNotifyMultiSelection(System.Collections.Generic.List<TileInfoKuro> tiles)
    {
        dataInfo = tiles;
        foreach (var item in HBoxContainerTiles.GetChildren())
        {
            HBoxContainerTiles.RemoveChild(item);
            item.QueueFree();
        }
        foreach (var item in tiles)
        {
            idMaterial = item.idMaterial;
            TextureRect textureRect = new TextureRect();            
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
            textureRect.Texture = item.texture;
            // Aseguramos que pueda recibir eventos
            textureRect.MouseFilter = Control.MouseFilterEnum.Stop;

            // Asignamos el evento de click
            textureRect.GuiInput += (InputEvent @event) =>
            {
                if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
                {
                    if (mouseEvent.ButtonIndex == MouseButton.Right)
                    {
                        // Eliminamos el TextureRect del contenedor
                        HBoxContainerTiles.RemoveChild(textureRect);
                        textureRect.QueueFree();
                        OnNotifyChangued?.Invoke(this);
                    }
                }
            };
            HBoxContainerTiles.AddChild(textureRect);            
        }
        OnNotifyChangued?.Invoke(this);
    }

    private void WindowTile_OnNotifySelected(TileSpriteData objectSelected)
    {
        TextureRect textureRect = new TextureRect();
        textureRect.SetMeta("id", objectSelected.id);
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        textureRect.Texture = MaterialManager.Instance.GetAtlasTextureInternal(objectSelected.spriteData.idMaterial,
            objectSelected.spriteData.x, objectSelected.spriteData.y, objectSelected.spriteData.widht, objectSelected.spriteData.height);
        // Aseguramos que pueda recibir eventos
        textureRect.MouseFilter = Control.MouseFilterEnum.Stop;

        // Asignamos el evento de click
        textureRect.GuiInput += (InputEvent @event) =>
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    // Eliminamos el TextureRect del contenedor
                    HBoxContainerTiles.RemoveChild(textureRect);
                    textureRect.QueueFree();
                    OnNotifyChangued?.Invoke(this);
                }
            }
        };

        HBoxContainerTiles.AddChild(textureRect);
        OnNotifyChangued?.Invoke(this);
    }

    public override void _Process(double delta)
	{
	}
}
