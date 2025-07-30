using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ControlSprite : ScrollContainer
{
    MaterialData materialData;
    int pixelSizeWidth = 16;
    int pixelSizeHeight = 16;

    SpriteData objectData;

    public SpriteData ObjectData { get => objectData; set => objectData = value; }

    public Sprite2D GetSprite()
    {
        return Sprite2DView;
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        objectData = new SpriteData();
        ButtonSearchMaterial.Pressed += ButtonBuscar_Pressed;

        ControlTextureLocal.OnNotifySelection += ControlTextureLocal_OnNotifySelection;
       
        ColliderContainer.OnNotifyPreview += Control_OnNotifyPreview;
        CheckBoxHasCollider.Pressed += CheckBoxHasCollider_Pressed;

        CheckBoxMirrorV.Pressed += CheckBoxMirrorV_Pressed;
        CheckBoxMirror.Pressed += CheckBoxMirror_Pressed;
        SpinBoxOffsetX.ValueChanged += SpinBoxOffXsetTile_ValueChanged;
        SpinBoxOffsetY.ValueChanged += SpinBoxOffYsetTile_ValueChanged;
        SpinBoxScale.ValueChanged += SpinBoxScale_ValueChanged;

        ButtonSplit.Pressed += ButtonSplit_Pressed;
        SpinBoxZoom.ValueChanged += SpinBoxZoom_ValueChanged;

        ViewItems.MultiSelected += ViewItems_MultiSelected;
        ViewItems.ItemSelected += ViewItems_ItemSelected;

        SpinBoxZoomGrid.ValueChanged += SpinBoxZoomGrid_ValueChanged;
    }

    private void SpinBoxOffXsetTile_ValueChanged(double value)
    {
        Sprite2DView.Offset = new Vector2((float)SpinBoxOffsetX.Value, (float)SpinBoxOffsetY.Value * (-1));
        objectData.offsetX = (float)SpinBoxOffsetX.Value;
        OnNotifyChangued?.Invoke(this);
    }

    private void SpinBoxZoomGrid_ValueChanged(double value)
    {
        ControlGrid.Scale = new Vector2((float)value, (float)value);
        PanelBase.CustomMinimumSize = new Vector2(ControlGrid.CustomMinimumSize.X * (float)value, (float)(ControlGrid.CustomMinimumSize.Y * value));
        
    }

    private void ViewItems_ItemSelected(long index)
    {
        int id = (int)ViewItems.GetItemMetadata((int)index);
        Sprite2DView.Texture = GetRegion(id);        
    }



    private void SpinBoxZoom_ValueChanged(double value)
    {
        ViewItems.IconScale = (float)value;
    }
    private void ButtonSplit_Pressed()
    {
        ViewItems.Clear();
        pixelSizeWidth = (int)SpinBoxWidthPixel.Value;
        pixelSizeHeight = (int)SpinBox2HeightPixel.Value;
        ViewItems.FixedIconSize = new Vector2I(pixelSizeWidth, pixelSizeHeight);
        if (pixelSizeWidth <= 64)
        {
            ViewItems.IconScale = 3;
            SpinBoxZoom.Value = 3;
        }
        else
        {
            ViewItems.IconScale = 1;
            SpinBoxZoom.Value = 1;
        }

        List<Texture> list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(materialData.pathTexture),
            new Vector2I(pixelSizeWidth, pixelSizeHeight));
        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!TextureHelper.IsTextureEmpty(item))
            {
                int idx = ViewItems.AddItem("ID:" + i, (Texture2D)item);
                ViewItems.SetItemMetadata(idx, i);
            }
        }

    }
    private void SpinBoxScale_ValueChanged(double value)
    {
        ControlSpriteInternal.Scale = new Vector2((float)value, (float)value);        
        objectData.scale = (float)SpinBoxScale.Value;
        OnNotifyChangued?.Invoke(this);
    }

    private void SpinBoxOffYsetTile_ValueChanged(double value)
    {
        Sprite2DView.Offset = new Vector2((float)SpinBoxOffsetX.Value, (float)SpinBoxOffsetY.Value * (-1));        
        objectData.offsetY = (float)SpinBoxOffsetY.Value;
        OnNotifyChangued?.Invoke(this);
    }

    private void CheckBoxMirrorV_Pressed()
    {
        Sprite2DView.FlipV = CheckBoxMirrorV.ButtonPressed;


        if (CheckBoxMirrorV.ButtonPressed)
        {
            objectData.heightFormat = (-1) * objectData.height;
        }
        else
        {
            objectData.heightFormat = objectData.height;
        }
        
        objectData.mirrorY = CheckBoxMirrorV.ButtonPressed;
        OnNotifyChangued?.Invoke(this);
    }

    private void CheckBoxMirror_Pressed()
    {
        Sprite2DView.FlipH = CheckBoxMirror.ButtonPressed;


        if (CheckBoxMirror.ButtonPressed)
        {
            objectData.widhtFormat = (-1) * objectData.widht;
        }
        else
        {
            objectData.widhtFormat = objectData.widht;
        }
        objectData.mirrorX = CheckBoxMirror.ButtonPressed;
        OnNotifyChangued?.Invoke(this);
    }
    private void Control_OnNotifyPreview(GodotEcsArch.sources.managers.Collision.GeometricShape2D itemData)
    {
        objectData.collisionBody = itemData;

        CollisionShapeView.Position = new Vector2((float)itemData.originPixelX, (float)itemData.originPixelY * (-1));
        switch (itemData)
        {
            case Rectangle:
                var shape = new RectangleShape2D();
                CollisionShapeView.Shape = shape;
                shape.Size = new Vector2((float)itemData.widthPixel, (float)itemData.heightPixel);
                break;
            case Circle:
                var shapeC = new CircleShape2D();
                CollisionShapeView.Shape = shapeC;
                shapeC.Radius = itemData.widthPixel;
                break;
            default:
                break;
        }
        OnNotifyChangued?.Invoke(this);
    }
    public void SetData(SpriteData data)
    {
        if (data == null)
        {  return; }
        objectData = data;
        materialData = MaterialManager.Instance.GetMaterial(objectData.idMaterial);
        
        Sprite2DView.Texture = MaterialManager.Instance.GetAtlasTextureInternal(objectData.idMaterial, objectData.x, objectData.y, objectData.widht, objectData.height);
        Sprite2DView.FlipH = CheckBoxMirror.ButtonPressed;
        Sprite2DView.FlipV = CheckBoxMirrorV.ButtonPressed;

        if (objectData.colorString != null)
        {
            var components = objectData.colorString.Trim('(', ')')
                       .Split(',')
                       .Select(s => float.Parse(s.Trim()))
                       .ToArray();
            objectData.color = new Color(components[0], components[1], components[2], components[3]);
        }

        ColorButtonBase.Color = objectData.color;
        SpinBoxOffsetY.Value = objectData.offsetY;
        SpinBoxOffsetX.Value = objectData.offsetX;
        
        CheckBoxHasCollider.ButtonPressed = objectData.haveCollider;
        CheckBoxMirror.ButtonPressed = objectData.mirrorX;
        CheckBoxMirrorV.ButtonPressed = objectData.mirrorY;
        SpinBoxScale.Value = objectData.scale;
        CheckBoxMirrorV_Pressed();
        CheckBoxMirror_Pressed();
        WindowViewDb_OnRequestSelectedItem(materialData.id);
        if (objectData.haveCollider)
        {
            ColliderContainer.SetData(objectData.collisionBody);
            CheckBoxHasCollider.ButtonPressed = true;
            Control_OnNotifyPreview(objectData.collisionBody);
            CheckBoxHasCollider_PressedUI();
            CheckBoxHasCollider_Pressed();
        }

    }
    private void SaveAll()
    {
       
        objectData.idMaterial = materialData.id;
        AtlasTexture atlasTexture = (AtlasTexture)Sprite2DView.Texture;
        objectData.x = atlasTexture.Region.Position.X;
        objectData.y = atlasTexture.Region.Position.Y;
        objectData.widht = atlasTexture.Region.Size.X;
        objectData.height = atlasTexture.Region.Size.Y;
        
        objectData.colorString = ColorButtonBase.Color.ToString();

        CheckBoxMirrorV_Pressed();
        CheckBoxMirror_Pressed();
        objectData.scale = (float)SpinBoxScale.Value;
        if (!objectData.haveCollider)
        {
            objectData.collisionBody = null;
        }
        //OnNotifyChanguedSimple?.Invoke();
      
       // QueueFree();
    }
    private void CheckBoxHasCollider_Pressed()
    {
        if (CheckBoxHasCollider.ButtonPressed)
        {
            ColliderContainer.Visible = true;
            CollisionShapeView.Visible = true;
            if (objectData.collisionBody == null)
            {
                Control_OnNotifyPreview(new Rectangle(16, 16, 0, 0));
            }
        }
        else
        {
            ColliderContainer.Visible = false;
            CollisionShapeView.Visible = false;
        }
        objectData.haveCollider = CheckBoxHasCollider.ButtonPressed;
        OnNotifyChangued?.Invoke(this);
    }
    private void ControlTextureLocal_OnNotifySelection(float x, float y, float width, float height)
    {
        if (materialData != null)
        {
            Sprite2DView.Texture = MaterialManager.Instance.GetAtlasTextureInternal(materialData.id, x, y, width, height);
            SaveAll();
            OnNotifyChangued?.Invoke(this);
        }
        
    }

    private void ViewItems_MultiSelected(long index, bool selected)
    {
        int id = (int)ViewItems.GetItemMetadata((int)index);
        Sprite2DView.Texture = GetRegion(id);
    }
    private AtlasTexture GetRegion(int internalPosition)
    {

        AtlasTexture atlasTexture = new AtlasTexture();
        atlasTexture.Atlas = (Texture2D)(materialData.textureMaterial);
        int columns = (int)(materialData.widhtTexture / pixelSizeWidth); //8


        int row = internalPosition / columns; // Fila correspondiente al índice
        int column = internalPosition % columns; // Columna correspondiente al índice

        // Calcular las coordenadas de la subimagen a partir del índice
        int x = column * pixelSizeWidth;
        int y = row * pixelSizeHeight;


        atlasTexture.Region = new Rect2(x, y, pixelSizeWidth, pixelSizeHeight);
        return atlasTexture;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    private void ButtonBuscar_Pressed()
    {
        WindowViewDb windowViewDb = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowViewDB.tscn").Instantiate<WindowViewDb>();
        AddChild(windowViewDb);
        windowViewDb.Show();
        windowViewDb.EnableFilter(false);
        windowViewDb.LoadItems<MaterialData>();
        windowViewDb.OnRequestSelectedItem += WindowViewDb_OnRequestSelectedItem;
    }

    private void WindowViewDb_OnRequestSelectedItem(int id)
    {

        ViewItems.Clear();


        materialData = DataBaseManager.Instance.FindById<MaterialData>(id);

        ControlTextureLocal.SetTexture((Texture2D)materialData.textureMaterial);
        SpinBox2HeightPixel.Value = materialData.divisionPixelY;
        SpinBoxWidthPixel.Value = materialData.divisionPixelX;

        pixelSizeWidth = (int)SpinBoxWidthPixel.Value;
        pixelSizeHeight = (int)SpinBox2HeightPixel.Value;

        ViewItems.FixedIconSize = new Vector2I(pixelSizeWidth, pixelSizeHeight);
        if (pixelSizeWidth <= 64)
        {
            ViewItems.IconScale = 3;
            SpinBoxZoom.Value = 3;
        }
        else
        {
            ViewItems.IconScale = 1;
            SpinBoxZoom.Value = 1;
        }
        List<Texture> list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(materialData.pathTexture), new Vector2I(materialData.divisionPixelX, materialData.divisionPixelY));
        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!TextureHelper.IsTextureEmpty(item))
            {
                int idx = ViewItems.AddItem("ID:" + i, (Texture2D)item);
                ViewItems.SetItemMetadata(idx, i);
            }
        }
    }
}
