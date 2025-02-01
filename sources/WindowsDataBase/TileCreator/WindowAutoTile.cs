using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;

public partial class WindowAutoTile : Window, IDetailWindow
{
    List<WindowAutoTileItem> items;
    VBoxContainer vBoxContainerItems;
    LineEdit lineName;
    LineEdit lineid;
    WindowState state;
    AutoTileData autoTileData;
    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        CloseRequested += Close_Button;
        vBoxContainerItems = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer/ScrollContainer/VBoxContainer");

        lineName = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEdit3");
        lineid = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEdit");

        GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/VBoxContainer/Button").Pressed += Save_Pressed;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/VBoxContainer/Button2").Pressed += NewRule_Pressed;

        state = WindowState.NEW;
        items = new List<WindowAutoTileItem>();
    }

    private void Close_Button()
    {
        QueueFree();
    }

    private void NewRule_Pressed()
    {
        WindowAutoTileItem item = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowAutoTileItem.tscn").Instantiate<WindowAutoTileItem>();
        vBoxContainerItems.AddChild(item);
        items.Add(item);
    }

    private void Save_Pressed()
    {
        List<TileRuleData> tileRuleDatas = new List<TileRuleData>();
        foreach (var item in items)
        {
            tileRuleDatas.Add(item.tileRuleData);
        }
        if (state == WindowState.UPDATE)
        {            
            autoTileData.id = int.Parse(lineid.Text);
            autoTileData.name = lineName.Text;
            autoTileData.arrayTiles = tileRuleDatas.ToArray();
            DataBaseManager.Instance.InsertUpdate(autoTileData, autoTileData.id);
        }
        else
        {
            AutoTileData autoTileData = new AutoTileData();
            autoTileData.id = int.Parse(lineid.Text);
            autoTileData.name = lineName.Text;
            autoTileData.arrayTiles = tileRuleDatas.ToArray();
            DataBaseManager.Instance.InsertUpdate(autoTileData);
        }

        OnRequestUpdate?.Invoke();
        QueueFree();

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void LoadData(int id)
    {
        state = WindowState.UPDATE;
        autoTileData=  DataBaseManager.Instance.FindById<AutoTileData>(id);
        lineid.Text = autoTileData.id.ToString();
        lineName.Text = autoTileData.name;

        foreach (var item in autoTileData.arrayTiles)
        {
            WindowAutoTileItem itemWin = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowAutoTileItem.tscn").Instantiate<WindowAutoTileItem>();
            vBoxContainerItems.AddChild(itemWin);
            itemWin.LoadData(item);
            items.Add(itemWin);
        }
    }
}
