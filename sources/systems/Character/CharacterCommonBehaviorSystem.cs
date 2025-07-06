using Arch.Buffer;
using Arch.Core;
using Arch.System;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Behaviors.States;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Profiler;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Character;

internal class CharacterCommonBehaviorSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<CharacterCommonBehaviorComponent, CharacterComponent>();


    public CharacterCommonBehaviorSystem(World world) : base(world)
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
            ref var pointerCharacterCommonBehaviorComponent = ref chunk.GetFirst<CharacterCommonBehaviorComponent>();
            ref var pointerCharacterComponent = ref chunk.GetFirst<CharacterComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);

                ref CharacterAnimationComponent characterAnimationComponent = ref Unsafe.Add(ref pointerCharacterAnimationComponent, entityIndex);
                ref CharacterCommonBehaviorComponent characterCommonBehaviorComponent = ref Unsafe.Add(ref pointerCharacterCommonBehaviorComponent, entityIndex);
                ref CharacterComponent characterComponent = ref Unsafe.Add(ref pointerCharacterComponent, entityIndex);
                managers.Behaviors.BehaviorsInterface.ICharacterMoveBehavior moveBehavior = BehaviorManager.Instance.GetMoveBehavior(characterCommonBehaviorComponent.idMoveBehavior);
                managers.Behaviors.BehaviorsInterface.ICharacterStateBehavior stateBehavior =BehaviorManager.Instance.GetStateBehavior(characterCommonBehaviorComponent.idStateBehavior);
                managers.Behaviors.BehaviorsInterface.ICharacterAttackBehavior attackBehavior = BehaviorManager.Instance.GetAttackBehavior(characterCommonBehaviorComponent.idAttackBehavior);

                moveBehavior.ControllerBehavior(entity, ref characterComponent, ref characterCommonBehaviorComponent, ref _commandBuffer, _deltaTime);
                attackBehavior.ControllerBehavior(entity, ref characterComponent, ref characterCommonBehaviorComponent, ref _commandBuffer, _deltaTime);
                stateBehavior.ControllerState(entity, ref characterComponent, ref characterAnimationComponent, ref characterCommonBehaviorComponent, ref _commandBuffer);
             

            }

        }
    }
    public override void Update(in float t)
    {
        using (new ProfileScope("Behavior System"))
        {
            World.InlineParallelChunkQuery(in query, new JobQuery(commandBuffer, t));
            commandBuffer.Playback(World);
        }
    }
}