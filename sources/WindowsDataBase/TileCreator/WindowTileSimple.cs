using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
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


    Sprite2D spriteSelection;
    SpinBox bodyWidthSpin;
    SpinBox bodyHeightSpin;
    SpinBox bodyOffsetXSpin;
    SpinBox bodyOffsetYSpin;
    CheckBox collisionCheckBox;

    CollisionShape2D collisionBody;
    //----

    MaterialData currentMaterialData;
    TileSimpleData currentSimpleData;

    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

        spriteSelection = GetNode<Sprite2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Control/CenterContainer/Sprite2D");
        collisionBody = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Control/CenterContainer/Sprite2D/CollisionBody");
        bodyWidthSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        bodyHeightSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        bodyOffsetXSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        bodyOffsetYSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");
        collisionCheckBox = GetNode<CheckBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/CheckBox");

        bodyWidthSpin.ValueChanged += bodyValueChanged;
        bodyHeightSpin.ValueChanged += bodyValueChanged;
        bodyOffsetXSpin.ValueChanged += bodyValueChanged;
        bodyOffsetYSpin.ValueChanged += bodyValueChanged;

        collisionCheckBox.Pressed += CollisionCheckBox_Pressed;

        CloseRequested += WindowTileCreator_CloseRequested;

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Button").Pressed += Save_Click;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/HBoxContainer/Button2").Pressed += GenerateAll_Click;
        
        GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton").ItemSelected += ComboMaterial_Selected;

        
        optionMaterial = GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton");


        lineId = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/LineEdit");
        linePositionTile = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/LineEdit3");

        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/HSplitContainer/VBoxContainer/ItemList");
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;
        LoadMaterials();
        windowState = WindowState.NEW;
        currentSimpleData = new TileSimpleData();
    }

    private void CollisionCheckBox_Pressed()
    {
        collisionBody.Visible = collisionCheckBox.ButtonPressed;
        GetNode<HBoxContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3").Visible = collisionCheckBox.ButtonPressed;
    }

    private void bodyValueChanged(double value)
    {
        collisionBody.Position = new Vector2((float)bodyOffsetXSpin.Value, (float)bodyOffsetYSpin.Value * (-1));
        var shape = (RectangleShape2D)collisionBody.Shape;
        shape.Size = new Vector2((float)bodyWidthSpin.Value, (float)bodyHeightSpin.Value);
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
                tile.haveCollider = collisionCheckBox.ButtonPressed;
                tile.scale = 1;
                if (tile.haveCollider)
                {
                    tile.collisionBody = new Rectangle((float)bodyWidthSpin.Value, (float)bodyHeightSpin.Value, (float)bodyOffsetXSpin.Value, (float)bodyOffsetYSpin.Value); 
                }
                
                DataBaseManager.Instance.InsertUpdate(tile);
            }
            
        }
        OnRequestUpdate?.Invoke();
        QueueFree();
    }

    private void Save_Click()
    {
        //if (!DataBaseManager.Instance.ExistTile(currentSimpleData.idMaterial, currentSimpleData.idInternalPosition))
        {
            currentSimpleData.id = int.Parse( lineId.Text);
            currentSimpleData.haveCollider = collisionCheckBox.ButtonPressed;
            currentSimpleData.idMaterial = currentMaterialData.id;
            currentSimpleData.scale = 1;
            if (currentSimpleData.haveCollider)
            {
                currentSimpleData.collisionBody = new Rectangle((float)bodyWidthSpin.Value, (float)bodyHeightSpin.Value, (float)bodyOffsetXSpin.Value, (float)bodyOffsetYSpin.Value);
            }
            
            DataBaseManager.Instance.InsertUpdate(currentSimpleData);
            OnRequestUpdate?.Invoke();
            QueueFree();
        }
    }

    private void ItemListTiles_ItemSelected(long index)
    {
      
            int postile = (int)itemListTiles.GetItemMetadata((int)index);
            linePositionTile.Text = postile.ToString();
            spriteSelection.Texture = itemListTiles.GetItemIcon((int)index);
            
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
        collisionCheckBox.ButtonPressed = currentSimpleData.haveCollider;
        CollisionCheckBox_Pressed();
        linePositionTile.Text = currentSimpleData.idInternalPosition.ToString();
        lineId.Text = currentSimpleData.id.ToString();

        currentMaterialData = MaterialManager.Instance.GetMaterial(currentSimpleData.idMaterial);
        
        ComboMaterial_Selected(currentMaterialData.id);
        optionMaterial.Selected = currentMaterialData.id;
        spriteSelection.Texture = currentSimpleData.textureVisual;
        windowState = WindowState.UPDATE;
        if (currentSimpleData.haveCollider)
        {
            Rectangle rect = (Rectangle)currentSimpleData.collisionBody;
            bodyWidthSpin.Value = rect.widthPixel;
            bodyHeightSpin.Value = rect.heightPixel;
            bodyOffsetXSpin.Value = rect.originPixelX;
            bodyOffsetYSpin.Value = rect.originPixelY;
        }
        
    }
}
