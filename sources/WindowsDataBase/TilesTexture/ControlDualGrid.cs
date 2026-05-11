using Godot;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class ControlDualGrid : PanelContainer
{
    DualTileTemplate dualTileTemplate;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonBuscarTextura.Pressed += ButtonBuscarTextura_Pressed;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;
        ButtonGuardar.Pressed+= ButtonGuardar_Pressed;  
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        KuroItems.OnObjectPressed += KuroItems_OnObjectPressed;
        LoadAllItems();
    }

    private void LoadAllItems()
    {
        var items = DataBaseManager.Instance.FindAll<DualTileTemplate>();
        foreach (var item in items)
        {
            KuroItems.AddObject(item, item.name,item.textureVisual);
        }        
    }

    private void KuroItems_OnObjectPressed(object obj)
    {
        dualTileTemplate = (DualTileTemplate)obj;
        
        GenerarSlots(dualTileTemplate);
    }

    private void ButtonGuardar_Pressed()
    {
        dualTileTemplate.name = LineEditName.Text;
        DataBaseManager.Instance.InsertUpdate(dualTileTemplate);
        KuroItems.RemoveObject(dualTileTemplate);
        KuroItems.AddObject(dualTileTemplate, dualTileTemplate.name, dualTileTemplate.textureVisual);
    }

    private void ButtonEliminar_Pressed()
    {
        
    }

    private void ButtonNuevo_Pressed()
    {
        
        GenerarSlots(new DualTileTemplate());
    }

    private void GenerarSlots(DualTileTemplate dualTileTemplate)
    {
        LineEditName.Text = dualTileTemplate.name;
        this.dualTileTemplate = dualTileTemplate; 
        GridContainerItems.ClearChildrens();
        foreach (var item in dualTileTemplate.GetSlots())
        {
            var s = RuntimeServices.NodeRegistry.Create<ControlSlotTileDual>();
            GridContainerItems.AddChild(s);
            s.SetData(item, ControlVisualizadorTexture);
        }        
    }

    private void ButtonBuscarTextura_Pressed()
    {
        var w = RuntimeServices.NodeRegistry.Create<RuntimeEditorWindowMaterial>();
        w.OnSelection += W_OnSelection;
        AddChild(w);
        w.SetTipoTexturas(GodotEcsArch.sources.WindowsDataBase.Materials.MaterialType.TERRENO);
        w.Popup();
    }

    private void W_OnSelection(MaterialData materialData, ModInfo modInfo)
    {
        
        Vector2I cellsize = new Vector2I(materialData.divisionPixelX, materialData.divisionPixelY);
        ControlVisualizadorTexture.SetTexture(
            (Texture2D)materialData.textureMaterial,
            cellsize,
            materialData.id,
            materialData.idNameMod
        );
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
