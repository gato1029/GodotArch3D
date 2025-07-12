using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

public partial class ControlAnimationAccesory : ScrollContainer
{
    // Called when the node enters the scene tree for the first time.

    MaterialData materialData;
    int pixelSizeWidth = 16;
    int pixelSizeHeight = 16;
    ColliderScene control;
    double currentfps = 0;
    int indexFrame = 0;
    AnimationData animationData;
    List<FrameData> tiles = new List<FrameData>();

    public MaterialData MaterialData { get => materialData; set => materialData = value; }
    public SpriteAnimationData ObjectData { get => objectData; set => objectData = value; }

    SpriteAnimationData objectData;




    public void SetData(SpriteAnimationData data)
    {
        animationData = new AnimationData();
        objectData = data;
        ColorButtonBase.Color = objectData.color;
        SpinBoxOffsetY.Value = objectData.offsetY;
        SpinBoxOffsetX.Value = objectData.offsetX;
        materialData = MaterialManager.Instance.GetMaterial(objectData.idMaterial);
        CheckBoxHasCollider.ButtonPressed = objectData.haveCollider;
        CheckBoxMirror.ButtonPressed = objectData.mirrorX;
        CheckBoxMirrorV.ButtonPressed = objectData.mirrorY;
        SpinBoxScale.Value = objectData.scale;
        SpinBoxDuration.Value = objectData.frameDuration;
        WindowViewDb_OnRequestSelectedItem(materialData.id);
        if (objectData.haveCollider)
        {
            ColliderContainer.SetData(objectData.collisionBody);
            CheckBoxHasCollider.ButtonPressed = true;
            Control_OnNotifyPreview(objectData.collisionBody);
            CheckBoxHasCollider_PressedUI();
            CheckBoxHasCollider_Pressed();
        }

        FramesArray.SetData(objectData.framesArray);
        Sprite2DView.FlipH = CheckBoxMirror.ButtonPressed;
        Sprite2DView.FlipV = CheckBoxMirrorV.ButtonPressed;
    }
    private void ButtonSave_Pressed()
    {
       
        objectData.offsetY = (int)SpinBoxOffsetY.Value;
        objectData.offsetY = (int)SpinBoxOffsetX.Value;
        objectData.idMaterial = materialData.id;
        objectData.haveCollider = CheckBoxHasCollider.ButtonPressed;
        objectData.framesArray = FramesArray.GetAllDataFrames().ToArray();
        objectData.mirrorX = CheckBoxMirror.ButtonPressed;
        objectData.mirrorY = CheckBoxMirrorV.ButtonPressed;
        objectData.colorString = ColorButtonBase.Color.ToString();
        objectData.scale = (float)SpinBoxScale.Value;
        objectData.frameDuration = (float)SpinBoxDuration.Value;
        if (!CheckBoxHasCollider.ButtonPressed)
        {
            objectData.collisionBody = null;
        }
        OnNotifyChangued?.Invoke(this);                
    }
    public override void _Ready()
    {
        InitializeUI();
        objectData = new SpriteAnimationData();
        ButtonBuscar.Pressed += ButtonBuscar_Pressed;
        animationData = new AnimationData();

        ViewItems.MultiSelected += ViewItems_MultiSelected;
        CheckBoxModeSelection.Pressed += CheckBoxModeSelection_Pressed;
        SpinBoxDuration.ValueChanged += SpinBoxDuration_ValueChanged;
        CheckBoxLoop.Pressed += CheckBoxLoop_Pressed;
        CheckBoxMirror.Pressed += CheckBoxMirror_Pressed;
        CheckBoxFrameDuplicate.Pressed += CheckBoxFrameDuplicate_Pressed;

        ButtonSplit.Pressed += ButtonSplit_Pressed;

        SpinBoxZoom.ValueChanged += SpinBoxZoom_ValueChanged;
        CheckBoxMirrorV.Pressed += CheckBoxMirrorV_Pressed;
        ColliderContainer.OnNotifyPreview += Control_OnNotifyPreview;
        CheckBoxHasCollider.Pressed += CheckBoxHasCollider_Pressed;
        SpinBoxOffsetX.ValueChanged += SpinBoxOffsetTile_ValueChanged;
        SpinBoxOffsetY.ValueChanged += SpinBoxOffsetTile_ValueChanged;
        SpinBoxZoomGrid.ValueChanged += SpinBoxZoomGrid_ValueChanged;
        ButtonSave.Pressed += ButtonSave_Pressed;
        SpinBoxScale.ValueChanged += SpinBoxScale_ValueChanged;

        ScrollContainerGrid.ScrollVertical = 250;
        ScrollContainerGrid.ScrollHorizontal = 250;
    }

