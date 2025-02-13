
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using ImGuiGodot;
using ImGuiNET;
using System.Drawing;

public partial class Main : Node2D
{

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private PackedScene _enemyScene;
    private float updateCounter = 1;
    public   override void _Ready()
    {


    }

    private Vector2 GetRandomVector2(Vector2 min, Vector2 max)
    {
        // Generar valores aleatorios dentro del rango para x y y
        float randomX = _rng.RandfRange(min.X, max.X);
        float randomY = _rng.RandfRange(min.Y, max.Y);

        // Retornar el Vector2 con los valores aleatorios
        return new Vector2(randomX, randomY);
    }
    public override void _Process(double delta)
    {
       
        EcsManager.Instance.UpdateSystems((float)delta, 0);
    }
    
    public override void _PhysicsProcess(double delta)
    {
        
        TimeGodot.UpdateDelta((float)delta);
        
        EcsManager.Instance.UpdateSystemsPhysics((float)delta, 0);
        //updateCounter += (float) delta;        
        //if (updateCounter >= 1)
        //{
        //    //GD.Print(updateCounter);
        //    updateCounter = 0;
        //}

        //int tick = Mathf.RoundToInt((updateCounter * 60.0f));
        //GD.Print(tick);
        // Logic Batch

    }
}
