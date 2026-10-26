using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Portals;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;

public partial class RuntimeControlPortals : PanelContainer
{
	public PortalConfigData data;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        KuroItems.ReloadObjects<PortalConfigData>();
        KuroItems.OnObjectPressed += KuroItems_OnObjectPressed;
        ButtonGuardar.Pressed += ButtonGuardar_Pressed;
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;
        TileSpriteSelector.OnItemSelectedChanged += TileSpriteSelector_OnItemSelected;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;

        foreach (var item in Enum.GetValues(typeof(PortalType)))
        {
            // Obtenemos el valor numérico/enum y su representación en texto
            var valor = (PortalType)item;
            string texto = valor.ToString(); // O puedes usar atributos/descripciones si las tienes
            TipoPortal.AddItemWithData(texto, valor);
        }
      
        ContenedorDatos.Visible = false;
    }    

    private void TileSpriteSelector_OnItemSelected(TileSpriteData obj, object obje)
    {

        data.idTileSprite = obj.id;
    }
    private void ClearAll()
    {
        data = new PortalConfigData();
        LineEditName.Text = string.Empty;
        SpinBoxDificultad.Value = 0;
    }

    private void ButtonNuevo_Pressed()
    {
        ContenedorDatos.Visible = true;
        ClearAll();
    }
    private void ButtonEliminar_Pressed()
    {
        KuroItems.RemoveObject(data);
        DataBaseManager.Instance.RemoveDirectById<PortalConfigData>(data.id);

    }

    private void ButtonGuardar_Pressed()
    {
        data.name = LineEditName.Text;
        data.difficultyLevel = (int)SpinBoxDificultad.Value;
        data.type = (PortalType)TipoPortal.GetSelectedData();
        DataBaseManager.Instance.InsertUpdate(data);
        KuroItems.RefreshObject(data);
        ContenedorDatos.Visible = false;
    }
    private void KuroItems_OnObjectPressed(object obj)
    {
        ClearAll();
        data = (PortalConfigData)obj;
        SpinBoxDificultad.Value = data.difficultyLevel;
        TipoPortal.SelectByData(data.type);
        var sprite = DataBaseManager.Instance.FindById<TileSpriteData>(data.id);
        TileSpriteSelector.LoadData(sprite);
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
