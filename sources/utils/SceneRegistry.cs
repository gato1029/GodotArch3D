using Godot;
using System;
using System.Collections.Generic;

public static class SceneRegistry
{
    /// 🔹 Mapa principal: Tipo → PackedScene
    private static readonly Dictionary<Type, PackedScene> _scenes = new();

    // =========================================================
    // 🔹 REGISTRO
    // =========================================================

    /// <summary>
    /// Registra una escena usando el tipo como clave.
    /// </summary>
    public static void Register<T>(string path) where T : Node
    {
        Type type = typeof(T);

        if (_scenes.ContainsKey(type))
        {
            GD.Print($"[SceneRegistry] Ya registrado: {type.Name}");
            return;
        }

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"[SceneRegistry] Error cargando: {path}");
            return;
        }

        _scenes[type] = scene;

        GD.Print($"[SceneRegistry] Registrado: {type.Name} -> {path}");
    }

    /// <summary>
    /// Registra automáticamente usando una instancia (usa SceneFilePath).
    /// </summary>
    public static void Register(Node node)
    {
        if (node == null) return;

        string path = node.SceneFilePath;

        if (string.IsNullOrEmpty(path))
        {
            GD.PrintErr($"[SceneRegistry] Nodo sin SceneFilePath: {node.Name}");
            return;
        }

        Type type = node.GetType();

        if (_scenes.ContainsKey(type))
            return;

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"[SceneRegistry] No se pudo cargar: {path}");
            return;
        }

        _scenes[type] = scene;

        GD.Print($"[SceneRegistry] Auto-registrado: {type.Name} -> {path}");
    }

    // =========================================================
    // 🔹 INSTANCIACIÓN
    // =========================================================

    /// <summary>
    /// Instancia una escena usando SOLO el tipo.
    /// </summary>
    public static T Instantiate<T>() where T : Node
    {
        Type type = typeof(T);

        if (!_scenes.TryGetValue(type, out var scene))
        {
            GD.PrintErr($"[SceneRegistry] Tipo no registrado: {type.Name}");
            return null;
        }

        return scene.Instantiate<T>();
    }

    /// <summary>
    /// Instancia basado en otro nodo registrado.
    /// </summary>
    public static T InstantiateFrom<T>(Node node) where T : Node
    {
        if (node == null) return null;

        return Instantiate<T>();
    }

    // =========================================================
    // 🔹 UTILIDADES
    // =========================================================

    public static bool IsRegistered<T>() where T : Node
    {
        return _scenes.ContainsKey(typeof(T));
    }

    public static void Unregister<T>() where T : Node
    {
        _scenes.Remove(typeof(T));
    }

    public static void Clear()
    {
        _scenes.Clear();
    }

    /// <summary>
    /// Debug: imprime todo lo registrado
    /// </summary>
    public static void PrintAll()
    {
        GD.Print("=== SceneRegistry ===");

        foreach (var kv in _scenes)
        {
            GD.Print($"{kv.Key.Name} -> {kv.Value.ResourcePath}");
        }
    }
}