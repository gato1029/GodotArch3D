using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using LiteDB;
using System.Collections.Generic;
using GodotEcsArch.sources.WindowsDataBase.Materials;

public partial class WindowDataSearch : Window
{
    PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/AccesoryControl.tscn");
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI            
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}


 
}
