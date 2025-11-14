using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Profiler;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Collisions;
public class MovementCollisionSystem : BaseSystem<World, float>
{
    private int batchIndex = 0;
    private float batchTimer = 0f;
    private const float batchInterval = 0.02f; // cada 20ms rota batch
    private const int targetUpdatesPerFrame = 600; // cuántos colliders máximo por frame
    private int numBatches = 1;

    private CommandBuffer commandBuffer;
    private World sharedworld;

    // 🔹 Query: entidades que se mueven y requieren detección de colisión
    private QueryDescription queryMovementCollision = new QueryDescription()
        .WithAll<PositionComponent, VelocityComponent, CharacterComponent, ColliderComponent>();

    public MovementCollisionSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
    {
        commandBuffer = sharedCommandBuffer;
        sharedworld = world;
    }

    private struct ChunkJobMovementCollision : IChunkJob
    {
        private readonly CommandBuffer _commandBuffer;
        private readonly float _deltaTime;
        private readonly int _batchIndex;
        private readonly int _numBatches;

        public ChunkJobMovementCollision(CommandBuffer commandBuffer, float deltaTime, int batchIndex, int numBatches) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            _batchIndex = batchIndex;
            _numBatches = numBatches;
        }

        public void Execute(ref Chunk chunk)
        {
      
            ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
            ref var pointerVel = ref chunk.GetFirst<VelocityComponent>();
            ref var pointerChar = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerCol = ref chunk.GetFirst<ColliderComponent>();
            ref var pointerDir = ref chunk.GetFirst<DirectionComponent>();

            foreach (var entityIndex in chunk)
            {                                
                ref Entity entity = ref chunk.Entity(entityIndex);
                if (!entity.IsAlive() || entity == Entity.Null)
                    continue; // 🚫 saltar entidades muertas

                ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                ref VelocityComponent velocity = ref Unsafe.Add(ref pointerVel, entityIndex);
                ref CharacterComponent character = ref Unsafe.Add(ref pointerChar, entityIndex);
                ref ColliderComponent collider = ref Unsafe.Add(ref pointerCol, entityIndex);
                ref DirectionComponent direction = ref Unsafe.Add(ref pointerDir, entityIndex);
                if (!entity.IsAlive())
                {
                    continue;
                }
                if (entity.Has<PendingDestroyComponent>() || entity.Has<DeadComponent>())
                {
                    continue;
                }
                if (character.characterStateType == CharacterStateType.BLOCKED || character.characterStateType == CharacterStateType.IDLE)
                {
                    character.characterStateType = CharacterStateType.IDLE;
                    continue;
                }
                if (position.position == position.positionFuture || character.characterStateType == CharacterStateType.ATTACK || character.characterStateType == CharacterStateType.ATTACKING)
                {
                    //character.characterStateType = CharacterStateType.BLOCKED;
                    continue;
                }
                var dataModel = CharacterModelManager.Instance.GetCharacterModel(character.idCharacterBaseData);
                GeometricShape2D collisionShapeCharacterMove = dataModel.collisionMove;

                Vector2 nextStep = direction.value * velocity.velocity * _deltaTime;

                Vector2 startPos = position.lastPosition;
                Vector2 endPos = position.positionFuture; // ya actualizado por el MovementSystem
                Vector2 pointNextCollision = endPos + collisionShapeCharacterMove.OriginCurrent;// + nextStep;

                bool existCollision = false;

                // 🔹 Batch ID (para distribuir carga)
                int batchId = collider.idCollider % _numBatches;

                existCollision = CollisionManager.CheckAnyCollisionMoveUnitOnly(collider.idCollider, pointNextCollision, collisionShapeCharacterMove);

                // 🔹 Enfriamiento local
                if (batchId == _batchIndex)
                {
                    //existCollision = CollisionManager.CheckAnyCollisionMoveUnitOnly(entity, pointNextCollision, collisionShapeCharacterMove);
                    //if (!existCollision)
                    //{
                    //    existCollision = CollisionManager.CheckSegmentCollision(startPos, endPos, collisionShapeCharacterMove);
                    //}
                    //character.collisionCheckCooldown = 0.05f; // cada 50ms
                }
                //else
                //{
                //    character.collisionCheckCooldown -= _deltaTime;
                //}


                // 🔹 Estáticos siempre se chequean
                if (!existCollision)
                {
             
                    existCollision = CollisionManager.CheckAnyCollisionStatic(entity, pointNextCollision, collisionShapeCharacterMove);
              
                }

                // 🔹 Reaccionar a colisión
                if (existCollision)
                {
                    // Dirección del movimiento actual
                    Vector2 movementDir = (endPos - startPos).Normalized();

                    ////// Empuje hacia atrás (escape)
                    ////// Puedes ajustar "pushBackDistance" según el tamaño de tus colliders
                    float pushBackDistance = .01f; // en píxeles, por ejemplo

                    //position.position = endPos - movementDir * pushBackDistance;

                    //position.position = position.lastPosition;
                    // Forzar estado bloqueado (excepto si atacando)
                    if (character.characterStateType != CharacterStateType.ATTACK)
                    {
                        character.characterStateType = CharacterStateType.BLOCKED;
                        character.blockedCooldown = 0.15f; // por ejemplo 150ms
                    }
                }
                else
                {
                    position.lastPosition = position.position;
                    position.position = position.positionFuture;
                }
                //else
                //{
                //    // Si no hubo colisión y estaba bloqueado → volver a MOVING
                //    if (character.characterStateType == CharacterStateType.BLOCKED)
                //    {
                //        character.characterStateType = CharacterStateType.MOVING;
                //    }
                //}
            }
        }
    }

    public override void Update(in float t)
    {
        using (new ProfileScope("MovementCollisionSystem"))
        {
            // 🔹 calcular cuántos batches necesito
            int totalColliders = CollisionManager.Instance.characterCollidersEntities.Count;
            numBatches = Math.Max(1, (int)Math.Ceiling(totalColliders / (float)targetUpdatesPerFrame));

            // 🔹 avanzar el índice de batch con deltaTime
            batchTimer += t;
            if (batchTimer >= batchInterval)
            {
                batchTimer -= batchInterval;
                batchIndex = (batchIndex + 1) % numBatches;
            }
            sharedworld.InlineParallelChunkQuery(in queryMovementCollision,
              new ChunkJobMovementCollision(commandBuffer, t, batchIndex, numBatches));
           
        }
       

      
    }
}
