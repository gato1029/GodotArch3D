using Godot;

using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class WindowTileSimple : Window, IDetailWindow
{
    WindowState windowState;
    ItemList itemListTiles;
   
    OptionButton optionMaterial;

    LineEdit lineId;
    LineEdit linePositionTile;
    TextureRect textureSelection;

    //----

    MaterialData currentMaterialData;
    TileSimpleData currentSimpleData;

    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        CloseRequested += WindowTileCreator_CloseRequested;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Button").Pressed += Save_Click;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/HBoxContainer/Button2").Pressed += GenerateAll_Click;
        
        GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton").ItemSelected += ComboMaterial_Selected;

        
        optionMaterial = GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton");
        textureSelection = GetNode<TextureRect>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/TextureRect");

        lineId = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/LineEdit");
        linePositionTile = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/LineEdit3");

        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/HSplitContainer/VBoxContainer/ItemList");
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;
        LoadMaterials();
        windowState = WindowState.NEW;
        currentSimpleData = new TileSimpleData();
    }

    private void ComboMaterial_Selected(long index)
    {
        if (index>0)
        {
            int idMat = (int)optionMaterial.GetItemMetadata((int)index);
            MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMat);
            currentMaterialData = materialData;
            itemListTiles.Clear();

            List<Texture> list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(materialData.pathTexture), new Vector2I(materialData.divisionPixelX, materialData.divisionPixelY));
            for (int i = 0; i < list.Count; i++)
            {
                Texture item = list[i];
                if (!TextureHelper.IsTextureEmpty(item))
                {
                    int idx = itemListTiles.AddItem("ID:" + i, (Texture2D)item);
                    itemListTiles.SetItemMetadata(idx, i);
                }
            }
        }      
    }
  
    private void GenerateAll_Click()
    {
        for (int i = 0; i<itemListTiles.ItemCount; i++)
        {
            int postile = (int)itemListTiles.GetItemMetadata(i);

            if (!DataBaseManager.Instance.ExistTile(currentMaterialData.id, postile))
            {
                TileSimpleData tile = new TileSimpleData();
                tile.idMaterial = currentMaterialData.id;
                tile.idInternalPosition = postile;
                DataBaseManager.Instance.InsertUpdate(tile);
            }
            
        }
        OnRequestUpdate?.Invoke();
        QueueFree();
    }

    private void Save_Click()
    {
        if (!DataBaseManager.Instance.ExistTile(currentSimpleData.idMaterial, currentSimpleData.idInternalPosition))
        {
            DataBaseManager.Instance.InsertUpdate(currentSimpleData);
            OnRequestUpdate?.Invoke();
            QueueFree();
        }
    }

    private void ItemListTiles_ItemSelected(long index)
    {
      
            int postile = (int)itemListTiles.GetItemMetadata((int)index);
            linePositionTile.Text = postile.ToString();
            textureSelection.Texture = itemListTiles.GetItemIcon((int)index);
            
            currentSimpleData.idMaterial = currentMaterialData.id;
            currentSimpleData.idInternalPosition = postile;           
    }

    private void LoadMaterials()
    {
        var list = DataBaseManager.Instance.FindAll<MaterialData>();
        for (int i = 0; i < list.Count; i++)
        {
            MaterialData item = list[i];
            optionMaterial.AddItem(item.name);
            optionMaterial.SetItemMetadata(i+1,item.id);
        }
    }

    private void WindowTileCreator_CloseRequested()
    {
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void LoadData(int id)
    {
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/HBoxContainer/Button2").Visible = false;
        currentSimpleData = DataBaseManager.Instance.FindById<TileSimpleData>(id);
        linePositionTile.Text = currentSimpleData.idInternalPosition.ToString();
        lineId.Text = currentSimpleData.id.ToString();

        currentMaterialData = MaterialManager.Instance.GetMaterial(currentSimpleData.idMaterial);
        
        ComboMaterial_Selected(currentMaterialData.id);
        optionMaterial.Selected = currentMaterialData.id;
        textureSelection.Texture = currentSimpleData.textureVisual;
        windowState = WindowState.UPDATE;        
    }
}
