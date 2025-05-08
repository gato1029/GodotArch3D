using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;


public partial class ColliderScene : VBoxContainer
{
	[Export] Button buttonRemove;
    [Export] Button buttonPreview;

    [Export] SpinBox spinBoxWidth;
    [Export] SpinBox spinBoxHeight;
    [Export] SpinBox spinBoxOffsetX;
    [Export] SpinBox spinBoxOffsetY;
    [Export] OptionButton optionButtonType;
    public GeometricShape2D data { get; set; }

    ColliderType type = ColliderType.RECTANGLE;
    public delegate void RequestNotifyPreview(GeometricShape2D itemData);
    public event RequestNotifyPreview OnNotifyPreview;

    // Called when the node enters the scene tree for the first time.

    public void SetData(GeometricShape2D Data)
    {
        this.data = Data;
        switch (data)
        {
            case Rectangle rect1:
                type = ColliderType.RECTANGLE;
                spinBoxHeight.Visible = true;             
                spinBoxHeight.Value = rect1.heightPixel;
                spinBoxWidth.Value = rect1.widthPixel;
                spinBoxOffsetX.Value = rect1.originPixelX;
                spinBoxOffsetY.Value = rect1.originPixelY;
                optionButtonType.Select(0);
                break;
            case Circle circle1:
                type = ColliderType.CIRCLE;                
                spinBoxHeight.Visible = false;               
                spinBoxWidth.Value = circle1.widthPixel;
                spinBoxOffsetX.Value = circle1.originPixelX;
                spinBoxOffsetY.Value = circle1.originPixelY;
                optionButtonType.Select(1);
                break;
            default:
                break;
        }
        createCollider();
        OnNotifyPreview?.Invoke(data);
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
        createCollider();
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
