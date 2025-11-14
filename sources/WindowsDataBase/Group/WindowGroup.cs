using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Group;
using System;

public partial class WindowGroup : Window,  IFacadeWindow<GroupData>
{
    GroupData groupData = new GroupData();

    public event IFacadeWindow<GroupData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonSave.Pressed += ButtonSave_Pressed;
	}

    private void ButtonSave_Pressed()
    {
        groupData.name = LineEditName.Text;
        groupData.idMaterial = ControlTile.idMaterial;
        groupData.x = ControlTile.x;
        groupData.y = ControlTile.y;
        groupData.width = ControlTile.width;
        groupData.height = ControlTile.height;
        DataBaseManager.Instance.InsertUpdate(groupData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void SetData(GroupData data)
    {
        groupData = data;
        LineEditName.Text = groupData.name;
        ControlTile.SetData(groupData.idMaterial, groupData.x, groupData.y, groupData.width, groupData.height);
    }
}
