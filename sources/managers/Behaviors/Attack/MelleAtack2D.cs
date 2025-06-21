using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.systems;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.Attack;
public class MelleAtack2D : ICharacterAttackBehavior
{
    public void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterCommonBehaviorComponent characterCommonBehaviorComponent , ref CommandBuffer commandBuffer, float delta)
    {
        ref CharacterUnitMovementFixedComponent unitMovementComponent = ref entity.TryGetRef<CharacterUnitMovementFixedComponent>(out bool exist);
        ref DirectionComponent directionComponent = ref entity.Get<DirectionComponent>();
        ref PositionComponent positionComponent = ref entity.Get<PositionComponent>();
        
        if ((characterComponent.characterStateType == CharacterStateType.IDLE || characterComponent.characterStateType == CharacterStateType.MOVING ) && AtackIsPosible(entity, positionComponent, directionComponent, characterComponent))
        {
            characterComponent.characterStateType = CharacterStateType.ATTACK;            
        }
        else
        {
            if (characterComponent.characterStateType == CharacterStateType.EXECUTE_ATTACK)
            {
                ExecuteAtack(entity, positionComponent, directionComponent, characterComponent);
            }
        }
    }

    private void ExecuteAtack(Entity entity, PositionComponent positionComponent, DirectionComponent directionComponent, CharacterComponent characterComponent)
    {
        var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
        var animacionData = dataCharacterModel.animationCharacterBaseData.animationDataArray[2].animationData[(int)directionComponent.animationDirection];
      
        if (animacionData.hasCollider)
        {
          
            GeometricShape2D collision = animacionData.collider.Multiplicity(dataCharacterModel.scale);

            Vector2 positionRelative = positionComponent.position + collision.OriginCurrent;

            Rect2 aabb = new Rect2(positionRelative, collision.GetSizeQuad() * 2);
            Dictionary<int, Dictionary<int, Entity>> data = CollisionManager.Instance.characterCollidersEntities.QueryAABB(aabb);
            if (data != null)
            {
                foreach (var item in data.Values)
                {
                    foreach (var itemInternal in item)
                    {
                        Entity entityObjetive = itemInternal.Value;
                        if (entityObjetive.Id != entity.Id)
                        {

                            ref CharacterComponent characterComponentB = ref itemInternal.Value.TryGetRef<CharacterComponent>(out bool exist);
                            var dataCharacterModelB = CharacterModelManager.Instance.GetCharacterModel(characterComponentB.idCharacterBaseData);

                            AnimationCharacterBaseData characterB = dataCharacterModelB.animationCharacterBaseData;
                            GeometricShape2D colliderB = characterB.collisionBody.Multiplicity(dataCharacterModelB.scale);
                            var positionB = itemInternal.Value.Get<PositionComponent>().position + colliderB.OriginCurrent;

                            if (Collision2D.Collides(collision, colliderB, positionRelative, positionB))
                            {
                                characterComponentB.characterStateType = CharacterStateType.TAKE_HIT;
                                BehaviorManager.Instance.AplyDamageCharacter(entity, entityObjetive);
                            }
                        }
                    }

                }
            }

        }
    }

    private bool AtackIsPosible(Entity entity, PositionComponent positionComponent, DirectionComponent directionComponent, CharacterComponent characterComponent)
    {
        var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
        var animacionData = dataCharacterModel.animationCharacterBaseData.animationDataArray[2].animationData[(int)directionComponent.animationDirection];        
        GeometricShape2D collision = animacionData.collider.Multiplicity(dataCharacterModel.scale);

        CharacterUnitComponent unit = entity.Get<CharacterUnitComponent>();

        Vector2 posAtack = positionComponent.position + collision.OriginCurrent;
    

        Rect2 aabb = new Rect2(posAtack, collision.GetSizeQuad() * 2);
        Dictionary<int, Dictionary<int, Entity>> data = CollisionManager.Instance.characterCollidersEntities.QueryAABB(aabb);
        if (data != null)
        {
            foreach (var item in data.Values)
            {
                foreach (var itemInternal in item)
                {
                    Entity entB = itemInternal.Value;
                    if (itemInternal.Value.Id != entity.Id && unit.team != entB.Get<CharacterUnitComponent>().team)
                    {
                        ref CharacterComponent characterComponentB = ref itemInternal.Value.TryGetRef<CharacterComponent>(out bool exist1);
                        var dataCharacterModelB = CharacterModelManager.Instance.GetCharacterModel(characterComponentB.idCharacterBaseData);

                        AnimationCharacterBaseData characterB = dataCharacterModelB.animationCharacterBaseData;
                        GeometricShape2D colliderB = characterB.collisionBody.Multiplicity(dataCharacterModelB.scale);
                        var positionB = itemInternal.Value.Get<PositionComponent>().position + colliderB.OriginCurrent;

                        if (Collision2D.Collides(collision, colliderB, posAtack, positionB))
                        {
                            return true;
                        }

                    }
                }
            }

        }
        return false;
    }
}
