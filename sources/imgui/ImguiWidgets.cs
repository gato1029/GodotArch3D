using CommunityToolkit.HighPerformance.Helpers;
using Godot;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.imgui
{
    internal class ImguiWidgets
    {

        private static Dictionary<object, bool> visibilityStates = new();

        /// <summary>
        /// Renderiza un grupo de contenido con visibilidad alternable.
        /// </summary>
        /// <param name="id">Identificador único para el widget</param>
        /// <param name="buttonText">Texto del botón</param>
        /// <param name="renderContent">Acción para renderizar el contenido del grupo</param>
        public static void ToggleableGroup(object id, string buttonText, Action renderContent)
        {
            // Inicializar el estado de visibilidad si no existe
            if (!visibilityStates.ContainsKey(id))
            {
                visibilityStates[id] = true; // Por defecto, visible
            }

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(10f, 0f));
            ImGui.BeginChild(buttonText, new System.Numerics.Vector2(0, 0), ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.AlwaysUseWindowPadding, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDecoration);

            // Colores para el botón
            uint transparentColor = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.3f, 0.3f, 0.1f, 0.1f));  // Transparente

            // Comienzo del menú
            ImGui.BeginMenuBar();

            // Estilo del botón
            ImGui.PushStyleColor(ImGuiCol.Button, transparentColor);         // Fondo del botón transparente
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, transparentColor); // Fondo al pasar el cursor
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, transparentColor);  // Fondo al hacer clic

            // Botón para alternar visibilidad
            if (ImGui.Button(buttonText))
            {
                visibilityStates[id] = !visibilityStates[id];
            }

            // Restaurar colores originales
            ImGui.PopStyleColor(3);

            ImGui.EndMenuBar();

            // Renderizar contenido si es visible
            if (visibilityStates[id])
            {
           
                    //ImGui.BeginTable((string)id, 1, ImGuiTableFlags.None);
                    //ImGui.TableNextColumn();
                    renderContent?.Invoke();
                    //ImGui.EndTable();                       
            }

            ImGui.EndChild();
            ImGui.PopStyleVar(2);
        }
        public static void WindowHidenGroupEnd()
        {

        }
    }
}
