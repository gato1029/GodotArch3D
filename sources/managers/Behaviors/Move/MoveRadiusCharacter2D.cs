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
    public void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterCommonBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer, float delta, int batchIndex, int numBatches)
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
                Move(entity, ref characterComponent, ref unitMovementComponent, ref positionComponent, ref directionComponent, ref characterBehaviorComponent, delta, batchIndex,numBatches);
            }
            
        }
    }

    private void Move(Entity entity, ref CharacterComponent characterComponent, ref CharacterUnitMovementFixedComponent unitMovementComponent, ref PositionComponent positionComponent, ref DirectionComponent directionComponent, ref CharacterCommonBehaviorComponent characterBehaviorComponent, float delta, int batchIndex, int numBatches)
    {
        ref VelocityComponent velocityComponent = ref entity.Get<VelocityComponent>();
        ColliderComponent colliderComponent = entity.Get<ColliderComponent>();
        var dataModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
        GeometricShape2D collisionMove = dataModel.collisionMove;

        Vector2 startPos = positionComponent.lastPosition;   // ← NUEVO
        Vector2 movement = directionComponent.value * velocityComponent.velocity * delta;
        Vector2 endPos = positionComponent.position + movement;
        Vector2 pointNextCollision = positionComponent.position + movement + collisionMove.OriginCurrent;

        bool existCollision = false;

        //-------------------------
        // 🔹 Estrategia híbrida:
        // 1) Batching → repartimos entidades por lotes
        // 2) Cooldown → no chequeamos colisión cada frame
        //-------------------------        
        // Asigna batch según el Id del collider (o entity.Id si prefieres)
        int batchId = (colliderComponent.idCollider % numBatches);

        // Cooldown local para cada unidad
        if (characterBehaviorComponent.collisionCheckCooldown <= 0f && batchId == batchIndex)
        {
            //existCollision = CollisionManager.CheckAnyCollisionMoveUnitOnly(entity, pointNextCollision, collisionMove);
            if (!existCollision)
            {
                existCollision = CollisionManager.CheckSegmentCollision(startPos, endPos, collisionMove);
            }
            characterBehaviorComponent.collisionCheckCooldown = 0.05f; // 50ms entre checks por unidad
        }
        else
        {
            characterBehaviorComponent.collisionCheckCooldown -= delta;
        }
        // Estáticos: siempre se chequean (más barato porque son pocos y no cambian)
        if (!existCollision)
        {
            existCollision = CollisionManager.CheckAnyCollisionStatic(entity, pointNextCollision, collisionMove);
        }
        if (!existCollision)
        {

            float distanceToTarget = (unitMovementComponent.nextDestination - positionComponent.position).Length();
            float movementDistance = velocityComponent.velocity * delta;
            if (movementDistance >= distanceToTarget)
            {
                unitMovementComponent.arriveDestination = true;
                positionComponent.lastPosition = positionComponent.position;   // ← GUARDAR antes de mover
                positionComponent.position = unitMovementComponent.nextDestination;
                characterComponent.characterStateType = CharacterStateType.IDLE;
            }
            else
            {
                characterComponent.characterStateType = CharacterStateType.MOVING;
                positionComponent.lastPosition = positionComponent.position;   // ← GUARDAR antes de mover
                positionComponent.position += movement;
            }
        }
        else
        {
            characterComponent.characterStateType = CharacterStateType.IDLE;
        }
    }
}
