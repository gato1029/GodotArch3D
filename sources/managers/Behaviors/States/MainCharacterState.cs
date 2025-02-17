using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.States
{
    internal class MainCharacterState : IStateBehavior
    {
        public string Name => "MainCharacter";

        public void StateController(Entity entity, ref Animation animation, ref StateComponent stateComponent, ref CommandBuffer commandBuffer)
        {
            Entity entityCharacter = default;
            ref Relationship<CharacterWeapon> parentOfRelation = ref entity.GetRelationships<CharacterWeapon>();
            foreach (var child in parentOfRelation)
            {
                entityCharacter = child.Key;
            }
            ref Animation animationCharacter = ref entityCharacter.Get<Animation>();

            switch (stateComponent.currentType)
            {
                case StateType.IDLE:
                    animation.updateAction = AnimationAction.IDLE_WEAPON;
                    animationCharacter.updateAction = AnimationAction.NONE;
                    break;
                case StateType.MOVING:
                    animation.updateAction = AnimationAction.WALK;
                    break;
                case StateType.EXECUTE_ATTACK:
                    stateComponent.currentType = StateType.IDLE;
                    animationCharacter.updateAction = AnimationAction.NONE;
                    break;
                case StateType.ATTACK:

                    animation.updateAction = AnimationAction.ATACK;
                    animationCharacter.updateAction = AnimationAction.IDLE_WEAPON;
                    if (animation.currentAction == AnimationAction.ATACK && animation.complete)
                    {
                        stateComponent.currentType = StateType.EXECUTE_ATTACK;
                    }
                    break;
                case StateType.TAKE_HIT:
                    //animation.updateAction = AnimationAction.HIT;
                    //if (animation.currentAction == AnimationAction.HIT && animation.complete)
                    //{
                    stateComponent.currentType = StateType.IDLE;
                    //}
                    break;
                case StateType.TAKE_STUN:
                    //animation.updateAction = AnimationAction.STUN;
                    //if (animation.currentAction == AnimationAction.STUN && animation.complete)
                    //{
                    stateComponent.currentType = StateType.IDLE;
                    //}
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
