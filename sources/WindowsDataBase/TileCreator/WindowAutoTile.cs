using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;

public partial class WindowAutoTile : Window, IFacadeWindow<AutoTileData>
{
    List<WindowAutoTileItem> items;
            
    WindowState state;
    AutoTileData autoTileData;
    
    public event IFacadeWindow<AutoTileData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI                                
        ButtonSave.Pressed += Save_Pressed;
        ButtonSaveActive.Pressed += SaveActive_Pressed;
        ButtonNewRule.Pressed+= NewRule_Pressed;
        state = WindowState.NEW;
        items = new List<WindowAutoTileItem>();
        autoTileData = new AutoTileData();
    }

    private void SaveActive_Pressed()
    {
        SaveAll();
    }



    private void NewRule_Pressed()
    {
        int insertPosition = items.Count; // por defecto al final

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].IsSelected)
            {
                insertPosition = i;
                break;
            }
        }

        WindowAutoTileItem item = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowAutoTileItem.tscn").Instantiate<WindowAutoTileItem>();

        // Insertar en la UI y en la lista lógica
        GridContainerItems.AddChild(item);
        GridContainerItems.MoveChild(item, insertPosition);
        items.Insert(insertPosition, item);

        // Reasignar posiciones visuales
        for (int i = 0; i < items.Count; i++)
            items[i].SetPosition(i);

        item.OnRequestOrderItem += Item_OnRequestOrderItem;
        item.OnDeleteItem += Item_OnDeleteItem;
        item.OnSelected += () => Item_OnSelected(item); // nuevo evento

        item.SetSelected(true); // seleccionar el nuevo por defecto
    }
    private void Item_OnSelected(WindowAutoTileItem selectedItem)
    {
        foreach (var item in items)
            item.SetSelected(item == selectedItem);
    }
    private void Item_OnDeleteItem(int position, WindowAutoTileItem windowAutoTileItem)
    {
        
        for (int i = position+1; i < GridContainerItems.GetChildCount(); i++)
        {
            var node = GridContainerItems.GetChild<WindowAutoTileItem>(i);
            node.SetPosition(i-1);
        }
        items.Remove(windowAutoTileItem);
    }

    private void Item_OnRequestOrderItem(int id,int position, WindowAutoTileItem windowAutoTileItem)
    {

        if (id == 1) //up
        {
            if (position < GridContainerItems.GetChildCount())
            {
                var node = GridContainerItems.GetChild<WindowAutoTileItem>(position+1);
                node.SetPosition(position);
                GridContainerItems.MoveChild(windowAutoTileItem, position + 1);
                windowAutoTileItem.SetPosition(position + 1);
                
            }
            
        }
        if (id==0) // down
        {
            if (position>0)
            {
                var node = GridContainerItems.GetChild<WindowAutoTileItem>(position - 1);
                node.SetPosition(position);
                GridContainerItems.MoveChild(windowAutoTileItem, position - 1);
                windowAutoTileItem.SetPosition(position - 1);
            }
            
        }
        
        
    }


    private void SaveAll()
    {
        List<TileRuleData> tileRuleDatas = new List<TileRuleData>();
        foreach (var item in GridContainerItems.GetChildren())
        {
            WindowAutoTileItem windowAutoTileItem = (WindowAutoTileItem)item;
            tileRuleDatas.Add(windowAutoTileItem.tileRuleData);
        }
        if (state == WindowState.UPDATE)
        {
            autoTileData = new AutoTileData(tileRuleDatas.ToArray(), true);
            autoTileData.id = int.Parse(LineEditId.Text);
            autoTileData.name = LineEditName.Text;
            DataBaseManager.Instance.InsertUpdate(autoTileData, autoTileData.id);
        }
        else
        {
            autoTileData = new AutoTileData(tileRuleDatas.ToArray(), true);
            autoTileData.id = int.Parse(LineEditId.Text);
            autoTileData.name = LineEditName.Text;
            DataBaseManager.Instance.InsertUpdate(autoTileData);
        }
        if (autoTileData.id!=0)
        {
            AutoTileManager.Instance.RegisterTileData(autoTileData.id);
        }
        
      
    }
    private void Save_Pressed()
    {
        SaveAll();
        OnNotifyChanguedSimple?.Invoke();        
        QueueFree();

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

   

    public void SetData(AutoTileData data)
    {
        state = WindowState.UPDATE;
        autoTileData = data;
        LineEditId.Text = autoTileData.id.ToString();
        LineEditName.Text = autoTileData.name;

        foreach (var item in autoTileData.arrayTiles)
        {
            WindowAutoTileItem itemWin = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowAutoTileItem.tscn").Instantiate<WindowAutoTileItem>();
            GridContainerItems.AddChild(itemWin);
            itemWin.LoadData(item);
            itemWin.SetPosition(items.Count);
            items.Add(itemWin);
            itemWin.OnRequestOrderItem += Item_OnRequestOrderItem;
            itemWin.OnDeleteItem += Item_OnDeleteItem;
            itemWin.OnSelected += () => Item_OnSelected(itemWin); // nuevo evento
        }
    }
}
