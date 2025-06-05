using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
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
    public CharacterModelBaseData CharacterBaseData;
    public int healthBase;
    public int damageBase;
    public float speedAtackBase;    
    public AccessoryData[] accessoryArray;
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
        CharacterModelBaseData characterBaseData = CharacterLocalBase.Instance.GetCharacterBaseData(idCharacterBase);
        int idMaterial = 0;
        if (!multimeshMaterialDict.ContainsKey(characterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial))
        {
            idMaterial = characterBaseData.animationCharacterBaseData.animationDataArray[0].idMaterial;
            MultimeshMaterial multimeshMaterial = new MultimeshMaterial(MaterialManager.Instance.GetMaterial(idMaterial));
            multimeshMaterialDict.Add(idMaterial, multimeshMaterial);
        }


        Entity entity = EcsManager.Instance.World.Create();
        AddBase(entity, characterBaseData);
        AddRender(entity, characterBaseData, multimeshMaterialDict[idMaterial], positionInitial);
        AddMove(entity, positionInitial);
        AddAnimations(entity, characterBaseData);
        AddSoulCharacter(entity, characterBaseData);

        if (characterBaseData.animationCharacterBaseData.collisionBody != null || characterBaseData.animationCharacterBaseData.collisionMove != null)
        {
            AddColliderBody(entity, characterBaseData.animationCharacterBaseData, positionInitial);
        }

        
    }

    private void AddRender(Entity entity, CharacterModelBaseData characterBaseData, MultimeshMaterial multimeshMaterial, Vector2 positionInitial)
    {
        var inst = multimeshMaterial.CreateInstance();
        
        Transform3D transform = new Transform3D(Basis.Identity, Vector3.Zero);
        transform.Origin = new Vector3(positionInitial.X, positionInitial.Y, (positionInitial.Y * CommonAtributes.LAYER_MULTIPLICATOR) + 0);
        transform = transform.ScaledLocal(new Vector3(characterBaseData.scale, characterBaseData.scale, 1));
        entity.Add(new RenderGPUComponent { rid = inst.Item1, instance = inst.Item2, layerRender = (int)characterBaseData.animationCharacterBaseData.zOrderingOrigin, transform = transform, zOrdering = positionInitial.Y  } );
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
            entity.Add(new CharacterComponent { CharacterBaseData = baseData, damageBase = 10, healthBase = 10, speedAtackBase = 0.1f, accessoryArray = accessoryArray  });
        }
        else
        {
            entity.Add(new CharacterComponent { CharacterBaseData = baseData, damageBase = 10, healthBase = 10, speedAtackBase = 0.1f, accessoryArray = null });
        }                       
    }
    private void AddMove(Entity entity, Vector2 positionInitial)
    {
        entity.Add(new PositionComponent { position = positionInitial });
        entity.Add(new DirectionComponent { animationDirection = AnimationDirection.LEFT });
        entity.Add(new VelocityComponent { velocity = 5 });
        
    }

    private void AddSoulCharacter(Entity entity , CharacterModelBaseData characterBaseData)
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

 
 

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}
