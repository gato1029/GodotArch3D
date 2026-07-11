using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class RuntimeSuperficieControl : PanelContainer
{
    SuperficieData data;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        KuroItems.ReloadObjects<SuperficieData>();
        KuroItems.OnObjectPressed += KuroItems_OnObjectPressed;
        ButtonGuardar.Pressed += ButtonGuardar_Pressed;
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;
        ButtonDual.Pressed += ButtonDual_Pressed;
        
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
        data = new SuperficieData();
        LineEditName.Text = string.Empty;
    }

    private void ButtonNuevo_Pressed()
    {
        ClearAll();

    }


    private void ButtonEliminar_Pressed()
    {
        KuroItems.RemoveObject(data);
        DataBaseManager.Instance.RemoveDirectById<SuperficieData>(data.id);

    }

    private void ButtonGuardar_Pressed()
    {
        data.name = LineEditName.Text;
        DataBaseManager.Instance.InsertUpdate(data);
        KuroItems.RefreshObject(data);
    }
    private void KuroItems_OnObjectPressed(object obj)
    {
        data = (SuperficieData)obj;
        var dual = MasterDataManager.GetData<DualTileTemplate>(data.idDualTemplate);
        LineEditName.Text = data.name;
        if (dual != null)
        {
            ButtonDual.IconTexture = dual.textureVisual;
        }
      
    }
}
