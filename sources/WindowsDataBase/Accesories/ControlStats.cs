using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

using System;

public partial class ControlStats : HBoxContainer
{
    private StatsData statsData;

    [Export]
	public OptionButton optionButtonCombo { get; set; }
    [Export]
    public SpinBox spinBoxValue { get; set; }
    [Export]
    public Button buttonDelete { get; set; }
    public StatsData StatsData { get => statsData; set => statsData = value; }

    public StatsData GetData()
    {
        statsData.type = (StatsType)optionButtonCombo.GetSelectedId();
        statsData.value = (int) spinBoxValue.Value;
        return statsData;
    }
    public void SetData(StatsData DamageData)
    {
        statsData = DamageData;
        spinBoxValue.Value = statsData.value;
        optionButtonCombo.Selected = (int)statsData.type;
    }
    public override void _Ready()
	{
        buttonDelete.Pressed += ButtonDelete_Pressed;
        spinBoxValue.ValueChanged += SpinBoxValue_ValueChanged;
        optionButtonCombo.ItemSelected += OptionButtonCombo_ItemSelected;
        optionButtonCombo.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        foreach (StatsType type in Enum.GetValues(typeof(StatsType)))
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
        statsData.value=(int)value;
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
