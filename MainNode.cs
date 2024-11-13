using Godot;
using ImGuiGodot;
using ImGuiNET;
using System;
using static Godot.DisplayServer;
public partial class MainNode : Node
{
    public uint idDocker;

    public override void _Process(double delta)
    {
        DockSpace();

    }


    public void DockSpace()
    {

        ImGuiGD.SetMainViewport(this.GetViewport());


        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);
        ImGui.SetNextWindowViewport(viewport.ID);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, 0.0f);
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground;

        bool flag = true;

        ImGui.Begin("DockSpace Demo", ref flag, windowFlags);


        idDocker = ImGui.GetID("myDockingSpace");
        ImGui.DockSpace(idDocker, new System.Numerics.Vector2(0, 0), ImGuiDockNodeFlags.None);

        ImGui.End();
        ImGui.PopStyleVar(3);
    }
    public override void _Ready()
    {
        ImGuiConfigFlags configFlags = ImGuiConfigFlags.DockingEnable;

        ImGuiIOPtr io = ImGui.GetIO();
        io.ConfigFlags = configFlags;
        ServiceLocator.Instance.RegisterService(this);
    }
}
