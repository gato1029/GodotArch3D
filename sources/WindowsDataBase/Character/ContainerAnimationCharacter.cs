using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;


public partial class ContainerAnimationCharacter : Window, IFacadeWindow<AnimationCharacterBaseData>
{
    // Called when the node enters the scene tree for the first time.

    AnimationCharacterBaseData objectData;

    public event IFacadeWindow<AnimationCharacterBaseData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called every frame. 'delta' is the elapsed time since the previous frame.

    public override void _Ready()
    {
        InitializeUI();
        objectData = new AnimationCharacterBaseData();
        PanelMovimiento.OnNotifyPreview += PanelMovimiento_OnNotifyPreview;
        PanelCuerpo.OnNotifyPreview += PanelCuerpo_OnNotifyPreview;
        SpinBoxZordering.ValueChanged += SpinBoxZordering_ValueChanged;
        CheckBoxAnimationComposite.Pressed += CheckBoxAnimationComposite_Pressed;
        Animacion_Base.OnNotifyChangued += Animacion_Base_OnNotifyChangued;
        Animacion_Extra.OnNotifyChangued += Animacion_Extra_OnNotifyChangued;
        LineEditName.TextChanged += LineEditName_TextChanged;
        ButtonSave.Pressed += ButtonSave_Pressed;
        ButtonSaveAll.Pressed += ButtonSaveAll_Pressed;
        CloseRequested += ContainerAnimationCharacter_CloseRequested;

    }

    private void ButtonSaveAll_Pressed()
    {

        if (objectData.animationDataArray != null)
        {
            foreach (var item in objectData.animationDataArray)
            {
                item.idMaterial = Animacion_Base.MaterialData.id;
            }
        }

        if (objectData.animationExtraDataArray != null)
        {
            foreach (var item in objectData.animationExtraDataArray)
            {
                item.idMaterial = Animacion_Extra.MaterialData.id;
            }
        }
        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChangued?.Invoke(this);
        OnNotifyChanguedSimple?.Invoke();        
    }

    private void ButtonSave_Pressed()
    {
        if (objectData.animationDataArray != null)
        {
            foreach (var item in objectData.animationDataArray)
            {
                item.idMaterial = Animacion_Base.MaterialData.id;
            }
        }

        if (objectData.animationExtraDataArray != null)
        {
            foreach (var item in objectData.animationExtraDataArray)
            {
                item.idMaterial = Animacion_Extra.MaterialData.id;
            }
        }

        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChangued?.Invoke(this);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void LineEditName_TextChanged(string newText)
    {
        objectData.name = newText;
    }

    private void ContainerAnimationCharacter_CloseRequested()
    {
        OnNotifyChanguedSimple?.Invoke();
    }

    public void SetData(AnimationCharacterBaseData data)
    {
       objectData = data;
       LineEditName.Text = data.name;
       SpinBoxZordering.Value = data.zOrderingOrigin;
       CheckBoxAnimationComposite.ButtonPressed = data.hasCompositeAnimation;
       Animacion_Base.SetData(data.animationDataArray);
       Animacion_Extra.SetData(data.animationExtraDataArray);
       PanelMovimiento.SetData(data.collisionMove);
       PanelCuerpo.SetData(data.collisionBody);        
        if (data.animationDataArray != null)
        {
            AnimationStateData dataAnim = data.animationDataArray[0];
            FrameData iFrame = dataAnim.animationData[0].frameDataArray[0];
            var dataTexture = MaterialManager.Instance.GetAtlasTexture(dataAnim.idMaterial, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
            Sprite2DView.Texture = dataTexture;
        }

    }
    private void Animacion_Extra_OnNotifyChangued(ContainerAnimation objectControl)
    {
        objectData.animationExtraDataArray = objectControl.GetData().ToArray();
        
    }

    private void Animacion_Base_OnNotifyChangued(ContainerAnimation objectControl)
    {         
        objectData.animationDataArray = objectControl.GetData().ToArray();
       
        if (objectControl.GetData() != null)
        {
            AnimationStateData data = objectControl.GetData()[0];
            if (data.animationData[0].frameDataArray!=null)
            {
                FrameData iFrame = data.animationData[0].frameDataArray[0];
                var dataTexture = MaterialManager.Instance.GetAtlasTexture(data.idMaterial, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
                Sprite2DView.Texture = dataTexture;
            }
           
        }
    }

    private void CheckBoxAnimationComposite_Pressed()
    {
        objectData.hasCompositeAnimation = CheckBoxAnimationComposite.ButtonPressed;
    }

    private void SpinBoxZordering_ValueChanged(double value)
    {
        objectData.zOrderingOrigin = (float)value;
    }

    private void PanelCuerpo_OnNotifyPreview(GeometricShape2D geometricShape2D)
    {
        objectData.collisionBody = geometricShape2D;
        CollisionShapeView.Position = new Vector2((float)geometricShape2D.originPixelX, (float)geometricShape2D.originPixelY * (-1));
        switch (geometricShape2D)
        {
            case Rectangle:
                var shape = new RectangleShape2D();
                CollisionShapeView.Shape = shape;
                shape.Size = new Vector2((float)geometricShape2D.widthPixel, (float)geometricShape2D.heightPixel);
                break;
            case Circle:
                var shapeC = new CircleShape2D();
                CollisionShapeView.Shape = shapeC;
                shapeC.Radius = geometricShape2D.widthPixel;
                break;
            default:
                break;
        }
    }

    private void PanelMovimiento_OnNotifyPreview(GeometricShape2D geometricShape2D)
    {
        objectData.collisionMove = geometricShape2D;
        CollisionShapeView.Position = new Vector2((float)geometricShape2D.originPixelX, (float)geometricShape2D.originPixelY * (-1));
        switch (geometricShape2D)
        {
            case Rectangle:
                var shape = new RectangleShape2D();
                CollisionShapeView.Shape = shape;
                shape.Size = new Vector2((float)geometricShape2D.widthPixel, (float)geometricShape2D.heightPixel);
                break;
            case Circle:
                var shapeC = new CircleShape2D();
                CollisionShapeView.Shape = shapeC;
                shapeC.Radius = geometricShape2D.widthPixel;
                break;
            default:
                break;
        }
    }

 
}
