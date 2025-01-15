using Arch.Core;
using Godot;
using GodotEcsArch.sources.imgui;
using ImGuiGodot;
using ImGuiNET;
using System;

public partial class MainWindowGodot : SubViewport
{
    System.Numerics.Vector2 availableSize;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        RenderWindowGui.Instance.SetNode(this);
       
    }
    bool flag= true;
    uint idDocker;
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        DockerImguiManager.Instance.RenderWindows();
        CenterViewPort();                     
    }
    bool visible = true;
    void CenterViewPort()
    {
        DockerImguiManager.Instance.AddContentToWindow( DockerIMGUI.MIDDLE , "render", () =>
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(20, 20));
            ImGuiGD.SubViewportWidget(this);
            availableSize = ImGui.GetWindowSize() - new System.Numerics.Vector2(20, 40);
            RenderWindowGui.Instance.IsActive = ImGui.IsWindowHovered();
            if (Size.X != availableSize.X || Size.Y != availableSize.Y)
            {
                this.Size = new Vector2I((int)availableSize.X, (int)availableSize.Y);
                flag = false;
            }
            ImGui.PopStyleVar(1);
        });

        DockerImguiManager.Instance.AddContentToWindow( DockerIMGUI.RIGHT, "FPS", () =>
        {
            ImguiWidgets.ToggleableGroup("fps","Frames by Second", () =>
            {
              
                ImGui.Text($"FPS: {ImGui.GetIO().Framerate:F1}");                
                ImGui.Text($"FPS Godot: {Engine.GetFramesPerSecond()}");
               
            });
        });
    }
  

  
}
