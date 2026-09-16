using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.States;
public class CommonState2D : ICharacterStateBehavior
{
    public void ControllerState(Entity entity, ref CharacterComponent characterComponent, ref CharacterAnimationComponent animation, ref CharacterCommonBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer, float deltaTime)
    {
        switch (characterComponent.characterStateType)
        {
            case StateType.IDLE:
                animation.stateAnimation = 0;
                break;
            case StateType.MOVING:
                animation.stateAnimation = 1;
                break;
            case StateType.EXECUTE_ATTACK:
                characterComponent.characterStateType = StateType.IDLE;
                break;
            case StateType.ATTACK:
                animation.stateAnimation = 2;
                if (animation.animationComplete)
                {
                    characterComponent.characterStateType = StateType.EXECUTE_ATTACK;
                }
                break;
            case StateType.TAKE_HIT:
                animation.stateAnimation = 4;
                //if (animation.animationComplete)
                //{                  
                //    characterComponent.characterStateType = CharacterStateType.IDLE;
                //}
                characterComponent.hitStunTimer -= deltaTime; // o tu delta global

                // Si ya pasó el tiempo de "stun", volvemos a IDLE
                if (characterComponent.hitStunTimer <= 0f)
                {
                    characterComponent.characterStateType = StateType.IDLE;
                }
                break;
            case StateType.TAKE_STUN:
                animation.stateAnimation = 5;
                if (animation.animationComplete)
                {
                    characterComponent.characterStateType = StateType.IDLE;
                }
                break;
            case StateType.DIE:
                animation.stateAnimation = 3;
                if (animation.animationComplete)
                {
                    if (!entity.Has<PendingRemove>())
                    {
                        commandBuffer.Add<PendingRemove>(in entity);
                    }
                }                
                break;
            default:
                break;
        }
    }
}
