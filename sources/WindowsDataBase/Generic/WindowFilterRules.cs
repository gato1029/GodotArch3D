using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System.Collections.Generic;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;


public partial class WindowFilterRules : Window
{
    ItemList itemListTiles;      
    WindowState windowState;
	OptionButton optionButton;

    MaterialData currentMaterialData;
    public delegate void RequestItemSelectedHandler(int id);
    public event RequestItemSelectedHandler OnRequestSelectedItem;
    public override void _Ready()
	{
        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/ItemList");
        optionButton = GetNode<OptionButton>("Panel/MarginContainer/VBoxContainer/OptionButton");
        optionButton.ItemSelected += ComboMaterial_Selected;
        optionButton.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LoadMaterials();
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;
        this.CloseRequested += WindowFilterRules_CloseRequested;
    }
    int type=1;
    public void SetType(int type)
    {
        this.type = type;
        
    }
    private void WindowFilterRules_CloseRequested()
    {
        QueueFree();
    }

    private void ComboMaterial_Selected(long index)
    {
        int id = (int)index;

        if (id > 0)
        {
            int idMat = (int)optionButton.GetItemMetadata((int)index);
            MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMat);
            currentMaterialData = materialData;   
            loadItems();
        }

    }
    private void ItemListTiles_ItemSelected(long index)
    {
        int id = (int)itemListTiles.GetItemMetadata((int)index);
        OnRequestSelectedItem?.Invoke(id);
        QueueFree();

    }
    void loadItems()
    {
        itemListTiles.Clear();
        
        switch (type)
        {
            case 1:
                var itemsA = DataBaseManager.Instance.FindAllByMaterial<TileSimpleData>(currentMaterialData.id);
                foreach (var item in itemsA)
                {
                    int idx = itemListTiles.AddItem(item.id.ToString(), item.textureVisual);
                    itemListTiles.SetItemMetadata(idx, item.id);
                }
                break;
            case 2:
                var itemsB = DataBaseManager.Instance.FindAllByMaterial<TileDynamicData>(currentMaterialData.id);
                foreach (var item in itemsB)
                {
                    int idx = itemListTiles.AddItem(item.id.ToString(), item.textureVisual);
                    itemListTiles.SetItemMetadata(idx, item.id);
                }
                break;
            case 3:
                var itemsC = DataBaseManager.Instance.FindAllByMaterial<TileAnimateData>(currentMaterialData.id);
                foreach (var item in itemsC)
                {
                    int idx = itemListTiles.AddItem(item.id.ToString(), item.textureVisual);
                    itemListTiles.SetItemMetadata(idx, item.id);
                }
                break;
            default:
                break;
        }
        
        
      
    }

    private void LoadMaterials()
    {
        var list = DataBaseManager.Instance.FindAll<MaterialData>();
        for (int i = 0; i < list.Count; i++)
        {
            MaterialData item = list[i];
            optionButton.AddItem(item.name, item.id);
            optionButton.SetItemMetadata(i + 1, item.id);
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
