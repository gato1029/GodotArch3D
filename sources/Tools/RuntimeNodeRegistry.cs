using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public class RuntimeNodeRegistry
{
    private readonly Dictionary<Type, PackedScene> registryByType = new();
    private readonly Dictionary<string, PackedScene> registryByName = new();
    private readonly Dictionary<string, string> registryPaths = new();

    public IReadOnlyDictionary<Type, PackedScene> RegistryByType => registryByType;

    public void RegisterAllScenesFromFolder(string folder)
    {
        GD.Print($"[RuntimeNodeRegistry] Escaneando carpeta: {folder}");
        ScanRecursive(folder);
        GD.Print($"[RuntimeNodeRegistry] Escenas registradas: {registryByType.Count}");
    }

    private void ScanRecursive(string path)
    {
        var dir = DirAccess.Open(path);
        if (dir == null)
        {
            GD.PrintErr($"[RuntimeNodeRegistry] No se pudo abrir carpeta: {path}");
            return;
        }

        dir.ListDirBegin();

        while (true)
        {
            string file = dir.GetNext();

            if (string.IsNullOrEmpty(file))
                break;

            if (file.StartsWith("."))
                continue;

            string fullPath = path.TrimEnd('/') + "/" + file;

            if (dir.CurrentIsDir())
            {
                ScanRecursive(fullPath);
                continue;
            }

            if (!file.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
                continue;

            RegisterScene(fullPath);
        }

        dir.ListDirEnd();
    }

    private void RegisterScene(string scenePath)
    {
        PackedScene packed = ResourceLoader.Load<PackedScene>(scenePath);
        if (packed == null)
        {
            GD.PrintErr($"[RuntimeNodeRegistry] No se pudo cargar PackedScene: {scenePath}");
            return;
        }

        Node instance = packed.Instantiate();
        if (instance == null)
        {
            GD.PrintErr($"[RuntimeNodeRegistry] No se pudo instanciar: {scenePath}");
            return;
        }

        Type realType = ResolveRealNodeType(instance, scenePath);
        string className = realType.Name;

        if (!registryByType.ContainsKey(realType))
            registryByType.Add(realType, packed);

        if (!registryByName.ContainsKey(className))
            registryByName.Add(className, packed);

        if (!registryPaths.ContainsKey(className))
            registryPaths.Add(className, scenePath);

        GD.Print($"[RuntimeNodeRegistry] Registrado: {className} -> {scenePath}");

        instance.QueueFree();
    }

    private Type ResolveRealNodeType(Node node, string scenePath)
    {
        Script script = (Script)node.GetScript();

        if (script is CSharpScript csharpScript)
        {
            string scriptPath = csharpScript.ResourcePath;
            string className = Path.GetFileNameWithoutExtension(scriptPath);

            Type foundType = FindTypeByName(className);
            if (foundType != null)
                return foundType;
        }

        return node.GetType();
    }

    private Type FindTypeByName(string className)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = asm.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (var t in types)
            {
                if (t.Name == className)
                    return t;
            }
        }

        return null;
    }

    public bool Has<T>() where T : Node
    {
        return registryByType.ContainsKey(typeof(T));
    }

    public bool Has(string className)
    {
        return registryByName.ContainsKey(className);
    }

    public T Create<T>() where T : Node
    {
        Type t = typeof(T);

        if (!registryByType.TryGetValue(t, out PackedScene packed))
        {
            GD.PrintErr($"[RuntimeNodeRegistry] No existe escena registrada para {t.Name}");
            return null;
        }

        return packed.Instantiate<T>();
    }

    public Node Create(string className)
    {
        if (!registryByName.TryGetValue(className, out PackedScene packed))
        {
            GD.PrintErr($"[RuntimeNodeRegistry] No existe escena registrada para {className}");
            return null;
        }

        return packed.Instantiate();
    }

    public string GetPath<T>() where T : Node
    {
        string name = typeof(T).Name;
        return registryPaths.TryGetValue(name, out string path) ? path : null;
    }

    public string GetPath(string className)
    {
        return registryPaths.TryGetValue(className, out string path) ? path : null;
    }
}