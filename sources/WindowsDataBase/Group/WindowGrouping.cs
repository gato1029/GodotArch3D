using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Group;
using System;

public partial class WindowGrouping : Window, IFacadeWindow<GroupingData>
{
    public GroupingData objectData = new GroupingData();
    public event IFacadeWindow<GroupingData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonSave.Pressed += ButtonSave_Pressed;
	}

    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.idMaterial = TileTexture.idMaterial;
        objectData.x = TileTexture.x;
        objectData.y = TileTexture.y;
        objectData.width = TileTexture.width;
        objectData.height = TileTexture.height;
        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void SetData(GroupingData data)
    {
       this.objectData = data;
        LineEditName.Text = data.name;
        TileTexture.SetData(objectData.idMaterial, objectData.x, objectData.y, objectData.width, objectData.height);
    }
}
