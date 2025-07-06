using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.managers.Collision;

public partial class WindowTiles : Window, IFacadeWindow<TileDynamicData>
{
    MaterialData materialData;
    int pixelSizeWidth = 16;
    int pixelSizeHeight = 16;

    TileDynamicData objectData;

    public event IFacadeWindow<TileDynamicData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        objectData = new TileDynamicData();
        ButtonSearchMaterial.Pressed += ButtonBuscar_Pressed;
        
        ControlTextureLocal.OnNotifySelection += ControlTextureLocal_OnNotifySelection;
        ButtonSave.Pressed += ButtonSave_Pressed;
        ColliderContainer.OnNotifyPreview += Control_OnNotifyPreview;
        CheckBoxHasCollider.Pressed += CheckBoxHasCollider_Pressed;

        CheckBoxMirrorV.Pressed += CheckBoxMirrorV_Pressed;
        CheckBoxMirror.Pressed += CheckBoxMirror_Pressed;
        SpinBoxOffsetX.ValueChanged += SpinBoxOffsetTile_ValueChanged;
        SpinBoxOffsetY.ValueChanged += SpinBoxOffsetTile_ValueChanged;
        SpinBoxScale.ValueChanged += SpinBoxScale_ValueChanged;

        ButtonSplit.Pressed += ButtonSplit_Pressed;
        SpinBoxZoom.ValueChanged += SpinBoxZoom_ValueChanged;
        CheckBoxMultiSelection.Pressed += CheckBoxMultiSelection_Pressed;
        ViewItems.MultiSelected += ViewItems_MultiSelected;
        ViewItems.ItemSelected += ViewItems_ItemSelected;
        CheckBoxGenerateAll.Pressed += CheckBoxGenerateAll_Pressed;
        TabContainerOptions.TabChanged += TabContainerOptions_TabChanged;

        SpinBoxZoomGrid.ValueChanged += SpinBoxZoomGrid_ValueChanged;
    }
    private void SpinBoxZoomGrid_ValueChanged(double value)
    {
        ControlGrid.Scale = new Vector2((float)value, (float)value);
        PanelBase.CustomMinimumSize = new Vector2(ControlGrid.CustomMinimumSize.X * (float)value, (float)(ControlGrid.CustomMinimumSize.Y * value));
    }
    private void TabContainerOptions_TabChanged(long tab)
    {
        if (tab == 1)
        {
            CheckBoxGenerateAll.ButtonPressed = false;
            CheckBoxGenerateAll_Pressed();
        }
    }

    private void CheckBoxGenerateAll_Pressed()
    {
        if (CheckBoxGenerateAll.ButtonPressed)
        {
            ButtonSave.Text = "Guardar Todos Tiles";
        }
        else
        {
            ButtonSave.Text = "Guardar Tile";
        }
    }

    private void ViewItems_ItemSelected(long index)
    {        
        int id = (int)ViewItems.GetItemMetadata((int)index);
        Sprite2DView.Texture = GetRegion(id);
    }

    private void CheckBoxMultiSelection_Pressed()
    {
        ViewItems.DeselectAll();        
        if (CheckBoxMultiSelection.ButtonPressed)
        {
            ViewItems.SelectMode = ItemList.SelectModeEnum.Toggle;
        }
        else
        {
            ViewItems.SelectMode = ItemList.SelectModeEnum.Single;
        }
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
        ControlSprite.Scale = new Vector2((float)value, (float)value);
    }

    private void SpinBoxOffsetTile_ValueChanged(double value)
    {
        Sprite2DView.Offset = new Vector2((float)SpinBoxOffsetX.Value, (float)SpinBoxOffsetY.Value * (-1));
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
    }
    public void SetData(TileDynamicData data)
    {
        objectData = data;
        ColorButtonBase.Color = objectData.color;
        SpinBoxOffsetY.Value = objectData.offsetY;
        SpinBoxOffsetX.Value = objectData.offsetX;
        materialData = MaterialManager.Instance.GetMaterial(objectData.idMaterial);
        CheckBoxHasCollider.ButtonPressed = objectData.haveCollider;
        CheckBoxMirror.ButtonPressed = objectData.mirrorX;
        CheckBoxMirrorV.ButtonPressed = objectData.mirrorY;
        SpinBoxScale.Value = objectData.scale;
       
        WindowViewDb_OnRequestSelectedItem(materialData.id);
        if (objectData.haveCollider)
        {
            ColliderContainer.SetData(objectData.collisionBody);
            CheckBoxHasCollider.ButtonPressed = true;
            Control_OnNotifyPreview(objectData.collisionBody);
            CheckBoxHasCollider_PressedUI();
            CheckBoxHasCollider_Pressed();
        }
       Sprite2DView.Texture = MaterialManager.Instance.GetAtlasTexture(objectData.idMaterial, objectData.x, objectData.y, objectData.widht, objectData.height);
        Sprite2DView.FlipH = CheckBoxMirror.ButtonPressed;
        Sprite2DView.FlipV = CheckBoxMirrorV.ButtonPressed;
    }
    private void ButtonSave_Pressed()
    {
        objectData.name = materialData.name +"_"+objectData.id.ToString();
        objectData.haveCollider = CheckBoxHasCollider.ButtonPressed;
        objectData.idMaterial = materialData.id;
        AtlasTexture atlasTexture = (AtlasTexture)Sprite2DView.Texture;
        objectData.x = atlasTexture.Region.Position.X;
        objectData.y = atlasTexture.Region.Position.Y;
        objectData.widht = atlasTexture.Region.Size.X;
        objectData.height = atlasTexture.Region.Size.Y;
        objectData.scale = (float)SpinBoxScale.Value;
        objectData.offsetX = (float)SpinBoxOffsetX.Value;
        objectData.offsetY = (float)SpinBoxOffsetY.Value;

        objectData.mirrorX = CheckBoxMirror.ButtonPressed;
        objectData.mirrorY = CheckBoxMirrorV.ButtonPressed;
        objectData.colorString = ColorButtonBase.Color.ToString();

        CheckBoxMirrorV_Pressed();
        CheckBoxMirror_Pressed();
        if (!objectData.haveCollider)
        {
            objectData.collisionBody = null;            
        }

        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
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
    }
    private void ControlTextureLocal_OnNotifySelection(float x, float y, float width, float height)
    {
        if (materialData!=null)
        {
            Sprite2DView.Texture = MaterialManager.Instance.GetAtlasTexture(materialData.id, x, y, width, height);
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
