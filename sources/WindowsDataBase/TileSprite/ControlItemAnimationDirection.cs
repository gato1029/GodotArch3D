using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Info;
using System;
using System.Collections.Generic;

public partial class ControlItemAnimationDirection : PanelContainer
{
	private AnimationDirection animationDirection;
	private SpriteAnimationData spriteAnimationData;	
    // Called when the node enters the scene tree for the first time.
    
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        spriteAnimationData = new SpriteAnimationData();
        ButtonVisualizar.Pressed += ButtonVisualizar_Pressed;
        ControlListTexturesAnimated.OnNotifyChangued += ControlListTexturesAnimated_OnNotifyChangued;

    }



    private void ControlListTexturesAnimated_OnNotifyChangued(ControlListTextures objectControl)
    {
        var datalist = objectControl.GetData();
        List<FrameData> framesArray = new();
        foreach (var item in datalist)
        {
            framesArray.Add(new FrameData() { x = item.x, y = item.y, widht = item.width, height = item.height });
        }
        spriteAnimationData.framesArray = framesArray.ToArray();
        spriteAnimationData.idMaterial = objectControl.idMaterial;
        spriteAnimationData.idModMaterial = MasterDataManager.GetData<InfoModData>(1).name+":" + objectControl.idMaterial;
        OnNotifyChangued?.Invoke(this);
    }

    private Texture2D textureNormal = (Texture2D)GD.Load("res://resources/Textures/iconos/ItemPoint.png");
    private Texture2D textureSelection = (Texture2D)GD.Load("res://resources/Textures/iconos/hand.png");

    public void SetVisualizarNormal()
    {
        ButtonVisualizar.TextureNormal = textureNormal;
    }

    public void SetVisualizarSeleccion()
    {
        ButtonVisualizar.TextureNormal = textureSelection;
    }
    private void ButtonVisualizar_Pressed()
	{      
		OnNotifyChangued?.Invoke(this);		
	}
	public (AnimationDirection direction, SpriteAnimationData sprite) GetData()
	{
		return (animationDirection, spriteAnimationData);
	}
    public void SetData(AnimationDirection direction, SpriteAnimationData spriteAnimationData)
	{
		animationDirection = direction;
		this.spriteAnimationData = spriteAnimationData;
		LineEditName.Text = direction.ToString();
        if (spriteAnimationData.framesArray!=null)
        {
            List<TileInfoKuro> dataInfo = new List<TileInfoKuro>();
            foreach (var item in spriteAnimationData.framesArray)
            {
                dataInfo.Add(new TileInfoKuro()
                {
                    idMaterial = spriteAnimationData.idMaterial,
                    x = (int)item.x,
                    y = (int)item.y,
                    width = (int)item.widht,
                    height = (int)item.height,
                    texture = MaterialManager.Instance.GetAtlasTextureInternal(spriteAnimationData.idMaterial, item.x, item.y, item.widht, item.height)
                });
            }
            ControlListTexturesAnimated.SetData(dataInfo);
        }
      
    }

	public void SetSpriteAnimation(SpriteAnimationData spriteAnimationData)
	{
        this.spriteAnimationData = spriteAnimationData;
    }
	public void SetDirection(AnimationDirection direction)
	{ this.animationDirection = direction;
      LineEditName.Text = direction.ToString();
    }


	public void ForcedData(float offsetX, float offsetY, float ydepthRender, float scale)
	{
		spriteAnimationData.yDepthRender =  ydepthRender;
		spriteAnimationData.scale = scale;
		spriteAnimationData.offsetX = offsetX;
		spriteAnimationData.offsetY = offsetY;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
