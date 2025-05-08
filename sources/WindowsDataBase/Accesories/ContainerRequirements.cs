using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;

public partial class ContainerRequirements : PanelContainer
{
    private RequirementsData objectData;

    public RequirementsData GetRequirementsData()
    {        
        objectData.statsDataArray = ContainerStats.GetAllStats().ToArray();
        return this.objectData;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		InitializeUI();
        objectData = new RequirementsData();
        SpinBoxLevel.ValueChanged += SpinBoxLevel_ValueChanged;        
	}

    private void SpinBoxLevel_ValueChanged(double value)
    {
        objectData.level = (int)SpinBoxLevel.Value;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}

    internal void SetRequirementsData(RequirementsData requirementsData)
    {
       objectData = requirementsData;
       SpinBoxLevel.Value = requirementsData.level;
        ContainerStats.SetAllData(requirementsData.statsDataArray);
    }
}
