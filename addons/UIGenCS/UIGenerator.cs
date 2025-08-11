using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


public enum UIGeneratorMode
{
   Public,
   Private
}
[Tool]
public partial class UIGenerator : EditorScript
{
    UIGeneratorMode uiGeneratorMode = UIGeneratorMode.Private;
    string access = "private";

    public UIGeneratorMode UiGeneratorMode { get => uiGeneratorMode; set => uiGeneratorMode = value; }

    public override void _Run()
    {
        
        switch (uiGeneratorMode)
        {
            case UIGeneratorMode.Public:
                access = "public";
                break;
            case UIGeneratorMode.Private:
                access = "private";
                break;
            default:
                break;
        }
        var rootNode = EditorInterface.Singleton.GetEditedSceneRoot();
        if (rootNode == null)
        {
            GD.PrintErr("¡Error! No hay ninguna escena cargada.");
            return;
        }

        CSharpScript rootScript = (CSharpScript)rootNode.GetScript();
        if (rootScript == null)
        {
            GD.PrintErr("¡Error! El nodo raíz debe tener un script asociado.");
            return;
        }

        var scriptPath = rootScript.ResourcePath;
        var scriptDir = scriptPath.GetBaseDir();
        var scriptName = Path.GetFileNameWithoutExtension(scriptPath);

        var uiGenDir = $"{scriptDir}/UIGenerate";

        if (!DirAccess.DirExistsAbsolute(uiGenDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(uiGenDir);
        }

        var outputPath = $"{uiGenDir}/{scriptName}.generated.cs";
        GD.Print($"Generando archivo en: {outputPath}");

        var scriptText = GenerateScript(rootNode, scriptName);

        using var file = Godot.FileAccess.Open(outputPath, Godot.FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PrintErr("¡Error! No se pudo abrir el archivo para escribir.");
            return;
        }

        file.StoreString(scriptText);
        file.Close();
        GD.Print($"Código generado correctamente en: {outputPath}");

        // Leer código original
        var originalCode = Godot.FileAccess.GetFileAsString(scriptPath);

        // Verificar si ya existe la llamada a InitializeUI();
        if (originalCode.Contains("InitializeUI();"))
        {
            GD.Print("La llamada a InitializeUI ya existe. No se modifica el archivo original.");
            return;
        }

        var hasReady = originalCode.Contains("public override void _Ready()");
        var newLines = new List<string>();

        if (hasReady)
        {
            var lines = originalCode.Split('\n');
            bool insideReady = false;
            

            foreach (var line in lines)
            {
                newLines.Add(line);
                if (line.TrimStart().StartsWith("public override void _Ready()"))
                {
                    insideReady = true;
                    continue;
                }

                if (insideReady && line.Trim() == "{")
                {
                    newLines.Add("        InitializeUI(); // Insertado por el generador de UI");                    
                    insideReady = false;
                }
            }

            originalCode = string.Join("\n", newLines);
        }
        else
        {
            originalCode += "\n\n    public override void _Ready()\n    {\n        InitializeUI(); // Insertado por el generador de UI\n    }\n";
            GD.Print("Agregado método _Ready con InitializeUI.");
        }

        var f = Godot.FileAccess.Open(scriptPath, Godot.FileAccess.ModeFlags.Write);
        if (f != null)
        {
            f.StoreString(originalCode);
            f.Close();
            GD.Print("Archivo original actualizado con llamada a InitializeUI.");
        }
        else
        {
            GD.PrintErr("¡Error! No se pudo reescribir el archivo original.");
        }
    
    }


    private string GenerateScript(Node rootNode, string classNameBase)
    {
        string baseClass = rootNode.GetClass();
        List<string> lines = new()
    {
        "// AUTO-GENERATED FILE. DO NOT EDIT.",
        "using Godot;",
        "using System;\n",
        $"public partial class {classNameBase} : {baseClass}",
        "{",
        $"    public delegate void EventNotifyChangued({classNameBase} objectControl);",
        "    public event EventNotifyChangued OnNotifyChangued;\n"
    };

        List<string> nodeDefs = new();
        List<string> initLines = new();
        List<string> extraMethods = new();

        var nodes = CollectNodes(rootNode);

        if (rootNode is Window)
        {
            initLines.Add("        CloseRequested += CloseRequestedWindow;");
        }

        foreach (var node in nodes)
        {           
            string nodeType = node.GetClass();
            CSharpScript script = (CSharpScript)node.GetScript();
            if ( script!=null && script.ResourcePath != "")
            {
                var scriptPath = script.ResourcePath;
                nodeType = System.IO.Path.GetFileNameWithoutExtension(scriptPath);
            }

            var nameMatch = new Regex($"^{nodeType}(\\d*)$");


            if (nameMatch.IsMatch(node.Name) && (CSharpScript)node.GetScript() == null)
                continue;


            NodePath relPath = rootNode.GetPathTo(node);
            string varName = node.Name.ToString().Replace(" ", "_");

            Dictionary<string, List<string>> result;

            switch (nodeType)
            {
                //case "CheckBox": // Check this case
                //    result = ProcessCheckbox(node, varName, relPath);
                //    break;
                case "OptionButton":
                    result = ProcessOptionButton(node, varName, relPath);
                    break;
                default:
                    result = ProcessGenericNode(node, varName, relPath, nodeType);
                    break;
            }

            nodeDefs.AddRange(result["node_defs"]);
            initLines.AddRange(result["init_lines"]);
            extraMethods.AddRange(result["extra_methods"]);
        }

        lines.AddRange(nodeDefs);
        lines.Add("\n    public void InitializeUI()");
        lines.Add("    {");
        lines.AddRange(initLines);
        lines.Add("    }");

        if (rootNode is Window)
        {
            lines.Add("\n    private void CloseRequestedWindow()");
            lines.Add("    {");
            lines.Add("        QueueFree();");
            lines.Add("    }\n");
        }

        lines.AddRange(extraMethods);
        lines.Add("}");

        return string.Join("\n", lines);
    }
    private Dictionary<string, List<string>> ProcessCheckbox(Node n, string varName, NodePath relPath)
    {
        string originalText = (n as CheckBox)?.Text ?? "";

        return new Dictionary<string, List<string>>
        {
            ["node_defs"] = new List<string>
        {
            $"    {access} CheckBox {varName};"
        },
            ["init_lines"] = new List<string>
        {
            $"        {varName} = GetNode<CheckBox>(\"{relPath}\");",
            $"        {varName}.Pressed += {varName}_PressedUI;",
            $"        {varName}_PressedUI();"
        },
            ["extra_methods"] = new List<string>
        {
            "",
            $"    private void {varName}_PressedUI()",
            "    {",
            $"        if ({varName}.ButtonPressed)",
            $"            {varName}.Text = \"{originalText}\";",
            "        else",
            $"            {varName}.Text = \"No {originalText}\";",
            "    }"
        }
        };
    }

    private Dictionary<string, List<string>> ProcessOptionButton(Node n, string varName, NodePath relPath)
    {
        return new Dictionary<string, List<string>>
        {
            ["node_defs"] = new List<string>
        {
            $"    {access} OptionButton {varName};"
        },
            ["init_lines"] = new List<string>
        {
            $"        {varName} = GetNode<OptionButton>(\"{relPath}\");",
            $"        {varName}.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;"
        },
            ["extra_methods"] = new List<string>()
        };
    }
    private Dictionary<string, List<string>> ProcessGenericNode(Node n, string varName, NodePath relPath, string nodeType)
    {
        return new Dictionary<string, List<string>>
        {
            ["node_defs"] = new List<string>
        {
            $"    {access} {nodeType} {varName};"
        },
            ["init_lines"] = new List<string>
        {
            $"        {varName} = GetNode<{nodeType}>(\"{relPath}\");"
        },
            ["extra_methods"] = new List<string>()
        };
    }
    private bool IsInstanceOfScene(Node node)
    {
        if (node == null)
            return false;

        // Debe venir de un .tscn
        if (string.IsNullOrEmpty(node.SceneFilePath))
            return false;

        // Si no tiene Owner o el Owner es él mismo → es la raíz, no una instancia embebida
        if (node.Owner == null || node.Owner == node)
            return false;
        return true; // Es una instancia dentro de otra escena
    }
    private void Traverse(Node node, List<Node> acc, bool isRoot = false)
    {
        acc.Add(node);

        if (IsInstanceOfScene(node))
        {
            GD.Print($"Nodo '{node.Name}' es una instancia de 'PackedScene', omitiendo hijos propios...");
            foreach (var child in node.GetChildren())
            {
                if (child is Node childNode && childNode.Owner != node)
                    Traverse(childNode, acc);
            }
            return;
        }

        if (!isRoot && (CSharpScript)node.GetScript() != null)
        {
            GD.Print($"Nodo '{node.Name}' tiene script asociado, omitiendo hijos propios...");
            foreach (var child in node.GetChildren())
            {
                if (child is Node childNode && childNode.Owner != node)
                    Traverse(childNode, acc);
            }
            return;
        }

        foreach (var child in node.GetChildren())
        {
            if (child is Node childNode)
                Traverse(childNode, acc);
        }
    }
    private List<Node> CollectNodes(Node root)
    {
        var list = new List<Node>();
        Traverse(root,list, isRoot: true);
        list.Remove(root); // Remueve el root porque se agregó primero
        return list;
    }


}
