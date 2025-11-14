using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters; // para CommonOperations
using GodotEcsArch.sources.managers.Profiler;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Movement;

public class RadiusMovementSystem : BaseSystem<World, float>
{
    private QueryDescription queryRadiusMovement = new QueryDescription()
        .WithAll<PositionComponent, VelocityComponent, RadiusMovementComponent, CharacterComponent, DirectionComponent>().WithNone<TargetPositionComponent>();

    private Random rng = new Random();
    private CommandBuffer commandBuffer;
    private World sharedWorld;
    private int batchIndex = 0;
    private float batchTimer = 0f;
    private const float batchInterval = 0.02f;  // cada 20ms avanza al siguiente batch
    private const int updatesPerFrame = 600;    // cuántas entidades máximo procesar por frame
    private int numBatches = 1;
    public RadiusMovementSystem(World world, CommandBuffer sharedCommandBuffer) : base(world) { 
        commandBuffer = sharedCommandBuffer;
        sharedWorld = world;
    
    }

    private struct ChunkJobRadiusMovement : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly Random _rng;
        private readonly CommandBuffer _commandBuffer;
        private readonly int _batchIndex;
        private readonly int _numBatches;


        public ChunkJobRadiusMovement(float deltaTime, Random rng, CommandBuffer commandBuffer, int batchIndex, int numBatches) : this()
        {
            _deltaTime = deltaTime;
            _rng = rng;
            _commandBuffer = commandBuffer;
            _batchIndex = batchIndex;
            _numBatches = numBatches;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
            ref var pointerVel = ref chunk.GetFirst<VelocityComponent>();
            ref var pointerRadius = ref chunk.GetFirst<RadiusMovementComponent>();
            ref var pointerChar = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerDir = ref chunk.GetFirst<DirectionComponent>();
            ref var pointerCollider = ref chunk.GetFirst<ColliderComponent>();
            foreach (var entityIndex in chunk)
            { 
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                ref VelocityComponent velocity = ref Unsafe.Add(ref pointerVel, entityIndex);
                ref RadiusMovementComponent radiusMove = ref Unsafe.Add(ref pointerRadius, entityIndex);
                ref CharacterComponent character = ref Unsafe.Add(ref pointerChar, entityIndex);
                ref DirectionComponent direction = ref Unsafe.Add(ref pointerDir, entityIndex);
                ref ColliderComponent collider = ref Unsafe.Add(ref pointerCollider, entityIndex);
                // batch filter

                int batchId = collider.idCollider % _numBatches;
                
                if (batchId != _batchIndex) continue;

                // cooldown
                if (radiusMove.cooldownTimer > 0f)
                {
                    radiusMove.cooldownTimer -= _deltaTime;
                    continue;
                }

                if (character.characterStateType == CharacterStateType.IDLE)
                {
                    switch (radiusMove.mode)
                    {
                        case MovementMode.PointToPoint:
                            ComputePointToPoint(ref entity, ref position, ref radiusMove);
                            break;

                        case MovementMode.FreeWander:
                            ComputeFreeWander(ref entity, ref position, ref radiusMove);
                            break;
                    }

                    // reset cooldown
                    radiusMove.cooldownTimer = radiusMove.movementCooldown;
            }
        }
    }

        private void ComputePointToPoint(
            ref Entity entity,
            ref PositionComponent position,
            ref RadiusMovementComponent radiusMove)
        {
          
                radiusMove.currentTarget = GetRandomPointInCircle(radiusMove.center, radiusMove.desiredRadius);
                radiusMove.hasTarget = true;
          

            // Asignar como target de movimiento
            _commandBuffer.Add(entity,new TargetPositionComponent
            {
                targetPosition = radiusMove.currentTarget,
                arrivalThreshold = 0.1f
            });
        }

        private void ComputeFreeWander(
            ref Entity entity,
            ref PositionComponent position,
            ref RadiusMovementComponent radiusMove)
        {

       
                radiusMove.currentTarget = GetRandomPointInCircle(position.position, radiusMove.desiredRadius);
                radiusMove.hasTarget = true;
            

            _commandBuffer.Add(entity, new TargetPositionComponent
            {
                targetPosition = radiusMove.currentTarget,
                arrivalThreshold = 0.1f
            });
        }

        private Vector2 GetRandomPointInCircle(Vector2 center, float radius)
        {
            double angle = _rng.NextDouble() * Math.PI * 2;
            double r = Math.Sqrt(_rng.NextDouble()) * radius;

            return center + new Vector2(
                (float)(Math.Cos(angle) * r),
                (float)(Math.Sin(angle) * r)
            );
        }
    }

    public override void Update(in float deltaTime)
    {
        using (new ProfileScope("RadiusMovementSystem"))
        {
            int totalEntities = World.CountEntities(queryRadiusMovement);
            numBatches = Math.Max(1, (int)Math.Ceiling(totalEntities / (float)updatesPerFrame));

            batchTimer += deltaTime;
            if (batchTimer >= batchInterval)
            {
                batchTimer -= batchInterval;
                batchIndex = (batchIndex + 1) % numBatches;
            }

            sharedWorld.InlineParallelChunkQuery(in queryRadiusMovement,
                new ChunkJobRadiusMovement(deltaTime, rng, commandBuffer, batchIndex, numBatches));
            //commandBuffer.Playback(World);
        }
    }
}
