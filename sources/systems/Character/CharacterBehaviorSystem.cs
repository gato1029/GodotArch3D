using Arch.Buffer;
using Arch.Core;
using Arch.System;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems.Character;
internal class CharacterBehaviorSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<CharacterBehaviorComponent,CharacterComponent>();


    public CharacterBehaviorSystem(World world) : base(world)
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
            ref var pointerCharacterBehaviorComponent = ref chunk.GetFirst<CharacterBehaviorComponent>();
            ref var pointerCharacterComponent = ref chunk.GetFirst<CharacterComponent>();
                  
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);

                ref CharacterAnimationComponent characterAnimationComponent = ref Unsafe.Add(ref pointerCharacterAnimationComponent, entityIndex);
                ref CharacterBehaviorComponent characterBehaviorComponent = ref Unsafe.Add(ref pointerCharacterBehaviorComponent, entityIndex);
                ref CharacterComponent characterComponent = ref Unsafe.Add(ref pointerCharacterComponent, entityIndex);

                characterBehaviorComponent.characterBehavior.ControllerState(entity, ref characterComponent, ref characterAnimationComponent,ref characterBehaviorComponent, ref _commandBuffer);
                characterBehaviorComponent.characterBehavior.ControllerBehavior(entity,ref characterComponent, ref characterBehaviorComponent, ref _commandBuffer, _deltaTime);

            }

        }
    }
    public override void Update(in float t)
    {
        World.InlineParallelChunkQuery(in query, new JobQuery(commandBuffer, t));
        commandBuffer.Playback(World);
    }
}
