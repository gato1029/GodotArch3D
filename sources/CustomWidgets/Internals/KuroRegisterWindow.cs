using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.CustomWidgets.Internals;

[AttributeUsage(AttributeTargets.Class)]
public class KuroRegisterWindow : Attribute
{
    public string Path { get; }

    public KuroRegisterWindow(string path)
    {
        Path = path;
    }
}

public static class KuroWindowFactory
{
    private static readonly Dictionary<string, string> _registeredWindows = new();

    static KuroWindowFactory()
    {
        RegisterAllAnnotatedWindows();
        GD.Print($"[KuroWindowFactory] Inicializado. Ventanas registradas: {_registeredWindows.Count}");
    }
    /// <summary>
    /// Retorna el path de la ventana registrada
    /// </summary>
    public static string GetPath<T>() where T : Window
    {
        string className = typeof(T).Name;

        if (!_registeredWindows.TryGetValue(className, out var path))
        {
            GD.PrintErr($"[KuroWindowFactory] {className} no registrado.");
            return null;
        }

        return path;
    }
    /// <summary>
    /// Registra todas las clases marcadas con [KuroRegisterWindow("ruta")].
    /// </summary>
    private static void RegisterAllAnnotatedWindows()
    {
        var windowTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Window)));

        foreach (var type in windowTypes)
        {
            var attr = type.GetCustomAttribute<KuroRegisterWindow>();
            if (attr == null)
                continue;

            if (string.IsNullOrEmpty(attr.Path))
            {
                GD.PrintErr($"[KuroWindowFactory] {type.Name} tiene un atributo [KuroRegisterWindow] sin ruta.");
                continue;
            }

            _registeredWindows[type.Name] = attr.Path;
            GD.Print($"[KuroWindowFactory] {type.Name} → {attr.Path}");
        }
         windowTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Popup)));

        foreach (var type in windowTypes)
        {
            var attr = type.GetCustomAttribute<KuroRegisterWindow>();
            if (attr == null)
                continue;

            if (string.IsNullOrEmpty(attr.Path))
            {
                GD.PrintErr($"[KuroWindowFactory] {type.Name} tiene un atributo [KuroRegisterWindow] sin ruta.");
                continue;
            }

            _registeredWindows[type.Name] = attr.Path;
            GD.Print($"[KuroWindowFactory] {type.Name} → {attr.Path}");
        }
    }

    /// <summary>
    /// Crea una instancia de la ventana registrada.
    /// </summary>
    public static T? Create<T>() where T : Window
    {
        string className = typeof(T).Name;

        if (!_registeredWindows.TryGetValue(className, out var path))
        {
            GD.PrintErr($"[KuroWindowFactory] {className} no registrado.");
            return null;
        }

        var scene = GD.Load<PackedScene>(path);
        if (scene == null)
        {
            GD.PrintErr($"[KuroWindowFactory] No se pudo cargar la escena: {path}");
            return null;
        }

        return scene.Instantiate<T>();
    }

    public static T? Create<T>(Node parent) where T : Window
    {
        string className = typeof(T).Name;

        if (!_registeredWindows.TryGetValue(className, out var path))
        {
            GD.PrintErr($"[KuroWindowFactory] {className} no registrado.");
            return null;
        }

        var scene = GD.Load<PackedScene>(path);
        if (scene == null)
        {
            GD.PrintErr($"[KuroWindowFactory] No se pudo cargar la escena: {path}");
            return null;
        }

        return scene.Instantiate<T>();
    }
}