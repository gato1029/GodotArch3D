using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Reflection;
using System.Collections.Generic;


public partial class WindowDataGeneric : Window
{
    Type type;
    ItemList itemListTiles;
    Func<List<IdData>> loadDataDB;
    PackedScene packedSceneWindowDetail;
    WindowState windowState;
    PopupMenu menuButton;

    MaterialData currentMaterialData;
    public delegate void RequestItemSelectedHandler(int id);
    public event RequestItemSelectedHandler OnRequestSelectedItem;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CloseRequested += WindowTileCreator_CloseRequested;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/Button").Pressed += New_Click;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/Button2").Pressed += Update_Click;

        menuButton = GetNode<PopupMenu>("Panel/MarginContainer/VBoxContainer/MenuBar/Materiales");
        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/ItemList");
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;
        windowState = WindowState.CRUD;
        LoadMaterials();

        menuButton.IdPressed += MenuItemClick;
    }

    private void MenuItemClick(long id)
    {
        int idInternal = (int)id;
    }

    private void LoadMaterials()
    {
        var list = DataBaseManager.Instance.FindAll<MaterialData>();
        for (int i = 0; i < list.Count; i++)
        {
            MaterialData item = list[i];
            menuButton.AddItem(item.name,item.id);
          //  optionMaterial.SetItemMetadata(i + 1, item.id);
        }
    }


    public void SetLoaddBAction(Func<List<IdData>> function)
    {
        loadDataDB = function;
        LoadDB();
    }
    public void SetWindowDetail(PackedScene InpackedSceneWindowDetail, WindowState windowState, string nameWindow,bool oneLineItems=false)
    {
        if (oneLineItems)
        {
            itemListTiles.MaxColumns = 1;
            itemListTiles.IconMode = ItemList.IconModeEnum.Left;
        }
        this.Title = nameWindow;
        this.windowState = windowState; 

        packedSceneWindowDetail = InpackedSceneWindowDetail;
        if (windowState == WindowState.SELECTOR)
        {
            GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/Button").Visible = false;
            GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/Button2").Visible = false;
        }
    }
    private void Update_Click()
    {
        if (itemListTiles.GetSelectedItems().Length > 0)
        {
            int id = (int)itemListTiles.GetItemMetadata(itemListTiles.GetSelectedItems()[0]);
            if (id != -1)
            {
                Window window = packedSceneWindowDetail.Instantiate<Window>();
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
        Window window = packedSceneWindowDetail.Instantiate<Window>();
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
        if (windowState == WindowState.SELECTOR)
        {
            OnRequestSelectedItem?.Invoke(id);
            QueueFree();
        }
    }
   

    private void LoadDB()
    {
        var result =loadDataDB?.Invoke();
        itemListTiles.Clear();
        foreach (IdData item in result)
        {            
            int idx = itemListTiles.AddItem("ID:" + item.id.ToString() + "\n" + item.name,item.textureVisual);
            itemListTiles.SetItemMetadata(idx, item.id);
        }

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
