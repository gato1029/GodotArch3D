using Godot;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;

public partial class ControlBiomaRegla : PanelContainer
{
    // Called when the node enters the scene tree for the first time.
    public BiomePlacementRuleData data;

    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonBioma.Pressed += ButtonBioma_Pressed;
        SpinBoxAlturaMaxima.ValueChanged += SpinBoxAlturaMaxima_ValueChanged;
        SpinBoxAlturaMinima.ValueChanged += SpinBoxAlturaMinima_ValueChanged;
        SpinBoxPeso.ValueChanged += SpinBoxPeso_ValueChanged;
        ButtonEliminar.Pressed += ButtonEliminar_Pressed;
        CheckBoxBiomaBase.Pressed += CheckBoxBiomaBase_Pressed;
        data = new BiomePlacementRuleData();
	}

    private void CheckBoxBiomaBase_Pressed()
    {
        data.isLowerBase = CheckBoxBiomaBase.ButtonPressed;
    }

    private void ButtonEliminar_Pressed()
    {
        QueueFree();
    }

    public void SetData(BiomePlacementRuleData data)
    {
        this.data = data; 
        SpinBoxAlturaMaxima.Value = data.alturaMaxima;
        SpinBoxAlturaMinima.Value = data.alturaMinima;
        SpinBoxPeso.Value = data.peso;
        ButtonBioma.Text = data.bioma.name;
        CheckBoxBiomaBase.ButtonPressed = data.isLowerBase;
    }

    private void SpinBoxPeso_ValueChanged(double value)
    {
        data.peso = (int)value;
    }

    private void SpinBoxAlturaMinima_ValueChanged(double value)
    {
        data.alturaMinima = (int)value;
    }

    private void SpinBoxAlturaMaxima_ValueChanged(double value)
    {
        data.alturaMaxima = (int)value;
    }

    private void ButtonBioma_Pressed()
    {
        WindowDataEditor w = RuntimeServices.NodeRegistry.Create<WindowDataEditor>();
        w.OnItemSelected += W_OnItemSelectedBioma;
        AddChild(w);
        w.Popup();
        w.LoadData<BiomaData>();
    }

    private void W_OnItemSelectedBioma(object obj)
    {
        BiomaData biomaData = (BiomaData)obj;
        data.idBioma = biomaData.id;
        ButtonBioma.Text = biomaData.name;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
