using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class ControlTileDual : PanelContainer
{
	ControlBlackyAtlasTexture commonBlackyAtlasTexture;
    DualTilePart dualTilePart;


	private TileTextureData tileTextureData;
	private bool isAnimated => tileTextureData != null;
	private AtlasTexture[] atlasTextures;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		TileTexture.GuiInput += TileTexture_GuiInput;
    }

    private void TileTexture_GuiInput(InputEvent @event)
    {
        // Solo procesar click izquierdo
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.Pressed &&
            mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (dualTilePart != null)
            {
                // Formato esperado: "mod:id"
                string[] split = dualTilePart.IdMod.Split(':');

                if (split.Length >= 2)
                {
                    string idMod = split[0];

                    if (int.TryParse(split[1], out int idMaterial))
                    {
                        var mat = AtlasModsManager.Get<MaterialData>(
                            dualTilePart.IdMod,
                            idMaterial
                        );

                        if (mat != null)
                        {
                            commonBlackyAtlasTexture.SetTexture(
                                mat.textureVisual,
                                new Vector2I(mat.divisionPixelX, mat.divisionPixelY),
                                idMaterial,
                                idMod
                            );

                            commonBlackyAtlasTexture.SetSelection(
                                dualTilePart.TileIndex
                            );
                        }
                    }
                }
            }
        }
    }

    public void SetCommonBlackyAtlasTexture( ControlBlackyAtlasTexture common )
	{
		this.commonBlackyAtlasTexture = common;
    }
	public DualTilePart GetDualTilePart()
	{
		return dualTilePart;
    }

	public void SetDualTilePart( DualTilePart dualTilePart)
	{        
        this.dualTilePart = dualTilePart;
        var temp = AtlasModsManager.GetAtlasTexture(dualTilePart.IdMod,dualTilePart.TileIndex, out bool isAnimated, out TileTextureData tileTextureData);
		if (!isAnimated)
		{
            TileTexture.Texture = temp[0];
			isAnimated = false;
			tileTextureData = null;
        }
		else
		{
			atlasTextures = temp;
            isAnimated = true;
            this.tileTextureData = tileTextureData;
        }
		
    }

	int currentFrame = 0;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        // Si el tile es animado, actualizamos la textura según el frame actual
		if (isAnimated && tileTextureData != null)
		{ 			
			TileTexture.Texture = atlasTextures[currentFrame];
			currentFrame++;
			if (currentFrame > atlasTextures.Length)
			{
				currentFrame = 0;
            }
        }
    }
}
