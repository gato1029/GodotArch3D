using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using System;

public partial class ControlAccesoryAnimationBodyView : PanelContainer
{
    AccesoryAnimationBodyData objectData;

    public AccesoryAnimationBodyData ObjectData { get => objectData; set => objectData = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonSearch.Pressed += ButtonSearch_Pressed;
	}

    private void ButtonSearch_Pressed()
    {
        FacadeWindowDataSearch<AccesoryAnimationBodyData> window = new FacadeWindowDataSearch<AccesoryAnimationBodyData>("res://sources/WindowsDataBase/Accesories/WindowAccesoryAnimation.tscn", this, WindowType.SELECTED);
        window.OnNotifySelected += Window_OnNotifySelected;
    }

    private void Window_OnNotifySelected(AccesoryAnimationBodyData objectSelected)
    {
        objectData = objectSelected;
        ControlAnimationState.SetData(objectData.animationStateData);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    internal void LoadById(int idBodyAnimationBaseData)
    {
        objectData = DataBaseManager.Instance.FindById<AccesoryAnimationBodyData>(idBodyAnimationBaseData);
        ControlAnimationState.SetData(objectData.animationStateData);
    }
}
