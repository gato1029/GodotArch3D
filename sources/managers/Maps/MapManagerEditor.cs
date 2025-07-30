using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;

public enum EditorMode
{
    APAGADO,
    TERRENO,
    RECURSOS,
    UNIDADES
}
public class MapManagerEditor : SingletonBase<MapManagerEditor>
{    
    public MapLevelData currentMapLevelData { get; set; }
    public EditorMode editorMode { get; set; }

    public bool enableEditor { get; set; }
    public int idData { get; set; }
    public void Input(Godot.InputEvent @event)
    {
        switch (editorMode)
        {
            case EditorMode.TERRENO:
                EditResources(@event);
                break;
            case EditorMode.RECURSOS:
                break;
            case EditorMode.UNIDADES:
                break;
            default:
                break;
        }
        
       
    }

    private void InputMouse(Godot.InputEvent @event)
    {
        
    }

    private void EditResources(InputEvent @event)
    {
        InputMouse(@event);
        InputKeyboard(@event);
    }
    public void DrawTilesBluePrint(int idData)
    {
        
    }
    private void InputKeyboard(Godot.InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && !keyEvent.Echo)
        {
            switch (keyEvent.Keycode)
            {
                case Key.W:
                    if (keyEvent.Pressed)
                        GD.Print("Tecla W PRESIONADA");
                    else
                        GD.Print("Tecla W LIBERADA");
                    break;

                case Key.A:
                    if (keyEvent.Pressed)
                        GD.Print("Tecla A PRESIONADA");
                    else
                        GD.Print("Tecla A LIBERADA");
                    break;

                case Key.S:
                    if (keyEvent.Pressed)
                        GD.Print("Tecla S PRESIONADA");
                    else
                        GD.Print("Tecla S LIBERADA");
                    break;

                case Key.D:
                    if (keyEvent.Pressed)
                        GD.Print("Tecla D PRESIONADA");
                    else
                        GD.Print("Tecla D LIBERADA");
                    break;

                case Key.Space:
                    if (keyEvent.Pressed)
                        GD.Print("Espacio PRESIONADO");
                    else
                        GD.Print("Espacio LIBERADO");
                    break;

                default:
                    if (keyEvent.Pressed)
                        GD.Print($"Otra tecla presionada: {keyEvent.Keycode}");
                    else
                        GD.Print($"Otra tecla liberada: {keyEvent.Keycode}");
                    break;
            }
        }
    }

    protected override void Initialize()
    {
 
    }

    protected override void Destroy()
    {
 
    }
}
