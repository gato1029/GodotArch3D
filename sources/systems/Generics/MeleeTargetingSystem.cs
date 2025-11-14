using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.utils;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Combat
{
    internal class MeleeTargetingSystem : BaseSystem<World, float>
    {
        private int batchIndex = 0;
        private float batchTimer = 0f;
        private const float batchInterval = 0.02f;
        private const int targetUpdatesPerFrame = 600;
        private int numBatches = 1;
        private CommandBuffer commandBuffer;
        private World sharedworld;

        // 🔹 Query: unidades melee con radio de búsqueda
        private QueryDescription queryTargeting = new QueryDescription()
            .WithAll<TeamComponent, PositionComponent, MeleeAttackComponent>().WithNone<BuildingComponent>();

        public MeleeTargetingSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
        {
            commandBuffer = sharedCommandBuffer;
            sharedworld = world;
        }

        private struct ChunkJobTargetingMelee : IChunkJob
        {
            private readonly CommandBuffer _commandBuffer;
            private readonly int _batchIndex;
            private readonly int _numBatches;
            private readonly float _deltaTime;
            public ChunkJobTargetingMelee(CommandBuffer commandBuffer, float deltaTime, int batchIndex, int numBatches) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
                _batchIndex = batchIndex;
                _numBatches = numBatches;
            }

            public void Execute(ref Chunk chunk)
            {                                
                ref var pointerTeam = ref chunk.GetFirst<TeamComponent>();
                ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
                ref var pointerChar = ref chunk.GetFirst<CharacterComponent>();
                ref var pointerCollider = ref chunk.GetFirst<ColliderComponent>();
                ref var pointerAttack = ref chunk.GetFirst<MeleeAttackComponent>();
               
                foreach (var entityIndex in chunk)
                {
                    ref Entity self = ref Unsafe.Add(ref chunk.Entity(0), entityIndex); // entidad no alineada con componentes

                    ref TeamComponent team = ref Unsafe.Add(ref pointerTeam, entityIndex);
                    ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                    ref CharacterComponent character = ref Unsafe.Add(ref pointerChar, entityIndex);
                    ref ColliderComponent collider = ref Unsafe.Add(ref pointerCollider, entityIndex);
                    ref MeleeAttackComponent attack = ref Unsafe.Add(ref pointerAttack, entityIndex);
                                          
                    if (!self.IsAlive())
                    {
                        continue;
                    }

                    int batchId = collider.idCollider % _numBatches;
                    if (batchId != _batchIndex) continue;

                    // cooldown
                    if (attack.cooldownTimer > 0f)
                    {
                        attack.cooldownTimer -= _deltaTime;
                        continue;
                    }
              
                    attack.cooldownTimer = 0.05f;

                    //if (character.characterStateType == CharacterStateType.IDLE)
                    {
                        Entity target = FindNearestEnemy(self, position.position, attack.attackRange*2, team.team);
                        if (target != Entity.Null)
                        {                                                        
                            _commandBuffer.Add(self, new MeleeTargetCandidateComponent
                            {
                                target = target,
                                lastKnownPosition = target.Get<PositionComponent>().position,
                                timeToLive = 2.0f // por ejemplo, 2 segundos de validez
                            });
                            if (self.Has<TargetPositionComponent>())
                            {
                                _commandBuffer.Set(self, new TargetPositionComponent
                                {
                                    targetPosition = target.Get<PositionComponent>().position,
                                    arrivalThreshold = 0.1f
                                });
                            }
                            else
                            {
                                _commandBuffer.Add(self, new TargetPositionComponent
                                {
                                    targetPosition = target.Get<PositionComponent>().position,
                                    arrivalThreshold = 0.1f
                                });
                            }
                            
                        }                 
                    }

                }
            }

            private Entity FindNearestEnemy(Entity self, Vector2 selfPos, float searchRadius, int selfTeam)
            {
                float searchRadiusSq = searchRadius * searchRadius;
                float minDistSq = float.MaxValue;
                Entity nearest = Entity.Null;

                // Función local para chequear un conjunto de colisionadores
                void CheckCandidates(IEnumerable<(Entity Owner, managers.Collision.GeometricShape2D Shape, Vector2 Position)> candidates)
                {
                    foreach (var item in candidates)
                    {
                        var owner = item.Owner;
                        if (!owner.IsAlive())
                        {
                            continue;
                        }
                        if (owner.Has<DeadComponent>()) continue;
                        if (owner.Has<PendingDestroyComponent>()) continue;

                        if (owner.Id == self.Id) continue;
                        if (!owner.Has<TeamComponent>()) continue;
                        var team = owner.Get<TeamComponent>(); // revisar Urgente
                        if (team.team == selfTeam) continue;

                        float dx = item.Position.X - selfPos.X;
                        float dy = item.Position.Y - selfPos.Y;
                        float distSq = dx * dx + dy * dy;

                        if (distSq <= searchRadiusSq && distSq < minDistSq)
                        {
                            minDistSq = distSq;
                            nearest = owner;
                        }
                    }
                }

                Vector2 size = new Vector2(searchRadius * 2, searchRadius * 2);
                Rect2 aabb = new Rect2(selfPos - (size / 2), size);

                // Chequear unidades
                List<(Entity Owner, managers.Collision.GeometricShape2D Shape, Vector2 Position)> charCandidates = CollisionManager.Instance.characterCollidersEntities.QueryAABBBrute(aabb);
                CheckCandidates(charCandidates);

                // Chequear edificios
                var buildingCandidates = CollisionManager.Instance.BuildingsColliders.QueryAABBBrute(aabb);
                CheckCandidates(buildingCandidates);

                return nearest;
            }

        }

        public override void Update(in float t)
        {
            
            using (new ProfileScope("MeleeTargetingSystem"))
            {
                int totalColliders = CollisionManager.Instance.characterCollidersEntities.Count;
                numBatches = Math.Max(1, (int)Math.Ceiling(totalColliders / (float)targetUpdatesPerFrame));

                batchTimer += t;
                if (batchTimer >= batchInterval)
                {
                    batchTimer -= batchInterval;
                    batchIndex = (batchIndex + 1) % numBatches;
                }
                //var query = World.Query(in queryTargeting);
                sharedworld.InlineParallelChunkQuery(in queryTargeting, new ChunkJobTargetingMelee(commandBuffer,t,batchIndex,numBatches));
                 //vv = World.CountEntities(in queryTargeting);
                //commandBuffer.Playback(World);
            }
        }
    }
}
