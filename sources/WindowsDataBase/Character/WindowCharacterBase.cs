using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;

public partial class WindowCharacterBase : Window
{
	Button buttonCollider;

    CharacterBaseData characterBaseData;

    List<WindowsCharacterCollisionAtack> panelColliders;
    List<WindowCharacterAnimation> panelAnimation;

    GridContainer gridContainerColliders;
    GridContainer gridContainerAnimations;
    
    OptionButton optionMaterial;
    ItemList itemListTiles;

    MaterialData currentMaterialData;

    TextureRect textureSelection;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        characterBaseData = new CharacterBaseData();
        panelColliders = new List<WindowsCharacterCollisionAtack>();
        panelAnimation = new List<WindowCharacterAnimation>();
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Animations/Animations/Button").Pressed += button_AddAnimation;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Colliders Atack/Colliders Atack/Button").Pressed += button_AddCollider;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/ScrollContainer/VBoxContainer/Button").Pressed += button_Save;

        gridContainerColliders = GetNode<GridContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Colliders Atack/Colliders Atack/ScrollContainer/GridContainer");
        gridContainerAnimations = GetNode<GridContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Animations/Animations/ScrollContainer/GridContainer");

        optionMaterial = GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton");
        GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton").ItemSelected += ComboMaterial_Selected;

        textureSelection = GetNode<TextureRect>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/TextureRect");

        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/HSplitContainer/VBoxContainer/ItemList");
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;
        LoadMaterials();
    }
    private void ItemListTiles_ItemSelected(long index)
    {
        int postile = (int)itemListTiles.GetItemMetadata((int)index);
       
        textureSelection.Texture = itemListTiles.GetItemIcon((int)index); 
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

            foreach (var item in panelAnimation)
            {
                item.SetMaterial(idMat);
            }
        }
    }
    private void button_Save()
    {
        List<CharacterColliderAtackData> dataCollider = new List<CharacterColliderAtackData>();
        foreach (var item in panelColliders) {
            dataCollider.Add(item.data);
        }
       characterBaseData.atackDataCollidersArray = dataCollider.ToArray();
    }

    private void button_AddAnimation()
    {
        if (currentMaterialData!=null)
        {
            PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windowCharacterAnimation.tscn");
            var node = packedScene.Instantiate<WindowCharacterAnimation>();
            node.OnRequestDelete += Node_OnRequestDeleteAnimation;
            node.SetMaterial(currentMaterialData.id);
            panelAnimation.Add(node);
            gridContainerAnimations.AddChild(node);
            
        }
     
    }

    private void Node_OnRequestDeleteAnimation(WindowCharacterAnimation item)
    {
        item.QueueFree();
        panelAnimation.Remove(item);
    }

    private void button_AddCollider()
    {        
        PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windosCharacterCollisionAtack.tscn");
        var node = packedScene.Instantiate<WindowsCharacterCollisionAtack>();     
        node.OnRequestDelete += Node_OnRequestDelete;
     
        panelColliders.Add(node);
        gridContainerColliders.AddChild(node);        
    }

    private void Node_OnRequestDelete(WindowsCharacterCollisionAtack item)
    {
        item.QueueFree();
        panelColliders.Remove(item);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
