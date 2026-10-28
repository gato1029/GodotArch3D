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
        ContenedorDatos.Visible = false;
	}

    private void TileSpriteSelector_OnItemSelected(TileSpriteData obj, object obje)
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
        ContenedorDatos.Visible = true;
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
        ContenedorDatos.Visible = false;
    }
    private void KuroItems_OnObjectPressed(object obj)
    {
        data = (DecorationData)obj;
        ContenedorDatos.Visible = true;
        LineEditName.Text = data.name;
        var sprite = DataBaseManager.Instance.FindById<TileSpriteData>(data.idTileSprite);
        TileSpriteSelector.LoadData(sprite);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
