using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;

public partial class WindowDataEditor : Window
{
    public event Action<object> OnItemSelected;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        KuroItemsData.OnObjectPressed += KuroItemsData_OnObjectPressed;
        KuroOptionButtonMod.OnDataSelected += KuroOptionButtonMod_OnDataSelected;
        LoadMods();
    }

    private void LoadMods()
    {
        var items =TableMods.Instance.ObtenerTodos();
        foreach (var item in items)
        {            
            KuroOptionButtonMod.AddItemWithData(item.Value.Name, item.Value);
        }
    }

    private void KuroOptionButtonMod_OnDataSelected(object obj)
    {
        throw new NotImplementedException();
    }

    private void KuroItemsData_OnObjectPressed(object obj)
    {
        OnItemSelected?.Invoke(obj);
        QueueFree();
    }

    public void LoadData<T>() where T : class
    {
        
        var modInfo = (ModInfo)KuroOptionButtonMod.GetSelectedData();
        Type parentType = typeof(T).BaseType;
        if (parentType == typeof(IdDataLong))
        {
            KuroItemsData.ReloadObjectsByModIDLong<T>(modInfo.Name);
        }
        else if (parentType == typeof(IdData))
        {
            KuroItemsData.ReloadObjectsByModID<T>(modInfo.Name);
        }
        else
        {
            throw new Exception($"Tipo no soportado: {typeof(T).Name}");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
