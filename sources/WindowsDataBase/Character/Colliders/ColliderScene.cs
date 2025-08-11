using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;


public partial class ColliderScene : VBoxContainer
{
	[Export] Button buttonRemove;
    [Export] Button buttonPreview;

    [Export] SpinBox spinBoxWidth;
    [Export] SpinBox spinBoxHeight;
    [Export] SpinBox spinBoxOffsetX;
    [Export] SpinBox spinBoxOffsetY;
    [Export] OptionButton optionButtonType;
    [Export] SpinBox spinBoxRotation;
    public GeometricShape2D data { get; set; }

    ColliderType type = ColliderType.RECTANGLE;
    public delegate void RequestNotifyPreview(GeometricShape2D itemData);
    public event RequestNotifyPreview OnNotifyPreview;

    // Called when the node enters the scene tree for the first time.
    bool flag = false;
    public void SetData(GeometricShape2D Data)
    {
        flag = false;
        this.data = Data;
    //    spinBoxRotation.Value = data.rotation;
        switch (data)
        {
            case Rectangle rect1:
                type = ColliderType.RECTANGLE;
                optionButtonType.Select(0);
                spinBoxHeight.Visible = true;             
                spinBoxHeight.Value = data.heightPixel;
                spinBoxWidth.Value = data.widthPixel;
                spinBoxOffsetX.Value = data.originPixelX;
                spinBoxOffsetY.Value = data.originPixelY;
                
                break;
            case Circle circle1:
                type = ColliderType.CIRCLE;
                optionButtonType.Select(1);
                spinBoxHeight.Visible = false;               
                spinBoxWidth.Value = data.widthPixel;
                spinBoxOffsetX.Value = data.originPixelX;
                spinBoxOffsetY.Value = data.originPixelY;                
                break;
            default:
                break;
        }
        flag = true;
    }

    public void SetOcluccionButton()
    {
        buttonPreview.Visible = false;
        buttonRemove.Visible = false;
    }
    public override void _Ready()
	{
        type = ColliderType.RECTANGLE;
        optionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;

        optionButtonType.ItemSelected += OptionButtonType_ItemSelected;

        buttonRemove.Pressed += ButtonRemove_Pressed;
        buttonPreview.Pressed += ButtonPreview_Pressed;

        spinBoxWidth.ValueChanged += SpinBox_ValueChanged;
        spinBoxHeight.ValueChanged += SpinBox_ValueChanged;
        spinBoxOffsetX.ValueChanged += SpinBox_ValueChanged;
        spinBoxOffsetY.ValueChanged += SpinBox_ValueChanged;
        spinBoxRotation.ValueChanged += SpinBoxRotation_ValueChanged;
        flag = true;
    }

    private void SpinBoxRotation_ValueChanged(double value)
    {
        createCollider();
        OnNotifyPreview?.Invoke(data);
    }

    private void OptionButtonType_ItemSelected(long index)
    {
        switch (index)
        {
            case 0:
                type = ColliderType.RECTANGLE; 
                spinBoxHeight.Visible = true; 
                break;
            case 1:
                type = ColliderType.CIRCLE; 
                spinBoxHeight.Visible = false;
                break;
            default:
                break;
        }
        createCollider();
        OnNotifyPreview?.Invoke(data);
    }

    private void SpinBox_ValueChanged(double value)
    {
        createCollider();
        OnNotifyPreview?.Invoke(data);
    }

    void createCollider()
    {
        if (flag)
        {
            switch (type)
            {
                case ColliderType.RECTANGLE:
                    data = new Rectangle((float)spinBoxWidth.Value, (float)spinBoxHeight.Value, (float)spinBoxOffsetX.Value, (float)spinBoxOffsetY.Value);
                    break;
                case ColliderType.CIRCLE:
                    data = new Circle((float)spinBoxWidth.Value, (float)spinBoxOffsetX.Value, (float)spinBoxOffsetY.Value);
                    break;
                default:
                    break;
            }
            //data.rotation = (float)spinBoxRotation.Value;
        }
     
    }
    private void ButtonPreview_Pressed()
    {
        OnNotifyPreview?.Invoke(data);
    }

    private void ButtonRemove_Pressed()
    {
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
