using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class WindowWeapons : Window, IDetailWindow
{
	[Export] FileDialog fileDialog;
	[Export] Button buttonDialog;
    [Export] Button buttonSplit;
    [Export] ItemList itemListTiles;
    [Export] SpinBox spinPixel_X;
    [Export] SpinBox spinPixel_Y;

    [Export] OptionButton optionButtonCharacterBase;
    [Export] OptionButton optionButtonCharacterAnimationID;
    [Export] AnimationScene animationSceneCharacter;

    [Export] AnimationPreview animationPreviewCharacter;
    [Export] AnimationPreview animationPreviewWeapon;

    [Export] AnimationPanel animationPanel;
    [Export] PanelColliders colliderPanel;

    [Export] CollisionShape2D collisionShape2DCollider;
    string pathTexture;
    WindowState state;

    MaterialData materialData;
    CharacterBaseData characterBaseData;

    AnimationBaseData weaponBaseData;

    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        buttonDialog.Pressed += ButtonDialog_Pressed;
        buttonSplit.Pressed += ButtonSplit_Pressed;
        fileDialog.FileSelected += OnFileSelected;
        state = WindowState.NEW;
        materialData = new MaterialData();
        optionButtonCharacterBase.ItemSelected += OptionButtonCharacterBase_ItemSelected;
        optionButtonCharacterAnimationID.ItemSelected += OptionButtonCharacterAnimationID_ItemSelected;
        optionButtonCharacterBase.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        optionButtonCharacterAnimationID.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        animationPanel.OnNotifyPreview += AnimationPanel_OnNotifyPreview;
        animationPanel.OnNotifySave += AnimationPanel_OnNotifySave;
        colliderPanel.OnNotifySave += AnimationPanel_OnNotifySave;
        animationSceneCharacter.OnNotifyPreview += AnimationSceneCharacter_OnNotifyPreview;
        LoadCharacters();
        colliderPanel.OnNotifyPreview += ColliderPanel_OnNotifyPreview;
        weaponBaseData = new AnimationBaseData();

        CloseRequested += WindowWeapons_CloseRequested;
    }

    private void WindowWeapons_CloseRequested()
    {
        QueueFree();
    }

    private void AnimationPanel_OnNotifySave()
    {
        string pathLocal = "";
        if (state == WindowState.NEW)
        {
            if (weaponBaseData.pathTexture == string.Empty)
            {
                int idnext = DataBaseManager.Instance.NextID<AnimationBaseData>();
                weaponBaseData.id = idnext;
                pathLocal = FileHelper.CopyFileToAssetExternals(pathTexture, "Weapons", idnext.ToString());
            }
            else
            {

                pathLocal = FileHelper.CopyFileToAssetExternals(pathTexture, "Weapons", weaponBaseData.id.ToString());
            }
        }
        else
        {
            if (weaponBaseData.pathTexture != pathTexture)
            {
                 pathLocal = FileHelper.CopyFileToAssetExternals(pathTexture, "Weapons", weaponBaseData.id.ToString());
            }
            else
            {
                pathLocal = weaponBaseData.pathTexture;
            }          
        }

        
        weaponBaseData.divisionPixelX = (int)spinPixel_X.Value;
        weaponBaseData.divisionPixelY = (int)spinPixel_Y.Value;
        weaponBaseData.pathTexture = pathLocal;
        weaponBaseData.heightTexture = materialData.heightTexture;
        weaponBaseData.widhtTexture = materialData.widhtTexture;
        weaponBaseData.animationDataArray = animationPanel.GetAllAnimationData().ToArray();
        weaponBaseData.colliders = colliderPanel.GetAllCollidersData().ToArray();
        
        DataBaseManager.Instance.InsertUpdate(weaponBaseData);
    }

    private void ColliderPanel_OnNotifyPreview(GodotEcsArch.sources.managers.Collision.GeometricShape2D itemData)
    {
        collisionShape2DCollider.Position = new Vector2((float)itemData.originPixelX, (float)itemData.originPixelY * (-1));     
        switch (itemData)
        {
            case Rectangle:
                var shape = new RectangleShape2D();
                collisionShape2DCollider.Shape = shape;
                shape.Size = new Vector2((float)itemData.widthPixel, (float)itemData.heightPixel);
                break;
            case Circle:
                var shapeC = new CircleShape2D();
                collisionShape2DCollider.Shape = shapeC;
                shapeC.Radius = itemData.widthPixel;                
                break;
            default:
                break;
        }
     
       
    }

    private void OptionButtonCharacterAnimationID_ItemSelected(long index)
    {
        int id = (int)index;    
        animationPreviewCharacter.data = characterBaseData.animationDataArray[id];
        animationSceneCharacter.SetData(characterBaseData.animationDataArray[id], characterBaseData.idMaterial);
    }

    private void AnimationSceneCharacter_OnNotifyPreview(AnimationStateData itemData, int currentIdState)
    {    
        animationPreviewCharacter.idMaterial = characterBaseData.idMaterial;
        animationPreviewCharacter.currentIdState = currentIdState;
    }

    private void AnimationPanel_OnNotifyPreview(AnimationStateData itemData, int currentIdState)
    {
        animationPreviewWeapon.data = itemData;
        animationPreviewWeapon.idMaterial = materialData.id;
        animationPreviewWeapon.currentIdState = currentIdState;
    }

    private void LoadCharacters()
    {
        var dataList = DataBaseManager.Instance.FindAll<CharacterBaseData>();

        optionButtonCharacterBase.AddItem("Seleccione", 0);

        int i = 1;
        foreach (var character in dataList)
        {
            optionButtonCharacterBase.AddItem(character.name+" "+character.id , i);
            optionButtonCharacterBase.SetItemMetadata(i, character.id);
            i++;
        }
    }

    private void OptionButtonCharacterBase_ItemSelected(long index)
    {
        if (index > 0)
        {
            int id = (int)optionButtonCharacterBase.GetItemMetadata((int)index);
            characterBaseData = DataBaseManager.Instance.FindById<CharacterBaseData>(id);
            animationSceneCharacter.SetData(characterBaseData.animationDataArray[0], characterBaseData.idMaterial);
            animationPreviewCharacter.idMaterial = characterBaseData.idMaterial;
            animationPreviewCharacter.data = characterBaseData.animationDataArray[0];

            foreach (var item in characterBaseData.animationDataArray)
            {
                optionButtonCharacterAnimationID.AddItem(item.id.ToString());
            }
            
        }
    }

    private void ButtonSplit_Pressed()
    {
        TextureSplit_Clik();
    }

    private void ButtonDialog_Pressed()
    {
        fileDialog.PopupCentered();
    }
    private void TextureSplit_Clik()
    {
        Godot.Image image;
        Texture2D texture2D;
        itemListTiles.Clear();
        List<Texture> list;
        if (state == WindowState.NEW)
        {
            list = TextureHelper.SplitTexture(pathTexture, new Vector2I((int)spinPixel_X.Value, (int)spinPixel_Y.Value));
            image = TextureHelper.LoadImageLocal(pathTexture);
            texture2D = (Texture2D)TextureHelper.LoadTextureLocal(pathTexture);
        }
        else
        {
            list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(pathTexture), new Vector2I((int)spinPixel_X.Value, (int)spinPixel_Y.Value));
            image = TextureHelper.LoadImageLocal(FileHelper.GetPathGameDB(pathTexture));
            texture2D = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(pathTexture));
        }

        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!TextureHelper.IsTextureEmpty(item))
            {
                itemListTiles.AddItem("ID:" + i, (Texture2D)item);
            }
        }
     
       
        materialData.id = -1;
        materialData.name = "temporal";
        materialData.pathTexture = pathTexture;                 
        materialData.heightTexture = image.GetHeight();
        materialData.widhtTexture = image.GetWidth();
        materialData.divisionPixelX = (int)spinPixel_X.Value;
        materialData.divisionPixelY = (int)spinPixel_Y.Value;

       
        materialData.textureMaterial = texture2D;
    }
    private void OnFileSelected(string path)
    {
        pathTexture = path;
        TextureSplit_Clik();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void LoadData(int id)
    {
        state = WindowState.UPDATE;
        weaponBaseData = DataBaseManager.Instance.FindById<AnimationBaseData>(id);
        pathTexture = weaponBaseData.pathTexture;
        spinPixel_X.Value = weaponBaseData.divisionPixelX;
        spinPixel_Y.Value = weaponBaseData.divisionPixelY;
        TextureSplit_Clik();

        foreach (var item in weaponBaseData.animationDataArray)
        {
            animationPanel.AddAnimation(item);
        }
        foreach (var item in weaponBaseData.colliders)
        {
            colliderPanel.addCollider(item);
        }
    }
}
