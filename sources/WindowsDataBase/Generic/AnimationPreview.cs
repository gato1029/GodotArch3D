using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class AnimationPreview : Sprite2D
{

    public AnimationStateData data { get; set; }
    public int idMaterial { get; set; }
    public int currentIdState { get; set; }
    int indexFrame = 0;
    double currentfps = 0;
    public override void _Ready()
	{
        currentIdState = 0;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (data !=null)
        {
            if (data.animationData[currentIdState] != null && data.animationData[currentIdState].idFrames != null)
            {
                currentfps += delta;
                if (currentfps >= data.frameDuration && data.animationData[currentIdState].idFrames.Length > 0)
                {
                    var iFrame = data.animationData[currentIdState].idFrames[indexFrame];
                    indexFrame++;
                    currentfps = 0;
                    var dataTexture = MaterialManager.Instance.GetAtlasTexture(idMaterial, iFrame);
                    Texture = dataTexture;
                }
                if (indexFrame >= data.animationData[currentIdState].idFrames.Length)
                {
                    indexFrame = 0;
                }
            }
        }
       
    }
}
