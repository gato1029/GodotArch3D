using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;

public partial class WindowAccesoryAnimation : Window,IFacadeWindow<AccesoryAnimationBodyData>
{
    AccesoryAnimationBodyData objectData;

    public event IFacadeWindow<AccesoryAnimationBodyData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    public AccesoryAnimationBodyData ObjectData { get => objectData; set => objectData = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        objectData =new AccesoryAnimationBodyData();        
        ButtonSave.Pressed += ButtonSave_Pressed;
        ControlContainerAnimation.OnNotifyChangued += ControlContainerAnimation_OnNotifyChangued;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
	}

    private void ButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveById<AccesoryAnimationBodyData>(objectData.id);
        QueueFree();
    }

    private void ControlContainerAnimation_OnNotifyChangued(ContainerAnimation objectControl)
    {
        if (objectControl.GetData() != null && objectControl.GetData().Count > 0)
        {
            objectData.animationStateData = objectControl.GetData().ToArray()[0];
        }
    }

    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;                
        DataBaseManager.Instance.InsertUpdate(objectData);  
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void SetData(AccesoryAnimationBodyData data)
    {
        objectData = data;
        LineEditName.Text = data.name;
        if (data.animationStateData!=null)
        {
            List<AnimationStateData> animationStateDatas = [data.animationStateData];
            ControlContainerAnimation.SetData(animationStateDatas.ToArray());
        }
        
    }
}
