using Godot;
using GodotEcsArch.sources.KuroTiles;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
public partial class ControlListResourcesSources : MarginContainer
{
    public override void _Ready()
    {
        InitializeUI(); // Generado por el UI Builder
        PanelContainerTile.GuiInput += HBoxContainerTiles_GuiInput;
    }

    public List<ResourceSourceData> dataInfo = null;    
    WindowDataSearch windowLocal = null;

    // 🔸 Devuelve la lista de ResourceEntry con su probabilidad
    public List<ResourceEntry> GetData()
    {
        List<ResourceEntry> entries = new List<ResourceEntry>();

        foreach (var child in HBoxContainerTiles.GetChildren())
        {
            if (child is VBoxContainer vbox)
            {
                if (vbox.GetChildCount() < 2)
                    continue;

                var texture = vbox.GetChild<TextureRect>(0);
                var spin = vbox.GetChild<SpinBox>(1);

                long id = (long)texture.GetMeta("id");
                float probability = (float)spin.Value;
                var template = MasterDataManager.GetData<ResourceSourceData>(id);
                entries.Add(new ResourceEntry(id, probability,template.idSave,template.resourceSourceType));
            }
        }

        return entries;
    }

    // 🔸 Carga los recursos desde la lista
    internal void SetData(List<ResourceEntry> entries)
    {
        foreach (var entry in entries)
        {
            var data = MasterDataManager.GetData<ResourceSourceData>(entry.ResourceSourceId);
            if (data != null)
                AddResourceTile(data, entry.Probability);
        }
    }

    private void HBoxContainerTiles_GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    FacadeWindowDataSearch<ResourceSourceData> winResources =
                        new FacadeWindowDataSearch<ResourceSourceData>(
                            "res://sources/WindowsDataBase/Resources/WindowResources.tscn",
                            this,
                            WindowType.SELECTED
                        );

                    windowLocal = winResources.windowDataSearch;
                    windowLocal.TreeExited += () => windowLocal = null;

                    Vector2I globalPos = (Vector2I)GetScreenPosition();
                    Vector2I size = (Vector2I)GetGlobalRect().Size;

                    windowLocal.Position = new Vector2I(globalPos.X + size.X + 10, globalPos.Y);
                    windowLocal.Popup();

                    winResources.OnNotifySelected += WinResources_OnNotifySelected;
                    break;
            }
        }
    }

    // 🔸 Cuando el usuario selecciona un recurso
    private void WinResources_OnNotifySelected(ResourceSourceData objectSelected)
    {
        AddResourceTile(objectSelected, 1f);
    }

    // 🔸 Crea un bloque (VBox) con la textura y spinbox
    private void AddResourceTile(ResourceSourceData data, float probability)
    {
        // VBox para agrupar textura + spin
        VBoxContainer vbox = new VBoxContainer
        {                    
            CustomMinimumSize = new Vector2(32, 32)
        };
        vbox.SizeFlagsVertical = SizeFlags.Expand;
        

        // Textura
        TextureRect textureRect = new TextureRect();
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeight;
        textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        textureRect.CustomMinimumSize = new Vector2(32, 32);
        textureRect.Size = new Vector2(32, 32);
        textureRect.Texture = data.textureVisual;
        textureRect.SetMeta("id", data.id);
        textureRect.MouseFilter = Control.MouseFilterEnum.Stop;

        // SpinBox
        SpinBox spinBox = new SpinBox
        {
            MinValue = 0,
            MaxValue = 100,
            Step = 0.1f,
            Value = probability,
            Suffix = " %",
            TooltipText = "Probabilidad de aparición del recurso"
        };
        spinBox.SetMeta("id", data.id);
        spinBox.ValueChanged += (double newValue) =>
        {    
            OnNotifyChangued?.Invoke(this);
        };

        // Click derecho para eliminar el bloque
        textureRect.GuiInput += (InputEvent @event) =>
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    HBoxContainerTiles.RemoveChild(vbox);
                    vbox.QueueFree();
                    OnNotifyChangued?.Invoke(this);
                }
            }
        };

        // Agregar en orden: textura arriba, spin abajo
        vbox.AddChild(textureRect);
        vbox.AddChild(spinBox);

        // Añadir el bloque al contenedor principal
        HBoxContainerTiles.AddChild(vbox);
       // HBoxContainerTiles.AddChild(textureRect);
        OnNotifyChangued?.Invoke(this);
    }

}

