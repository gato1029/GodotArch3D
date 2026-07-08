using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using System;

public partial class RuntimeDecorationControl : PanelContainer
{
	DecorationData data;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        KuroItems.ReloadObjects<DecorationData>();
        KuroItems.OnObjectPressed += KuroItems_OnObjectPressed;
        ButtonGuardar.Pressed += ButtonGuardar_Pressed;
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;
        TileSpriteSelector.OnItemSelectedChanged += TileSpriteSelector_OnItemSelected;               
	}

    private void TileSpriteSelector_OnItemSelected(TileSpriteData obj)
    {
        data.idTileSprite = obj.id;
    }

    private void ClearAll()
    {
        data = new DecorationData();
        LineEditName.Text = string.Empty;                
    }

    private void ButtonNuevo_Pressed()
    {
        ClearAll();

    }


    private void ButtonEliminar_Pressed()
    {
        KuroItems.RemoveObject(data);
        DataBaseManager.Instance.RemoveDirectById<DecorationData>(data.id);

    }

    private void ButtonGuardar_Pressed()
    {
        data.name = LineEditName.Text;        
        DataBaseManager.Instance.InsertUpdate(data);
        KuroItems.RefreshObject(data);
    }
    private void KuroItems_OnObjectPressed(object obj)
    {
        data = (DecorationData)obj;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
