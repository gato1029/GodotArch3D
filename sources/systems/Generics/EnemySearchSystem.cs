using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Profiler;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Combat
{
    internal class EnemySearchSystem : BaseSystem<World, float>
    {
        private int batchIndex = 0;
        private float batchTimer = 0f;
        private const float batchInterval = 0.02f;
        private const int targetUpdatesPerFrame = 600;
        private int numBatches = 1;
        private CommandBuffer commandBuffer;
        private World sharedWorld;
        // 🔹 Query: unidades melee con radio de búsqueda
        private QueryDescription queryTargeting = new QueryDescription()
            .WithAll<TeamComponent, EnemySearchRadiusComponent, PositionComponent>().WithNone<TargetPositionComponent>();

        public EnemySearchSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
        {
            commandBuffer = sharedCommandBuffer;
            sharedWorld = world;
        }

        private struct ChunkJobEnemySearch : IChunkJob
        {
            private readonly CommandBuffer _commandBuffer;
            private readonly int _batchIndex;
            private readonly int _numBatches;
            private readonly float _deltaTime;
            public ChunkJobEnemySearch(CommandBuffer commandBuffer, float deltaTime, int batchIndex, int numBatches) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
                _batchIndex = batchIndex;
                _numBatches = numBatches;
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);
                ref var pointerSearch = ref chunk.GetFirst<EnemySearchRadiusComponent>();
                ref var pointerTeam = ref chunk.GetFirst<TeamComponent>();
                ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
                ref var pointerChar = ref chunk.GetFirst<CharacterComponent>();
                ref var pointerCollider = ref chunk.GetFirst<ColliderComponent>();
                foreach (var entityIndex in chunk)
                {
                    ref Entity self = ref Unsafe.Add(ref pointerEntity, entityIndex);
                    ref EnemySearchRadiusComponent search = ref Unsafe.Add(ref pointerSearch, entityIndex);
                    ref TeamComponent team = ref Unsafe.Add(ref pointerTeam, entityIndex);
                    ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                    ref CharacterComponent character = ref Unsafe.Add(ref pointerChar, entityIndex);
                    ref ColliderComponent collider = ref Unsafe.Add(ref pointerCollider, entityIndex);

                    if (!self.IsAlive()||self==Entity.Null)
                    {
                        GD.Print("corrupto");
                    }
                    if (!self.IsAlive()|| self.Has<DeadComponent>() || self.Has<PendingDestroyComponent>())
                    {
                        continue;
                    }
                    int batchId = collider.idCollider % _numBatches;
                    if (search.longCooldownTimer > 0f)
                    {
                        search.longCooldownTimer -= _deltaTime;
                        continue; // 🚫 está en "modo dormido", no busca
                    }

                    if (search.cooldownSearch > 0f || batchId != _batchIndex)
                    {
                        search.cooldownSearch -= _deltaTime;
                        continue;
                    }

                    // reseteo cooldown normal
                    search.cooldownSearch = 0.05f;

                    if (character.characterStateType == CharacterStateType.IDLE)
                    {
                        Entity target = FindNearestEnemy(self, position.position, search.radius, team.team);
                        if (target != Entity.Null)
                        {
                            // ✅ Encontró un enemigo → resetear contador de fallos
                            search.failedAttempts = 0;
                            if (!self.Has<TargetPositionComponent>())
                            {
                                _commandBuffer.Add(self, new TargetPositionComponent
                                {
                                    targetPosition = target.Get<PositionComponent>().position,
                                    arrivalThreshold = 0.1f
                                });
                            }



                        }
                        else
                        {
                            // ❌ no encontró → aumentar contador
                            search.failedAttempts++;

                            // Si falló, por ejemplo, 10 veces seguidas → apagar la búsqueda un rato
                            if (search.failedAttempts >= 5)
                            {
                                search.failedAttempts = 0;
                                search.longCooldownTimer = 10.0f; // esperar 2 segundos antes de volver a buscar
                            }
                        }
                    }

                }
            }

            private Entity FindNearestEnemy(Entity self, Vector2 selfPos, float searchRadius, int selfTeam)
            {
                Vector2 size = new Vector2(searchRadius * 2, searchRadius * 2);
                Rect2 aabb = new Rect2(selfPos - (size / 2), size);

                var data = CollisionManager.Instance.characterCollidersEntities.QueryAABBBrute(aabb);

                foreach (var item in data)
                {
                    if (!item.Owner.IsAlive())
                    {
                        continue;
                    }
                    if (item.Owner.Has<DeadComponent>())
                        continue;
                    if (item.Owner.Has<PendingDestroyComponent>())
                        continue;
                    if (!item.Owner.Has<TeamComponent>())
                        continue;
                    var team = item.Owner.Get<TeamComponent>();
                    if (team.team == selfTeam) continue;
                    if (item.Owner.Id == self.Id) continue;

                    float dx = item.Position.X - selfPos.X;
                    float dy = item.Position.Y - selfPos.Y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= searchRadius * searchRadius)
                        return item.Owner;
                }

                // Revisar también edificios
                data = CollisionManager.Instance.BuildingsColliders.QueryAABBBrute(aabb);

                foreach (var item in data)
                {
                    if (!item.Owner.IsAlive())
                    {
                        continue;
                    }
                    if (item.Owner.Has<DeadComponent>())
                        continue;
                    if (item.Owner.Has<PendingDestroyComponent>())
                        continue;
                    if (!item.Owner.Has<TeamComponent>())
                        continue;
                    var team = item.Owner.Get<TeamComponent>();
                    if (team.team == selfTeam) continue;
                    if (item.Owner.Id == self.Id) continue;

                    float dx = item.Position.X - selfPos.X;
                    float dy = item.Position.Y - selfPos.Y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= searchRadius * searchRadius)
                        return item.Owner;
                }

                return Entity.Null;
            }
        }

        public override void Update(in float t)
        {

            using (new ProfileScope("EnemySearchSystem"))
            {
                int totalColliders = CollisionManager.Instance.characterCollidersEntities.Count;
                numBatches = Math.Max(1, (int)Math.Ceiling(totalColliders / (float)targetUpdatesPerFrame));

                batchTimer += t;
                if (batchTimer >= batchInterval)
                {
                    batchTimer -= batchInterval;
                    batchIndex = (batchIndex + 1) % numBatches;
                }
                sharedWorld.InlineParallelChunkQuery(in queryTargeting, new ChunkJobEnemySearch(commandBuffer, t, batchIndex, numBatches));
                //commandBuffer.Playback(World);
            }
        }
    }
}
