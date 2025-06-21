using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;

public partial class WindowTileDinamic : Window, IDetailWindow
{
    WindowState windowState;


    OptionButton optionMaterial;

    LineEdit lineId;
    SpinBox scaleSpinbox;


    Sprite2D spriteSelection;
    SpinBox bodyWidthSpin;
    SpinBox bodyHeightSpin;
    SpinBox bodyOffsetXSpin;
    SpinBox bodyOffsetYSpin;
    CheckBox collisionCheckBox;

    SpinBox textureOffsetXSpin;
    SpinBox textureOffsetYSpin;

    CollisionShape2D collisionBody;
    //----

    MaterialData currentMaterialData;
    TileDynamicData currentData;

    ImageSelectionControl imageSelectionControl;

    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        spriteSelection = GetNode<Sprite2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Control/CenterContainer/Sprite2D");
        collisionBody = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Control/CenterContainer/CollisionBody");
        bodyWidthSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        bodyHeightSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        bodyOffsetXSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        bodyOffsetYSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");
        collisionCheckBox = GetNode<CheckBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/CheckBox");

        imageSelectionControl = GetNode<ImageSelectionControl>("Panel/MarginContainer/HSplitContainer/VBoxContainer/MarginContainer/ScrollContainer/CenterContainer");
        textureOffsetXSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/HBoxContainer/SpinBox");
        textureOffsetYSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/HBoxContainer/SpinBox2");


        textureOffsetXSpin.ValueChanged += textureValueChanged;
        textureOffsetYSpin.ValueChanged += textureValueChanged;

        bodyWidthSpin.ValueChanged += bodyValueChanged;
        bodyHeightSpin.ValueChanged += bodyValueChanged;
        bodyOffsetXSpin.ValueChanged += bodyValueChanged;
        bodyOffsetYSpin.ValueChanged += bodyValueChanged;

        collisionCheckBox.Pressed += CollisionCheckBox_Pressed;

        CloseRequested += WindowTileCreator_CloseRequested;

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Button").Pressed += Save_Click;
 

        GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton").ItemSelected += ComboMaterial_Selected;


        optionMaterial = GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton");


        lineId = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/LineEdit");
        scaleSpinbox = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/GridContainer/SpinBox");

        scaleSpinbox.ValueChanged += ScaleSpinbox_ValueChanged;
  
        LoadMaterials();
        windowState = WindowState.NEW;
        currentData = new TileDynamicData();

        optionMaterial.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
    }

    private void textureValueChanged(double value)
    {
        spriteSelection.Position = new Vector2((float)textureOffsetXSpin.Value, (float)textureOffsetYSpin.Value * (-1));
        collisionBody.Position = new Vector2((float)(bodyOffsetXSpin.Value + textureOffsetXSpin.Value), (float)(((float)(bodyOffsetYSpin.Value + textureOffsetYSpin.Value) * (-1))));
    }

    private void ScaleSpinbox_ValueChanged(double value)
    {
        //spriteSelection.Scale = new Vector2((float)value, (float)value);
    }

    private void CollisionCheckBox_Pressed()
    {
        collisionBody.Visible = collisionCheckBox.ButtonPressed;
        GetNode<HBoxContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/HBoxContainer3").Visible = collisionCheckBox.ButtonPressed;
    }

    private void bodyValueChanged(double value)
    {
        collisionBody.Position = new Vector2((float)(bodyOffsetXSpin.Value + textureOffsetXSpin.Value), (float)(((float)(bodyOffsetYSpin.Value + textureOffsetYSpin.Value) * (-1)) ));
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
            imageSelectionControl.SetMaterial(materialData.id);
        }
    }



    private void Save_Click()
    {
        //if (!DataBaseManager.Instance.ExistTile(currentSimpleData.idMaterial, currentSimpleData.idInternalPosition))
        {
            currentData.id = int.Parse(lineId.Text);
            currentData.haveCollider = collisionCheckBox.ButtonPressed;
            currentData.idMaterial = currentMaterialData.id;
            AtlasTexture atlasTexture = (AtlasTexture)spriteSelection.Texture;
            currentData.x = atlasTexture.Region.Position.X;
            currentData.y = atlasTexture.Region.Position.Y;
            currentData.widht = atlasTexture.Region.Size.X;
            currentData.height = atlasTexture.Region.Size.Y;
            currentData.scale = (float)scaleSpinbox.Value;
            currentData.offsetX = (float)textureOffsetXSpin.Value;
            currentData.offsetY = (float)textureOffsetYSpin.Value;
            if (currentData.haveCollider)
            {
                currentData.collisionBody = new Rectangle((float)bodyWidthSpin.Value, (float)bodyHeightSpin.Value, (float)bodyOffsetXSpin.Value, (float)bodyOffsetYSpin.Value);
            }

            DataBaseManager.Instance.InsertUpdate(currentData);

  
            OnRequestUpdate?.Invoke();
            QueueFree();
        }
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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void LoadData(int id)
    {
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/HBoxContainer/Button2").Visible = false;
        currentData = DataBaseManager.Instance.FindById<TileDynamicData>(id);
        collisionCheckBox.ButtonPressed = currentData.haveCollider;
        CollisionCheckBox_Pressed();
     
        lineId.Text = currentData.id.ToString();

        currentMaterialData = MaterialManager.Instance.GetMaterial(currentData.idMaterial);

        scaleSpinbox.Value = currentData.scale;
        ComboMaterial_Selected(currentMaterialData.id);
        optionMaterial.Selected = currentMaterialData.id;
        spriteSelection.Texture = currentData.textureVisual;
        //spriteSelection.Scale = new Vector2(currentData.scale,currentData.scale);
        windowState = WindowState.UPDATE;

        textureOffsetXSpin.Value = currentData.offsetX;
        textureOffsetYSpin.Value = currentData.offsetY;
        if (currentData.haveCollider)
        {
            Rectangle rect = (Rectangle)currentData.collisionBody;
            bodyWidthSpin.Value = rect.widthPixel;
            bodyHeightSpin.Value = rect.heightPixel;
            bodyOffsetXSpin.Value = rect.originPixelX;
            bodyOffsetYSpin.Value = rect.originPixelY;
        }
        bodyValueChanged(0);

    }
}
