using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.systems;
using System;

public partial class Main3D : Node3D
{
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    public override void _Ready()
	{
        

    
        EcsManager.Instance.SetNode3DMain(this);

        
        SpriteManager.Instance.LoadTexture("res://resources/cdemo.png", new Vector3(32,32,32), new Vector3(0, 0, 0),new Vector2(32,32));
        SpriteManager.Instance.LoadTextureMultimesh("res://resources/Textures/Monster/Hongito.png", new Vector3(80, 64, 32),new Vector3(0,0,0),new Vector2(40,32));

        Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteMulti(10);

        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
    
        xform = xform.Translated(new Vector3(1, 1, 1));
        
        RenderingServer.MultimeshInstanceSetTransform(sprite3d.idRid, sprite3d.idInstance, xform);




        for (int i = 0; i < 10; i++)
        {
            Position position = new Position { value = GetRandomVector2(new Vector2(-15, -15), new Vector2(15, 15)) };
            //Position position = new Position { value = new Vector2(2,-0.5f) };
            CharacterManager.Instance.CreateCharacter(10, position.value);
        }
        MainCharacterManager.Instance.CreateCharacter(new Vector2(0,0));

        //for (int i = 0; i < 13; i++)
        //{
        //    Position position = new Position { value = GetRandomVector2(new Vector2(-550, -550), new Vector2(550, 550)) };
        //    CreateUnitIA(position);
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
     
        EcsManager.Instance.UpdateSystems((float)delta, 0);   
        
    }

    public override void _PhysicsProcess(double delta)
    {

        TimeGodot.UpdateDelta((float)delta);

        EcsManager.Instance.UpdateSystemsPhysics((float)delta, 0);
   

    }
}
