using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.Helpers;
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
    }

    private void ButtonGuardar_Pressed()
    {
        data.reglasBiomas = ObtenerReglasBioma();
        data.perfilGeografico = (PerfilGeografico)GeografiaCombo.GetSelectedData();
        data.tipoMundo = (BlackyWorldTypeDetail)TipoMundoCombo.GetSelectedData();
        
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
