using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class ControlTileDual : PanelContainer
{
	ControlBlackyAtlasTexture commonBlackyAtlasTexture;
    DualTilePart dualTilePart;

    public event Action<ControlTileDual> OnDualTilePartRemoved;
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
        if (@event is not InputEventMouseButton mouseButton)
            return;

        if (!mouseButton.Pressed)
            return;

        // CLICK DERECHO -> quitar tile
        if (mouseButton.ButtonIndex == MouseButton.Right)
        {
            dualTilePart.IdMod = "";
            dualTilePart.TileIndex = -1;
            RefreshTexture();
            OnDualTilePartRemoved?.Invoke(this);            
            return;
        }

             
        if (mouseButton.ButtonIndex == MouseButton.Middle)
        {
            if (dualTilePart != null && dualTilePart.IdMod != "")
            {
                string[] split = dualTilePart.IdMod.Split(':');

                if (split.Length >= 2)
                {
                    string idMod = split[0];

                    if (int.TryParse(split[1], out int idMaterial))
                    {
                        var mat = AtlasModsManager.Get<MaterialData>(
                            idMod,
                            idMaterial
                        );

                        if (mat != null)
                        {
                            commonBlackyAtlasTexture.SetTexture(
                                mat.textureVisual,
                                new Vector2I(mat.divisionPixelX, mat.divisionPixelY),
                                idMaterial,
                                dualTilePart.IdMod
                            );

                            commonBlackyAtlasTexture.SetSelection(
                                dualTilePart.TileIndex + 1
                            );
                        }
                    }
                }
            }

            return;
        }
       if (mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (commonBlackyAtlasTexture!=null)
            {
                // CLICK SIMPLE -> asignar tile actual
                var selection = commonBlackyAtlasTexture.GetCurrentSelectionMatrix();
                if (selection!=null)
                {
                    var tileSelect = selection.matrix[0, 0];

                    dualTilePart.IdMod = tileSelect.idMod;
                    dualTilePart.TileIndex = tileSelect.index - 1;
                    RefreshTexture();
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

    private void RefreshTexture()
    {
        if (dualTilePart != null && dualTilePart.IdMod != "")
        {
            var temp = AtlasModsManager.GetAtlasTexture(dualTilePart.IdMod, dualTilePart.TileIndex, out bool isAnimated, out TileTextureData tileTextureData);
            if (!isAnimated)
            {
                TileTexture.Texture = temp[0];
                this.tileTextureData = null;
            }
            else
            {
                atlasTextures = temp;
                this.tileTextureData = tileTextureData;
            }
        }
        else
        {
            TileTexture.Texture = null;
            this.tileTextureData = null;
        }
    }
    public void SetDualTilePart( DualTilePart dualTilePart)
	{        
        this.dualTilePart = dualTilePart;
        if (dualTilePart.IdMod=="")
        {
            // aun sin asignar, no hacemos nada
            return;
        }
        RefreshTexture();

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
