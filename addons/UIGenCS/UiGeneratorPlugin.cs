#if TOOLS
using Godot;
using System;

[Tool]
public partial class UiGeneratorPlugin: EditorPlugin
{
    private Control configPanel;   
    public override void _EnterTree()
    {   
        var scene = GD.Load<PackedScene>("res://addons/UIGenCS/UI/UIInterfaceGenerator.tscn");
        configPanel = scene.Instantiate<Control>();
        AddControlToContainer(CustomControlContainer.CanvasEditorMenu, configPanel);          
    }
    public override void _ExitTree()
    {             
        RemoveControlFromContainer(CustomControlContainer.CanvasEditorMenu, configPanel);
        configPanel.QueueFree();     
    }
}
#endif
