using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.Flecs;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Info;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;

public partial class Main3D : Node3D
{
    private MultimeshMaterial multimeshMaterial;
    private MapTerrain terrainMap;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private int squareId;
    List<Vector2> points = new List<Vector2>();
    Polygon polygon;
    public override void _Ready()
    {                
        NodeMainHelper.SetNode3DMain(this);
        PerformanceTimer.Instance.Enabled = true;
        ModHelper.Init();        

        MultimeshManager.Instance.Init();
        ChunkManager.Initialize();

        //var w = EcsManager.Instance.World;
        //var dat =ProjectilePool.Instance.SpawnProjectile();
        //ProjectilePool.Instance.ReturnProjectile(dat);
        //ProjectilePool.Instance._commandBuffer.Playback(w);

      //  WireShape.Instance.DrawGrid(1024, 1024, 16, new Vector2(0, 0), -50, Colors.DarkCyan);


        //DataBaseManager.Instance.MigrateRegenerateIdSavePerGroup<ResourceSourceData>();

        //InfoModData infoModData = new InfoModData()
        //{
        //    id = 1, 
        //    name = "Base",
        //    Author = "Josue Cc.",
        //    Description = "Mod base para el juego",
        //    Version = "1.0"
        //};
        //DataBaseManager.Instance.InsertUpdate(infoModData);

        //NormalizeMods();
        
    }

    private void NormalizeMods()
    {
        var mod = DataBaseManager.Instance.FindById<InfoModData>(1);
        var data = DataBaseManager.Instance.FindAll<TileSpriteData>();
        foreach (var item in data)
        {
            switch (item.tileSpriteType)
            {
                case TileSpriteType.Static:
                    item.spriteData.idModMaterial = mod.name + ":" + item.spriteData.idMaterial;
                    break;
                case TileSpriteType.Animated:
                    item.animationData.idModMaterial = mod.name + ":" + item.animationData.idMaterial;
                    break;
                case TileSpriteType.AnimatedDirectionMultiple:
                    var ff = item.spriteMultipleAnimationDirection.animations;
                    foreach (var animation in ff)
                    {
                        var gg = animation.Value.animations;
                        foreach (var anim in gg)
                        {
                            anim.Value.idModMaterial = mod.name + ":" + anim.Value.idMaterial;
                        }
                    }

                    var ii = item.spriteMultipleAnimationDirection.animationsTypes;
                    foreach (var animation in ii)
                    {
                        var gg = animation.Value.animations;
                        foreach (var anim in gg)
                        {
                            anim.Value.idModMaterial = mod.name + ":" + anim.Value.idMaterial;
                        }
                    }
                    break;
                case TileSpriteType.AnimatedMultiple:
                    break;
                default:
                    break;
            }
            DataBaseManager.Instance.InsertUpdate(item, item.id);
        }

    }

    public override void _ExitTree()
    {
    }

    public override void _Input(InputEvent @event)
    {
    }



    private int frameCounter = 0;

    bool first =true;
    public override void _Process(double delta)
    {
        //Vector2I screenSize = DisplayServer.ScreenGetSize();
        //GD.Print("Resolución máxima del monitor: " + screenSize);
        RenderCommandQueue.ExecuteFrame();       
        ChunkManager.Instance.UpdatePlayerPosition(PositionsManager.Instance.positionCamera);
        CurrentWorlds.Instance.GetAllWorld().ForEach(world =>
        {
            world.Update((float)delta);
        });

    }

    public override void _PhysicsProcess(double delta)
    {
        frameCounter++;
        //TimeGodot.UpdateDelta((float)delta);
        //EcsManager.Instance.UpdateSystemsPhysics((float)delta, 0);
        //PerformanceTimer.Instance.PrintAll(30, frameCounter);
    }
}