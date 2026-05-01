using Godot;
using System;

public partial class EditorTerreno : CenterContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        KuroButton.Pressed += KuroButton_Pressed;
	}

    private void KuroButton_Pressed()
    {
       var w =  RuntimeServices.NodeRegistry.Create<RuntimeEditorWindowMaterial>();
       w.OnSelection += W_OnSelection;
       AddChild(w);
       w.Popup();
    }

    private void W_OnSelection(GodotEcsArch.sources.WindowsDataBase.Materials.MaterialData materialData)
    {
        var w = RuntimeServices.NodeRegistry.Create<WindowEditorRuntimeTerrain>();        
        AddChild(w);
        w.Popup();
        w.SetMaterial(materialData);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
