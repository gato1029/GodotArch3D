using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static Flecs.NET.Core.Ecs.Units;

public partial class WindowTileSprite : Window, IFacadeWindow<TileSpriteData>
{
    TileSpriteData objectData = new TileSpriteData();

    public event IFacadeWindow<TileSpriteData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		ControlTile.OnNotifyChangued += ControlTile_OnNotifyChangued;
        ControlSpriteEdit.ClearDraw();
        SpinBoxOffsetX.ValueChanged += SpinBoxOffsetX_ValueChanged;
        SpinBoxOffsetY.ValueChanged += SpinBoxOffsetY_ValueChanged;
        SpinBoxScale.ValueChanged += SpinBoxScale_ValueChanged;
        CheckBoxMirrorHorizontal.Pressed += CheckBoxMirrorHorizontal_Pressed;
        CheckBoxMirrorVertical.Pressed += CheckBoxMirrorVertical_Pressed;
        CheckBoxHasCollider.Pressed += CheckBoxHasCollider_Pressed;
        ControlCollider.OnNotifyPreview += ControlCollider_OnNotifyPreview;
        SpinBoxDepht.ValueChanged += SpinBoxDepht_ValueChanged;
        LineEditGrouping.GuiInput += LineEditGrouping_GuiInput;
        objectData.spriteData = new SpriteData();

        ButtonSave.Pressed += ButtonSave_Pressed;
        ButtonSaveSimilar.Pressed += ButtonSaveSimilar_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        SpinBoxFps.ValueChanged += SpinBoxFps_ValueChanged;
        CheckBoxLoop.Pressed += CheckBoxLoop_Pressed;


        foreach (TileSpriteType item in Enum.GetValues(typeof(TileSpriteType)))
        {
            OptionButtonType.AddItem(item.ToString());
        }
        OptionButtonType.ItemSelected += OptionButtonType_ItemSelected;

        ControlListTexturesAnimated.OnNotifyChangued += ControlListTexturesAnimated_OnNotifyChangued;

