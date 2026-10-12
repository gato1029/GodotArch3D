using Godot;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using System;

public partial class ControlBiomaRegla : PanelContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonBioma.Pressed += ButtonBioma_Pressed;
	}

    private void ButtonBioma_Pressed()
    {
        WindowDataEditor w = RuntimeServices.NodeRegistry.Create<WindowDataEditor>();
        w.OnItemSelected += W_OnItemSelectedBioma;
        AddChild(w);
        w.Popup();
        w.LoadData<BiomaData>();
    }

    private void W_OnItemSelectedBioma(object obj)
    {
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
