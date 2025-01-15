using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
namespace GodotEcsArch.sources.imgui
{
    internal class ImGuiWindow
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public ImGuiWindowFlags Flags { get; set; }
        private Dictionary<string,Action> ContentList { get; set; } // Lista de acciones para el contenido

        // Constructor
        public ImGuiWindow(string name, Vector2 position, Vector2 size, ImGuiWindowFlags flags)
        {
            Name = name;
            Position = position;
            Size = size;
            Flags = flags;
            ContentList = new Dictionary<string, Action>();
        }

        // Método para agregar contenido dinámicamente
        public void AddUpdateContent(string id,Action content)
        {
            if (ContentList.ContainsKey(id))
            {
                ContentList[id]= content;
            }
            else
            {
                ContentList.Add(id, content);
            }
            
        }

        // Método para renderizar la ventana
        public void Render()
        {
            // Configurar la posición y tamaño de la ventana
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(Position.X,Position.Y), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(Size.X, Size.Y), ImGuiCond.Once);

            // Crear la ventana
            ImGui.Begin(Name, Flags);

            // Dibujar todo el contenido agregado dinámicamente
            foreach (var content in ContentList.Values)
            {
                content?.Invoke();
            }

            ImGui.End();
        }
    }
}