        ControlSpriteEdit.SetSelectedTile(0,0);
        ControlSpriteEdit.SetBlockedTile(0, 0);
    }

   

    private void CheckBoxLoop_Pressed()
    {
        ControlSpriteEdit.SetLoopAnimation(CheckBoxLoop.ButtonPressed);
    }

    private void SpinBoxFps_ValueChanged(double value)
    {
        ControlSpriteEdit.SetFpsAnimation((float)value);
    }

    private void ControlListTexturesAnimated_OnNotifyChangued(ControlListTextures objectControl)
    {
        var datalist=objectControl.GetData();
        List<FrameData> framesArray = new();
        foreach (var item in datalist)
        {
            framesArray.Add(new FrameData() { x= item.x, y= item.y, widht= item.width, height= item.height });
        }
        objectData.animationData.idMaterial = datalist[0].idMaterial;
        objectData.animationData.framesArray = framesArray.ToArray();

        ControlSpriteEdit.SetTextureAnimation(datalist);
        ControlSpriteEdit.SetScaleTexture((float)SpinBoxScale.Value);        
        ControlSpriteEdit.SetCellSize(datalist[0].width, datalist[0].height);
    }

    private void OptionButtonType_ItemSelected(long index)
    {
        TileSpriteType selectedType = (TileSpriteType)index;
        switch (selectedType)
        {
            case TileSpriteType.Static:
                ControlTile.Visible = true;
                ControlListTexturesAnimated.Visible = false;
                objectData.animationData = null;
                objectData.spriteData = new SpriteData();
                GridContainerAnimated.Visible=false;
                break;
            case TileSpriteType.Animated:
                ControlTile.Visible = false;
                ControlListTexturesAnimated.Visible = true;
                objectData.animationData = new SpriteAnimationData();
                objectData.spriteData = null;
                GridContainerAnimated.Visible = true;
                break;
            default:
                break;
        }
    }

    private void ButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveDirectById<TileSpriteData>(objectData.id);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }
    private void SaveAnimation()
    {
        objectData.animationData.scale = (float)SpinBoxScale.Value;
        objectData.animationData.haveCollider = CheckBoxHasCollider.ButtonPressed;
        objectData.animationData.mirrorY = CheckBoxMirrorVertical.ButtonPressed;
        objectData.animationData.mirrorX = CheckBoxMirrorVertical.ButtonPressed;
        objectData.animationData.offsetX = (float)SpinBoxOffsetX.Value;
        objectData.animationData.offsetY = (float)SpinBoxOffsetY.Value;
        objectData.animationData.yDepthRender = (float)SpinBoxDepht.Value;
        objectData.animationData.frameDuration =  (float)SpinBoxFps.Value;
        objectData.animationData.loop = CheckBoxLoop.ButtonPressed;        
    }
    private void SaveSprite()
    {
        objectData.spriteData.scale = (float)SpinBoxScale.Value;
        objectData.spriteData.haveCollider = CheckBoxHasCollider.ButtonPressed;
        objectData.spriteData.mirrorY = CheckBoxMirrorVertical.ButtonPressed;
        objectData.spriteData.mirrorX = CheckBoxMirrorVertical.ButtonPressed;
        objectData.spriteData.offsetX = (float)SpinBoxOffsetX.Value;
        objectData.spriteData.offsetY = (float)SpinBoxOffsetY.Value;
        objectData.spriteData.yDepthRender = (float)SpinBoxDepht.Value;
        
    }
    private void ButtonSaveSimilar_Pressed()
    {
        objectData.tileSpriteType = (TileSpriteType)OptionButtonType.Selected;
        objectData.tilesOcupancy = ControlSpriteEdit.GetSelectedTiles();
        objectData.ReGerenateId();
        switch (objectData.tileSpriteType)
        {
            case TileSpriteType.Static:
                SaveSprite();
                break;
            case TileSpriteType.Animated:
                SaveAnimation();
                break;
            default:
                break;
        }

        
        if (objectData.idGrouping == 0)
        {
            Message.ShowMessage(this, "Agrupador Necesario");
            return;
        }
        
        string unique =objectData.CreateUniqueId();
        bool exist= DataBaseManager.Instance.FindByName<TileSpriteData>(unique);
        if (exist)
        {
            Message.ShowConfirmation(this, "Duplicidad Detectada, Deseas Continuar?").Confirmed += () => {
                DataBaseManager.Instance.InsertUpdate(objectData);
                OnNotifyChanguedSimple?.Invoke();
            };
        }
        else
        {
            DataBaseManager.Instance.InsertUpdate(objectData);
            OnNotifyChanguedSimple?.Invoke();         
        }
    }

    private void LineEditGrouping_GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                FacadeWindowDataSearch<GroupingData> windowGrouping = new FacadeWindowDataSearch<GroupingData>("res://sources/WindowsDataBase/Group/WindowGrouping.tscn", this, WindowType.SELECTED);
                windowGrouping.OnNotifySelected += WindowGrouping_OnNotifySelected;
            }            
        }
    }

    private void WindowGrouping_OnNotifySelected(GroupingData objectSelected)
    {
        objectData.idGrouping = objectSelected.id;
        LineEditGrouping.Text = objectSelected.name;
    }

    private void SetDataSprite()
    {
        ControlTile.SetData(objectData.spriteData.idMaterial, objectData.spriteData.x,
        objectData.spriteData.y, objectData.spriteData.widht, objectData.spriteData.height);

        ControlSpriteEdit.SetTexture(ControlTile.GetTileData().texture);
        ControlSpriteEdit.SetScaleTexture(objectData.spriteData.scale);
        ControlSpriteEdit.SetOffsetPositionTexture(new Vector2(objectData.spriteData.offsetX, objectData.spriteData.offsetY));
        ControlSpriteEdit.SetOffsetCenterY(-objectData.spriteData.yDepthRender);
        ControlSpriteEdit.SetFlipX(objectData.spriteData.mirrorX);
        ControlSpriteEdit.SetFlipY(objectData.spriteData.mirrorY);
        ControlSpriteEdit.SetCellSize((int)objectData.spriteData.widht, (int)objectData.spriteData.height);
        SpinBoxOffsetX.Value = objectData.spriteData.offsetX;
        SpinBoxOffsetY.Value = objectData.spriteData.offsetY;
        SpinBoxScale.Value = objectData.spriteData.scale;
        CheckBoxMirrorHorizontal.ButtonPressed = objectData.spriteData.mirrorX;
        CheckBoxMirrorVertical.ButtonPressed = objectData.spriteData.mirrorY;
        CheckBoxHasCollider.ButtonPressed = objectData.spriteData.haveCollider;
        SpinBoxDepht.Value = objectData.spriteData.yDepthRender;

        if (objectData.spriteData.haveCollider)
        {
            ControlCollider.Visible = true;
            Size = new Vector2I((int)(820 + ControlCollider.Size.X), 600);
            if (objectData.spriteData.listCollisionBody!=null)
            {
                ControlCollider.SetData(objectData.spriteData.listCollisionBody.ToList());
            }
            
        }
        else
        {
            ControlCollider.Visible = false;
            Size = new Vector2I(820, 600);
        }
    }
    private void SetDataSpriteAnimated()
    {
        List<TileInfoKuro> dataInfo = new List<TileInfoKuro>();  
        foreach (var item in objectData.animationData.framesArray)
        {
            dataInfo.Add(new TileInfoKuro()
            {
                idMaterial = objectData.animationData.idMaterial,
                x = (int)item.x,
                y = (int)item.y,
                width = (int)item.widht,
                height = (int)item.height,
                texture = MaterialManager.Instance.GetAtlasTextureInternal(objectData.animationData.idMaterial, item.x, item.y, item.widht, item.height)
            });
        }
        ControlListTexturesAnimated.SetData(dataInfo);

        ControlSpriteEdit.SetTextureAnimation(dataInfo);
        ControlSpriteEdit.SetScaleTexture(objectData.animationData.scale);
        ControlSpriteEdit.SetOffsetPositionTexture(new Vector2(objectData.animationData.offsetX, objectData.animationData.offsetY));
        ControlSpriteEdit.SetOffsetCenterY(-objectData.animationData.yDepthRender);
        ControlSpriteEdit.SetFlipX(objectData.animationData.mirrorX);
        ControlSpriteEdit.SetFlipY(objectData.animationData.mirrorY);
        ControlSpriteEdit.SetCellSize(dataInfo[0].width, dataInfo[0].height);
        ControlSpriteEdit.SetFpsAnimation(objectData.animationData.frameDuration);
        ControlSpriteEdit.SetLoopAnimation(objectData.animationData.loop);

        SpinBoxOffsetX.Value = objectData.animationData.offsetX;
        SpinBoxOffsetY.Value = objectData.animationData.offsetY;
        SpinBoxScale.Value = objectData.animationData.scale;
        CheckBoxMirrorHorizontal.ButtonPressed = objectData.animationData.mirrorX;
        CheckBoxMirrorVertical.ButtonPressed = objectData.animationData.mirrorY;
        CheckBoxHasCollider.ButtonPressed = objectData.animationData.haveCollider;
        SpinBoxDepht.Value = objectData.animationData.yDepthRender;
        SpinBoxFps.Value = objectData.animationData.frameDuration;
        CheckBoxLoop.ButtonPressed = objectData.animationData.loop;

        if (objectData.animationData.haveCollider)
        {
            ControlCollider.Visible = true;
            Size = new Vector2I((int)(820 + ControlCollider.Size.X), 600);
            if (objectData.animationData.collisionBodyArray!=null)  
            {
                ControlCollider.SetData(objectData.animationData.collisionBodyArray.ToList());
            }
            
        }
        else
        {
            ControlCollider.Visible = false;
            Size = new Vector2I(820, 600);
        }
    }
    public void SetData(TileSpriteData data)
    {
        
        objectData = data;
        ControlSpriteEdit.SetSelectedTiles(objectData.tilesOcupancy);

        var temp =DataBaseManager.Instance.FindById<GroupingData>(objectData.idGrouping);
        if (temp != null)
        {
            LineEditGrouping.Text= temp.name;
        }       
        LineEditCode.Text = objectData.id.ToString();
        OptionButtonType.Selected = (int)objectData.tileSpriteType;
                
        switch (objectData.tileSpriteType)
        {
            case TileSpriteType.Static:
                SetDataSprite();
                ControlTile.Visible = true;
                ControlListTexturesAnimated.Visible = false;
                GridContainerAnimated.Visible = false;
                break;
            case TileSpriteType.Animated:
                SetDataSpriteAnimated();
                ControlTile.Visible = false;
                ControlListTexturesAnimated.Visible = true;
                GridContainerAnimated.Visible = true;
                break;
            default:
                break;
        }
    }
    private void ButtonSave_Pressed()
    {
        objectData.tileSpriteType = (TileSpriteType)OptionButtonType.Selected;
        objectData.tilesOcupancy = ControlSpriteEdit.GetSelectedTiles();
        switch (objectData.tileSpriteType)
        {
            case TileSpriteType.Static:
                SaveSprite();
                break;
            case TileSpriteType.Animated:
                SaveAnimation();
                break;
            default:
                break;
        }
        
       

        if (objectData.idGrouping==0)
        {
            Message.ShowMessage(this, "Agrupador Necesario");
            return;
        }
        string lastUnique= objectData.name;
        string unique = objectData.CreateUniqueId();        
        if (lastUnique != unique)
        {
            bool exist = DataBaseManager.Instance.FindByName<TileSpriteData>(unique);
            if (exist)
            {
                Message.ShowConfirmation(this, "Duplicidad Detectada, Deseas Continuar?").Confirmed += () =>
                {
                    DataBaseManager.Instance.InsertUpdate(objectData);
                    OnNotifyChanguedSimple?.Invoke();
                    QueueFree();
                };
            }
            else
            {
                DataBaseManager.Instance.InsertUpdate(objectData);
                OnNotifyChanguedSimple?.Invoke();
                QueueFree();
            }
        }
        else
        {
            DataBaseManager.Instance.InsertUpdate(objectData);
            OnNotifyChanguedSimple?.Invoke();
            QueueFree();
        }
    }

    private void SpinBoxDepht_ValueChanged(double value)
    {
        ControlSpriteEdit.SetOffsetCenterY(-(float)value);
        objectData.spriteData.yDepthRender = (float)value;
    }

    private void ControlCollider_OnNotifyPreview(GodotEcsArch.sources.managers.Collision.GeometricShape2D itemData)
    {
        if (itemData != null)
        {
           // objectData.spriteData.collisionBody = itemData;
            Vector2 position = new Vector2((float)itemData.originPixelX, (float)(-itemData.originPixelY));
            ControlSpriteEdit.SetPositionShape(new Vector2((float)itemData.originPixelX, (float)(-itemData.originPixelY)) * new Vector2 ((float)SpinBoxScale.Value, (float)SpinBoxScale.Value));
          
            switch (itemData)
            {
                case Rectangle:
                    ControlSpriteEdit.DrawSquare(position, new Vector2((float)itemData.widthPixel, (float)itemData.heightPixel));
                    break;
                case Circle:
                    ControlSpriteEdit.DrawCircle(position, (float)itemData.widthPixel);
                    break;
                default:
                    break;
            }
            var data = ControlCollider.GetAllCollidersData();

            switch ((TileSpriteType)OptionButtonType.Selected)
            {
                case TileSpriteType.Static:
                    objectData.spriteData.listCollisionBody = data.ToArray();
                    break;
                case TileSpriteType.Animated:
                    objectData.animationData.collisionBodyArray = data;
                    break;
                default:
                    break;
            }                      
        }
        else
        {
            ControlSpriteEdit.ClearDraw();
        }
    }

    private void CheckBoxHasCollider_Pressed()
    {
        if (CheckBoxHasCollider.ButtonPressed)
        {
            ControlCollider.Visible = true;
            Size = new Vector2I((int)(820 +ControlCollider.Size.X), 600);            
        }
        else
        {
            ControlSpriteEdit.ClearDraw();
            ControlCollider.Visible = false;
            Size = new Vector2I(820 , 600);
            switch ((TileSpriteType)OptionButtonType.Selected)
            {
                case TileSpriteType.Static:
                    objectData.spriteData.listCollisionBody = null;
                    break;
                case TileSpriteType.Animated:
                    objectData.animationData.collisionBodyArray = null;
                    break;
                default:
                    break;
            }
            
        }
        
    }

    private void CheckBoxMirrorVertical_Pressed()
    {
        if (CheckBoxMirrorVertical.ButtonPressed)
        {
            ControlSpriteEdit.SetFlipY(true);
        }
        else
        {
            ControlSpriteEdit.SetFlipY(false);
        }

    }

    private void CheckBoxMirrorHorizontal_Pressed()
    {
        if (CheckBoxMirrorHorizontal.ButtonPressed)
        {
            ControlSpriteEdit.SetFlipX(true);
        }
        else
        {
            ControlSpriteEdit.SetFlipX(false);
        }

    }

    private void SpinBoxScale_ValueChanged(double value)
    {
        ControlSpriteEdit.SetScaleTexture((float)value);
        
    }

    private void SpinBoxOffsetY_ValueChanged(double value)
    {
        ControlSpriteEdit.SetOffsetPositionTexture(new Vector2((float)SpinBoxOffsetX.Value, -(float)value));
        
    }

    private void SpinBoxOffsetX_ValueChanged(double value)
    {
        ControlSpriteEdit.SetOffsetPositionTexture(new Vector2((float)value, -(float)SpinBoxOffsetY.Value));                
    }

    private void ControlTile_OnNotifyChangued(ControlKuroTile objectControl)
    {
        var tileData = objectControl.GetTileData();
        ControlSpriteEdit.SetTexture(tileData.texture);
        ControlSpriteEdit.SetScaleTexture((float)SpinBoxScale.Value);

        objectData.spriteData.x = tileData.x;
        objectData.spriteData.y = tileData.y;
        objectData.spriteData.widht = tileData.width;
        objectData.spriteData.height = tileData.height;
        objectData.spriteData.idMaterial = tileData.idMaterial;

        ControlSpriteEdit.SetCellSize(tileData.width, tileData.height);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}


}
