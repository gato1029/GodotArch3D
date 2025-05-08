using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

using System;

public partial class ControlElements : HBoxContainer
{
    private ElementsData elementData;

    [Export]
    public OptionButton optionButtonCombo { get; set; }
    [Export]
    public SpinBox spinBoxValue { get; set; }
    [Export]
    public Button buttonDelete { get; set; }
    public ElementsData ElementData { get => elementData; set => elementData = value; }

    public ElementsData GetData()
    {
        elementData.type = (ElementType)optionButtonCombo.GetSelectableItem();
        elementData.value = (float) spinBoxValue.Value;
        return elementData;
    }
    public void SetData(ElementsData Data)
    {
        elementData = Data;
        spinBoxValue.Value = elementData.value;
        optionButtonCombo.Selected = (int)elementData.type;
    }
    public override void _Ready()
    {
        buttonDelete.Pressed += ButtonDelete_Pressed;
        spinBoxValue.ValueChanged += SpinBoxValue_ValueChanged;
        optionButtonCombo.ItemSelected += OptionButtonCombo_ItemSelected;
        optionButtonCombo.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        foreach (ElementType type in Enum.GetValues(typeof(ElementType)))
        {
            optionButtonCombo.AddItem(type.ToString());
        }
    }

    private void OptionButtonCombo_ItemSelected(long index)
    {
        optionButtonCombo.GetItemIndex((int)index);
    }

    private void SpinBoxValue_ValueChanged(double value)
    {
        elementData.value = (float)value;
    }

    private void ButtonDelete_Pressed()
    {
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}