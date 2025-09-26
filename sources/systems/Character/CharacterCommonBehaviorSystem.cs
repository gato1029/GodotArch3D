using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Behaviors.States;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Profiler;
using System;
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
        private readonly int _batchIndex;
        private readonly int _numBatches;
        public JobQuery(CommandBuffer commandBuffer, float deltaTime, int batchIndex, int numBatches) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            _batchIndex = batchIndex;
            _numBatches = numBatches;
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

                moveBehavior.ControllerBehavior(entity, ref characterComponent, ref characterCommonBehaviorComponent, ref _commandBuffer, _deltaTime,_batchIndex,_numBatches);
                attackBehavior.ControllerBehavior(entity, ref characterComponent, ref characterCommonBehaviorComponent, ref _commandBuffer, _deltaTime, _batchIndex, _numBatches);
                stateBehavior.ControllerState(entity, ref characterComponent, ref characterAnimationComponent, ref characterCommonBehaviorComponent, ref _commandBuffer, _deltaTime);
             

            }

        }
    }
    private int batchIndex = 0;
    private float batchTimer = 0f;
    private const float batchInterval = 0.02f; // cada 20ms rota batch
    private const int TargetUpdatesPerFrame = 600; // cuántos colliders máximo por frame

    private int NumBatches = 1;

    public override void Update(in float t)
    {
        using (new ProfileScope("Behavior System"))
        {
            // 🔹 calcular cuántos batches necesito
            int totalColliders = CollisionManager.Instance.characterCollidersEntities.Count;
            NumBatches = Math.Max(1, (int)Math.Ceiling(totalColliders / (float)TargetUpdatesPerFrame));
          
            // 🔹 avanzar el índice de batch con deltaTime
            batchTimer += t;
            if (batchTimer >= batchInterval)
            {
                batchTimer -= batchInterval;
                batchIndex = (batchIndex + 1) % NumBatches;
            }

            World.InlineParallelChunkQuery(in query, new JobQuery(commandBuffer, t,batchIndex,NumBatches));
            commandBuffer.Playback(World);
        }
    }
}