    private void SpinBoxScale_ValueChanged(double value)
    {
        ControlSprite.Scale = new Vector2((float)value, (float)value);
    }

    //public override void _Input(InputEvent @event)
    //{
    //    if (@event is InputEventMouseButton mouseEvent)
    //    {
    //        if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
    //        {
    //            double currentTime = Time.GetTicksMsec() / 1000.0;

    //            if (currentTime - _lastClickTime < DoubleClickThreshold)
    //            {
    //                ToggleMaximize();
    //            }

    //            _lastClickTime = currentTime;
    //        }
    //    }
    //}
    //private void ToggleMaximize()
    //{
    //    if (_isMaximized)
    //    {
    //        Size = _originalSize;
    //        Position = _originalPosition;
    //    }
    //    else
    //    {
    //        _originalSize = Size;
    //        _originalPosition = Position;
    //        Size = DisplayServer.ScreenGetSize();
    //        Position = Vector2I.Zero;
    //    }

    //    _isMaximized = !_isMaximized;
    //}


    private void SpinBoxZoomGrid_ValueChanged(double value)
    {
        ControlGrid.Scale = new Vector2((float)value, (float)value);
        PanelBase.CustomMinimumSize = new Vector2(ControlGrid.CustomMinimumSize.X * (float)value, (float)(ControlGrid.CustomMinimumSize.Y * value));
    }

