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
        FileHelper.Initialize("D:\\GitKraken\\ModsGame");
        ModHelper.Init(true);
        AtlasTexturesModsManager.Instance.FirstLoad();
        NodeMainHelper.SetNode3DMain(mainRender);
        PerformanceTimer.Instance.Enabled = true;
        ChunkManager.Initialize();
        KuroButtonTerreno.Pressed += KuroButtonTerreno_Pressed;
    }
    private WindowEditorRuntimeTerrain _terrainWindow;
    private void KuroButtonTerreno_Pressed()
    {
        if (IsInstanceValid(_terrainWindow))
        {
            _terrainWindow.Popup();
            _terrainWindow.GrabFocus();
            return;
        }

        _terrainWindow = RuntimeServices.NodeRegistry.Create<WindowEditorRuntimeTerrain>();
        AddChild(_terrainWindow);

        _terrainWindow.TreeExited += () => _terrainWindow = null;

        _terrainWindow.Popup();
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


   
