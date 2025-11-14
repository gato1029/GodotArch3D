using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.KuroTiles;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.KuroTiles;
using RectangleBinPacking;
using System;

public partial class ControlMiniatureRule : PanelContainer
{

    TileRuleTemplate tileRuleTemplate;
    public override void _Ready()
    {
        AddToGroup("rule_items");
        InitializeUI();
        ButtonAddAfter.Pressed += ButtonAddAfter_Pressed;
        ButtonAddBehind.Pressed += ButtonAddBehind_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonEdit.Pressed += ButtonEdit_Pressed;
        ButtonDuplicate.Pressed += ButtonDuplicate_Pressed;
    }

    private void ButtonDuplicate_Pressed()
    {
        if (GetParent() is Container container)
        {
            var newRule = (ControlMiniatureRule)GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlMiniatureRule.tscn").Instantiate();
            int index = container.GetChildren().IndexOf(this);            
            container.AddChild(newRule);
            newRule.SetData(this.tileRuleTemplate);
            container.MoveChild(newRule, index + 1); // Insertar después (after)
        }
    }

    private void ButtonEdit_Pressed()
    {
        var window = KuroWindowFactory.Create<WindowKuroRuleItem>();
        window.OnNotifyChangued += D_OnNotifyChangued;        
        this.AddChild(window);
        window.SetData(tileRuleTemplate);

        Vector2I globalPos = (Vector2I)GetScreenPosition();     
        Vector2I size = (Vector2I)GetGlobalRect().Size;        
        window.Position = new Vector2I(globalPos.X + size.X + 30, globalPos.Y);

    }

    private void D_OnNotifyChangued(WindowKuroRuleItem objectControl)
    {
        tileRuleTemplate = objectControl.GetData();
        ControlKuroTileWid.SetData(tileRuleTemplate);
        ControlKuroTileWid.SetCentralTile(tileRuleTemplate.TileCentral);
        if (tileRuleTemplate.TileCentral.idTileSprite!=0)
        {
            var data = TileSpriteManager.Instance.GetData(tileRuleTemplate.TileCentral.idTileSprite);
            TextureRectCenter.Texture = data.textureVisual;
        }
        
    }
    public TileRuleTemplate GetData()
    {
        return tileRuleTemplate;
    }
    public void SetData(TileRuleTemplate tileRuleTemplateData)
    {
        if (tileRuleTemplateData==null)
        {
            return;
        }
        tileRuleTemplate = tileRuleTemplateData;
        ControlKuroTileWid.SetData(tileRuleTemplateData);
        ControlKuroTileWid.SetCentralTile(tileRuleTemplate.TileCentral);
        
        if (tileRuleTemplate.TileCentral.idTileSprite != 0)
        {
            var data = TileSpriteManager.Instance.GetData(tileRuleTemplate.TileCentral.idTileSprite);
            TextureRectCenter.Texture = data.textureVisual;
        }
    }

    private void ButtonDelete_Pressed()
    {
        Message.ShowConfirmation(this, "Estas seguro de eliminar?").Confirmed += () => { QueueFree(); };

    }

    private void ButtonAddBehind_Pressed()
    {
        if (GetParent() is Container container)
        {
            var newRule = (ControlMiniatureRule)GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlMiniatureRule.tscn").Instantiate();
            int index = container.GetChildren().IndexOf(this);
            container.AddChild(newRule);
            container.MoveChild(newRule, index); // Insertar antes (behind)
        }
    }

    private void ButtonAddAfter_Pressed()
    {
        if (GetParent() is Container container)
        {
            var newRule = (ControlMiniatureRule)GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlMiniatureRule.tscn").Instantiate();
            int index = container.GetChildren().IndexOf(this);
            container.AddChild(newRule);
            container.MoveChild(newRule, index + 1); // Insertar después (after)
        }
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {

        return data.VariantType == Variant.Type.Int; // Godot 4 puede usar Int64
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        ulong draggedId = (ulong)data;

        foreach (Node node in GetTree().GetNodesInGroup("rule_items"))
        {
            if (node.GetInstanceId() == draggedId && node is ControlMiniatureRule draggedItem && GetParent() is Container container)
            {
                int dropIndex = container.GetChildren().IndexOf(this);
                int fromIndex = container.GetChildren().IndexOf(draggedItem);

                if (fromIndex == dropIndex)
                    return;

                container.MoveChild(draggedItem, dropIndex);
  
                break;
            }
        }
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var id = GetInstanceId();
        SetDragPreview((Control)Duplicate());
        return id;
    }



 
}
