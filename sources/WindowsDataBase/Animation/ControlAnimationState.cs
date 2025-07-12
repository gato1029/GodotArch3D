using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Reflection;

public partial class ControlAnimationState : PanelContainer
{
    // Called when the node enters the scene tree for the first time.
    public delegate void EventNotifyCollision(GodotEcsArch.sources.managers.Collision.GeometricShape2D geometricShape2D);
    public event EventNotifyCollision OnNotifyCollisionSelected;

    public delegate void EventNotifyChanguedOrder(ControlAnimationState objectControl,DirectionArrowArray directionArrowArray);
    public event EventNotifyChanguedOrder OnNotifyChanguedOrder;

    public delegate void EventNotifyPointerSelect(ControlAnimationState objectControl, int animationDataPosition);
    public event EventNotifyPointerSelect OnNotifyPointerSelect;

    AnimationStateData objectData;

    public AnimationStateData ObjectData { get => objectData; set => objectData = value; }

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
      

        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonUp.Pressed += ButtonUp_Pressed;
        ButtonDown.Pressed += ButtonDown_Pressed;
        OptionButtonPosition_ItemSelected(0);

        OnNotifyPointerSelect?.Invoke(this, 0);
        ControlAnimationFrames.OnNotifyCollisionCurrent += ControlAnimationFrames_OnNotifyCollisionCurrent;
        LineEditName.TextChanged += LineEditName_TextChanged;
        ButtonSelection.Pressed += ButtonSelection_Pressed;
    }

    private void LineEditName_TextChanged(string newText)
    {
        objectData.name = LineEditName.Text;
        OnNotifyChangued?.Invoke(this);
      
    }

    private void ButtonSelection_Pressed()
    {
        OnNotifyChangued?.Invoke(this);
        OptionButtonPosition_ItemSelected(0);
    }



    private void ControlAnimationFrames_OnNotifyCollisionCurrent(ControlAnimation objectControl, GodotEcsArch.sources.managers.Collision.GeometricShape2D geometricShape2D)
    {
        OnNotifyCollisionSelected?.Invoke(geometricShape2D);
    }
    internal void SetData(AnimationStateData data)
    {
        objectData = data;
        LineEditName.Text = data.name;
        OptionButtonDirection.Selected = (int)data.directionAnimationType;
        nivelarPosiciones((int)data.directionAnimationType);

        ControlAnimationFrames.SetData(objectData.animationData[0]);
        ControlAnimationFrames.OnNotifyCollisionCurrent += ControlAnimationFrames_OnNotifyCollisionCurrent;

    }
    public void SetData(float frameDuration, bool mirrorX, bool mirrorY, bool loop, int positionAnimation, FrameData[] frameArray, int idMaterial )
    {


        objectData.idMaterial = idMaterial;

        objectData.animationData[positionAnimation].frameDataArray =  frameArray;
        objectData.animationData[positionAnimation].frameDuration = frameDuration;
        objectData.animationData[positionAnimation].mirrorHorizontal = mirrorX;
        objectData.animationData[positionAnimation].mirrorVertical = mirrorY;
        objectData.animationData[positionAnimation].loop = loop;

        ControlAnimationFrames.SetData(objectData.animationData[positionAnimation]);
        ControlAnimationFrames.OnNotifyCollisionCurrent += ControlAnimationFrames_OnNotifyCollisionCurrent;
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



    private void OptionButtonPosition_ItemSelected(long index)
    {
        PositionAnimationType position = (PositionAnimationType)index;
        ControlAnimationFrames.QueueFree();
        ControlAnimationFrames = GD.Load<PackedScene>("res://sources/WindowsDataBase/Animation/ControlAnimation.tscn").Instantiate<ControlAnimation>();
        VBoxContainer2.AddChild(ControlAnimationFrames);
        ControlAnimationFrames.SetData(objectData.animationData[(int)position]);
        ControlAnimationFrames.OnNotifyCollisionCurrent += ControlAnimationFrames_OnNotifyCollisionCurrent;
        OnNotifyPointerSelect?.Invoke(this, (int)position);
    }
    void nivelarPosiciones(int index)
    {
        OptionButtonPosition.Clear();
        int c = 0;
        switch (index)
        {
            case 0:
                OptionButtonPosition.AddItem(PositionAnimationType.CENTRO.ToString());
                break;

            case 1:
                foreach (PositionAnimationType type in Enum.GetValues(typeof(PositionAnimationType)))
                {
                    OptionButtonPosition.AddItem(type.ToString());
                    c++;
                    if (c == 2)
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
                    if (c == 4)
                    {
                        break;
                    }
                }
                break;
            case 3:
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
    }
    private void OptionButtonDirection_ItemSelected(long index)
    {
        objectData.directionAnimationType = (DirectionAnimationType)index;
      
        switch (objectData.directionAnimationType)
        {
            case DirectionAnimationType.NINGUNO:
                objectData.animationData = new AnimationData[1];
                break;
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
                    OptionButtonPosition.AddItem(PositionAnimationType.CENTRO.ToString());                                    
                break;

            case 1:
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
                case 2:
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
                case 3:
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
