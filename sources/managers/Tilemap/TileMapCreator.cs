using Godot;
using GodotEcsArch.sources.imgui;
using ImGuiGodot;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap
{
    internal class TileMapCreator : SingletonBase<TileMapCreator>
    {
        Texture2D texture = GD.Load<Texture2D>("res://resources/basic_ground_tiles.png");
        bool isDragging = false;
        System.Numerics.Vector2 PosMouse = new System.Numerics.Vector2(0, 0);
        System.Numerics.Vector2 PosMouseInWindow = new System.Numerics.Vector2(0, 0);
        System.Numerics.Vector2 tamanio = new System.Numerics.Vector2(32, 32);
        System.Numerics.Vector2 rectMin = new System.Numerics.Vector2(0,0);
        System.Numerics.Vector2 rectMax = new System.Numerics.Vector2(128,128); // Posición final (ancho y alto del rectángulo)
        public void RenderGUI()
        {            
            DockerImguiManager.Instance.AddContentToWindow(DockerIMGUI.LEFT, "Tile Creator", () =>
            {

        
                ImGui.Begin("Creador de Tiles", ImGuiWindowFlags.AlwaysHorizontalScrollbar);



                ImGui.BeginChild("Propiedades", new System.Numerics.Vector2(0, 0), ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.AutoResizeY);
                ImguiWidgets.ToggleableGroup("TileCreator_1", "Cuadrado Seleccion", () =>
                {

                    ImGui.Text($"Position");
                    ImGui.InputFloat2($"Tamano", ref rectMax);

                });
                ImGui.EndChild();

                ImGui.BeginChild("Textura", new System.Numerics.Vector2(0, 0), ImGuiChildFlags.AlwaysUseWindowPadding);
                System.Numerics.Vector2 windowPos = ImGui.GetWindowPos() + new System.Numerics.Vector2(0, 13);
                PosMouse = ImGui.GetMousePos();

                System.Numerics.Vector2 contentMin = ImGui.GetContentRegionAvail();

                nint id = ImGuiGD.BindTexture(texture);
                PosMouseInWindow = PosMouse - windowPos;

                System.Numerics.Vector2 delta = PosMouseInWindow /*- windowPos*/;

                // Redondear el movimiento en pasos de 16 píxeles
                delta.X = (float)Math.Floor(delta.X / 16) * 16;
                delta.Y = (float)Math.Floor(delta.Y / 16) * 16;
              
                PosMouseInWindow = delta + new System.Numerics.Vector2(6,10);
                ImGui.Image(id, new System.Numerics.Vector2(texture.GetWidth(), texture.GetHeight()), new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, 1));

                var drawList = ImGui.GetWindowDrawList();
                uint borderColor = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(6.0f, 26.0f, 46.0f, 1.0f)); // Negro opaco
                drawList.AddRect(windowPos + PosMouseInWindow + rectMin, windowPos + PosMouseInWindow + rectMax, borderColor, 0.0f, ImDrawFlags.Closed, 2.0f); // Grosor de borde: 2.0f
                ImGui.EndChild();

                ImGui.End();
               

            });

            
        }

    }
}
