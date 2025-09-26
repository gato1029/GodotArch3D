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

namespace GodotEcsArch.sources.systems.Buildings;
internal class AttackBuildingSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription queryTower = new QueryDescription().WithAll<BuildingComponent,TeamComponent, AttackCooldownComponent, AttackRangeComponent, HealthComponent,PositionComponent>();


    public AttackBuildingSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }
    private struct ChunkJobAnimation : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobAnimation(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerBuildingTowerComponent = ref chunk.GetFirst<BuildingComponent>();
            ref var pointerAttackCooldownComponent = ref chunk.GetFirst<AttackCooldownComponent>();
            ref var pointerAttackRangeComponent = ref chunk.GetFirst<AttackRangeComponent>();
            ref var pointerHealthComponent = ref chunk.GetFirst<HealthComponent>();
            ref var pointerPositionComponent = ref chunk.GetFirst<PositionComponent>();
            ref var pointerTeamComponent = ref chunk.GetFirst<TeamComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                
                ref TeamComponent team = ref Unsafe.Add(ref pointerTeamComponent, entityIndex);
                ref BuildingComponent tower = ref Unsafe.Add(ref pointerBuildingTowerComponent, entityIndex);
                ref AttackCooldownComponent cooldown = ref Unsafe.Add(ref pointerAttackCooldownComponent, entityIndex);
                ref AttackRangeComponent range = ref Unsafe.Add(ref pointerAttackRangeComponent, entityIndex);
                ref HealthComponent health = ref Unsafe.Add(ref pointerHealthComponent, entityIndex);
                ref PositionComponent position = ref Unsafe.Add(ref pointerPositionComponent, entityIndex);
               Entity? target= FindNearestEnemy(entity, position.position, range.attackRange, team.team);
                if (target != null)
                {
                    var positionTarget = target.Value.Get<PositionComponent>();
                    //WireShape.Instance.DrawCircle(5, positionTarget.position, 30, Colors.DarkCyan);
                }
            }
        }

        private Entity? FindNearestEnemy(Entity entity, Vector2 towerPos, float range, int towerTeam)
        {
            Vector2 vector2 = new Vector2(range*2, range*2);
            Rect2 aabb = new Rect2(towerPos - (vector2 / 2), vector2);
            var data = CollisionManager.Instance.characterCollidersEntities.QueryAABBBrute(aabb);
            Entity? nearest = null;
            float minDistSq = float.MaxValue;
            foreach (var item in data)
            {
                var team = item.Owner.Get<TeamComponent>();
                if (team.team==towerTeam)
                {
                    continue;
                }
                if (item.Owner.Id!= entity.Id)
                {
                    float dx = item.Position.X - towerPos.X;
                    float dy = item.Position.Y - towerPos.Y;
                    float distSq = dx * dx + dy * dy;

                    if (distSq <= range * range && distSq < minDistSq)
                    {
                        
                        minDistSq = distSq;
                        nearest = item.Owner;
                        break;
                    }
                }
            }                       
            return nearest;
        }
    }
    public override void Update(in float t)
    {

        World.InlineParallelChunkQuery(in queryTower, new ChunkJobAnimation(commandBuffer, t));
        commandBuffer.Playback(World);
    }
}