
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

        EcsManager.Instance.SetCanvasItemRid(GetCanvasItem(),this);
        for (int i = 0; i < 15000; i++)
        {
            Vector2 randomVector = GetRandomVector2(new Vector2(-50000, -50000), new Vector2(50000, 50000));
            Vector2 randomVectordes = GetRandomVector2(new Vector2(-10000, -10000), new Vector2(10000, 10000));

            //if (i == 0)
            //{
            //    randomVector = new Vector2(0, 0);
            //    randomVectordes = new Vector2(500, 500);
            //    UnitManager.CreateUnit(randomVector, randomVectordes, false);
            //}
            if (i == 1)
            {

                randomVector = new Vector2(500, 100);
                randomVectordes = new Vector2(0, 0);
                UnitManager.CreateUnit(randomVector, Vector2.Zero, false);
            }
            else
            {
                UnitManager.CreateUnit(randomVector, randomVectordes, true);
            }


        }

        
     

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
