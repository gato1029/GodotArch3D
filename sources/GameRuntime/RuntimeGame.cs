using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.GameRuntime.Internal;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using System;

public partial class RuntimeGame : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI  
     
    }
    internal void NuevaPartida()
    {
        // Generar un seed aleatorio para la partida
           NodeMainHelper.SetNode3DMain(mainRender, mainRender2D);
        ChunkManager.Initialize();
        int seed = GenerateRandomSeed();
        string name = "Partida_" + seed; // luego Cambiar por un nombre que el usuario pueda ingresar
        GameRuntime gameRuntime = new GameRuntime(name, seed);
    }

    internal void CargarPartida()
    {
        throw new NotImplementedException();
    }

    private int GenerateRandomSeed()
    {
        return (int)(GD.Randi() & 0x7FFFFFFF);
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        RenderCommandQueue.ExecuteFrame();
        ChunkManager.Instance.UpdatePlayerPosition(PositionsManager.Instance.positionCamera);

        var worlds = BlackyWorldRegistry.Instance.GetAllWorlds();

        foreach (var item in worlds)
        {
            item.Update((float)delta);
        }
    }

 
}
