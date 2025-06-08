using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Behaviors.Move;
using GodotEcsArch.sources.managers.Behaviors.States;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
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
    public CharacterStateType characterStateType; 
    public CharacterModelBaseData CharacterBaseData;
    public int healthBase;
    public int damageBase;
    public float speedAtackBase;    
    public AccessoryData[] accessoryArray;
}
[Component]
public struct CharacterAtackComponent
{
    public bool isAttack;
}
[Component]
public struct CharacterUnitMovementFixedComponent
{
    public Vector2 nextDestination;
    public float radiusMovement;
    public Vector2 postionOrigin;
    public bool arriveDestination;
}
[Component]
public struct CharacterUnitSearchMovementComponent
{
    public Vector2 nextDestination;
    public float radiusSearch;
    public float radiusMovement;
    public Vector2 postionOrigin;
}
[Component]
public struct CharacterUnitSearchFollowComponent
{
    public float radiusSearch;
    public bool follow;
    public float postionOrigin;
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
public struct CharacterAnimationComponent
{    
    public Color currentframeData;

    public int stateAnimation;
    public int lastStateAnimation;
    public int currentFrameIndex;
    public float TimeSinceLastFrame;
    public float frameDuration; // aqui si lo almacenamos por que con ello manejaremos la velocidad de ataque    
    public bool animationComplete;    
    public bool active;

    public Color[] currentframeDataAccesorys;
}

[Component]
public struct CharacterBehaviorComponent
{       
    public ICharacterBehavior characterBehavior;        
}

[Component]
public struct CharacterCommonBehaviorComponent
{
  
    public ICharacterAttackBehavior attackBehavior;
    public ICharacterMoveBehavior moveBehavior;
    public ICharacterStateBehavior stateBehavior;
}

internal class CharacterCreatorManager:SingletonBase<CharacterCreatorManager>
{
    public Dictionary<int, MultimeshMaterial> multimeshMaterialDict;
    private int layerRenderCharacters = 10;
    protected override void Initialize()
    {
        multimeshMaterialDict = new Dictionary<int, MultimeshMaterial>();
    }

    public void CreateNewCharacter(int idCharacterBase, Vector2 positionInitial)
    {
        CharacterModelBaseData characterBaseData = CharacterLocalBase.Instance.GetCharacterBaseData(idCharacterBase);
        int idMaterial = 0;
        if (!multimeshMaterialDict.ContainsKey(characterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial))
        {
            idMaterial = characterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial;
            MultimeshMaterial multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(idMaterial));
            multimeshMaterialDict.Add(idMaterial, multimeshMaterial);
        }
        else
        {
            idMaterial = characterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial;
        }


        Entity entity = EcsManager.Instance.World.Create();
        AddBase(entity, characterBaseData);
        AddRender(entity, characterBaseData, multimeshMaterialDict[idMaterial], positionInitial);
        AddMove(entity, positionInitial, characterBaseData);
        AddAnimations(entity, characterBaseData);
        AddSoulCharacter(entity, characterBaseData, positionInitial);

        if (characterBaseData.animationCharacterBaseData.collisionBody != null || characterBaseData.animationCharacterBaseData.collisionMove != null)
        {
            AddColliderBody(entity, characterBaseData.animationCharacterBaseData, positionInitial);
        }

        
    }

