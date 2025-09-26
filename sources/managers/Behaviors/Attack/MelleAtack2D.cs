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
    public void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterCommonBehaviorComponent characterCommonBehaviorComponent , ref CommandBuffer commandBuffer, float delta, int batchIndex, int numBatches)
    {
        ref CharacterUnitMovementFixedComponent unitMovementComponent = ref entity.TryGetRef<CharacterUnitMovementFixedComponent>(out bool exist);
        ref DirectionComponent directionComponent = ref entity.Get<DirectionComponent>();
        ref PositionComponent positionComponent = ref entity.Get<PositionComponent>();
        
        if ((characterComponent.characterStateType == CharacterStateType.IDLE || characterComponent.characterStateType == CharacterStateType.MOVING ) && AtackIsPosible(entity, positionComponent, directionComponent, characterComponent, characterCommonBehaviorComponent,delta, batchIndex,numBatches))
        {
            characterComponent.characterStateType = CharacterStateType.ATTACK;            
        }
        else
        {
            if (characterComponent.characterStateType == CharacterStateType.EXECUTE_ATTACK)
            {
                ExecuteAtack(entity, positionComponent, directionComponent, characterComponent, ref characterCommonBehaviorComponent);
            }
        }
    }

    private void ExecuteAtack(Entity entity, PositionComponent positionComponent, DirectionComponent directionComponent, CharacterComponent characterComponent, ref CharacterCommonBehaviorComponent characterCommonBehaviorComponent)
    {
        var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
        var animacionData = dataCharacterModel.animationCharacterBaseData.animationDataArray[2].animationData[(int)directionComponent.animationDirection];

        if (!animacionData.hasCollider) return;

        TeamComponent team = entity.Get<TeamComponent>();
        ColliderComponent colliderComponent = entity.Get<ColliderComponent>();

        // 1️⃣ Collider de ataque (hitbox del frame actual de animación)
        GeometricShape2D attackCollider = animacionData.collider.Multiplicity(dataCharacterModel.scale);
        Vector2 attackPos = positionComponent.position + attackCollider.OriginCurrent;

        // 2️⃣ Broad-phase: solo candidatos que caen en este AABB
        Rect2 attackAABB = new Rect2(attackPos - (attackCollider.GetSizeQuad() / 2), attackCollider.GetSizeQuad());

        var candidates = CollisionManager.Instance.characterCollidersEntities
            .GetCollidingOwnersInAABBExternal(dataCharacterModel.collisionBody, attackAABB, colliderComponent.idCollider);

        // 3️⃣ Narrow-phase: chequeo exacto contra cada candidato
        foreach (var candidate in candidates)
        {
            if (candidate.Id == entity.Id) continue; // No me golpeo a mí mismo

            TeamComponent teamB = candidate.Get<TeamComponent>();
            if (teamB.team == team.team) continue; // No atacar aliados

            ref CharacterComponent targetChar = ref candidate.TryGetRef<CharacterComponent>(out bool existTarget);
            if (!existTarget) continue;

            var targetModel = CharacterModelManager.Instance.GetCharacterModel(targetChar.idCharacterBaseData);
            GeometricShape2D targetCollider = targetModel.collisionBody;
            Vector2 targetPos = candidate.Get<PositionComponent>().position + targetCollider.OriginCurrent;

            // ✅ Narrow-phase exacta
            if (Collision2D.Collides(attackCollider, targetCollider, attackPos, targetPos))
            {
                targetChar.characterStateType = CharacterStateType.TAKE_HIT;              
                BehaviorManager.Instance.AplyDamageCharacter(entity, candidate);
            }
        }
    }
    private bool AtackIsPosible(
    Entity entity,
    PositionComponent positionComponent,
    DirectionComponent directionComponent,
    CharacterComponent characterComponent,
    CharacterCommonBehaviorComponent characterBehaviorComponent,
    float delta,
    int batchIndex,
    int numBatches)
    {
        ColliderComponent colliderComponent = entity.Get<ColliderComponent>();
        int batchId = (colliderComponent.idCollider % numBatches);

        // ⚡ Si ya estoy ejecutando ataque → no necesito volver a chequear
        if (characterComponent.characterStateType == CharacterStateType.EXECUTE_ATTACK)
            return false;

        // Cooldown para limitar frecuencia de chequeos
        if (characterBehaviorComponent.collisionAttackCheckCooldown > 0f || batchId != batchIndex)
        {
            characterBehaviorComponent.collisionAttackCheckCooldown -= delta;
            return false;
        }

        // Reinicio cooldown fijo (ej: 50ms entre chequeos)
        characterBehaviorComponent.collisionAttackCheckCooldown = 0.05f;

        // 🔹 Broad-phase con AABB
        var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
        var animacionData = dataCharacterModel.animationCharacterBaseData.animationDataArray[2].animationData[(int)directionComponent.animationDirection];
        GeometricShape2D collision = animacionData.collider.Multiplicity(dataCharacterModel.scale);

        TeamComponent team = entity.Get<TeamComponent>();
        Vector2 posAttack = positionComponent.position + collision.OriginCurrent;

        Rect2 aabb = new Rect2(posAttack - (collision.GetSizeQuad() / 2), collision.GetSizeQuad());

        var candidates = CollisionManager.Instance.characterCollidersEntities
            .GetCollidingOwnersInAABBExternal(dataCharacterModel.collisionBody, aabb, colliderComponent.idCollider);

        foreach (var candidate in candidates)
        {
            if (candidate.Id == entity.Id) continue; // ignorar self

            TeamComponent teamB = candidate.Get<TeamComponent>();
            if (teamB.team == team.team) continue; // ignorar aliados

            // ✅ Con broad-phase basta, no hacemos colisión exacta aquí
            return true;
        }

        return false;
    }

}
