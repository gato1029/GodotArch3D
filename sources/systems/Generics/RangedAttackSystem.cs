using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Buildings;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Combat
{
    internal class RangedAttackSystem : BaseSystem<World, float>
    {
        private CommandBuffer sharedCommandBuffer;
        private CommandBuffer internalCommandBuffer;
        private World sharedWorld;

        private QueryDescription queryAttack = new QueryDescription()
            .WithAll<AttackCooldownComponent, AttackRangeComponent, AttackDamageComponent, PositionComponent, TargetingRangeComponent>();

        public RangedAttackSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
        {
            this.sharedCommandBuffer = sharedCommandBuffer;
            sharedWorld = world;
            internalCommandBuffer = new CommandBuffer();
        }

        private struct ChunkJobAttack : IChunkJob
        {
            private readonly float _deltaTime;
            private CommandBuffer _commandBuffer;

            public ChunkJobAttack(CommandBuffer commandBuffer, float deltaTime) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerCooldown = ref chunk.GetFirst<AttackCooldownComponent>();
                ref var pointerRange = ref chunk.GetFirst<AttackRangeComponent>();
                ref var pointerDamage = ref chunk.GetFirst<AttackDamageComponent>();
                ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
                ref var pointerTargeting = ref chunk.GetFirst<TargetingRangeComponent>();

                foreach (var entityIndex in chunk)
                {
                    ref Entity self = ref chunk.Entity(entityIndex);
                    if (!self.IsAlive())
                        continue;
                    if (self.Has<DeadComponent>() || self.Has<PendingDestroyComponent>())
                        continue;

                    ref AttackCooldownComponent cooldown = ref Unsafe.Add(ref pointerCooldown, entityIndex);
                    ref AttackRangeComponent range = ref Unsafe.Add(ref pointerRange, entityIndex);
                    ref AttackDamageComponent damage = ref Unsafe.Add(ref pointerDamage, entityIndex);
                    ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                    ref TargetingRangeComponent targeting = ref Unsafe.Add(ref pointerTargeting, entityIndex);

                    // Reducir cooldown
                    if (cooldown.timeLeft > 0)
                    {
                        cooldown.timeLeft -= _deltaTime;
                        continue;
                    }

                    // Validar si hay target
                    if (!targeting.hasTarget || !targeting.targetEntity.IsAlive())
                        continue;


                    var targetPos = targeting.targetEntity.Get<PositionComponent>().position;
                    float distSq = (position.position - targetPos).LengthSquared();
                    if (distSq > range.attackRange * range.attackRange)
                    {
                        targeting.hasTarget = false;
                        targeting.targetEntity = Entity.Null;
                        continue;
                    }

                    if (targeting.targetEntity.Has<BuildingComponent>())
                    {
                        continue;
                    }
                        

                    // Atacar
                    PerformAttack(self, targeting.targetEntity, damage);

                    // marcar que ya no tiene target hasta que el targeting system lo actualice
                    targeting.hasTarget = false;
                    targeting.targetEntity = Entity.Null;
                    

                    // Resetear cooldown
                    cooldown.timeLeft = cooldown.maxCooldown;
                }
            }

            private void PerformAttack(Entity self, Entity target, AttackDamageComponent damage)
            {
                if (!target.IsAlive() || target.Has<DeadComponent>())
                    return;

                if (target.Has<BuildingComponent>())
                {
                    GameLog.LogCat("atacando a un edificio");
                }
                var selfPos = self.Get<PositionComponent>().position;
                var targetPos = target.Get<PositionComponent>().position;

                if (self.Has<BuildingComponent>())
                {
                    var id = self.Get<BuildingComponent>().id;
                    var spriteBase = BuildingManager.Instance.GetData(id).spriteBullet;

                    Entity projectile = _commandBuffer.Create(
                    [
                        typeof(ProjectileComponent),
                typeof(PositionComponent),
                typeof(DamageOnHitComponent),
                typeof(TakeHitComponent),
                typeof(ActiveProjectileComponent),
            ]);

                    _commandBuffer.Add(projectile, new PendingSpriteComponent
                    {
                        spriteData = spriteBase,
                        position = selfPos,
                        zIndex = 30
                    });

                    _commandBuffer.Set(projectile, new PositionComponent
                    {
                        position = selfPos,
                        lastPosition = selfPos
                    });

                    Vector2 direction = (targetPos - selfPos).Normalized();

                    _commandBuffer.Set(projectile, new ProjectileComponent
                    {
                        source = self,
                        target = target,
                        speed = 7f,
                        type = ProjectileTypeShoot.Direct,
                        direction = direction,
                        initialTargetPos = targetPos
                    });

                    _commandBuffer.Set(projectile, new DamageOnHitComponent
                    {
                        damage = damage.damage
                    });

                    if (self.Has<AttackEffectComponent>())
                    {
                        ref var effect = ref self.Get<AttackEffectComponent>();
                        if (effect.effectType == AttackEffectType.Stun)
                        {
                            _commandBuffer.Set(projectile, new TakeHitComponent
                            {
                                stunTime = effect.duration
                            });
                        }
                    }
                }
            }
        }


        public override void Update(in float t)
        {
            //GD.Print("antes:"+World.CountEntities(in queryAttack));  // Antes del Playback

            sharedWorld.InlineParallelChunkQuery(in queryAttack, new ChunkJobAttack(internalCommandBuffer, t));
            internalCommandBuffer.Playback(sharedWorld);
            //commandBuffer.Dispose();
            //GD.Print("Despues"+World.CountEntities(in queryAttack));  // Después del Playback        
        }
    }
}
