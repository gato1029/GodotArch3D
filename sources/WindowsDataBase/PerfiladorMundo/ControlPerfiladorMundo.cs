using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.Collections.Generic;

public partial class ControlPerfiladorMundo : PanelContainer
{
    TemplateWorldProfileData data;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonAgregarBioma.Pressed += ButtonAgregarBioma_Pressed;
        ButtonGuardar.Pressed += ButtonGuardar_Pressed;
        ButtonNuevo.Pressed += ButtonNuevo_Pressed;
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        foreach (var item in Enum.GetValues(typeof(PerfilGeografico)))
        {
            // Obtenemos el valor numérico/enum y su representación en texto
            var valor = (PerfilGeografico)item;
            string texto = valor.ToString(); // O puedes usar atributos/descripciones si las tienes

            GeografiaCombo.AddItemWithData(texto, valor);
        }
        foreach (var item in Enum.GetValues(typeof(BlackyWorldTypeDetail)))
        {
            // Obtenemos el valor numérico/enum y su representación en texto
            var valor = (BlackyWorldTypeDetail)item;
            string texto = valor.ToString(); // O puedes usar atributos/descripciones si las tienes

            TipoMundoCombo.AddItemWithData(texto, valor);
        }
        ContenedorDatos.Visible = false;
        KuroItems.OnObjectPressed += KuroItems_OnObjectPressed;
        LoadAllItems();
    }

    private void LoadAllItems()
    {
        var items = DataBaseManager.Instance.FindAll<TemplateWorldProfileData>();
        foreach (var item in items)
        {
            KuroItems.AddObject(item, item.name, item.textureVisual);
        }
    }

    private void KuroItems_OnObjectPressed(object obj)
    {
        data = (TemplateWorldProfileData)obj;
        CargarDatos(data);
    }
    public void CargarDatos(TemplateWorldProfileData data)
    {
        this.data = data;
        ContenedorDatos.Visible = true;
        LineEditName.Text = data.name;
        GeografiaCombo.SelectByData(data.perfilGeografico);
        TipoMundoCombo.SelectByData(data.tipoMundo);
        ContedorReglas.ClearChildrens();
        foreach (var item in data.reglasBiomas)
        {
            var control = RuntimeServices.NodeRegistry.Create<ControlBiomaRegla>();
            control.data = item;            
            ContedorReglas.AddChild(control);
            control.SetData(item);
        }
    }

    private void ButtonEliminar_Pressed()
    {
        DataBaseManager.Instance.RemoveDirectById<TemplateWorldProfileData>(data.id);
        ContenedorDatos.Visible = false;
    }

    private void ButtonNuevo_Pressed()
    {
        ContenedorDatos.Visible = true;
        data = new TemplateWorldProfileData();
        LineEditName.Text = "";
        ContedorReglas.ClearChildrens();
    }

    private void ButtonGuardar_Pressed()
    {
        data.name= LineEditName.Text;   
        data.reglasBiomas = ObtenerReglasBioma();
        data.perfilGeografico = (PerfilGeografico)GeografiaCombo.GetSelectedData();
        data.tipoMundo = (BlackyWorldTypeDetail)TipoMundoCombo.GetSelectedData();

        DataBaseManager.Instance.InsertUpdate(data);
        ContenedorDatos.Visible = false;

    }

    private void ButtonAgregarBioma_Pressed()
    {
        var control = RuntimeServices.NodeRegistry.Create<ControlBiomaRegla>();
        ContedorReglas.AddChild(control);

    }

    private List<BiomePlacementRuleData> ObtenerReglasBioma()
    {
        List<BiomePlacementRuleData> lista = new List<BiomePlacementRuleData>();
        foreach (var item in ContedorReglas.GetChildren())
        {
            ControlBiomaRegla cbr = (ControlBiomaRegla)item;
            lista.Add(cbr.data);
        }
        return lista;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
