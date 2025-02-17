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
        item.SetPosition(items.Count);
        items.Add(item);

        item.OnRequestOrderItem += Item_OnRequestOrderItem;
        item.OnDeleteItem += Item_OnDeleteItem;
        
    }

    private void Item_OnDeleteItem(int position, WindowAutoTileItem windowAutoTileItem)
    {
        
        for (int i = position+1; i < vBoxContainerItems.GetChildCount(); i++)
        {
            var node = vBoxContainerItems.GetChild<WindowAutoTileItem>(i);
            node.SetPosition(i-1);
        }
        items.Remove(windowAutoTileItem);
    }

    private void Item_OnRequestOrderItem(int id,int position, WindowAutoTileItem windowAutoTileItem)
    {

        if (id == 1) //up
        {
            if (position < vBoxContainerItems.GetChildCount())
            {
                var node = vBoxContainerItems.GetChild<WindowAutoTileItem>(position+1);
                node.SetPosition(position);
                vBoxContainerItems.MoveChild(windowAutoTileItem, position + 1);
                windowAutoTileItem.SetPosition(position + 1);
                
            }
            
        }
        if (id==0) // down
        {
            if (position>0)
            {
                var node = vBoxContainerItems.GetChild<WindowAutoTileItem>(position - 1);
                node.SetPosition(position);
                vBoxContainerItems.MoveChild(windowAutoTileItem, position - 1);
                windowAutoTileItem.SetPosition(position - 1);
            }
            
        }
        
        
    }

 

    private void Save_Pressed()
    {
        List<TileRuleData> tileRuleDatas = new List<TileRuleData>();
        foreach (var item in vBoxContainerItems.GetChildren())
        {
            WindowAutoTileItem windowAutoTileItem = (WindowAutoTileItem)item;
            tileRuleDatas.Add(windowAutoTileItem.tileRuleData);
        }
        if (state == WindowState.UPDATE)
        {
            AutoTileData autoTileData = new AutoTileData(tileRuleDatas.ToArray(), true);
            autoTileData.id = int.Parse(lineid.Text);
            autoTileData.name = lineName.Text;            
            DataBaseManager.Instance.InsertUpdate(autoTileData, autoTileData.id);
        }
        else
        {
            AutoTileData autoTileData = new AutoTileData(tileRuleDatas.ToArray(),true);
            autoTileData.id = int.Parse(lineid.Text);
            autoTileData.name = lineName.Text;          
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
            itemWin.SetPosition(items.Count);
            items.Add(itemWin);                          
            itemWin.OnRequestOrderItem += Item_OnRequestOrderItem;
            itemWin.OnDeleteItem += Item_OnDeleteItem;

        }
    }
}
