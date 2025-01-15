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
        private QueryDescription query = new QueryDescription().WithAll<StateComponent, Animation, BehaviorCharacter>();


        public StateSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }
        private struct JobQuery : IChunkJob
        {
            private readonly float _deltaTime;
            private CommandBuffer _commandBuffer;
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
                ref var pointerBehaviorCharacter = ref chunk.GetFirst<BehaviorCharacter>();
                ref var pointerStateComponent = ref chunk.GetFirst<StateComponent>();
             
                ref var pointerAnimation = ref chunk.GetFirst<Animation>();
            
                foreach (var entityIndex in chunk)
                {
                    ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                
                    ref Animation animation = ref Unsafe.Add(ref pointerAnimation, entityIndex);
                    ref StateComponent stateComponent = ref Unsafe.Add(ref pointerStateComponent, entityIndex);
                    ref BehaviorCharacter bc = ref Unsafe.Add(ref pointerBehaviorCharacter, entityIndex);

                    bc.stateBehavior.StateController(entity, ref animation, ref stateComponent, ref _commandBuffer);              
                }

            }
        }
        public override void Update(in float t)
        {
            World.InlineParallelChunkQuery(in query, new JobQuery(commandBuffer, t));
            commandBuffer.Playback(World);
        }
    }
}
