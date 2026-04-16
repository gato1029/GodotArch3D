using Godot;
using System;

public partial class ControlAnimationMultipleDirection : PanelContainer
{
    private ControlItemAnimationMultipleDirection selected;
    private SpriteMultipleAnimationDirection objectData;

    public delegate void EventNotifyChanguedItemAnimatedDirection(ControlItemAnimationDirection objectControl,SpriteAnimationDirection spriteAnimationDirection);
    public event EventNotifyChanguedItemAnimatedDirection OnNotifyChanguedAnimatedDirection;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonAdd.Pressed += ButtonAdd_Pressed;
        foreach (DirectionAnimationType item in Enum.GetValues(typeof(DirectionAnimationType)))
        {
            if (item == DirectionAnimationType.OCHO)
            {
                continue;
            }
            OptionButtonType.AddItem(item.ToString());
        }
        OptionButtonType.ItemSelected += OptionButtonType_ItemSelected;
        objectData = new SpriteMultipleAnimationDirection();
        ControlAnimatedDirectionItem.OnNotifyChanguedAnimatedDirection += ControlAnimatedDirectionItem_OnNotifyChanguedAnimatedDirection;
    }

    private void ControlAnimatedDirectionItem_OnNotifyChanguedAnimatedDirection(ControlItemAnimationDirection objectControl)
    {
        
        OnNotifyChanguedAnimatedDirection?.Invoke(objectControl, selected.GetData());
    }

    private void OptionButtonType_ItemSelected(long index)
    {
        selected = null;
        DirectionAnimationType d = (DirectionAnimationType)OptionButtonType.Selected;
        objectData.directionAnimationType = d;

        ControlAnimatedDirectionItem.ClearAll();
        foreach (var item in VBoxContainerItems.GetChildren())
        {
            //var data = (ControlItemAnimationMultipleDirection)item;
            item.QueueFree();
        }
    }

    public void SetData(SpriteMultipleAnimationDirection data)
    {
        var d = data.directionAnimationType;
        OptionButtonType.Selected = (int)d;
        //OptionButtonType_ItemSelected((long)d);
        objectData = data;

        foreach (var item in data.animations)
        {
            CreateItem(item.Value);
        }
    }

    public SpriteMultipleAnimationDirection GetData()
    {
        objectData.animations.Clear();
        objectData.animationsTypes.Clear();
        foreach (var item in VBoxContainerItems.GetChildren())
        {
            var data = (ControlItemAnimationMultipleDirection)item;
            var cldta= data.GetData();
            objectData.animations.Add(cldta.name, cldta);
            objectData.animationsTypes.Add(cldta.animationType, cldta);
        }
        DirectionAnimationType d = (DirectionAnimationType)OptionButtonType.Selected;
        objectData.directionAnimationType = d;
        return objectData;
    }
    
    public void SetNormalizeData(float offsetX, float offsetY, float Ydepht, float scale)
    {
        foreach (var item in VBoxContainerItems.GetChildren())
        {
            var data = (ControlItemAnimationMultipleDirection)item;
            var cldta = data.GetData();
            foreach (var itemAnim in cldta.animations)
            {
                var spt = itemAnim.Value;
                spt.offsetX = offsetX; 
                spt.offsetY = offsetY;
                spt.yDepthRender = Ydepht;
                spt.scale = scale;
            }            
        }
    }
    private void ButtonAdd_Pressed()
    {
        DirectionAnimationType d = (DirectionAnimationType)OptionButtonType.Selected;
        var n = new SpriteAnimationDirection(d,LineEditName.Text);        
        CreateItem(n);
    }

    private void CreateItem(SpriteAnimationDirection SpriteAnimationDirection)
    {
        var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlItemAnimationMultipleDirection.tscn");
        var data = scene.Instantiate<ControlItemAnimationMultipleDirection>();
        VBoxContainerItems.AddChild(data);
      
        data.SetData(SpriteAnimationDirection);
        data.OnNotifyChangued += Data_OnNotifyChangued;
        data.OnNotifyChanguedDelete += Data_OnNotifyChanguedDelete;

        Data_OnNotifyChangued(data);
    }

    private void Data_OnNotifyChanguedDelete(ControlItemAnimationMultipleDirection objectControl)
    {
        if (selected == objectControl)
        {
            selected = null; 
            ControlAnimatedDirectionItem.ClearAll();
        }
    }

    private void Data_OnNotifyChangued(ControlItemAnimationMultipleDirection objectControl)
    {   
        // Deseleccionar el anterior
        if (selected != null && selected != objectControl)
        {
            selected.SetVisualizarNormal();
        }

        // Seleccionar el nuevo
        selected = objectControl;
        selected.SetVisualizarSeleccion();
        ControlAnimatedDirectionItem.SetData(objectControl.GetData());
    }
}
