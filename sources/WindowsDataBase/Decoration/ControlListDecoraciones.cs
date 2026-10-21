using Godot;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using System;

public partial class ControlListDecoraciones : PanelContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        KuroButtonAgregar.Pressed += KuroButtonAgregar_Pressed;
	}

    private void KuroButtonAgregar_Pressed()
    {
        WindowDataEditor w = RuntimeServices.NodeRegistry.Create<WindowDataEditor>();
        w.OnItemSelected += W_OnItemSelectedDecoration;
        AddChild(w);
        w.Popup();
        w.LoadData<DecorationData>();
    }

    private void W_OnItemSelectedDecoration(object obj)
    {
      var node =  RuntimeServices.NodeRegistry.Create<ControlDecoration>();      
      ContenedorDatos.AddChild(node);
      node.SetData((DecorationData)obj);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
