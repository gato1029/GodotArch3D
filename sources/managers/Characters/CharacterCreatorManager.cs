using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;
namespace GodotEcsArch.sources.managers.Characters;

public enum CharacterStateType
{
    IDLE,
    MOVING,
    EXECUTE_ATTACK,
    ATTACK,
    TAKE_HIT,
    TAKE_STUN,
    DIE
}




[Component]
public struct CharacterComponent
{
    public CharacterBaseData CharacterBaseData;
    public int healthBase;
    public int damageBase;
}
[Component]
public struct CharacterAccesoriesComponent
{    
    public int idWeapon;
    public int idArmor;
}

[Component]
public struct CharacterColliderComponent { }

[Component]
public struct CharacterAnimationCompositeComponent
{

}

[Component]
public struct CharacterAnimationComponent
{    
    public int stateAnimation;
    public int currentFrame;
    public int currentFrameIndex;
    public float TimeSinceLastFrame;
    public float frameDuration; // aqui si lo almacenamos por que con ello manejaremos la velocidad de ataque
    
    public bool animationComplete;
    public int  horizontalMirror;

    public bool active;
}

[Component]
public struct CharacterBehaviorComponent
{    
    public CharacterStateType characterStateType;  // varia por cada logica de personaje
    public ICharacterBehavior characterBehavior;        
}

[Component]
public struct CharacterCustomBehaviorComponent
{    
    public CharacterStateType characterStateType;  // varia por cada logica de personaje
    public ICharacterBehavior characterBehavior;
    public IAttackBehavior attackBehavior;
    public IMoveBehavior moveBehavior;
}

internal class CharacterCreatorManager:SingletonBase<CharacterCreatorManager>
{
    Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
    protected override void Initialize()
    {
        multimeshMaterialDict = new Dictionary<int, MultimeshMaterial>();
    }

    public void CreateNewCharacter(int idCharacterBase, Vector2 positionInitial)
    {

        CharacterBaseData characterBaseData = CharacterLocalBase.Instance.GetCharacterBaseData(idCharacterBase);
       
        if (!multimeshMaterialDict.ContainsKey(characterBaseData.idMaterial))
        {
            MultimeshMaterial multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(characterBaseData.idMaterial));
            multimeshMaterialDict.Add(characterBaseData.idMaterial, multimeshMaterial);
        }
        

        Entity entity = EcsManager.Instance.World.Create();        
        AddBase(entity, characterBaseData);
        AddRender(entity, characterBaseData, multimeshMaterialDict[characterBaseData.idMaterial], positionInitial);
        AddMove(entity, positionInitial);        
        AddAnimations(entity, characterBaseData);        
        AddSoulCharacter(entity, characterBaseData);

        if (characterBaseData.collisionBody !=null || characterBaseData.collisionMove !=null)
        {
            AddColliderBody(entity, characterBaseData, positionInitial);
        }
    }

    private void AddRender(Entity entity, CharacterBaseData characterBaseData, MultimeshMaterial multimeshMaterial, Vector2 positionInitial)
    {
        var inst = multimeshMaterial.CreateInstance();
        
        Transform3D transform = new Transform3D(Basis.Identity, Vector3.Zero);
        transform.Origin = new Vector3(positionInitial.X, positionInitial.Y, (positionInitial.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        entity.Add(new RenderGPUComponent { rid = inst.Item1, instance = inst.Item2, layerRender =0, transform = transform, zOrdering = characterBaseData.collisionMove.OriginCurrent.Y } );
    }

    private void AddBase(Entity entity, CharacterBaseData baseData)
    {
        entity.Add(new CharacterComponent { CharacterBaseData = baseData, damageBase = 10, healthBase = 10 });
       
    }
    private void AddMove(Entity entity, Vector2 positionInitial)
    {
        entity.Add(new PositionComponent { position = positionInitial });
        entity.Add(new DirectionComponent { animationDirection = AnimationDirection.LEFT });
        entity.Add(new VelocityComponent { velocity = 5 });
        
    }

    private void AddSoulCharacter(Entity entity , CharacterBaseData characterBaseData)
    {
        // aqui agregar el componente de comportamiento, Luego seria genial extenderlo a lua :)
        //if (characterBaseData.attackIDBehavior!=0|| characterBaseData.moveIDBehavior != 0)
        //{
        //    CharacterCustomBehaviorComponent characterCustomBehaviorComponent;

        //    characterCustomBehaviorComponent.characterStateType = CharacterStateType.IDLE;
        //    characterCustomBehaviorComponent.characterBehavior = BehaviorManager.Instance.GetBehavior(1);

        //    // me falta completar este
        //}
        //else
        {
            CharacterBehaviorComponent behaviorCharacterComponent;
            behaviorCharacterComponent.characterStateType = CharacterStateType.IDLE;
            behaviorCharacterComponent.characterBehavior = BehaviorManager.Instance.GetBehavior(1);
            entity.Add(behaviorCharacterComponent);
        }        
    }
    private void AddAnimations(Entity entity, CharacterBaseData characterBaseData)
    {
        CharacterAnimationComponent characterAnimationComponent;
        characterAnimationComponent.stateAnimation = 0;
        characterAnimationComponent.currentFrame = 0;
        characterAnimationComponent.currentFrameIndex = 0;
        characterAnimationComponent.active = true;
        characterAnimationComponent.frameDuration = 0;
        characterAnimationComponent.horizontalMirror = 0;
        characterAnimationComponent.animationComplete = false;
        characterAnimationComponent.TimeSinceLastFrame = 0;

        entity.Add<CharacterAnimationComponent>(characterAnimationComponent);
    }

    private void AddColliderBody(Entity entity, CharacterBaseData characterBaseData, Vector2 positionInitial)
    {
        entity.Add<CharacterColliderComponent>();
        CollisionManager.Instance.characterCollidersEntities.AddUpdateItem(positionInitial, entity);
    }

 
 

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
