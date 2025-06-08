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
    public void ControllerState(Entity entity, ref CharacterComponent characterComponent, ref CharacterAnimationComponent animation, ref CharacterCommonBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer)
    {
        switch (characterComponent.characterStateType)
        {
            case CharacterStateType.IDLE:
                animation.stateAnimation = 0;
                break;
            case CharacterStateType.MOVING:
                animation.stateAnimation = 1;
                break;
            case CharacterStateType.EXECUTE_ATTACK:
                characterComponent.characterStateType = CharacterStateType.IDLE;
                break;
            case CharacterStateType.ATTACK:
                animation.stateAnimation = 2;
                if (animation.animationComplete)
                {
                    characterComponent.characterStateType = CharacterStateType.EXECUTE_ATTACK;
                }
                break;
            case CharacterStateType.TAKE_HIT:
                animation.stateAnimation = 4;  
                if (animation.animationComplete)
                {                  
                    characterComponent.characterStateType = CharacterStateType.IDLE;
                }                                                
                break;
            case CharacterStateType.TAKE_STUN:
                animation.stateAnimation = 5;
                if (animation.animationComplete)
                {
                    characterComponent.characterStateType = CharacterStateType.IDLE;
                }
                break;
            case CharacterStateType.DIE:
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
