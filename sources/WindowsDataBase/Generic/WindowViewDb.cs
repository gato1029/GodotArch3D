using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

public partial class WindowViewDb : Window
{
    bool onlyIcons = false;
    public delegate void RequestItemSelectedHandler(int id);
    public event RequestItemSelectedHandler OnRequestSelectedItem;


    public delegate void RequestFilterMaterial(int idMaterial);
    public event RequestFilterMaterial OnRequestFilterMaterial;

    // Called when the node enters the scene tree for the first time.

    private int materialFilter = 0;
    public override void _Ready()
	{
		InitializeUI();
        Items.ItemSelected += Items_ItemSelected;
        ButtonFindMaterial.Pressed += ButtonFindMaterial_Pressed;
        ButtonFindCustom.Pressed += ButtonFindCustom_Pressed;
        WorkMode();
    }

    private void ButtonFindCustom_Pressed()
    {
        WindowViewDb windowViewDb = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowViewDB.tscn").Instantiate<WindowViewDb>();
        AddChild(windowViewDb);
        windowViewDb.Show();
        windowViewDb.LoadItems<MaterialData>();
        windowViewDb.OnRequestSelectedItem += WindowViewDb_OnRequestSelectedItem;
    }

    public void EnableFilter(bool enable=true)
    {
        ButtonFindMaterial.Visible =enable;
    }
    private void ButtonFindMaterial_Pressed()
    {
        WindowViewDb windowViewDb = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowViewDB.tscn").Instantiate<WindowViewDb>();
        AddChild(windowViewDb);
        windowViewDb.Show();
        windowViewDb.LoadItems<MaterialData>();
        windowViewDb.OnRequestSelectedItem += WindowViewDb_OnRequestSelectedItem;
    }

    private void WindowViewDb_OnRequestSelectedItem(int id)
    {
        materialFilter = id;
        OnRequestFilterMaterial?.Invoke(materialFilter);
    }

    private void Items_ItemSelected(long index)
    {
        int id = (int)Items.GetItemMetadata((int)index);       
        OnRequestSelectedItem?.Invoke(id);
        QueueFree();

    }

    public void WorkMode(bool OnlyIcons=false)
    {
        onlyIcons = OnlyIcons;
        if (OnlyIcons)
        {
            Items.MaxColumns = 30;
        }
        else
        {
            Items.IconScale = 2;
            Items.MaxColumns = 1;
            Items.IconMode = ItemList.IconModeEnum.Left;
        }
    }
    public void LoadItemsByMaterial<T>() where T : class
    {
        Items.Clear();
        var list = DataBaseManager.Instance.FindAllByMaterial<T>(materialFilter);
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            IdData idData = item as IdData;
            if (onlyIcons)
            {
                int idx = Items.AddIconItem(idData.textureVisual);
                Items.SetItemMetadata(idx, idData.id);
            }
            else
            {
                int idx = Items.AddItem("ID:" + idData.id + " Nombre:" + idData.name, idData.textureVisual);
                Items.SetItemMetadata(idx, idData.id);
            }

        }
    }

    public void LoadItems<T>() where T : class
	{
        Items.Clear();
        var list = DataBaseManager.Instance.FindAll<T>();
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
			IdData idData = item as IdData;
            if (onlyIcons)
            {
                int idx = Items.AddIconItem(idData.textureVisual);
                Items.SetItemMetadata(idx, idData.id);
            }
            else
            {
                int idx = Items.AddItem("ID:" + idData.id + " Nombre:" + idData.name, idData.textureVisual);
                Items.SetItemMetadata(idx, idData.id);
            }
			
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
