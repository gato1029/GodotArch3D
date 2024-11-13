using Arch.Core;
using Godot;
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
        idDocker = ServiceLocator.Instance.GetService<MainNode>().idDocker;
        ImGui.SetWindowSize(new System.Numerics.Vector2(500, 500), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowDockID(idDocker, ImGuiCond.Once);        
        ImGui.Begin("Imgui in Godot, Render");        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(20, 20));
        ImGuiGD.SubViewportWidget(this);
        availableSize = ImGui.GetWindowSize() - new System.Numerics.Vector2(20, 40);
        RenderWindowGui.Instance.IsActive = ImGui.IsWindowHovered(); // ImGui.IsWindowFocused();



        
        System.Numerics.Vector2 mainPos = ImGui.GetWindowPos();
        System.Numerics.Vector2 mainSize = ImGui.GetWindowSize();

        
        System.Numerics.Vector2 fpsWindowPos = new System.Numerics.Vector2(
            mainPos.X + mainSize.X - 150, 
            mainPos.Y + 10                
        );

      
        ImGui.SetNextWindowPos(fpsWindowPos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 80), ImGuiCond.Always);
        ImGui.Begin("FPS Window", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar);

      
        ImGui.Text($"FPS: {ImGui.GetIO().Framerate:F1}");
        ImGui.Text($"FPS Godot: {Engine.GetFramesPerSecond()}");
            
        ImGui.End();

        ImGui.End();

        if (Size.X != availableSize.X || Size.Y != availableSize.Y)
        {
            this.Size = new Vector2I((int)availableSize.X, (int)availableSize.Y);
            flag = false;
        }
        ImGui.PopStyleVar(1);

      
    }
  

  
}
