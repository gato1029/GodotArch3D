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
public partial class Main3D : Node3D
{
    MultimeshMaterial multimeshMaterial;
    TerrainMap terrainMap;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

   
    public override void _Ready()
	{
        



        EcsManager.Instance.SetNode3DMain(this);
        ChunkManager.Initialize();
        
        SpriteManager.Instance.LoadTexture("res://resources/cdemo.png", new Vector3(32,32,32), new Vector3(0, 0, 0),new Vector2(32,32));
        SpriteManager.Instance.LoadTextureMultimesh("res://resources/Textures/Monster/Hongito.png", new Vector3(80, 64, 32),new Vector3(0,0,0),new Vector2(40,32));

        Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteMulti(10);

        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
    
        xform = xform.Translated(new Vector3(1, 1, 1));
        
        RenderingServer.MultimeshInstanceSetTransform(sprite3d.idRid, sprite3d.idInstance, xform);




        for (int i = 0; i < 100; i++)
        {
            Position position = new Position { value = GetRandomVector2(new Vector2(-150, -150), new Vector2(150, 150)) };
            //Position position = new Position { value = new Vector2(2,-0.5f) };
            CharacterManager.Instance.CreateCharacter(10, position.value);
        }
        MainCharacterManager.Instance.CreateCharacter(new Vector2(0,0));

        terrainMap = new TerrainMap();





        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                terrainMap.AddUpdateTile(new Vector2I(i, j), 7);
            }

        }

        //terrainMap.AddUpdateTile(new Vector2I(0, 0), 6);
        //terrainMap.AddUpdateTile(new Vector2I(2, 2), 5);

        //CharacterCreatorManager.Instance.CreateNewCharacter(1, new Vector2(5, 3));
        
        ChunkManager.Instance.ForcedUpdate();

        //terrainMap.RefreshChunk();
        //for (int i = 0; i < 13; i++)
        //{
        //    Position position = new Position { value = GetRandomVector2(new Vector2(-550, -550), new Vector2(550, 550)) };
        //    CreateUnitIA(position);
        //}        





        //TileDynamicData tileDynamicData = DataBaseManager.Instance.FindById<TileDynamicData>(129);
        //multimeshMaterial = new MultimeshMaterial(DataBaseManager.Instance.FindById<MaterialData>(tileDynamicData.idMaterial));
        //for (int i = 0; i < 10; i++)
        //{
         
        //    Transform3D xform2 = new Transform3D(Basis.Identity, Vector3.Zero);
        //    float profundidad = ((0 + tileDynamicData.offsetInternal.Y) * CommonAtributes.LAYER_MULTIPLICATOR) + 5;
        //    xform2.Origin = (new Vector3(i+tileDynamicData.offsetInternal.X,0+ tileDynamicData.offsetInternal.Y, profundidad));
        //    xform2 = xform2.ScaledLocal(new Vector3(tileDynamicData.scale, tileDynamicData.scale, 1));
            
        //    (Rid, int) instance = multimeshMaterial.CreateInstance();
        //    RenderingServer.MultimeshInstanceSetTransform(instance.Item1, instance.Item2, xform2);
        //    RenderingServer.MultimeshInstanceSetCustomData(instance.Item1, instance.Item2, new Godot.Color(tileDynamicData.x, tileDynamicData.y, tileDynamicData.widht, tileDynamicData.height));
        //}
    
       
    }


    
    void createTile(Position position)
    {
        Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteMulti(5);
        sprite3d.layer = 1;
        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);


        Entity entity = EcsManager.Instance.World.Create();
        entity.Add(sprite3d);
        entity.Add<RefreshPositionOnce>();
        entity.Add(position);
        entity.Add<Transform>(new Transform { transformInternal = xform });
           
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
    public override void _Process(double delta)
    {
        ChunkManager.Instance.UpdatePlayerPosition(PositionsManager.Instance.positionCamera);
        //terrainMap.UpdatePositionChunk(PositionsManager.Instance.positionCamera);
        EcsManager.Instance.UpdateSystems((float)delta, 0);   
        
    }

    public override void _PhysicsProcess(double delta)
    {
        TimeGodot.UpdateDelta((float)delta);
        EcsManager.Instance.UpdateSystemsPhysics((float)delta, 0);   
    }
}
