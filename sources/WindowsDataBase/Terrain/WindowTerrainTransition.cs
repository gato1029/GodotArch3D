using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;

[KuroRegisterWindow("res://sources/WindowsDataBase/Terrain/WindowTerrainTransition.tscn")]
public partial class WindowTerrainTransition : Window, IFacadeWindow<TerrainDataTransition>
{
    // Called when the node enters the scene tree for the first time.
    string nameOrigin = "";
    string nameDestini = "";
    string nameResult = "";

    public event IFacadeWindow<TerrainDataTransition>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    TerrainDataTransition objectData;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonSave.Pressed += ButtonSave_Pressed;
        ControlTerrainOrigin.OnNotifyChangued += ControlTerrainOrigin_OnNotifyChangued;
        ControlTerrainDestin.OnNotifyChangued += ControlTerrainDestin_OnNotifyChangued;
        ControlTerrainResult.OnNotifyChangued += ControlTerrainResult_OnNotifyChangued;
        objectData = new TerrainDataTransition();
	}

    private void ControlTerrainResult_OnNotifyChangued(ControlTerrain objectControl)
    {
        nameResult = objectControl.GetData().name;
        TextEditName.Text = nameOrigin + "-" + nameDestini + ":" + nameResult;
    }

    private void ControlTerrainDestin_OnNotifyChangued(ControlTerrain objectControl)
    {
        nameDestini = objectControl.GetData().name;
        TextEditName.Text = nameOrigin + "-" + nameDestini +":"+nameResult;
    }

    private void ControlTerrainOrigin_OnNotifyChangued(ControlTerrain objectControl)
    {
        nameOrigin = objectControl.GetData().name ;
        TextEditName.Text = nameOrigin + "-" + nameDestini + ":" + nameResult;
    }

    private void ButtonSave_Pressed()
    {
        objectData.idTerrainBeginId = ControlTerrainOrigin.GetData().idSave;
        objectData.idTerrainEndId = ControlTerrainDestin.GetData().idSave;
        objectData.idTerrainResoluteId = ControlTerrainResult.GetData().idSave;
        objectData.thickness = (int)SpinBoxAncho.Value;
        objectData.name = TextEditName.Text;
        DataBaseManager.Instance.InsertUpdate(objectData);
        MasterDataManager.UpdateRegisterData(objectData.id, objectData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void ButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveDirectById<TerrainDataTransition>(objectData.id);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void SetData(TerrainDataTransition data)
    {
        objectData = data;
        ControlTerrainOrigin.SetData(MasterDataManager.GetBySaveIds<TerrainData>(data.idTerrainBeginId));
        ControlTerrainDestin.SetData(MasterDataManager.GetBySaveIds<TerrainData>(data.idTerrainEndId));
        ControlTerrainResult.SetData(MasterDataManager.GetBySaveIds<TerrainData>(data.idTerrainResoluteId));  
        SpinBoxAncho.Value = data.thickness;
        TextEditName.Text = data.name;
    }
}
