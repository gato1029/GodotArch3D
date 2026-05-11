using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.utils;
using System;

public partial class PerfiladorAutoTileTexture : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
      
        NodeMainHelper.SetNode3DMain(mainRender);
        PerformanceTimer.Instance.Enabled = true;
        ChunkManager.Initialize();
        BlackyWorld blackyWorld = new BlackyWorld("Perfilador", BlackyWorldTypeDetail.MUNDO, 32, 5, 1, new Vector2I(64,64));
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
