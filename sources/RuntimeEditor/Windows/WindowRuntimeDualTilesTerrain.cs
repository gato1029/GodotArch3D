using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Linq;

public partial class WindowRuntimeDualTilesTerrain : Window
{
    public delegate void EventOnSelection(DualTileTemplate objeto);
    public event EventOnSelection OnSelection;
    
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI        
        ListaTipos.OnDataSelected += ListaTipos_OnDataSelected;
        ItemsList.OnDataSelected += ItemsList_OnDataSelected;
        CargarMods();

    }
   
    private void ItemsList_OnDataSelected(object obj)
    {        
        OnSelection?.Invoke((DualTileTemplate)obj);
        QueueFree();
    }

    private void ListaTipos_OnDataSelected(object obj)
    {
        CargarTerrenosDuales((ModInfo)obj);
    }
    private void CargarTerrenosDuales(ModInfo modInfo)
    {
        var items = AtlasModsManager.GetAll<DualTileTemplate>(modInfo.Name);
        foreach (var item in items)
        {      
           ItemsList.AddItemWithData(item.name, item, item.textureVisual);            
        }
    }



    private void CargarMods()
    {
        var listMods = TableMods.Instance.ObtenerTodos().ToList();
        foreach (System.Collections.Generic.KeyValuePair<byte, ModInfo> item in listMods)
        {
            ListaTipos.AddItemWithData(item.Value.Name, item.Value);
        }
        CargarTerrenosDuales(listMods[0].Value);
    }
}