    private void SpinBoxOffsetTile_ValueChanged(double value)
    {
        Sprite2DView.Offset = new Vector2((float)SpinBoxOffsetX.Value, (float)SpinBoxOffsetY.Value * (-1));
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

    private void ControlAnimationItems_OnNotifyChangued(ControlAnimationStateArray objectControl)
    {
        OnNotifyChangued?.Invoke(this);
    }

    private void ControlAnimationItems_OnNotifyAnimationSelected(ControlAnimationState objectControl, int animationDataPosition)
    {

        FramesArray.SetData(objectControl.ObjectData.animationData[animationDataPosition].frameDataArray);
        if (objectControl.ObjectData.animationData[animationDataPosition] != null && objectControl.ObjectData.animationData[animationDataPosition].frameDataArray != null)
        {
            if (objectControl.ObjectData.idMaterial != 0)
            {
                if (materialData == null || materialData.id != objectControl.ObjectData.idMaterial)
                {
                    materialData = MaterialManager.Instance.GetMaterial(objectControl.ObjectData.idMaterial);
                    WindowViewDb_OnRequestSelectedItem(materialData.id);
                }

                CheckBoxMirror.ButtonPressed = objectControl.ObjectData.animationData[animationDataPosition].mirrorHorizontal;
                CheckBoxMirrorV.ButtonPressed = objectControl.ObjectData.animationData[animationDataPosition].mirrorVertical;
                Sprite2DView.FlipH = CheckBoxMirror.ButtonPressed;
                Sprite2DView.FlipV = CheckBoxMirrorV.ButtonPressed;
                SpinBoxDuration.Value = objectControl.ObjectData.animationData[animationDataPosition].frameDuration;
                CheckBoxLoop.ButtonPressed = objectControl.ObjectData.animationData[animationDataPosition].loop;

                currentfps = 0;
                indexFrame = 0;
            }

        }

    }






    private void CheckBoxMirrorV_Pressed()
    {
        Sprite2DView.FlipV = CheckBoxMirrorV.ButtonPressed;

        if (CheckBoxMirrorV.ButtonPressed)
        {
            FramesArray.ReverseFramesVertical(true);
        }
        else
        {
            FramesArray.ReverseFramesVertical(false);
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

    internal void SetData(AnimationStateData[] animationDataArray)
    {
        if (animationDataArray != null && animationDataArray.Length > 0)
        {
            if (animationDataArray[0].idMaterial != 0)
            {
                materialData = MaterialManager.Instance.GetMaterial(animationDataArray[0].idMaterial);
                WindowViewDb_OnRequestSelectedItem(materialData.id);
            }


            if (animationDataArray[0].animationData != null)
            {
                FramesArray.SetData(animationDataArray[0].animationData[0].frameDataArray);
                //CheckBoxMirror.ButtonPressed = animationDataArray[0].mirrorHorizontal;
                //CheckBoxMirrorV.ButtonPressed = animationDataArray[0].mirrorVertical;
                Sprite2DView.FlipH = CheckBoxMirror.ButtonPressed;
                Sprite2DView.FlipV = CheckBoxMirrorV.ButtonPressed;
                //SpinBoxDuration.Value = animationDataArray[0].frameDuration;
                //CheckBoxLoop.ButtonPressed = animationDataArray[0].loop;
            }


        }

    }

    public override void _Process(double delta)
    {
        animationData.frameDataArray = FramesArray.GetAllDataFrames().ToArray();
        if (animationData.frameDataArray != null && animationData.frameDataArray.Length > 0 && indexFrame < animationData.frameDataArray.Length)
        {

            currentfps += delta;
            if (currentfps >= SpinBoxDuration.Value)
            {
                FrameData iFrame = animationData.frameDataArray[indexFrame];
                indexFrame++;
                currentfps = 0;
                var dataTexture = MaterialManager.Instance.GetAtlasTexture(materialData.id, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
                Sprite2DView.Texture = dataTexture;
            }
            if (CheckBoxLoop.ButtonPressed && indexFrame >= animationData.frameDataArray.Length)
            {
                indexFrame = 0;
            }

        }
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



    private void CheckBoxFrameDuplicate_Pressed()
    {
        //TextEditFrames.Text = "";
        ViewItems.DeselectAll();
        FramesArray.RemoveAll();
        animationData.frameDataArray = null;

        if (CheckBoxFrameDuplicate.ButtonPressed)
        {
            CheckBoxModeSelection.Visible = false;
            LabelSeleccion.Visible = false;
            ViewItems.SelectMode = ItemList.SelectModeEnum.Multi;
        }
        else
        {
            CheckBoxModeSelection.Visible = true;
            LabelSeleccion.Visible = true;
        }


    }


    private void CheckBoxLoop_Pressed()
    {
        indexFrame = 0;
    }

    private void CheckBoxMirror_Pressed()
    {
        Sprite2DView.FlipH = CheckBoxMirror.ButtonPressed;
        if (CheckBoxMirror.ButtonPressed)
        {
            FramesArray.ReverseFramesHorizontal(true);
        }
        else
        {
            FramesArray.ReverseFramesHorizontal(false);
        }

    }

    private void CheckBoxModeSelection_Pressed()
    {
        ViewItems.DeselectAll();
        FramesArray.RemoveAll();
        animationData.frameDataArray = null;
        if (CheckBoxModeSelection.ButtonPressed)
        {
            ViewItems.SelectMode = ItemList.SelectModeEnum.Toggle;
            CheckBoxFrameDuplicate.Visible = false;
            LabelFrameDuplicate.Visible = false;
        }
        else
        {
            CheckBoxFrameDuplicate.Visible = true;
            LabelFrameDuplicate.Visible = true;
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
    private void SpinBoxDuration_ValueChanged(double value)
    {
        //objectData.frameDuration = (float)value;
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
    private void ViewItems_MultiSelected(long index, bool selected)
    {


        indexFrame = 0;
        if (CheckBoxModeSelection.ButtonPressed)
        {
            var data = ViewItems.GetSelectedItems();
            FramesArray.RemoveAll();
            foreach (var item in data)
            {
                int id = (int)ViewItems.GetItemMetadata(item);

                Sprite2DView.Texture = GetRegion(id);

                AtlasTexture atlasTexture = GetRegion(id);
                FrameData frameData = new FrameData();
                frameData.x = atlasTexture.Region.Position.X;
                frameData.y = atlasTexture.Region.Position.Y;
                frameData.widht = atlasTexture.Region.Size.X;
                frameData.height = atlasTexture.Region.Size.Y;

                //tiles.Add(frameData);
                FramesArray.AddData(frameData);
            }


            //animationData.frameDataArray = FramesArray.GetAllDataFrames().ToArray();
        }
        if (CheckBoxFrameDuplicate.ButtonPressed)
        {
            int id = (int)ViewItems.GetItemMetadata((int)index);

            Sprite2DView.Texture = GetRegion(id);

            AtlasTexture atlasTexture = GetRegion(id);
            FrameData frameData = new FrameData();
            frameData.x = atlasTexture.Region.Position.X;
            frameData.y = atlasTexture.Region.Position.Y;
            frameData.widht = atlasTexture.Region.Size.X;
            frameData.height = atlasTexture.Region.Size.Y;
            FramesArray.AddData(frameData);
            //animationData.frameDataArray = FramesArray.GetAllDataFrames().ToArray();
        }

    }



    private void WindowViewDb_OnRequestSelectedItem(int id)
    {

        tiles.Clear();

        ViewItems.Clear();
        animationData.frameDataArray = null;

        materialData = DataBaseManager.Instance.FindById<MaterialData>(id);
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
