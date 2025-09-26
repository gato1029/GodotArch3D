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
    internal class TargetingSystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;

        // 🔹 Query: cualquier entidad que pueda tener objetivo
        private QueryDescription queryTargeting = new QueryDescription()
            .WithAll<TeamComponent, AttackRangeComponent, PositionComponent, TargetingComponent>();

        public TargetingSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }

        private struct ChunkJobTargeting : IChunkJob
        {
            private readonly CommandBuffer _commandBuffer;

            public ChunkJobTargeting(CommandBuffer commandBuffer) : this()
            {
                _commandBuffer = commandBuffer;
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);
                ref var pointerRange = ref chunk.GetFirst<AttackRangeComponent>();
                ref var pointerTeam = ref chunk.GetFirst<TeamComponent>();
                ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
                ref var pointerTargeting = ref chunk.GetFirst<TargetingComponent>();

                foreach (var entityIndex in chunk)
                {
                    ref Entity self = ref Unsafe.Add(ref pointerEntity, entityIndex);
                    ref AttackRangeComponent range = ref Unsafe.Add(ref pointerRange, entityIndex);
                    ref TeamComponent team = ref Unsafe.Add(ref pointerTeam, entityIndex);
                    ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                    ref TargetingComponent targeting = ref Unsafe.Add(ref pointerTargeting, entityIndex);

                    // Buscar enemigo más cercano
                    Entity? target = FindNearestEnemy(self, position.position, range.attackRange, team.team);

                    // Guardar en el TargetingComponent
                    targeting.targetEntity = target ?? Entity.Null;
                }
            }

            private Entity? FindNearestEnemy(Entity self, Vector2 selfPos, float range, int selfTeam)
            {
                Vector2 size = new Vector2(range * 2, range * 2);
                Rect2 aabb = new Rect2(selfPos - (size / 2), size);

                var data = CollisionManager.Instance.characterCollidersEntities.QueryAABBBrute(aabb);

                foreach (var item in data)
                {
                    if (item.Owner.Has<DeadComponent>())
                    {
                        continue; // ignorar si ya está muerto
                    }
                    var team = item.Owner.Get<TeamComponent>();
                    if (team.team == selfTeam) continue;       // ignorar aliados
                    if (item.Owner.Id == self.Id) continue;    // ignorar a sí mismo

                    float dx = item.Position.X - selfPos.X;
                    float dy = item.Position.Y - selfPos.Y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= range * range)
                    {
                        return item.Owner; // 🔹 devolver el primero que cumpla
                    }
                }

                return null;
            }
        }

        public override void Update(in float t)
        {
            World.InlineParallelChunkQuery(in queryTargeting, new ChunkJobTargeting(commandBuffer));
            commandBuffer.Playback(World);
        }
    }
}