    private void AddRender(Entity entity, CharacterModelBaseData characterBaseData, MultimeshMaterial multimeshMaterial, Vector2 positionInitial)
    {
        var inst = multimeshMaterial.CreateInstance();

        GeometricShape2D colliderB = characterBaseData.animationCharacterBaseData.collisionMove.Multiplicity(characterBaseData.scale);

        Vector2 originOffset = colliderB.OriginCurrent;

        Transform3D transform = new Transform3D(Basis.Identity, Vector3.Zero);
        transform.Origin = new Vector3(positionInitial.X, positionInitial.Y, (positionInitial.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        transform = transform.ScaledLocal(new Vector3(characterBaseData.scale, characterBaseData.scale, 1));
        entity.Add(new RenderGPUComponent { rid = inst.Item1, instance = inst.Item2, layerRender = layerRenderCharacters, originOffset = originOffset, transform = transform, zOrdering = (int)characterBaseData.animationCharacterBaseData.zOrderingOrigin } );


        if (characterBaseData.animationCharacterBaseData.hasCompositeAnimation)
        {
            GpuInstance[] instancedLinked = new GpuInstance[Enum.GetNames(typeof(AccesoryAvatarType)).Length];
            instancedLinked[(int)AccesoryAvatarType.WEAPON] = AccesoryAvatarManager.Instance.ChangueAccesory(AccesoryAvatarType.WEAPON, 2);
            for (int i = 1; i < instancedLinked.Length; i++)
            {
                instancedLinked[i] = default;
            }

            //crear cada rid
            entity.Add(new RenderGPULinkedComponent { instancedLinked = instancedLinked });
        }

    }

    private void AddBase(Entity entity, CharacterModelBaseData baseData)
    {
        if (baseData.animationCharacterBaseData.hasCompositeAnimation)
        {          
            AccessoryData[] accessoryArray = new AccessoryData[Enum.GetNames(typeof(AccesoryAvatarType)).Length];
            accessoryArray[(int)AccesoryAvatarType.WEAPON] = AccesoryManager.Instance.GetAccesory(2);
            entity.Add(new CharacterComponent { CharacterBaseData = baseData, damageBase = 10, healthBase = 10, speedAtackBase = 0.1f, accessoryArray = accessoryArray, characterStateType = CharacterStateType.IDLE  });
        }
        else
        {
            entity.Add(new CharacterComponent { CharacterBaseData = baseData, damageBase = 10, healthBase = 100, speedAtackBase = 0.0f, accessoryArray = null, characterStateType = CharacterStateType.IDLE });
        }                       
    }
    private void AddMove(Entity entity, Vector2 positionInitial, CharacterModelBaseData characterBaseData)
    {
        float velocity = 3;
        foreach (var item in characterBaseData.bonusDataArray)
        {
            switch (item.type)
            {
                case BonusType.DURABILITY:
                    break;
                case BonusType.VELOCITY_ATTACK:
                    break;
                case BonusType.SPACE_BAG:
                    break;
                case BonusType.VELOCITY_MOVE:
                    velocity = item.value;
                    break;
                default:
                    break;
            }
        }
        

        entity.Add(new PositionComponent { position = positionInitial });
        entity.Add(new DirectionComponent { animationDirection = AnimationDirection.LEFT });
        entity.Add(new VelocityComponent { velocity = 5 });
        
    }

    private void AddSoulCharacter(Entity entity , CharacterModelBaseData characterBaseData, Vector2 positionInitial)
    {
        switch (characterBaseData.characterBehaviorType)
        {
            case CharacterBehaviorType.NINGUNO:
                break;
            case CharacterBehaviorType.PERSONAJE_PRINCIPAL:
                CharacterBehaviorComponent behaviorCharacterComponent;
                CharacterAtackComponent characterAtackComponent = new CharacterAtackComponent { isAttack = false };
                behaviorCharacterComponent.characterBehavior = BehaviorManager.Instance.GetBehavior(1);
                entity.Add(behaviorCharacterComponent);
                entity.Add(characterAtackComponent);
                break;
            case CharacterBehaviorType.GENERICO:
               
                AddMoveType(entity,  characterBaseData, positionInitial);
                break;
            default:
                break;
        }        
    }

    private void AddMoveType(Entity entity, CharacterModelBaseData characterBaseData, Vector2 positionInitial)
    {

        switch (characterBaseData.unitMoveType)
        {
            case UnitMoveType.ALERTA: //No se mueve pero puede atacar desde su punto fijo
                break;
            case UnitMoveType.MOVIMIENTO_RADIO_FIJO: // se mueve en un radio fijo, basando su punto de origen
                CharacterCommonBehaviorComponent characterCommonBehaviorComponent;
                characterCommonBehaviorComponent.moveBehavior = BehaviorManager.Instance.GetMoveBehavior<MoveRadiusCharacter2D>();
                characterCommonBehaviorComponent.stateBehavior = BehaviorManager.Instance.GetStateBehavior<CommonState2D>();
                characterCommonBehaviorComponent.attackBehavior = null;
                entity.Add(characterCommonBehaviorComponent);

                CharacterUnitMovementFixedComponent characterUnitSearchFixedComponent = new CharacterUnitMovementFixedComponent
                {
                    nextDestination = Vector2.Zero, postionOrigin = positionInitial, radiusMovement = characterBaseData.unitMoveData.radiusMove, arriveDestination = true
                };
                entity.Add(characterUnitSearchFixedComponent);
                break;
            case UnitMoveType.BUSQUEDA_RADIO_FIJO:
                break;
            case UnitMoveType.MOVIMIENTO_RADIO_VARIABLE:
                break;
            case UnitMoveType.BUSQUEDA_RADIO_VARIBLE:
                break;
            default:
                break;
        }
    }

    private void AddAnimations(Entity entity, CharacterModelBaseData characterBaseData)
    {
        CharacterAnimationComponent characterAnimationComponent = default;
        if (characterBaseData.animationCharacterBaseData.hasCompositeAnimation)
        {
            characterAnimationComponent.currentframeData = new Color();
            characterAnimationComponent.currentframeDataAccesorys = new Color[Enum.GetNames(typeof(AccesoryAvatarType)).Length];
            
        }
        else
        {
            characterAnimationComponent.currentframeData = new Color();
        }

        characterAnimationComponent.stateAnimation = 0;
        characterAnimationComponent.lastStateAnimation = -1;
        characterAnimationComponent.currentFrameIndex = 0;
        characterAnimationComponent.active = true;
        characterAnimationComponent.frameDuration = 0;
        
        characterAnimationComponent.animationComplete = false;
        characterAnimationComponent.TimeSinceLastFrame = 0;

        entity.Add<CharacterAnimationComponent>(characterAnimationComponent);
    }

    private void AddColliderBody(Entity entity, AnimationCharacterBaseData characterBaseData, Vector2 positionInitial)
    {
        entity.Add<CharacterColliderComponent>();
        CollisionManager.Instance.characterCollidersEntities.AddUpdateItem(positionInitial, entity);
    }

    public void RemoveCharacter(Entity entity, CharacterComponent characterComponent, Rid rid, int instance)
    {
        int idMaterial = characterComponent.CharacterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial;

        CharacterCreatorManager.Instance.multimeshMaterialDict[idMaterial].FreeInstance(rid, instance);
        RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Color(-1, -1, -1, -1));

        CollisionManager.Instance.characterCollidersEntities.RemoveItem(entity.Reference());
    }
 

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
