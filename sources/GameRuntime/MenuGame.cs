using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using System;

public partial class MenuGame : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        continuar.Pressed += Continuar_Pressed;
        nuevaPartida.Pressed += NuevaPartida_Pressed;   
        salir.Pressed += Salir_Pressed;

        // Inicializar el sistema de mods y texturas, luego mover esto a la pantalla de carga o al inicio del juego
        FileHelper.Initialize("D:\\GitKraken\\ModsGame");
        ModHelper.Init(true);
        AtlasTexturesModsManager.Instance.FirstLoad();
    }

    private void Salir_Pressed()
    {
        // cierra el juego
        GetTree().Quit();
    }

    private  void NuevaPartida_Pressed()
    {
        string runGame = RuntimeServices.NodeRegistry.GetPath<RuntimeGame>();

        var packedScene = GD.Load<PackedScene>(runGame);
        var instance = packedScene.Instantiate<RuntimeGame>();

        var previousScene = GetTree().CurrentScene;

        GetTree().Root.AddChild(instance);
        GetTree().CurrentScene = instance;

        previousScene?.QueueFree();
        
        instance.NuevaPartida();
    }

    private async void Continuar_Pressed()
    {
        string runGame = RuntimeServices.NodeRegistry.GetPath<RuntimeGame>();

        var packedScene = GD.Load<PackedScene>(runGame);
        var instance = packedScene.Instantiate<RuntimeGame>();

        Error error = GetTree().ChangeSceneToNode(instance);

        if (error != Error.Ok)
        {
            GD.PrintErr("No se pudo cargar RuntimeGame.");
            return;
        }

        await ToSignal(instance, Node.SignalName.Ready);

        instance.CargarPartida();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
