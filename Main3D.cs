using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Godot;
using GodotEcsArch.sources.systems;
using System;

public partial class Main3D : Node3D
{
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    public override void _Ready()
	{        
		EcsManager.Instance.SetNode3DMain(this);


        //ArrayMesh quadMesh = MeshCreator.CreateSquareMesh(64, 64, 32f);
        //ResourceSaver.Save(quadMesh, "res://Quad_64_64.tres");
    


        SpriteManager.Instance.LoadTexture("res://resources/cdemo.png", new Vector3(32,32,32), new Vector3(0, 0, 0),new Vector2(32,32));
        SpriteManager.Instance.LoadTextureMultimesh("res://resources/Textures/Monster/Hongito.png", new Vector3(80, 64, 32),new Vector3(0,0,0),new Vector2(40,32));

        Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteMulti(10);

        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
    
        xform = xform.Translated(new Vector3(1, 1, 1));
        
        RenderingServer.MultimeshInstanceSetTransform(sprite3d.idRid, sprite3d.idInstance, xform);

        int atlasAnimation = SpriteManager.Instance.LoadTextureAnimation("res://resources/baseBlackDemo.png", new Vector3(64, 64, 32), new Vector3(0, 0, 0), new Vector2(32, 32));

        int idAnimation = SpriteManager.Instance.CreateAnimation(atlasAnimation, "BasePersonaje", 4);
        AnimationIndividual animationIndividual = SpriteManager.Instance.GetAnimation(idAnimation);

        float timePerFrame = 0.1f;
        TypeAnimation typeAnimation = new TypeAnimation(4, "Idle");
        typeAnimation.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(286, 2, timePerFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(299, 2, timePerFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(312, 2, timePerFrame));
        typeAnimation.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(325, 2, timePerFrame));
        animationIndividual.AddTypeAnimation(0, typeAnimation);

        TypeAnimation typeAnimation2 = new TypeAnimation(4, "Walk");
        typeAnimation2.AddFrame((int)DirectionAnimation.DOWN, new FrameAnimation(105, 9, timePerFrame));
        typeAnimation2.AddFrame((int)DirectionAnimation.LEFT, new FrameAnimation(118, 9, timePerFrame));
        typeAnimation2.AddFrame((int)DirectionAnimation.UP, new FrameAnimation(131, 9, timePerFrame));
        typeAnimation2.AddFrame((int)DirectionAnimation.RIGHT, new FrameAnimation(144, 9, timePerFrame));
        animationIndividual.AddTypeAnimation(1, typeAnimation2);

        //Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteSimple(1);

        //Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);

        //xform = xform.Translated(new Vector3(1, 1, 1));
        //RenderingServer.MultimeshInstanceSetTransform(sprite3d.idRid, sprite3d.idInstance, xform);



        // Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteMulti(118);
        //Position position2 = new Position { value = new Vector2(5, 5) };

        //CharacterManager.Instance.CreateCharacter(2, new Vector2(2, 2));
        for (int i = 0; i <10; i++)
        {
            Position position = new Position { value = GetRandomVector2(new Vector2(-10, -10), new Vector2(10, 10)) };
            CharacterManager.Instance.CreateCharacter(2, position.value);
        }

        CreateUnitHumanAnimate();
        //for (int i = 0; i < 13; i++)
        //{
        //    Position position = new Position { value = GetRandomVector2(new Vector2(-550, -550), new Vector2(550, 550)) };
        //    CreateUnitIA(position);
        //}

    }
    void CreateUnitHumanAnimate()
    {
        Sprite3D sprite3d = SpriteManager.Instance.CreateAnimationInstance(2);
        
        sprite3d.layer = 5;
        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);

        Position position = new Position { value = new Vector2(1, 1) };
        Entity entity = EcsManager.Instance.World.Create();
        entity.Add(sprite3d);
        entity.Add<RefreshPositionAlways>();
        entity.Add<MainCharacter>();
        entity.Add<Transform>(new Transform { transformInternal = xform });


        Body body = BodyFactory.CreateRectangle(CollisionManager.Instance.worldPhysic,1,1,0); 
        body.BodyType = BodyType.Dynamic;
        body.Position = new Microsoft.Xna.Framework.Vector2(1,1);

        entity.Add<Collider>(new Collider { body= body, rect = new Rect2(new Vector2(0, 0), new Vector2(1, 1)), rectTransform = new Rect2(new Vector2(0, 0), new Vector2(1, 1)), aplyRotation=false});
        entity.Add<Rotation>();
        entity.Add(position);

        entity.Add<Direction>();
        entity.Add<Animation>(new Animation { TimePerFrame= 0.1f, TimeSinceLastFrame = 0, currentAction = AnimationAction.NONE, horizontalOrientation = -1});
        entity.Add(new Velocity { value = 4f });
        entity.Add<Unit>(new Unit { team= 1, health= 10000, damage =30});
        entity.Add<StateComponent>();
        entity.Add<MelleCollider>(new MelleCollider { collider = new Collider { rect = new Rect2(new Vector2(1f, 0.5f), new Vector2(2, 1)), rectTransform = new Rect2(new Vector2(1f, 0.5f), new Vector2(2, 1)), aplyRotation = false } });
        //CollisionManager.Instance.dynamicCollidersEntities.AddUpdateItem(position.value, entity.Reference());
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
    void CreateUnitHuman()
    {
        Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteMulti(1);
        sprite3d.layer = 5;
        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);

        Position position = new Position { value = new Vector2(3, 3) };
        Entity entity = EcsManager.Instance.World.Create();
        entity.Add(sprite3d);
        entity.Add<RefreshPositionAlways>();
        entity.Add<HumanController>();
        entity.Add<Transform>(new Transform { transformInternal = xform });
        entity.Add<Collider>(new Collider { rect = new Rect2(new Vector2(0, 0), new Vector2(1, 1)), rectTransform = new Rect2(new Vector2(0,0), new Vector2(1,1)) });
        entity.Add<Rotation>();
        entity.Add(position);
        entity.Add<Direction>();
        entity.Add(new Velocity { value = 2f });

        CollisionManager.Instance.dynamicCollidersEntities.AddUpdateItem(position.value, entity.Reference());
    }

    void CreateUnitIA(Position position)
    {
        Sprite3D sprite3d = SpriteManager.Instance.CreateSpriteMulti(1);
        sprite3d.layer = 5;
        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);

        
        Entity entity = EcsManager.Instance.World.Create();
        entity.Add(sprite3d);
        entity.Add<RefreshPositionAlways>();
        entity.Add<IAController>();
        entity.Add<Transform>(new Transform { transformInternal = xform });
        entity.Add<Collider>(new Collider { rect = new Rect2(new Vector2(0, 0), new Vector2(1, 1)),  rectTransform = new Rect2(new Vector2(0, 0), new Vector2(1, 1)) });
        entity.Add<Rotation>();
        entity.Add(position);
        entity.Add<Direction>();
        entity.Add(new Velocity { value = 2f });
        entity.Add<SearchTargetMovement>();
        entity.Add<Unit>();
        entity.Add<AreaMovement>(new AreaMovement {  type = MovementType.CIRCLE_STATIC, widthRadius= 2, origin = position.value });

        CollisionManager.Instance.dynamicCollidersEntities.AddUpdateItem(position.value, entity.Reference());
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
