using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class RuntimeTerrainControl : PanelContainer
{
    TerrainBaseData data;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		KuroItems.ReloadObjects<TerrainBaseData>();
        KuroItems.OnObjectPressed += KuroItems_OnObjectPressed;
        ButtonGuardar.Pressed += ButtonGuardar_Pressed;
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;
        ButtonDual.Pressed += ButtonDual_Pressed;
        ClearAll();
    }

    private void KuroItems_OnObjectPressed(object obj)
    {
        data = (TerrainBaseData)obj;
        LineEditName.Text = data.name;
        ButtonDual.IconTexture = data.textureVisual;
    }

    private void ButtonDual_Pressed()
    {
        var w = RuntimeServices.NodeRegistry.Create<WindowDataGenericMod>();
        AddChild(w);
        w.PopupCentered();
        w.LoadData<DualTileTemplate>();
        w.OnItemSelected += W_OnItemSelected;
    }

    private void W_OnItemSelected(object obj)
    {
        var dualTileTemplate = (DualTileTemplate)obj;
        ButtonDual.IconTexture = dualTileTemplate.textureVisual;
        data.idDualTemplate = dualTileTemplate.id;
    }

    private void ClearAll()
    {
        data = new TerrainBaseData();
        LineEditName.Text = string.Empty;         
        ButtonDual.IconTexture = null;
    }

    private void ButtonNuevo_Pressed()
    {
        ClearAll();
        
    }


    private void ButtonEliminar_Pressed()
    {
        KuroItems.RemoveObject(data);
    }

    private void ButtonGuardar_Pressed()
    {
        data.name = LineEditName.Text;
        DataBaseManager.Instance.InsertUpdate(data);
        KuroItems.RefreshObject(data);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
