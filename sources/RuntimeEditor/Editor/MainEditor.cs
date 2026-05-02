using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.Flecs;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.utils;
using System;

public partial class MainEditor : Node
{
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
                        
        ModHelper.Init();
        NodeMainHelper.SetNode3DMain(mainRender);
        PerformanceTimer.Instance.Enabled = true;
        ChunkManager.Initialize();
        AtlasTexturesModsManager.Instance.FirstLoad();
    }
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


   
