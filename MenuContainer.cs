using Godot;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using System;

public partial class MenuContainer : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonDualGrid.Pressed += ButtonDualGrid_Pressed;
        ButtonTerreno.Pressed += ButtonTerreno_Pressed;
        ButtonGuardarMod.Pressed += ButtonGuardarMod_Pressed;
        ButtonDecoration.Pressed += ButtonDecoration_Pressed;
        ButtonRamps.Pressed += ButtonRamps_Pressed;
        ButtonCaminos.Pressed += ButtonCaminos_Pressed;
        ButtonSuperficie.Pressed += ButtonSuperficie_Pressed;
    }

    private void ButtonSuperficie_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control = RuntimeServices.NodeRegistry.Create<RuntimeSuperficieControl>();
        ContenedorEditor.AddChild(control);

    }

    private void ButtonCaminos_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control = RuntimeServices.NodeRegistry.Create<RuntimeCaminosControl>();
        ContenedorEditor.AddChild(control);
    }

    private void ButtonRamps_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control = RuntimeServices.NodeRegistry.Create<RuntimeRampsControl>();
        ContenedorEditor.AddChild(control);
    }

    private void ButtonDecoration_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control = RuntimeServices.NodeRegistry.Create<RuntimeDecorationControl>();
        ContenedorEditor.AddChild(control);
    }

    private void ButtonGuardarMod_Pressed()
    {
        DataBaseManager.Instance.CloseDataBase();
        string carpetaMod = "Base";
        FileHelper.CopiarCarpeta("D:\\GitKraken/AssetExternals/NuevosMods/"+carpetaMod, "D:\\GitKraken\\ModsGame\\Mods/"+carpetaMod);
        DataBaseManager.Instance.LoadCurrentDataBase();
    }

    private void ButtonTerreno_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control =  RuntimeServices.NodeRegistry.Create<RuntimeTerrainControl>();        
        ContenedorEditor.AddChild(control);
    }

    private void ButtonDualGrid_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control =  RuntimeServices.NodeRegistry.Create<ControlDualGrid>();        
        ContenedorEditor.AddChild(control);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
