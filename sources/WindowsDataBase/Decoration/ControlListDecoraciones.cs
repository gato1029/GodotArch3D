using Godot;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using System;
using System.Collections.Generic;

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

    public void SetData(List<DecorationEntry> idsDecorationData)
    {
        if (idsDecorationData==null)
        {
            return;
        }
        foreach (var item in idsDecorationData)
        {
            
            var node = RuntimeServices.NodeRegistry.Create<ControlDecoration>();
            ContenedorDatos.AddChild(node);
            node.SetData(item);
        }

    }

    private void W_OnItemSelectedDecoration(object obj)
    {
      var node =  RuntimeServices.NodeRegistry.Create<ControlDecoration>();      
      ContenedorDatos.AddChild(node);
      node.SetDataDecoration((DecorationData)obj);
    }

 
    public List<DecorationEntry> GetAllDataEntrys()
    {
        var list = new List<DecorationEntry>();
        foreach (var item in ContenedorDatos.GetChildren())
        {
            var data = (ControlDecoration)item;
            list.Add(data.GetData());
        }
        return list;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    internal void ClearAll()
    {
        ContenedorDatos.ClearChildrens();
    }
}
