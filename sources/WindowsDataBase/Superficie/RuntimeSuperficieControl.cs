using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase;
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
        PreviewSprite.OnItemSelectedChanged += TileSpriteSelector_OnItemSelectedChanged;
    }

    private void TileSpriteSelector_OnItemSelectedChanged(TileSpriteData objectControl)
    {
        data.idDualTemplate = objectControl.id;
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
        //data = (SuperficieData)obj;
        //LineEditName.Text = data.name;
        //if (data.idTileSprite == 0)
        //{
        //    return;
        //}
        //AtlasModsManager.GetSpriteUniqueId(data.idTileSprite, out TileSpriteData tileSpriteData);
        //PreviewSprite.LoadData(tileSpriteData);
    }
}
