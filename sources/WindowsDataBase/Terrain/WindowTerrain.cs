using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

[KuroRegisterWindow("res://sources/WindowsDataBase/Terrain/windowTerrain.tscn")]
public partial class WindowTerrain : Window, IFacadeWindow<TerrainData>
{
    TerrainData terrainData;
                                     
    public event IFacadeWindow<TerrainData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    public void SetData(TerrainData data)
    {
        terrainData = data;



        if (terrainData.idsElevacionResources.TryGetValue(0, out List<ResourceEntry> res1 ))
        {
            RecursosNivel0.SetData(res1);
        }
        if (terrainData.idsElevacionResources.TryGetValue(1, out List<ResourceEntry> res2))
        {
            RecursosNivel1.SetData(res2);
        }
        if (terrainData.idsElevacionResources.TryGetValue(2, out List<ResourceEntry> res3))
        {
            RecursosNivel2.SetData(res3);
        }
        if (terrainData.idsElevacionResources.TryGetValue(3, out List<ResourceEntry> res4))
        {
            RecursosNivel3.SetData(res4);
        }
        foreach (var item in terrainData.terrains)
        {
            var newRule = (ControlTerrainEntry)GD.Load<PackedScene>("res://sources/WindowsDataBase/Terrain/ControlTerrainEntry.tscn").Instantiate();
            VBoxContainerTerrain.AddChild(newRule);
            newRule.SetData(item.Value);
        }

        CheckBoxisWater.ButtonPressed = terrainData.isWater;
        SpinBoxAltura.Value = terrainData.heightBegin;
        LineEditName.Text = terrainData.name;
        SpinBoxHumetyMin.Value = terrainData.minHumidity;
        SpinBoxHumetyMax.Value = terrainData.maxHumidity;
        SpinBoxTempMax.Value = terrainData.maxTemperature;
        SpinBoxTempMin.Value = terrainData.minTemperature;
        CheckBoxTransicion.ButtonPressed = terrainData.isTransition;
        SpinBoxBorde.Value = terrainData.paddingBorder;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        terrainData = new TerrainData();
        ButtonSave.Pressed += ButtonSave_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonAdd.Pressed += ButtonAdd_Pressed;

    }

    private void ButtonAdd_Pressed()
    {
        var newRule = (ControlTerrainEntry)GD.Load<PackedScene>("res://sources/WindowsDataBase/Terrain/ControlTerrainEntry.tscn").Instantiate();
        VBoxContainerTerrain.AddChild(newRule);
    }

    private void ButtonDelete_Pressed()
    {        
        DataBaseManager.Instance.RemoveDirectById<TerrainData>(terrainData.id);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void ButtonSave_Pressed()
    {
        terrainData.terrains.Clear();
        foreach (var item in VBoxContainerTerrain.GetChildren())
        {
            if (item is ControlTerrainEntry entry)
            {
                var res = entry.GetData();
                res.heightReal = (int)(SpinBoxAltura.Value + res.height);
                terrainData.terrains.Add(res.heightReal, res);
            }
        }
        terrainData.idsElevacionResources.Clear();
        terrainData.idsElevacionResources.Add(0, RecursosNivel0.GetData());
        terrainData.idsElevacionResources.Add(1, RecursosNivel1.GetData());
        terrainData.idsElevacionResources.Add(2, RecursosNivel2.GetData());
        terrainData.idsElevacionResources.Add(3, RecursosNivel3.GetData());



        terrainData.isWater = CheckBoxisWater.ButtonPressed;
        terrainData.name = LineEditName.Text;
        terrainData.heightBegin = (int)SpinBoxAltura.Value;
        terrainData.minHumidity = (float)SpinBoxHumetyMin.Value;
        terrainData.maxHumidity = (float)SpinBoxHumetyMax.Value;
        terrainData.maxTemperature = (float)SpinBoxTempMax.Value;
        terrainData.minTemperature  = (float)SpinBoxTempMin.Value;
        terrainData.isTransition = CheckBoxTransicion.ButtonPressed;
        terrainData.paddingBorder = (int)SpinBoxBorde.Value;

        DataBaseManager.Instance.InsertUpdate<TerrainData>(terrainData);
        MasterDataManager.UpdateRegisterData(terrainData.id, terrainData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }
}
