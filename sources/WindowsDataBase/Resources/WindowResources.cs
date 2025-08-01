using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using System;

public partial class WindowResources : Window, IFacadeWindow<ResourceData>
{

	ResourceData objectData;

    public event IFacadeWindow<ResourceData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        objectData = new ResourceData();

        ButtonSave.Pressed += ButtonSave_Pressed;
        ControlSprite.OnNotifyChangued += ControlSprite_OnNotifyChangued;
        ControlBuildingGridBasic.OnNotifyChangued += ControlBuildingGridBasic_OnNotifyChangued;
        CheckBoxHasAnimation.Pressed += CheckBoxHasAnimation_Pressed;

    }
    ContainerAnimation ContainerAnimationBasico = null;
    ScrollContainer scrollContainer;
    private void CheckBoxHasAnimation_Pressed()
    {
        if (CheckBoxHasAnimation.ButtonPressed)
        {
            scrollContainer = new ScrollContainer();
            scrollContainer.Name = "Animacion";
            scrollContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            ContainerAnimationBasico = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/ContainerAnimation.tscn").Instantiate<ContainerAnimation>();
            ContainerAnimationBasico.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            ContainerAnimationBasico.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            scrollContainer.AddChild(ContainerAnimationBasico);
            TabContainerBase.AddChild(scrollContainer);
            
        }
        else
        {
            TabContainerBase.RemoveChild(scrollContainer);
            scrollContainer = null;
            ContainerAnimationBasico = null;
        }
    }

    private void ControlBuildingGridBasic_OnNotifyChangued(ControlBuildingGrid objectControl)
    {
        if (objectControl.SelectedBuildings.Count > 0)
        {
            objectData.buildingPosition = objectControl.GetBuildingPosition();
        }

    }

    private void ControlSprite_OnNotifyChangued(ControlSprite objectControl)
    {
        ControlBuildingGridBasic.SetTexture(objectControl.GetSprite().Texture);
        ControlBuildingGridBasic.SetScaleTexture(objectControl.ObjectData.scale);
        ControlBuildingGridBasic.SetOffsetTexture(ControlSprite.ObjectData.offsetX, ControlSprite.ObjectData.offsetY);
        Sprite2DViewCentral.Texture = objectControl.GetSprite().Texture;
    }
    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.description = TextEditDescription.Text;
        objectData.amount = (int)SpinBoxAmount.Value;
        objectData.spriteData = ControlSprite.ObjectData;
        objectData.isAnimated = CheckBoxHasAnimation.ButtonPressed;
        if (objectData.isAnimated)
        {
            objectData.animationData = ContainerAnimationBasico.GetData();
        }
        else
        {
            objectData.animationData = null;
        }
        objectData.miniatura = ControlSpriteMiniatura.ObjectData;
        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}

    public void SetData(ResourceData data)
    {
        objectData = data;
        LineEditName.Text = data.name;
        TextEditDescription.Text = data.description;
        SpinBoxAmount.Value = data.amount;
        CheckBoxHasAnimation.ButtonPressed = data.isAnimated;

        ControlSpriteMiniatura.SetData(objectData.miniatura);
        if (data.isAnimated)
        {
            CheckBoxHasAnimation_Pressed();
        }
        if (objectData.animationData!=null)
        {
            ContainerAnimationBasico.SetData(objectData.animationData.ToArray());
        }
        if (data.spriteData != null)
        {
            ControlSprite.SetData(data.spriteData);
            Sprite2DViewCentral.Texture = ControlSprite.GetSprite().Texture;
        }
    
        ControlBuildingGridBasic.SetTexture(ControlSprite.GetSprite().Texture);
        ControlBuildingGridBasic.SetScaleTexture(ControlSprite.ObjectData.scale);
        ControlBuildingGridBasic.SetBuildingPosition(objectData.buildingPosition);
        ControlBuildingGridBasic.SetOffsetTexture(ControlSprite.ObjectData.offsetX, ControlSprite.ObjectData.offsetY);
        }
}
