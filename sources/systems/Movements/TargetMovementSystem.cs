using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems.Movements;
public class TargetMovementSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private World sharedWorld;
    // 🔹 Query: entidades que se pueden mover hacia un objetivo
    private QueryDescription queryMoveToTarget = new QueryDescription()
        .WithAll<PositionComponent, VelocityComponent, TargetPositionComponent, CharacterComponent>();

    public TargetMovementSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
    {
        commandBuffer = sharedCommandBuffer;
        sharedWorld = world;
    }

    private struct ChunkJobMoveToTarget : IChunkJob
    {
        private readonly CommandBuffer _commandBuffer;
        private readonly float _deltaTime;

        public ChunkJobMoveToTarget(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
            ref var pointerVel = ref chunk.GetFirst<VelocityComponent>();
            ref var pointerTarget = ref chunk.GetFirst<TargetPositionComponent>();
            ref var pointerChar = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerDir = ref chunk.GetFirst<DirectionComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                ref VelocityComponent velocity = ref Unsafe.Add(ref pointerVel, entityIndex);
                ref TargetPositionComponent targetPos = ref Unsafe.Add(ref pointerTarget, entityIndex);
                ref CharacterComponent character = ref Unsafe.Add(ref pointerChar, entityIndex);
                ref DirectionComponent direction = ref Unsafe.Add(ref pointerDir, entityIndex);

                if (!entity.IsAlive()|| entity.Has<DeadComponent>() || entity.Has<PendingDestroyComponent>())
                {
                    continue;
                }
                if (!entity.Has<TargetPositionComponent>())
                {
                    continue;
                }
                if ((character.characterStateType == CharacterStateType.BLOCKED))
                {
                    character.characterStateType = CharacterStateType.IDLE;
                    _commandBuffer.Remove<TargetPositionComponent>(entity);
                    return;
                }
                if (character.characterStateType == CharacterStateType.IDLE || character.characterStateType == CharacterStateType.MOVING)
                {

                
                
                Vector2 dir = targetPos.targetPosition - position.position;
                float distance = dir.Length();

                if (distance <= targetPos.arrivalThreshold)
                {
                    // ✅ Llegó al destino
                    character.characterStateType = CharacterStateType.IDLE;
                    _commandBuffer.Remove<TargetPositionComponent>(entity);
                    continue;
                }

                dir = dir.Normalized();
                Vector2 movement = dir * velocity.velocity * _deltaTime;

                // Si va a pasarse, ajustamos exacto al destino
                if (movement.Length() >= distance)
                {
                    position.lastPosition = position.position;    // 🔹 guardamos dónde estaba
                    position.position = targetPos.targetPosition;
                    position.positionFuture = targetPos.targetPosition;
                    character.characterStateType = CharacterStateType.IDLE;
                    _commandBuffer.Remove<TargetPositionComponent>(entity);
                }
                else
                {
                    //position.lastPosition = position.position;    // 🔹 guardamos dónde estaba
                    //position.position += movement;                 
                    position.positionFuture = position.position + movement;

                    // 🔹 Actualizar dirección para animación
                    direction.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir);
                    direction.value = dir;
                    direction.normalized = new Vector2(Math.Sign(dir.X), Math.Sign(dir.Y));

                    character.characterStateType = CharacterStateType.MOVING;
                    }
                }
            }
        }
    }

    public override void Update(in float t)
    {
        sharedWorld.InlineParallelChunkQuery(in queryMoveToTarget, new ChunkJobMoveToTarget(commandBuffer, t));
        //commandBuffer.Playback(World);
    }
}


