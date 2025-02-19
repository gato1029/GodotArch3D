using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems.Character;
internal class CharacterStateSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<CharacterStateComponent, CharacterAnimationComponent>();


    public CharacterStateSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }
    private struct JobQuery : IChunkJob
    {
        private readonly float _deltaTime;
        private CommandBuffer _commandBuffer;
        
        public JobQuery(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;        
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerCharacterAnimationComponent = ref chunk.GetFirst<CharacterAnimationComponent>();
            ref var pointerCharacterStateComponent = ref chunk.GetFirst<CharacterStateComponent>();

            ref var pointerAnimation = ref chunk.GetFirst<Animation>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);

                ref CharacterAnimationComponent characterAnimationComponent = ref Unsafe.Add(ref pointerCharacterAnimationComponent, entityIndex);
                ref CharacterStateComponent characterStateComponent = ref Unsafe.Add(ref pointerCharacterStateComponent, entityIndex);
                characterStateComponent.stateBehavior.Controller(entity, ref characterAnimationComponent, ref characterStateComponent, ref _commandBuffer);
            }

        }
    }
    public override void Update(in float t)
    {
        World.InlineParallelChunkQuery(in query, new JobQuery(commandBuffer, t));
        commandBuffer.Playback(World);
    }
}
