using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Combat
{
    internal class RangedTargetingSystem : BaseSystem<World, float>
    {
        private CommandBuffer sharedCommandBuffer;
        private CommandBuffer internalCommandBuffer;
        private World sharedworld;
        // 🔹 Query: cualquier entidad que pueda tener objetivo
        private QueryDescription queryTargeting = new QueryDescription()
            .WithAll<TeamComponent, AttackRangeComponent, PositionComponent>();

        public RangedTargetingSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
        {
            this.sharedCommandBuffer = sharedCommandBuffer;
            sharedworld = world;
            internalCommandBuffer = new CommandBuffer();
        }

        private struct ChunkJobTargeting : IChunkJob
        {
            private readonly CommandBuffer _commandBuffer;
            public ChunkJobTargeting(CommandBuffer commandBuffer)
            {
                _commandBuffer = commandBuffer;
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerRange = ref chunk.GetFirst<AttackRangeComponent>();
                ref var pointerTeam = ref chunk.GetFirst<TeamComponent>();
                ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
                ref var pointerTargeting = ref chunk.GetFirst<TargetingRangeComponent>();

                foreach (var entityIndex in chunk)
                {
                    ref Entity self = ref chunk.Entity(entityIndex);
                    if (!self.IsAlive())
                        continue;

                    if (self.Has<DeadComponent>() || self.Has<PendingDestroyComponent>())
                        continue;

                    ref AttackRangeComponent range = ref Unsafe.Add(ref pointerRange, entityIndex);
                    ref TeamComponent team = ref Unsafe.Add(ref pointerTeam, entityIndex);
                    ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                    ref TargetingRangeComponent targeting = ref Unsafe.Add(ref pointerTargeting, entityIndex);

                    // Si ya tiene target válido, lo dejamos
                    if (targeting.hasTarget && targeting.targetEntity.IsAlive())
                        continue;

                    // Buscar nuevo target
                    Entity target = FindNearestEnemy(self, position.position, range.attackRange, team.team);
                    if (target != Entity.Null)
                    {
                        targeting.targetEntity = target;
                        targeting.hasTarget = true;
                        targeting.targetAabb = target.Get<ColliderComponent>().aabb;
                    }
                    else
                    {
                        targeting.hasTarget = false;
                        targeting.targetEntity = Entity.Null;
                    }
                }
            }

            private Entity FindNearestEnemy(Entity self, Vector2 selfPos, float range, int selfTeam)
            {
                Vector2 size = new Vector2(range * 2, range * 2);
                Rect2 aabb = new Rect2(selfPos - (size / 2), size);

                var data = CollisionManager.Instance.characterCollidersEntities.QueryAABBBrute(aabb);

                foreach (var item in data)
                {
                    if (!item.Owner.IsAlive() || item.Owner.Has<DeadComponent>())
                        continue;
                    if (!item.Owner.Has<TeamComponent>())
                        continue;

                    var team = item.Owner.Get<TeamComponent>();
                    if (team.team == selfTeam) continue;
                    if (item.Owner.Id == self.Id) continue;

                    float dx = item.Position.X - selfPos.X;
                    float dy = item.Position.Y - selfPos.Y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= range * range)
                        return item.Owner;
                }
                return Entity.Null;
            }
        }


        public override void Update(in float t)
        {
            sharedworld.InlineParallelChunkQuery(in queryTargeting, new ChunkJobTargeting(internalCommandBuffer));
            internalCommandBuffer.Playback(sharedworld);
        }
    }
}