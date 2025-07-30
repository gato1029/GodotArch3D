using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;

public partial class ControlRules : PanelContainer
{   
       
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI                                        
        ButtonNewRule.Pressed += NewRule_Pressed;
        ButtonSave.Pressed += ButtonSave_Pressed;
        SetNotifyTransform(true);        
    }

    private void ButtonSave_Pressed()
    {
        OnNotifyChangued?.Invoke(this);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            UpdateColumns();
        }
    }

    private void UpdateColumns()
    {
        if (GridContainerItems==null || GridContainerItems.GetChildCount() == 0)
            return;

        var firstChild = GridContainerItems.GetChild(0) as Control;
        if (firstChild == null)
            return;

        float itemWidth = firstChild.Size.X;

        // Si todavía no ha calculado tamaño (puede estar en 0), intenta con rect_min_size
        if (itemWidth <= 0)
            itemWidth = firstChild.GetCombinedMinimumSize().X;

        if (itemWidth <= 0)
            return;

        int columns = Math.Max(1, (int)(Size.X / itemWidth));
        GridContainerItems.Columns = columns;
    }

    private void NewRule_Pressed()
    {
        int insertPosition = GridContainerItems.GetChildCount(); // por defecto al final
     
        Godot.Collections.Array<Node> list = GridContainerItems.GetChildren();
        for (int i = 0; i < list.Count; i++)
        {
            Node itemNode = list[i];
            var data = itemNode as ControlRuleItem;
            if (data.IsSelected)
            {
                insertPosition = i; break;
            }
         
        }
        
        ControlRuleItem item = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/Controls/ControlRuleItem.tscn").Instantiate<ControlRuleItem>();
        
        // Insertar en la UI y en la lista lógica
        GridContainerItems.AddChild(item);
        GridContainerItems.MoveChild(item, insertPosition);
        item.SetPosition(insertPosition);



        // Reasignar posiciones visuales
        list = GridContainerItems.GetChildren();
        for (int i = 0; i < list.Count; i++)
        {
            Node itemNode = list[i];
            var data = itemNode as ControlRuleItem;
            data.SetPosition(i);
        }

        item.OnDeleteItem += Item_OnDeleteItem;
        item.OnSelected += () => Item_OnSelected(item); // nuevo evento

        item.SetSelected(true); // seleccionar el nuevo por defecto
        if (insertPosition == 0)
        {
            UpdateColumns();
        }
    }
    private void Item_OnSelected(ControlRuleItem selectedItem)
    {
        foreach (var item in GridContainerItems.GetChildren())
        {
            var data = item as ControlRuleItem;
            data.SetSelected(data == selectedItem);
        }
        
    }
    private void Item_OnDeleteItem(int position, ControlRuleItem windowAutoTileItem)
    {

        for (int i = position + 1; i < GridContainerItems.GetChildCount(); i++)
        {
            var node = GridContainerItems.GetChild<ControlRuleItem>(i);
            node.SetPosition(i - 1);
        }
      
    }

  
 
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public List<RuleData> GetData()
    {
        var list = new List<RuleData>();        
        foreach (var item in GridContainerItems.GetChildren())
        {
            var data = item as ControlRuleItem;
            list.Add(data.tileRuleData);
        }        
        return list;
    }
    public void SetData(RuleData[] data)
    {        
        for (int i = 0; i < data.Length; i++)
        {
            RuleData item = data[i];
            ControlRuleItem itemWin = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/Controls/ControlRuleItem.tscn").Instantiate<ControlRuleItem>();
            GridContainerItems.AddChild(itemWin);
            itemWin.LoadData(item);
            itemWin.SetPosition(i);
           
           
            itemWin.OnDeleteItem += Item_OnDeleteItem;
            itemWin.OnSelected += () => Item_OnSelected(itemWin); // nuevo evento
        }
    }
}
