using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class WindowTileTextureRule : Window
{
    public delegate void EventNotifySelection(NeighborCondition neighborCondition, bool isCenter=false);
    public event EventNotifySelection OnNotifySelection;

    NeighborCondition neighborCondition;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		KuroTextureButtonExacto.Pressed += KuroTextureButtonExacto_Pressed;
        KuroTextureButtonIgnorar.Pressed += KuroTextureButtonIgnorar_Pressed;
        KuroTextureButtonLleno.Pressed += KuroTextureButtonLleno_Pressed;
        KuroTextureButtonVacio.Pressed += KuroTextureButtonVacio_Pressed;
        KuroTextureButtonIgnorarCentro.Pressed += KuroTextureButtonIgnorarCentro_Pressed;
        FocusExited += WindowSearchTileMaterial_FocusExited;
    }

    private void KuroTextureButtonIgnorarCentro_Pressed()
    {
        neighborCondition = NeighborCondition.Ignore;
        OnNotifySelection?.Invoke(neighborCondition,true);
        QueueFree();
    }

    private void WindowSearchTileMaterial_FocusExited()
    {
        QueueFree();
    }

    private void KuroTextureButtonVacio_Pressed()
    {
        neighborCondition = NeighborCondition.Empty;
        OnNotifySelection?.Invoke(neighborCondition);
        QueueFree();
    }

    private void KuroTextureButtonLleno_Pressed()
    {
        neighborCondition = NeighborCondition.Filled;
        OnNotifySelection?.Invoke(neighborCondition);
        QueueFree();
    }

    private void KuroTextureButtonIgnorar_Pressed()
    {
        neighborCondition = NeighborCondition.Ignore;
        OnNotifySelection?.Invoke(neighborCondition);
        QueueFree();
    }

    private void KuroTextureButtonExacto_Pressed()
    {
        neighborCondition = NeighborCondition.Specific;
        OnNotifySelection?.Invoke(neighborCondition);
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
