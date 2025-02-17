using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.States
{
    public class MushroomState : IStateBehavior
    {
        public string Name { get => "Hongo"; }

        public void StateController(Entity entity, ref Animation animation, ref StateComponent stateComponent, ref CommandBuffer commandBuffer)
        {
            switch (stateComponent.currentType)
            {
                case StateType.IDLE:
                    animation.updateAction = AnimationAction.IDLE_WEAPON;
                    break;
                case StateType.MOVING:
                    animation.updateAction = AnimationAction.WALK;
                    break;
                case StateType.EXECUTE_ATTACK:
                    stateComponent.currentType = StateType.IDLE;
                    break;
                case StateType.ATTACK:
                    animation.updateAction = AnimationAction.ATACK;
                    if (animation.currentAction == AnimationAction.ATACK && animation.complete)
                    {
                        stateComponent.currentType = StateType.EXECUTE_ATTACK;
                    }
                    break;
                case StateType.TAKE_HIT:
                    animation.updateAction = AnimationAction.HIT;
                    if (animation.currentAction == AnimationAction.HIT && animation.complete)
                    {
                        stateComponent.currentType = StateType.IDLE;
                    }
                    break;
                case StateType.TAKE_STUN:
                    animation.updateAction = AnimationAction.STUN;
                    if (animation.currentAction == AnimationAction.STUN && animation.complete)
                    {
                        stateComponent.currentType = StateType.IDLE;
                    }
                    break;
                case StateType.DIE:
                    animation.updateAction = AnimationAction.DEATH;
                    if (animation.currentAction == AnimationAction.DEATH && animation.complete)
                    {
                        stateComponent.currentType = StateType.DIE;
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
}
