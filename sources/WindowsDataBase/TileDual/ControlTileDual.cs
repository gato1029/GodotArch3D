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
        ControlTileSprite.OnNotifyChangued += ControlTileSprite_OnNotifyChangued;
        ButtonRemove.Pressed += ButtonRemove_Pressed;
    }

    private void ButtonRemove_Pressed()
    {
        this.QueueFree();
    }

    private void ControlTileSprite_OnNotifyChangued(ControlListTileSprite objectControl)
    {
        if (dualTilePart!=null)
        {            
            dualTilePart.IdTileSpriteData = objectControl.GetidTile();
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
        ControlTileSprite.SetIdTile(dualTilePart.IdTileSpriteData);
        this.dualTilePart = dualTilePart;
        if (dualTilePart.IdMod=="")
        {
            // aun sin asignar, no hacemos nada
            return;
        }

    }

	int currentFrame = 0;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
    }
}
