using Godot;
using System;
using System.Collections.Generic;

public partial class RuntimeServices : Node
{

    private static RuntimeServices _instance;    
    public static RuntimeServices Instance
    {
        get
        {
            if (_instance == null)
                GD.PushError("RuntimeServices not initialized! Make sure it's added to the scene or autoloaded.");
            return _instance;
        }
    }


    public static RuntimeNodeRegistry NodeRegistry { get; private set; }

    public string RegistryFolder = "res://sources/";

    public override void _Ready()
    {
        _instance = this;
        NodeRegistry = new RuntimeNodeRegistry();
        NodeRegistry.RegisterAllScenesFromFolder(RegistryFolder);
        GD.Print("[RuntimeServices] Servicios inicializados.");
    }
}