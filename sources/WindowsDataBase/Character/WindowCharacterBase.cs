using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class WindowCharacterBase : Window, IDetailWindow
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

    
    Sprite2D spriteSelection;
    LineEdit nameLine;
    SpinBox idBaseSpin;

    SpinBox moveWidthSpin;
    SpinBox moveHeightSpin;
    SpinBox moveOffsetXSpin;
    SpinBox moveOffsetYSpin;

    SpinBox bodyWidthSpin;
    SpinBox bodyHeightSpin;
    SpinBox bodyOffsetXSpin;
    SpinBox bodyOffsetYSpin;

    CollisionShape2D collisionBody;
    CollisionShape2D collisionMove;
    CollisionShape2D collisionLeft;
    CollisionShape2D collisionRight;
    CollisionShape2D collisionUp;
    CollisionShape2D collisionDown;
    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        CloseRequested += Window_CloseRequested;
        characterBaseData = new CharacterBaseData();
        panelColliders = new List<WindowsCharacterCollisionAtack>();
        panelAnimation = new List<WindowCharacterAnimation>();

        collisionBody = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/Control/CenterContainer/Sprite2D/CollisionBody");
        collisionMove = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/Control/CenterContainer/Sprite2D/CollisionMove");
        collisionLeft = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/Control/CenterContainer/Sprite2D/CollisionAtackLeft");
        collisionRight = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/Control/CenterContainer/Sprite2D/CollisionAtackRight");
        collisionDown = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/Control/CenterContainer/Sprite2D/CollisionAtackDown");
        collisionUp = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/Control/CenterContainer/Sprite2D/CollisionAtackUp");
        

        moveWidthSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        moveHeightSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        moveOffsetXSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        moveOffsetYSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");

        bodyWidthSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer2/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox");
        bodyHeightSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer2/HBoxContainer3/HBoxContainer/VBoxContainer/HBoxContainer/SpinBox2");
        bodyOffsetXSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer2/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox");
        bodyOffsetYSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/VBoxContainer2/HBoxContainer3/HBoxContainer2/VBoxContainer/HBoxContainer/SpinBox2");

        idBaseSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/GridContainer/SpinBox");
        nameLine = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Collider Basic/VBoxContainer/GridContainer/LineEdit");

        moveWidthSpin.ValueChanged += moveValueChanged;
        moveHeightSpin.ValueChanged += moveValueChanged;
        moveOffsetXSpin.ValueChanged += moveValueChanged;
        moveOffsetYSpin.ValueChanged += moveValueChanged;


        bodyWidthSpin.ValueChanged += bodyValueChanged;
        bodyHeightSpin.ValueChanged += bodyValueChanged;
        bodyOffsetXSpin.ValueChanged += bodyValueChanged;
        bodyOffsetYSpin.ValueChanged += bodyValueChanged;

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Animations/Animations/Button").Pressed += button_AddAnimation;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Colliders Atack/Colliders Atack/Button").Pressed += button_AddCollider;
       

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/HBoxContainer/Button").Pressed += button_Save;
        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/HBoxContainer/Button2").Pressed += button_Copy;

        gridContainerColliders = GetNode<GridContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Colliders Atack/Colliders Atack/ScrollContainer/GridContainer");
        gridContainerAnimations = GetNode<GridContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer/Animations/Animations/ScrollContainer/GridContainer");

        optionMaterial = GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton");
        GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/OptionButton").ItemSelected += ComboMaterial_Selected;

        
        spriteSelection = GetNode<Sprite2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/VBoxContainer2/Control/CenterContainer/Sprite2D");
        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/HSplitContainer/VBoxContainer/ItemList");
        itemListTiles.ItemSelected += ItemListTiles_ItemSelected;
        LoadMaterials();

        spriteSelection.Hframes = 1;
        spriteSelection.Vframes = 1;
        optionMaterial.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
    }



    private void button_Copy()
    {
        characterBaseData.id = 0;
        characterBaseData.name = nameLine.Text +"_Copy";
        DataBaseManager.Instance.InsertUpdate(characterBaseData);
        OnRequestUpdate?.Invoke();
        QueueFree();
    }

    public void LoadData(int id)
    {
        //GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer2/HBoxContainer/Button2").Visible = false;
        characterBaseData = DataBaseManager.Instance.FindById<CharacterBaseData>(id);     
        currentMaterialData = MaterialManager.Instance.GetMaterial(characterBaseData.idMaterial);
        ComboMaterial_Selected(currentMaterialData.id);
        optionMaterial.Selected = currentMaterialData.id;

        idBaseSpin.Value = characterBaseData.idGroup;
        nameLine.Text = characterBaseData.name;

        bodyWidthSpin.Value = characterBaseData.collisionBody.widthPixel;
        bodyHeightSpin.Value = characterBaseData.collisionBody.heightPixel;
        bodyOffsetXSpin.Value = characterBaseData.collisionBody.originPixelX;
        bodyOffsetYSpin.Value = characterBaseData.collisionBody.originPixelY;

        moveWidthSpin.Value = characterBaseData.collisionMove.widthPixel;
        moveHeightSpin.Value = characterBaseData.collisionMove.heightPixel;
        moveOffsetXSpin.Value = characterBaseData.collisionMove.originPixelX;
        moveOffsetYSpin.Value = characterBaseData.collisionMove.originPixelY;

        foreach (var item in characterBaseData.animationDataArray)
        {
            PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windowCharacterAnimation.tscn");
            var node = packedScene.Instantiate<WindowCharacterAnimation>();
            node.OnRequestDelete += Node_OnRequestDeleteAnimation;
            node.OnNotifyChangue += Node_OnNotifyChangueAnimation;
            node.OnRequestOrderItem += Node_OnRequestOrderItem;

            node.SetMaterial(currentMaterialData.id);      
            panelAnimation.Add(node);
            gridContainerAnimations.AddChild(node);
            node.SetData(item);
        }

       
            PackedScene packedScene2 = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windosCharacterCollisionAtack.tscn");
            var node2 = packedScene2.Instantiate<WindowsCharacterCollisionAtack>();
            node2.OnRequestDelete += Node_OnRequestDelete;
            node2.OnNotifyChangue += Node_OnNotifyChangue;
            int i = panelColliders.Count;
            panelColliders.Add(node2);
            gridContainerColliders.AddChild(node2);
            node2.SetData(characterBaseData.atackDataColliders);
       
        


    }
    private void bodyValueChanged(double value)
    {
        collisionBody.Position = new Vector2((float)bodyOffsetXSpin.Value, (float)bodyOffsetYSpin.Value * (-1));
        var shape = (RectangleShape2D)collisionBody.Shape;
        shape.Size = new Vector2((float)bodyWidthSpin.Value, (float)bodyHeightSpin.Value);
       
    }

    private void moveValueChanged(double value)
    {
        collisionMove.Position = new Vector2((float)moveOffsetXSpin.Value, (float)moveOffsetYSpin.Value * (-1));
        var shape = (RectangleShape2D) collisionMove.Shape;
        shape.Size = new Vector2((float)moveWidthSpin.Value, (float)moveHeightSpin.Value);
        
    }

    private void ItemListTiles_ItemSelected(long index)
    {
        int postile = (int)itemListTiles.GetItemMetadata((int)index);
        spriteSelection.Texture = itemListTiles.GetItemIcon((int)index);
   
        spriteSelection.Hframes = 1;
        spriteSelection.Vframes = 1;
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

    private void Window_CloseRequested()
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

        List<AnimationStateData> dataAnimations = new List<AnimationStateData>();
        foreach (var item in gridContainerAnimations.GetChildren())
        {
            WindowCharacterAnimation windowAutoTileItem = (WindowCharacterAnimation)item;
            dataAnimations.Add(windowAutoTileItem.data);
        }

        characterBaseData.idMaterial = currentMaterialData.id;
        characterBaseData.atackDataColliders = dataCollider[0];
        characterBaseData.animationDataArray = dataAnimations.ToArray();

        characterBaseData.collisionBody = new Rectangle((float)bodyWidthSpin.Value, (float)bodyHeightSpin.Value, (float)bodyOffsetXSpin.Value, (float)bodyOffsetYSpin.Value);
        characterBaseData.collisionMove = new Rectangle((float)moveWidthSpin.Value, (float)moveHeightSpin.Value, (float)moveOffsetXSpin.Value, (float)moveOffsetYSpin.Value);

        characterBaseData.idGroup = (int) idBaseSpin.Value;
        characterBaseData.name = nameLine.Text; 

        DataBaseManager.Instance.InsertUpdate(characterBaseData);

        OnRequestUpdate?.Invoke();
    }

    private void button_AddAnimation()
    {
        if (currentMaterialData!=null)
        {
            PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windowCharacterAnimation.tscn");
            var node = packedScene.Instantiate<WindowCharacterAnimation>();
            node.OnRequestDelete += Node_OnRequestDeleteAnimation;
            node.OnNotifyChangue += Node_OnNotifyChangueAnimation;
            node.OnRequestOrderItem += Node_OnRequestOrderItem;
            node.SetMaterial(currentMaterialData.id);
            
            int i = panelAnimation.Count;
            panelAnimation.Add(node);
            gridContainerAnimations.AddChild(node);
            node.SetID(i);
        }
     
    }

    private void Node_OnRequestOrderItem(int id, int position, WindowCharacterAnimation windowAutoTileItem)
    {
        if (id == 1) //down
        {
            if ((position +1) < gridContainerAnimations.GetChildCount())
            {
                var node = gridContainerAnimations.GetChild<WindowCharacterAnimation>(position + 1);
                node.SetID(position);
                gridContainerAnimations.MoveChild(windowAutoTileItem, position + 1);
                windowAutoTileItem.SetID(position + 1);

            }

        }
        if (id == 0) // up
        {
            if ((position -1) >= 0)
            {
                var node = gridContainerAnimations.GetChild<WindowCharacterAnimation>(position - 1);
                node.SetID(position);
                gridContainerAnimations.MoveChild(windowAutoTileItem, position - 1);
                windowAutoTileItem.SetID(position - 1);
            }

        }
    }

    private void Node_OnNotifyChangueAnimation(AnimationStateData itemData, int state)
    {
        currentIdState = state;
        data = itemData;
        button_Save();


    }

    private void Node_OnRequestDeleteAnimation(WindowCharacterAnimation item)
    {
        item.QueueFree();
        panelAnimation.Remove(item);        
        for (int i = 0; i < panelAnimation.Count; i++)
        {
            WindowCharacterAnimation itemVal = panelAnimation[i];
            itemVal.SetID(i);            
        }

    }

    private void button_AddCollider()
    {
        if (panelColliders.Count<1)
        {
            PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windosCharacterCollisionAtack.tscn");
            var node = packedScene.Instantiate<WindowsCharacterCollisionAtack>();
            node.OnRequestDelete += Node_OnRequestDelete;
            node.OnNotifyChangue += Node_OnNotifyChangue;
            int i = panelColliders.Count;
            panelColliders.Add(node);
            gridContainerColliders.AddChild(node);
            node.SetID(i);
        }
   
    }

    private void Node_OnNotifyChangue(CharacterColliderAtackData itemData)
    {

        var atackColliderUP = (Rectangle) itemData.colliders[0];
        var atackColliderDown = (Rectangle)itemData.colliders[1];
        var atackColliderLeft = (Rectangle)itemData.colliders[2];
        var atackColliderRight = (Rectangle)itemData.colliders[3];

        collisionUp.Position = new Vector2(atackColliderUP.OriginCurrent.X, atackColliderUP.OriginCurrent.Y * (-1));
        var shape = (RectangleShape2D)collisionUp.Shape;
        shape.Size = new Vector2((float)atackColliderUP.Width, (float)atackColliderUP.Height);

        collisionDown.Position = new Vector2(atackColliderDown.OriginCurrent.X, atackColliderDown.OriginCurrent.Y * (-1));
        var shape2 = (RectangleShape2D)collisionDown.Shape;
        shape2.Size = new Vector2((float)atackColliderDown.Width, (float)atackColliderDown.Height);

        collisionLeft.Position = new Vector2(atackColliderLeft.OriginCurrent.X, atackColliderLeft.OriginCurrent.Y * (-1));
        var shape3 = (RectangleShape2D)collisionLeft.Shape;
        shape3.Size = new Vector2((float)atackColliderLeft.Width, (float)atackColliderLeft.Height);

        collisionRight.Position = new Vector2(atackColliderRight.OriginCurrent.X, atackColliderRight.OriginCurrent.Y * (-1));
        var shape4 = (RectangleShape2D)collisionRight.Shape;
        shape4.Size = new Vector2((float)atackColliderRight.Width, (float)atackColliderRight.Height);

        button_Save();
    }

    private void Node_OnRequestDelete(WindowsCharacterCollisionAtack item)
    {
        item.QueueFree();
        panelColliders.Remove(item);

        for (int i = 0; i < panelColliders.Count; i++)
        {
            WindowsCharacterCollisionAtack itemVal = panelColliders[i];
            itemVal.SetID(i);
        }
    }

    int indexFrame = 0;
    double currentfps = 0;
    int currentIdState = 0;
    public AnimationStateData data;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (data !=null && data.animationData[currentIdState] != null && data.animationData[currentIdState].idFrames != null)
        {
            currentfps += delta;
            if (currentfps >= data.frameDuration && data.animationData[currentIdState].idFrames.Length > 0)
            {
                var iFrame = data.animationData[currentIdState].idFrames[indexFrame];
                indexFrame++;
                currentfps = 0;
                var dataTexture = MaterialManager.Instance.GetAtlasTexture(currentMaterialData.id, iFrame);
                spriteSelection.Texture = dataTexture;
            }
            if (indexFrame >= data.animationData[currentIdState].idFrames.Length)
            {
                indexFrame = 0;
            }
        }

    }
}
