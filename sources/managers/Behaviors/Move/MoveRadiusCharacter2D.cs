using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.systems;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.SpringBoneSimulator3D;
using static Godot.TextServer;

namespace GodotEcsArch.sources.managers.Behaviors.Move;
public class MoveRadiusCharacter2D : ICharacterMoveBehavior
{
    public void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterCommonBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer, float delta)
    {
        ref CharacterUnitMovementFixedComponent unitMovementComponent = ref entity.TryGetRef<CharacterUnitMovementFixedComponent>(out bool exist);
        ref DirectionComponent directionComponent  = ref entity.Get<DirectionComponent>();
        ref PositionComponent positionComponent = ref entity.Get<PositionComponent>();

        if (unitMovementComponent.arriveDestination)
        {
            unitMovementComponent.nextDestination = CommonOperations.NewPointInCircle(unitMovementComponent.postionOrigin, unitMovementComponent.radiusMovement);
            unitMovementComponent.arriveDestination = false;

            Vector2 targetDirection = (unitMovementComponent.nextDestination - positionComponent.position).Normalized();

            directionComponent.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(targetDirection);
            directionComponent.value = targetDirection; 
            directionComponent.normalized = new Vector2(Math.Sign(targetDirection.X), Math.Sign(targetDirection.Y));
        }
        else
        {
            if (characterComponent.characterStateType != CharacterStateType.ATTACK &&characterComponent.characterStateType != CharacterStateType.TAKE_HIT && characterComponent.characterStateType != CharacterStateType.EXECUTE_ATTACK && characterComponent.characterStateType != CharacterStateType.DIE && characterComponent.characterStateType != CharacterStateType.TAKE_STUN)
            {
                Move(entity, ref characterComponent, ref unitMovementComponent, ref positionComponent, ref directionComponent, ref characterBehaviorComponent, delta);
            }
            
        }
    }

    private void Move(Entity entity, ref CharacterComponent characterComponent, ref CharacterUnitMovementFixedComponent unitMovementComponent, ref PositionComponent positionComponent, ref DirectionComponent directionComponent, ref CharacterCommonBehaviorComponent characterBehaviorComponent, float delta)
    {
        ref VelocityComponent velocityComponent = ref entity.Get<VelocityComponent>();
        var dataModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
        GeometricShape2D collisionMove = dataModel.animationCharacterBaseData.collisionMove.Multiplicity(dataModel.scale);
     
        Vector2 movement = directionComponent.value * velocityComponent.velocity * delta;
        Vector2 pointNextCollision = positionComponent.position + movement + collisionMove.OriginCurrent;

        bool existCollision = CollisionManager.CheckAnyCollision(entity, pointNextCollision, collisionMove);
      
        if (!existCollision)
        {

            float distanceToTarget = (unitMovementComponent.nextDestination - positionComponent.position).Length();
            float movementDistance = velocityComponent.velocity * delta;
            if (movementDistance >= distanceToTarget)
            {
                unitMovementComponent.arriveDestination = true;
                positionComponent.position = unitMovementComponent.nextDestination;
                characterComponent.characterStateType = CharacterStateType.IDLE;
            }
            else
            {
                characterComponent.characterStateType = CharacterStateType.MOVING;
                positionComponent.position += movement;
            }
        }
        else
        {
            characterComponent.characterStateType = CharacterStateType.IDLE;
        }
    }
}
