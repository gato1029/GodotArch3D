using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Reflection;

public partial class WindowMaterial : Window
{
    ItemList itemListTiles;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        CloseRequested += WindowTileCreator_CloseRequested;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/Button").Pressed += New_Click;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/Button2").Pressed += Update_Click;
        

        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/ItemList");
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;      
        LoadDB();
    }

    private void Update_Click()
    {
        if (itemListTiles.GetSelectedItems().Length > 0)
        {
            int id = (int)itemListTiles.GetItemMetadata(itemListTiles.GetSelectedItems()[0]);
            if (id != -1)
            {
                Window window = GD.Load<PackedScene>("res://sources/WindowsDataBase/Materials/windowNewMaterial.tscn").Instantiate<Window>();
                ((IDetailWindow)window).OnRequestUpdate += Window_OnRequestUpdate;
                AddChild(window);

                ((IDetailWindow)window).LoadData(id);
                window.Show();
            }
        }
        
    }

    private void Window_OnRequestUpdate()
    {
        LoadDB();
    }

    private void New_Click()
    {
        Window window = GD.Load<PackedScene>("res://sources/WindowsDataBase/Materials/windowNewMaterial.tscn").Instantiate<Window>();
        ((IDetailWindow)window).OnRequestUpdate += Window_OnRequestUpdate;
        AddChild(window);      
        window.Show();
    }

    private void WindowTileCreator_CloseRequested()
    {
        QueueFree();
    }

    private void ItemListTiles_ItemSelected(long index)
    {
        int id = (int)itemListTiles.GetItemMetadata((int)index);
    }

    private void LoadDB()
    {
        var lista = DataBaseManager.Instance.FindAll<MaterialData>();
        itemListTiles.Clear();
        foreach (MaterialData item in lista)
        {
            var texture = TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(item.pathTexture));
            int idx =   itemListTiles.AddItem("ID:"+item.id.ToString()+"\n" + item.name, (Texture2D)texture);            
            itemListTiles.SetItemMetadata(idx, item.id);            
        }
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
