using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Reflection;
using static System.Formats.Asn1.AsnWriter;

public partial class ControlAnimatedDirection : PanelContainer
{

    public delegate void EventNotifyChanguedItemAnimatedDirection(ControlItemAnimationDirection objectControl);
    public event EventNotifyChanguedItemAnimatedDirection OnNotifyChanguedAnimatedDirection;

    private ControlItemAnimationDirection selected;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
       

    }
    internal void SetData(SpriteAnimationDirection spriteAnimationDirection)
    {
        selected = null;
        foreach (var item in VBoxContainerItems.GetChildren())
        {
            item.QueueFree();
        }
        foreach (var item in spriteAnimationDirection.animations)
        {
            switch (item.Key)
            {
                case GodotEcsArch.sources.components.AnimationDirection.LEFT:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.LEFT,item.Value);
                    break;
                case GodotEcsArch.sources.components.AnimationDirection.RIGHT:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.RIGHT, item.Value);
                    break;
                case GodotEcsArch.sources.components.AnimationDirection.UP:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.UP, item.Value);
                    break;
                case GodotEcsArch.sources.components.AnimationDirection.DOWN:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.DOWN, item.Value);
                    break;
                case GodotEcsArch.sources.components.AnimationDirection.LEFTUP:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.LEFTUP, item.Value);
                    break;
                case GodotEcsArch.sources.components.AnimationDirection.RIGHTUP:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.RIGHTUP, item.Value);
                    break;
                case GodotEcsArch.sources.components.AnimationDirection.LEFTDOWN:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.LEFTDOWN, item.Value);
                    break;
                case GodotEcsArch.sources.components.AnimationDirection.RIGHTDOWN:
                    CreateItem(GodotEcsArch.sources.components.AnimationDirection.RIGHTDOWN, item.Value);
                    break;
                default:
                    break;
            }
        }
    }


    private void CreateItem(GodotEcsArch.sources.components.AnimationDirection direction, SpriteAnimationData spriteAnimationData)
    {
        var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlItemAnimationDirection.tscn");
        var data = scene.Instantiate<ControlItemAnimationDirection>();
        VBoxContainerItems.AddChild(data);        
        data.SetData(direction, spriteAnimationData);
        data.OnNotifyChangued += Data_OnNotifyChangued;
        Data_OnNotifyChangued(data);
    }

    private void Data_OnNotifyChangued(ControlItemAnimationDirection objectControl)
    {
        // Deseleccionar el anterior
        if (selected != null && selected != objectControl)
        {
            selected.SetVisualizarNormal(); 
        }

        // Seleccionar el nuevo
        selected = objectControl;
        selected.SetVisualizarSeleccion();
        OnNotifyChanguedAnimatedDirection?.Invoke(objectControl);
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    internal void ClearAll()
    {
        selected = null;
        foreach (var item in VBoxContainerItems.GetChildren())
        {
            item.QueueFree();
        }
    }
}
