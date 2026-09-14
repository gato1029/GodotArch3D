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
        KuroButtonRecursos.Pressed += KuroButtonRecursos_Pressed;
        KuroButtonEdificios.Pressed += KuroButtonEdificios_Pressed;
    }
    private WindowEditorRuntimeBuildings _windowEditorRuntimeBuildings;
    private void KuroButtonEdificios_Pressed()
    {
        if (IsInstanceValid(_windowEditorRuntimeBuildings))
        {
            _windowEditorRuntimeBuildings.Popup();
            _windowEditorRuntimeBuildings.GrabFocus();
            return;
        }

        _windowEditorRuntimeBuildings = RuntimeServices.NodeRegistry.Create<WindowEditorRuntimeBuildings>();
        AddChild(_windowEditorRuntimeBuildings);

        _windowEditorRuntimeBuildings.TreeExited += () => _windowEditorRuntimeBuildings = null;

        _windowEditorRuntimeBuildings.Popup();
    }

    private WindowEditorRuntimeResources _windowEditorRuntimeResources;
    private void KuroButtonRecursos_Pressed()
    {
        if (IsInstanceValid(_windowEditorRuntimeResources))
        {
            _windowEditorRuntimeResources.Popup();
            _windowEditorRuntimeResources.GrabFocus();
            return;
        }

        _windowEditorRuntimeResources = RuntimeServices.NodeRegistry.Create<WindowEditorRuntimeResources>();
        AddChild(_windowEditorRuntimeResources);

        _windowEditorRuntimeResources.TreeExited += () => _windowEditorRuntimeResources = null;

        _windowEditorRuntimeResources.Popup();
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
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            var worlds = BlackyWorldRegistry.Instance.GetAllWorlds();

            foreach (var item in worlds)
            {
                item.Dispose();
            }
            GetTree().Quit();
        }
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


   
