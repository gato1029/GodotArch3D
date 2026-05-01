using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;

public partial class WindowEditorRuntimeTerrain : Window
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		
	}

	public void SetMaterial(MaterialData materialData)
	{
		Vector2I cellsize = new Vector2I(materialData.divisionPixelX,materialData.divisionPixelY);
		EditorTextura.SetTexture((Texture2D)materialData.textureMaterial,cellsize,  materialData.id, materialData.idNameMod);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
