using Godot;
using GodotEcsArch.sources.KuroTiles;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using static CustomButtonRule;

public partial class ControlListTileSprite : MarginContainer
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        PanelContainerTile.GuiInput += HBoxContainerTiles_GuiInput;
    }
    public List<long> GetIdTiles()
    {
        List<long> ids = new List<long>();
        foreach (var item in HBoxContainerTiles.GetChildren())
        {
            long id = (long)item.GetMeta("id");
            ids.Add(id);
        }
        return ids;
    }
    public void SetIdTiles(List<long> ids)
    {        
        foreach (var id in ids)
        {
            var dataTile = TileSpriteManager.Instance.GetData(id);
            WindowTile_OnNotifySelected(dataTile);
        }
    }
    private void HBoxContainerTiles_GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    FacadeWindowDataSearch<TileSpriteData> windowTile = new FacadeWindowDataSearch<TileSpriteData>("res://sources/WindowsDataBase/TileSprite/WindowTileSprite.tscn", this, WindowType.SELECTED,true,true);
                    windowTile.EnableFilterGrouping(true);

                    windowTile.OnNotifySelected += WindowTile_OnNotifySelected;
                    break;
            }
        }
    }

    private void WindowTile_OnNotifySelected(TileSpriteData objectSelected)
    {
        TextureRect textureRect = new TextureRect();
        textureRect.SetMeta("id", objectSelected.id);
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
        switch (objectSelected.tileSpriteType)
        {
            case TileSpriteType.Static:
                textureRect.Texture = MaterialManager.Instance.GetAtlasTextureInternal(objectSelected.spriteData.idMaterial,
            objectSelected.spriteData.x, objectSelected.spriteData.y, objectSelected.spriteData.widht, objectSelected.spriteData.height);
                break;
            case TileSpriteType.Animated:
                textureRect.Texture = objectSelected.textureVisual;
                break;
            default:
                break;
        }
        
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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    internal void SetData(List<TileTemplate> randomTiles)
    {
        foreach (var item in randomTiles)
        {
            var dta = TileSpriteManager.Instance.GetData(item.idTileSprite);
            WindowTile_OnNotifySelected(dta);
        }
        
    }
}
