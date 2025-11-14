using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Creators;

internal class CharacterCreator : SingletonBase<CharacterCreator>
{
    private int _characterCount = 0;

    public Entity Create(int id, Godot.Vector2 position)
    {
        _characterCount++;
        CharacterModelBaseData characterBaseData = CharacterLocalBase.Instance.GetCharacterBaseData(id);
        var entity = FlecsManager.Instance.WorldFlecs.Entity("Character" + _characterCount);

        

        switch (characterBaseData.characterType)
        {
            case CharacterType.MAIN:
                return CreateMain(entity, characterBaseData, position);
                break;

            case CharacterType.NPC:
                break;

            case CharacterType.ENEMIGO:
                return CreateEnemy(entity, characterBaseData,position);
                break;

            default:
                break;
        }
        return entity;
    }

    private Entity CreateMain(Entity entity, CharacterModelBaseData characterBaseData, Vector2 position)
    {     
        GeometricShape2D colliderMove = characterBaseData.animationCharacterBaseData.collisionMove.Multiplicity(characterBaseData.scale);
        Circle circle = (Circle)colliderMove;
        GeometricShape2D colliderBody = characterBaseData.animationCharacterBaseData.collisionBody.Multiplicity(characterBaseData.scale);                

        entity.Set(new GodotFlecs.sources.Flecs.Components.CharacterComponent
        {
            characterStateType = CharacterStateType.IDLE,
            characterBehaviorType = CharacterBehaviorType.PERSONAJE_PRINCIPAL
        });

        CreateMainBodyWeapons(entity, characterBaseData, position);

        //entity.Set(new RenderTransformComponent(transform));
        //entity.Set(new RenderGPUComponent( instance.rid,   instance.instance, instance.material, instance.layerTexture, 20, 0, 1, originOffset));
        //entity.Set(new AnimationComponent(characterBaseData.id, EntityType.PERSONAJE, 0, -1, 0, 0, 0, false, true));
        //entity.Set(new RenderFrameDataComponent { uvMap = new Godot.Color(0, 0, 0, 0) });

        entity.Set(new TeamComponent(1));
        entity.Set(new IdGenericComponent(characterBaseData.id, EntityType.PERSONAJE));
   
        
        entity.Set(new PositionComponent(position, Vector2I.Zero));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero, GodotEcsArch.sources.components.AnimationDirection.LEFT));

       // int idAgent = RvoManager.Instance.RegisterAgent(position, circle.Radius, 3);
       // entity.Set(new RvoAgentIdComponent(idAgent, circle.Radius));

        entity.Set(new VelocityComponent(new Vector2(0, 0), 3, new Vector2(0, 0)));
        entity.Set(new MoveResolutorComponent(false, 0, position));
        entity.Set(new PlayerInputComponent());

        int idCollider = CollisionManager.Instance.characterEntitiesFlecs.AddColliderObject(entity, colliderBody, position,colliderMove);
        entity.Set(new ColliderComponent(idCollider, new Rect2(), colliderBody.OriginCurrent, new Rect2((colliderMove.GetSizeQuad() / 2), colliderMove.GetSizeQuad()), colliderMove.OriginCurrent));

        entity.Set(new HumanAttackComponent(10, 1f, 0.0f, 0.2f, 0));
        entity.Set(new HealthComponent(6000));
        entity.Set(new RvoAgentDebugComponent(CreateShapeDebug(circle.Radius, position), CreateShapeDebug(colliderBody.GetSizeQuad(), position), 0));

        entity.Add<UseBoidTag>();
        return entity;
    }
    private void CreateMainBodyWeapons(Entity entity, CharacterModelBaseData characterBaseData, Vector2 position)
    {
        Godot.Vector2 originOffset = new Vector2(characterBaseData.animationCharacterBaseData.offsetSpriteX * characterBaseData.scale, characterBaseData.animationCharacterBaseData.offsetSpriteY * characterBaseData.scale);
        int idMaterial = characterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial;
        var instance = MultimeshManager.Instance.CreateInstance(idMaterial);

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X, position.Y, (position.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        int numLayers = 2; // Cuerpo + arma
        AnimationComponent[] animations = new AnimationComponent[numLayers];
        RenderGPUComponent[] GPUData = new RenderGPUComponent[numLayers];
        RenderFrameDataComponent[] frames = new RenderFrameDataComponent [numLayers];
        RenderTransformComponent[] transforms = new RenderTransformComponent [numLayers];

        // Capa del cuerpo principal
        animations[0] = new AnimationComponent(characterBaseData.id, EntityType.PERSONAJE, 0, -1, 0, 0, 0, false, true,true);
        GPUData[0] = new RenderGPUComponent(instance.rid, instance.instance, instance.material, instance.layerTexture, 20, 0, 1, originOffset);
        frames[0] = new RenderFrameDataComponent { uvMap = new Godot.Color(0, 0, 0, 0) };
        transforms[0] = new RenderTransformComponent(transform);

        // Capa del arma 
        //var weaponBase =AccesoryManager.Instance.GetAccesory(2);
        int idWeapon = 2; // Aquí deberías obtener el ID del arma que deseas equipar
        var weaponBase = AccesoryAvatarManager.Instance.ChangueAccesory(AccesoryAvatarType.WEAPON, idWeapon);

        animations[1] = new AnimationComponent(idWeapon, EntityType.ACCESORIO, 1, -1, 0, 0, 0, false, true,false);
        GPUData[1] = new RenderGPUComponent(weaponBase.rid, weaponBase.instance, 0, 0, 20, 0, 1, originOffset);
        frames[1] = new RenderFrameDataComponent { uvMap = new Godot.Color(0, 0, 0, 0) };
        transforms[1] = new RenderTransformComponent(transform);

        
        entity.Set(new RenderLayerListComponent
            {
            Animations = animations,
            GPUData = GPUData,
            Frames = frames,
            Transforms = transforms
        });

        // Aquí puedes agregar la lógica para crear las armas del personaje principal
        // Por ejemplo, podrías agregar componentes relacionados con las armas
    }
    private Entity CreateEnemy(global::Flecs.NET.Core.Entity entity, CharacterModelBaseData characterBaseData, Godot.Vector2 position)
    {
        int idMaterial = characterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial;
        var instance = MultimeshManager.Instance.CreateInstance(idMaterial);

        GeometricShape2D colliderMove = characterBaseData.animationCharacterBaseData.collisionMove.Multiplicity(characterBaseData.scale);
        Circle circle = (Circle)colliderMove;

        GeometricShape2D colliderBody = characterBaseData.animationCharacterBaseData.collisionBody.Multiplicity(characterBaseData.scale);
        

        Godot.Vector2 originOffset = new Vector2(characterBaseData.animationCharacterBaseData.offsetSpriteX * characterBaseData.scale, characterBaseData.animationCharacterBaseData.offsetSpriteY* characterBaseData.scale);

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.X, position.Y, (position.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        transform = transform.ScaledLocal(new Godot.Vector3(characterBaseData.scale, characterBaseData.scale, 1));

        entity.Set(new GodotFlecs.sources.Flecs.Components.CharacterComponent
        {

            characterStateType = CharacterStateType.IDLE, 
            characterBehaviorType = CharacterBehaviorType.GENERICO
            
        });
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new TeamComponent(2));
        entity.Set(new IdGenericComponent(characterBaseData.id, EntityType.PERSONAJE));
        entity.Set(new RenderGPUComponent( instance.rid, instance.instance, instance.material, instance.layerTexture, 20, 0, 1, originOffset));
        entity.Set(new AnimationComponent(characterBaseData.id, EntityType.PERSONAJE, 0,-1,0,0,0,false,true, true));
        entity.Set(new RenderFrameDataComponent{uvMap = new Godot.Color(0,0,0,0)});
        entity.Set(new PositionComponent(position,Vector2I.Zero));
        entity.Set(new DirectionComponent(Godot.Vector2.Zero, Godot.Vector2.Zero, GodotEcsArch.sources.components.AnimationDirection.LEFT));

        //int idAgent = RvoManager.Instance.RegisterAgent(position, circle.Radius,3f);
        //entity.Set(new RvoAgentIdComponent(idAgent, circle.Radius));
        entity.Set(new VelocityComponent(new Vector2(0,0),3f,new Vector2(0,0)));
        entity.Set(new MoveResolutorComponent(false,0, position));
        

        int idCollider = CollisionManager.Instance.characterEntitiesFlecs.AddColliderObject(entity, colliderBody, position,colliderMove);
        entity.Set(new ColliderComponent(idCollider, new Rect2(), colliderBody.OriginCurrent,new Rect2((colliderMove.GetSizeQuad() / 2) , colliderMove.GetSizeQuad()), colliderMove.OriginCurrent));
        
        
        //entity.Set(new RvoAgentDebugComponent(CreateShapeDebug(circle.Radius, position),CreateShapeDebug(colliderBody.GetSizeQuad(),position), CreateShapeDebug(1, position)));


        entity.Add<UseBoidTag>();

        entity.Set(new HealthComponent(100));
        //entity.Set(new RangedAttackComponent(1,20,5f,0.5f,0,true,6));
        entity.Set(new MeleeAttackComponent(20, 1, 1f, 0));
        entity.Set(new AttackPendingComponent(false, default));
        entity.Set(new EnemySearchComponent(5, 2, 0));
        return entity;
    }

    int CreateShapeDebug(float radius,Vector2 position)
    {        
        return WireShape.Instance.DrawCircle(radius,position,30,Colors.OrangeRed, 32, WireShape.TypeDraw.NORMAL);
    }
    int CreateShapeDebug(Vector2 square, Vector2 position)
    {
        return WireShape.Instance.DrawSquare(square, position, 30, Colors.Green, WireShape.TypeDraw.NORMAL);
    }
}