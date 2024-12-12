using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace GodotEcsArch.sources.systems
{
    public enum StateType
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
    public struct StateComponent
    {
        public StateType currentType;
    }

    internal class StateSystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;
        private QueryDescription query = new QueryDescription().WithAll<StateComponent, Animation>();


        public StateSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }
        private struct JobQuery : IChunkJob
        {
            private readonly float _deltaTime;
            private readonly CommandBuffer _commandBuffer;
            private readonly RandomNumberGenerator rng;
            public JobQuery(CommandBuffer commandBuffer, float deltaTime) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
                rng = new RandomNumberGenerator();
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);

                ref var pointerStateComponent = ref chunk.GetFirst<StateComponent>();
             
                ref var pointerAnimation = ref chunk.GetFirst<Animation>();
            
                foreach (var entityIndex in chunk)
                {
                    ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                
                    ref Animation animation = ref Unsafe.Add(ref pointerAnimation, entityIndex);
                    ref StateComponent stateComponent = ref Unsafe.Add(ref pointerStateComponent, entityIndex);

                    switch (stateComponent.currentType)
                    {
                        case StateType.IDLE:
                            animation.updateAction = AnimationAction.IDLE;
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
                                    _commandBuffer.Add<PendingRemove>(in entity);
                                }                                
                            }
                            break;
                        default:
                            break;
                    }

                }

            }
        }
        public override void Update(in float t)
        {
            if (RenderWindowGui.Instance.IsActive)
            {
                World.InlineParallelChunkQuery(in query, new JobQuery(commandBuffer, t));
                commandBuffer.Playback(World);
            }
        }
    }
}
