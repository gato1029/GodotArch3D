using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Generic;
using GodotEcsArch.sources.systems;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
namespace GodotEcsArch.sources.managers.Characters;


[Component]
public struct CharacterComponent
{
    public CharacterBaseData CharacterBaseData;
    public int healthBase;
    public int damageBase;
}
[Component]
public struct CharacterColliderComponent { }

[Component]
public struct CharacterStateComponent
{
    public IStateCharacterBehavior stateBehavior;
    public int currentState;  // varia por cada logica de personaje
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
    public IAttackBehavior attackBehavior;
    public IMoveBehavior moveBehavior;
}

internal class CharacterCreatorManager:SingletonBase<CharacterCreatorManager>
{
    
    protected override void Initialize()
    {
        throw new NotImplementedException();
    }

    public void CreateNewCharacter(int idCharacterBase, Vector2 positionInitial)
    {
        CharacterBaseData characterBaseData = CharacterLocalBase.Instance.GetCharacterBaseData(idCharacterBase);
        Entity entity = EcsManager.Instance.World.Create();        
        AddBase(entity, characterBaseData);
        AddMove(entity, positionInitial);        
        AddAnimations(entity, characterBaseData);
        AddAnimationState(entity, characterBaseData);
        AddSoulCharacter(entity, characterBaseData.idCharacterBase);

        if (characterBaseData.collisionBody !=null || characterBaseData.collisionMove !=null)
        {
            AddColliderBody(entity, characterBaseData);
        }
    }

    private void AddAnimationState(Entity entity, CharacterBaseData characterBaseData)
    {
        entity.Add(new CharacterStateComponent { currentState = 0, stateBehavior = BehaviorManager.Instance.GetStateBehavior(1) }); // siempre inicia con el estado 0 que debe ser idle 
    }

    private void AddMove(Entity entity, Vector2 positionInitial)
    {
        entity.Add(new PositionComponent { x = positionInitial.X, y = positionInitial.Y });
        entity.Add(new DirectionComponent { animationDirection = AnimationDirection.LEFT });
    }

    private void AddSoulCharacter(Entity entity , int RootBase)
    {
        // aqui agregar el componente de comportamiento, Luego seria genial extenderlo a lua :)

        CharacterBehaviorComponent behaviorCharacterComponent;
        behaviorCharacterComponent.moveBehavior = BehaviorManager.Instance.GetMoveBehavior(1);
        behaviorCharacterComponent.attackBehavior = BehaviorManager.Instance.GetAttackBehavior(1);
        

        entity.Add(behaviorCharacterComponent);

    }
    private void AddAnimations(Entity entity, CharacterBaseData characterBaseData)
    {
        CharacterAnimationComponent characterAnimationComponent;
        characterAnimationComponent.stateAnimation = 0;
        characterAnimationComponent.currentFrame = 0;
        characterAnimationComponent.currentFrameIndex = 0;
        entity.Add<CharacterAnimationComponent>();
    }

    private void AddColliderBody(Entity entity, CharacterBaseData characterBaseData)
    {
        entity.Add<CharacterColliderComponent>();
    }

    private void AddBase(Entity entity, CharacterBaseData baseData)
    {
        entity.Add(new CharacterComponent { CharacterBaseData = baseData, damageBase =baseData.damageBase, healthBase = baseData.healthBase });
    }
 

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
