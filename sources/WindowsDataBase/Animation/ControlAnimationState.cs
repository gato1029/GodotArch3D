using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;

public partial class ControlAnimationState : PanelContainer
{
    // Called when the node enters the scene tree for the first time.

    public delegate void EventNotifyChanguedOrder(ControlAnimationState objectControl,DirectionArrowArray directionArrowArray);
    public event EventNotifyChanguedOrder OnNotifyChanguedOrder;

    public delegate void EventNotifyPointerSelect(ControlAnimationState objectControl, int animationDataPosition);
    public event EventNotifyPointerSelect OnNotifyPointerSelect;

    AnimationStateData objectData;
	public override void _Ready()
	{
        objectData = new AnimationStateData();
        InitializeUI(); // Insertado por el generador de UI

        foreach (DirectionAnimationType type in Enum.GetValues(typeof(DirectionAnimationType)))
        {
            OptionButtonDirection.AddItem(type.ToString());
        }
        
        OptionButtonDirection.ItemSelected += OptionButtonDirection_ItemSelected;
        OptionButtonDirection.Selected = 0;
        OptionButtonDirection_ItemSelected(0);

        OptionButtonPosition.ItemSelected += OptionButtonPosition_ItemSelected;
        CheckBoxLoop.Pressed += CheckBoxLoop_Pressed;
        CheckBoxMirror.Pressed += CheckBoxMirror_Pressed;

        SpinBoxDuration.ValueChanged += SpinBoxDuration_ValueChanged;

        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonUp.Pressed += ButtonUp_Pressed;
        ButtonDown.Pressed += ButtonDown_Pressed;
        OptionButtonPosition_ItemSelected(0);

        OnNotifyPointerSelect?.Invoke(this, 0);
    }
    public void SetData(float frameDuration, bool mirrorX, bool loop, int positionAnimation, FrameData[] frameArray )
    {
        SpinBoxDuration.Value = frameDuration;
        CheckBoxMirror.ButtonPressed = mirrorX;
        CheckBoxLoop.ButtonPressed = loop;

        objectData.mirrorHorizontal = CheckBoxMirror.ButtonPressed;
        objectData.loop = CheckBoxLoop.ButtonPressed;
        objectData.frameDuration = (float)SpinBoxDuration.Value;
        objectData.animationData[positionAnimation].frameDataArray =  frameArray;
        ControlAnimationFrames.SetData(objectData.animationData[positionAnimation]);
    }
    public void SetPosition(int position)
    {
        SpinBoxPosition.Value = position;
    }
    private void ButtonDown_Pressed()
    {
        OnNotifyChanguedOrder?.Invoke(this, DirectionArrowArray.DOWN);
    }

    private void ButtonUp_Pressed()
    {
        OnNotifyChanguedOrder?.Invoke(this, DirectionArrowArray.UP);
    }

    private void ButtonDelete_Pressed()
    {
        OnNotifyChanguedOrder?.Invoke(this, DirectionArrowArray.REMOVE);
        QueueFree();
    }

    private void SpinBoxDuration_ValueChanged(double value)
    {
        objectData.frameDuration = (float)SpinBoxDuration.Value;
    }

    private void CheckBoxMirror_Pressed()
    {
        objectData.mirrorHorizontal = CheckBoxMirror.ButtonPressed;
    }

    private void CheckBoxLoop_Pressed()
    {
        objectData.loop = CheckBoxLoop.ButtonPressed;        
    }

    private void OptionButtonPosition_ItemSelected(long index)
    {
        PositionAnimationType position = (PositionAnimationType)index;
        ControlAnimationFrames.QueueFree();
        ControlAnimationFrames = GD.Load<PackedScene>("res://sources/WindowsDataBase/Animation/ControlAnimation.tscn").Instantiate<ControlAnimation>();
        VBoxContainer2.AddChild(ControlAnimationFrames);
        ControlAnimationFrames.SetData(objectData.animationData[(int)position]);

        OnNotifyPointerSelect?.Invoke(this, (int)position);
    }

    private void OptionButtonDirection_ItemSelected(long index)
    {
        objectData.directionAnimationType = (DirectionAnimationType)index;
      
        switch (objectData.directionAnimationType)
        {
            case DirectionAnimationType.DOS:
                objectData.animationData = new AnimationData[2];
                break;
            case DirectionAnimationType.CUATRO:
                objectData.animationData = new AnimationData[4];
                break;
            case DirectionAnimationType.OCHO:
                objectData.animationData = new AnimationData[8];
                break;
            default:
                break;
        }
        for (int i = 0; i < objectData.animationData.Length; i++) {
            objectData.animationData[i] = new AnimationData();
        }

        OptionButtonPosition.Clear();
        int c = 0;
        switch (index)
        {
            case 0:
                foreach (PositionAnimationType type in Enum.GetValues(typeof(PositionAnimationType)))
                {
                    OptionButtonPosition.AddItem(type.ToString());
                    c++;
                    if (c==2)
                    {
                        break;
                    }
                }                
                break; 
                case 1:
                foreach (PositionAnimationType type in Enum.GetValues(typeof(PositionAnimationType)))
                {
                    OptionButtonPosition.AddItem(type.ToString());
                    c++;
                    if (c == 4)
                    {
                        break;
                    }
                }
                break;
                case 2:
                foreach (PositionAnimationType type in Enum.GetValues(typeof(PositionAnimationType)))
                {
                    OptionButtonPosition.AddItem(type.ToString());
                    c++;
                    if (c == 8)
                    {
                        break;
                    }
                }
                break;
            default:
                break;
        }

        OptionButtonPosition_ItemSelected(0);
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
