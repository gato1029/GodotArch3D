using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.systems;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;
using System.Reflection.Emit;

using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.managers.Buildings;
using GodotEcsArch.sources.components;
public partial class Main3D : Node3D
{
    MultimeshMaterial multimeshMaterial;
    TerrainMap terrainMap;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();


    int squareId;
    public override void _Ready()
	{

        PerformanceTimer.Instance.Enabled = true;
        EcsManager.Instance.SetNode3DMain(this);
        ChunkManager.Initialize();

        //var w = EcsManager.Instance.World;
        //var dat =ProjectilePool.Instance.SpawnProjectile();
        //ProjectilePool.Instance.ReturnProjectile(dat);
        //ProjectilePool.Instance._commandBuffer.Playback(w);
        

        WireShape.Instance.DrawGrid(1024, 1024, 16, new Vector2(0, 0), 20, Colors.DarkCyan);
        //int idg=WireShape.Instance.DrawIsometricGrid(10, 10, 32, new Vector2(0, 0), 20, Colors.ForestGreen,true);
        //float tt = (10 * 16)/2;
        //WireShape.Instance.UpdateShapePositionPixel(idg, new Vector2(-tt,-tt));

        int id =WireShape.Instance.DrawFilledSquare(new Vector2(16, 16), new Vector2(0, 0), 30, Colors.SeaGreen, .5f);
        WireShape.Instance.UpdateShapePositionPixel(id, new Vector2(24,24));
        //MapLevelData mapLevelData = new MapLevelData("Demo", new Vector2I(100, 100), MapType.Mapa, 10, "Demo");
        //MapManagerEditor.Instance.currentMapLevelData = mapLevelData;
        //var dd =MultimeshManager.Instance;

        //terrainMap = TerrainMap.LoadMapfromFile("Mapa1");


        //terrainMap = new TerrainMap("Mapa2", 1, false);

        //for (int i = -10; i < 10; i++)
        //{
        //    for (int j = -10; j < 10; j++)
        //    {
        //        terrainMap.AddUpdateTile(new Vector2I(i, j), 1);
        //    }

        //}
        //terrainMap.AddUpdateTile(new Vector2I(8, 8), 8);


        //terrainMap.LoadMapData();
        //terrainMap.SaveAllMap();

        //MapResources mapResources = new MapResources("recursos", 10, false);
        //for (int i = -20; i < 20; i++)
        //{
        //    for (int j = -20; j < 20; j++)
        //    {
        //        mapResources.AddSprite(new Vector2I(i, j), 1);
        //    }

        //}




        //ChunkManager.Instance.ForcedUpdate();


        CharacterCreatorManager.Instance.CreateNewCharacter(1, new Vector2(0, 0));






        //Vector2 wdd = new Vector2(5, 5);
        //CharacterCreatorManager.Instance.CreateNewCharacter(2, wdd);
        // Dibujar un cuadrado
        //squareId = WireShape.Instance.DrawSquare(
        //    new Vector2(32, 32),
        //    new Vector2(1, 1),
        //    20,
        //    Colors.Green
        //);


        //int arrowId = WireShape.Instance.DrawArrow(new Vector2(0,0), new Vector2(0, 32),20, Colors.Green);

        //WireShape.Instance.RotateArrow(arrowId, 90);

        //var sprite =BuildingManager.Instance.GetData(5).spriteBullet;
        //SpriteHelper.CreateEntity(sprite, 1, new Vector2(3, 3), 30, new Vector2(0, 0));

        //var spriteBase = BuildingManager.Instance.GetData(5).spriteBullet;
        //var sprite = SpriteHelper.CreateSpriteRenderGpuComponent(spriteBase, spriteBase.scale, new Vector2(0,0), 30);
        //Entity entity = EcsManager.Instance.World.Create();
        //entity.Add(sprite);
        //entity.Add(new PositionComponent { position = new Vector2(4, 4) });
        for (int i = 0; i < 0; i++)
        {

            //Vector2 dd = GetRandomVector2(new Vector2(-50, -50), new Vector2(100, 100));
            //CharacterCreatorManager.Instance.CreateNewCharacter(2, dd);
       
        }        
    }
    
    public override void _ExitTree()
    {
       
        
    }

    public override void _Input(InputEvent @event)
    {
         
    }


    private Vector3 GetRandomVector3(Vector2 min, Vector2 max)
    {
        // Generar valores aleatorios dentro del rango para x y y
        float randomX = _rng.RandfRange(min.X, max.X);
        float randomY = _rng.RandfRange(min.Y, max.Y);

        // Retornar el Vector2 con los valores aleatorios
        return new Vector3(randomX, randomY,1);
    }
    private Vector2 GetRandomVector2(Vector2 min, Vector2 max)
    {
        // Generar valores aleatorios dentro del rango para x y y
        float randomX = _rng.RandfRange(min.X, max.X);
        float randomY = _rng.RandfRange(min.Y, max.Y);

        // Retornar el Vector2 con los valores aleatorios
        return new Vector2(randomX, randomY);
    }
    int frameCounter = 0;
    public override void _Process(double delta)
    {

       

        //Vector2I screenSize = DisplayServer.ScreenGetSize();
        //GD.Print("Resolución máxima del monitor: " + screenSize);

        ChunkManager.Instance.UpdatePlayerPosition(PositionsManager.Instance.positionCamera);
        //terrainMap.UpdatePositionChunk(PositionsManager.Instance.positionCamera);
        EcsManager.Instance.UpdateSystems((float)delta, 0);
        
    }

    public override void _PhysicsProcess(double delta)
    {
        frameCounter++;
        TimeGodot.UpdateDelta((float)delta);
        EcsManager.Instance.UpdateSystemsPhysics((float)delta, 0);
      //  PerformanceTimer.Instance.PrintAll(30, frameCounter);
    }
}
