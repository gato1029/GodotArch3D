using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using System;

public partial class ControlItemAnimationMultipleDirection : HBoxContainer
{
    // Called when the node enters the scene tree for the first time.
    public event EventNotifyChangued OnNotifyChanguedDelete;
    SpriteAnimationDirection spriteAnimationDirection;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        spriteAnimationDirection = new SpriteAnimationDirection();
        ButtonSelect.Pressed += ButtonSelect_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        LineEditName.TextChanged += LineEditName_TextChanged;
        foreach (AnimationType item in Enum.GetValues(typeof(AnimationType)))
        {
            if (item!= AnimationType.NINGUNA)
            {
                OptionButtonType.AddItem(item.ToString());
            }
            
        }
        OptionButtonType.ItemSelected += OptionButtonType_ItemSelected;
    }

    private void OptionButtonType_ItemSelected(long index)
    {
        AnimationType ct = (AnimationType)index;
        spriteAnimationDirection.animationType = ct;
    }

    private void LineEditName_TextChanged(string newText)
    {
        spriteAnimationDirection.name = newText;
    }

    private void ButtonDelete_Pressed()
    {
        OnNotifyChanguedDelete?.Invoke(this);
        QueueFree();
    }

    private void ButtonSelect_Pressed()
    {
        OnNotifyChangued?.Invoke(this);
    }

    private Texture2D textureNormal = (Texture2D)GD.Load("res://resources/Textures/iconos/ItemPoint.png");
    private Texture2D textureSelection = (Texture2D)GD.Load("res://resources/Textures/iconos/hand.png");

    public void SetVisualizarNormal()
    {
        ButtonSelect.TextureNormal = textureNormal;
    }

    public void SetData(SpriteAnimationDirection spriteAnimationDirection)
    {
        this.spriteAnimationDirection = spriteAnimationDirection;
        LineEditName.Text = spriteAnimationDirection.name;
        OptionButtonType.Selected = (int)spriteAnimationDirection.animationType;
        
    }
    public SpriteAnimationDirection GetData()
    {        
        return spriteAnimationDirection;
    }
    public void SetVisualizarSeleccion()
    {
        ButtonSelect.TextureNormal = textureSelection;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
