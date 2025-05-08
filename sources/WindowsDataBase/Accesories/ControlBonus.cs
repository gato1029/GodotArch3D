using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

using System;

public partial class ControlBonus : HBoxContainer
{
    private BonusData bonusData;

    [Export]
    public OptionButton optionButtonCombo { get; set; }
    [Export]
    public SpinBox spinBoxValue { get; set; }
    [Export]
    public Button buttonDelete { get; set; }
    public BonusData BonusData { get => bonusData; set => bonusData = value; }

    public BonusData GetData()
    {
        bonusData.type = (BonusType) optionButtonCombo.GetSelectedId();
        bonusData.value = (float)spinBoxValue.Value;
        return bonusData;
    }
    public void SetData(BonusData Data)
    {
        bonusData = Data;
        spinBoxValue.Value = bonusData.value;
        optionButtonCombo.Selected = (int)bonusData.type;
    }
    public override void _Ready()
    {
        bonusData = new BonusData();
        buttonDelete.Pressed += ButtonDelete_Pressed;
        spinBoxValue.ValueChanged += SpinBoxValue_ValueChanged;
        optionButtonCombo.ItemSelected += OptionButtonCombo_ItemSelected;
        optionButtonCombo.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        foreach (BonusType type in Enum.GetValues(typeof(BonusType)))
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
        bonusData.value = (float)value;
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