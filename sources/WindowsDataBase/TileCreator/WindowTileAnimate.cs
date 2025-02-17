using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Reflection;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.managers.Collision;

public partial class WindowTileAnimate : Window, IDetailWindow
{
    WindowState windowState;
    ItemList itemListTiles;

    OptionButton optionMaterial;

    LineEdit lineId;
    LineEdit lineFramesTiles;
    SpinBox fpsSpin;


    Sprite2D spriteSelection;
    SpinBox bodyWidthSpin;
    SpinBox bodyHeightSpin;
    SpinBox bodyOffsetXSpin;
    SpinBox bodyOffsetYSpin;
    CheckBox collisionCheckBox;

    CollisionShape2D collisionBody;

    //----

    MaterialData currentMaterialData;
    TileAnimateData currentTileAnimateSimpleData;

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
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer/Button2").Pressed += Save_Click;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer/Button").Pressed += Update_Click;

        GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton").ItemSelected += ComboMaterial_Selected;


        optionMaterial = GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton");
       
        fpsSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/SpinBox");

        lineId = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/LineEdit");
        lineFramesTiles = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/LineEdit3");

        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/HSplitContainer/VBoxContainer/ItemList");
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;
        LoadMaterials();
        windowState = WindowState.NEW;
        currentTileAnimateSimpleData = new TileAnimateData();
    }

    private void CollisionCheckBox_Pressed()
    {
        currentTileAnimateSimpleData.haveCollider = collisionCheckBox.ButtonPressed;
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
        if (index > 0)
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

    private void Update_Click()
    {
        if (lineFramesTiles.Text.Contains(",") || lineFramesTiles.Text.Contains("-"))
        {
            string[] frames = lineFramesTiles.Text.Split(',');
            if (lineFramesTiles.Text.Contains("-"))
            {
                frames = lineFramesTiles.Text.Split('-');
                int ini = int.Parse(frames[0]);
                int fin = int.Parse(frames[1]);
                int[] arrayFrame = new int[(fin-ini)+1];

             
                int index = 0;
                for (int i = ini; i <=fin; i++)
                {
                    arrayFrame[index] = i;
              

                    currentTileAnimateSimpleData.idMaterial = currentMaterialData.id;
                    currentTileAnimateSimpleData.idFrames = arrayFrame;
                    currentTileAnimateSimpleData.frameDuration = (float)fpsSpin.Value;
                    index++;
                    
         
                }                                
            }
            else
            {
                int index = 0;
                int[] arrayFrame = new int[frames.Length];
               
                foreach (string item in frames)
                {
                    int frame = int.Parse(item);
                    arrayFrame[index] = frame;                  
                    currentTileAnimateSimpleData.idMaterial = currentMaterialData.id;
                    currentTileAnimateSimpleData.idFrames = arrayFrame;
                    currentTileAnimateSimpleData.frameDuration = (float) fpsSpin.Value;
                    index ++;              
                }
            }
        }
    }

    private void Save_Click()
    {
        currentTileAnimateSimpleData.scale = 1;
        if (currentTileAnimateSimpleData.haveCollider)
        {
            currentTileAnimateSimpleData.collisionBody = new Rectangle((float)bodyWidthSpin.Value, (float)bodyHeightSpin.Value, (float)bodyOffsetXSpin.Value, (float)bodyOffsetYSpin.Value);
        }        
        DataBaseManager.Instance.InsertUpdate(currentTileAnimateSimpleData);
        OnRequestUpdate?.Invoke();
        QueueFree();

    }

    private void ItemListTiles_ItemSelected(long index)
    {


    }

    private void LoadMaterials()
    {
        var list = DataBaseManager.Instance.FindAll<MaterialData>();
        for (int i = 0; i < list.Count; i++)
        {
            MaterialData item = list[i];
            optionMaterial.AddItem(item.name);
            optionMaterial.SetItemMetadata(i + 1, item.id);
        }
    }

    private void WindowTileCreator_CloseRequested()
    {
        QueueFree();
    }


    int indexFrame = 0;
    double currentfps = 0;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (currentTileAnimateSimpleData.idFrames!=null)
        {
            currentfps += delta;
            if (currentfps >= currentTileAnimateSimpleData.frameDuration && currentTileAnimateSimpleData.idFrames.Length > 0)
            {
                var iFrame = currentTileAnimateSimpleData.idFrames[indexFrame];
                indexFrame++;
                currentfps = 0;
                var data = MaterialManager.Instance.GetAtlasTexture(currentTileAnimateSimpleData.idMaterial, iFrame);
                spriteSelection.Texture = data;
            }
            if (indexFrame >= currentTileAnimateSimpleData.idFrames.Length)
            {
                indexFrame = 0;
            }
        }
     
    }

    public void LoadData(int id)
    {      
          
        currentTileAnimateSimpleData = DataBaseManager.Instance.FindById<TileAnimateData>(id);
        collisionCheckBox.ButtonPressed = currentTileAnimateSimpleData.haveCollider;
        CollisionCheckBox_Pressed();
        string strFrames ="";
        foreach (var item in currentTileAnimateSimpleData.idFrames)
        {
            strFrames = strFrames+"," + item.ToString();
        }
        strFrames = strFrames.Substr(1, strFrames.Length);
        lineFramesTiles.Text = strFrames;
        lineId.Text = currentTileAnimateSimpleData.id.ToString();
        fpsSpin.Value = currentTileAnimateSimpleData.frameDuration;
        currentMaterialData = MaterialManager.Instance.GetMaterial(currentTileAnimateSimpleData.idMaterial);

        ComboMaterial_Selected(currentMaterialData.id);
        optionMaterial.Selected = currentMaterialData.id;
        spriteSelection.Texture = currentTileAnimateSimpleData.textureVisual;
        windowState = WindowState.UPDATE;
        if (currentTileAnimateSimpleData.haveCollider)
        {
            Rectangle rect = (Rectangle)currentTileAnimateSimpleData.collisionBody;
            bodyWidthSpin.Value = rect.widthPixel;
            bodyHeightSpin.Value = rect.heightPixel;
            bodyOffsetXSpin.Value = rect.originPixelX;
            bodyOffsetYSpin.Value = rect.originPixelY;
        }
     
    }
}
