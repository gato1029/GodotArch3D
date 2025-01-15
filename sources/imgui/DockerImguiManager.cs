using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
namespace GodotEcsArch.sources.imgui
{
    public enum DockerIMGUI
    {
        LEFT, RIGHT, TOP, BOTTOM, MIDDLE
    }
    internal class DockerImguiManager : SingletonBase<DockerImguiManager>
    {

        private Dictionary<DockerIMGUI, ImGuiWindow> windows = new Dictionary<DockerIMGUI, ImGuiWindow>();

        // Método para agregar una nueva ventana
        public void AddWindow(DockerIMGUI IdWindow, string name, Vector2 position, Vector2 size, ImGuiWindowFlags flags)
        {
            var newWindow = new ImGuiWindow(name, position, size, flags);
            windows.Add(IdWindow, newWindow);
        }

        // Método para renderizar todas las ventanas
        public void RenderWindows()
        {
            foreach (var window in windows.Values)
            {
                window.Render();
            }
        }

        // Método para agregar contenido a una ventana existente
        public void AddContentToWindow(DockerIMGUI IdWindowDock, string idContent, Action content)
        {            
            if (windows.ContainsKey(IdWindowDock))
            {
                windows[IdWindowDock].AddUpdateContent(idContent, content);
            }
        }

        protected override void Initialize()
        {
            AddWindow(DockerIMGUI.LEFT,"Panel Izquierdo", new Vector2(0, 0), new Vector2(300, ImGui.GetIO().DisplaySize.Y), ImGuiWindowFlags.None);
            AddWindow(DockerIMGUI.RIGHT , "Detalles", new Vector2(ImGui.GetIO().DisplaySize.X * 0.8f, 0), new Vector2(300, ImGui.GetIO().DisplaySize.Y), ImGuiWindowFlags.None);
            //AddWindow("Panel Superior", new Vector2(0, 0), new Vector2(400, 100), ImGuiWindowFlags.None);
            AddWindow(DockerIMGUI.BOTTOM,"Panel Inferior", new Vector2(0, ImGui.GetIO().DisplaySize.Y * 0.8f), new Vector2(400, 100), ImGuiWindowFlags.None);
            AddWindow(DockerIMGUI.MIDDLE,"Render", new Vector2(ImGui.GetIO().DisplaySize.X * 0.2f, ImGui.GetIO().DisplaySize.Y * 0.2f), new Vector2(800, 600), ImGuiWindowFlags.None);
        }

        protected override void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
