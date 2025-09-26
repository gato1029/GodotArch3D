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
        private CommandBuffer commandBuffer;

        private QueryDescription queryAttack = new QueryDescription()
            .WithAll<AttackCooldownComponent, AttackRangeComponent, AttackDamageComponent, PositionComponent, TargetingComponent>();

        public RangedAttackSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }

        private struct ChunkJobAttack : IChunkJob
        {
            private readonly float _deltaTime;
            private  CommandBuffer _commandBuffer;
           

            public ChunkJobAttack(CommandBuffer commandBuffer, float deltaTime) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
              
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);
                ref var pointerCooldown = ref chunk.GetFirst<AttackCooldownComponent>();
                ref var pointerRange = ref chunk.GetFirst<AttackRangeComponent>();
                ref var pointerDamage = ref chunk.GetFirst<AttackDamageComponent>();
                ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
                ref var pointerTargeting = ref chunk.GetFirst<TargetingComponent>();

                foreach (var entityIndex in chunk)
                {
                    ref Entity self = ref Unsafe.Add(ref pointerEntity, entityIndex);
                    ref AttackCooldownComponent cooldown = ref Unsafe.Add(ref pointerCooldown, entityIndex);
                    ref AttackRangeComponent range = ref Unsafe.Add(ref pointerRange, entityIndex);
                    ref AttackDamageComponent damage = ref Unsafe.Add(ref pointerDamage, entityIndex);
                    ref PositionComponent position = ref Unsafe.Add(ref pointerPos, entityIndex);
                    ref TargetingComponent targeting = ref Unsafe.Add(ref pointerTargeting, entityIndex);

                    // Reducir cooldown
                    if (cooldown.timeLeft > 0)
                    {
                        cooldown.timeLeft -= _deltaTime;
                        continue;
                    }

                    // Validar si hay un objetivo
                    if (!targeting.targetEntity.IsAlive())
                        continue;

                    // Validar que siga en rango
                    var targetPos = targeting.targetEntity.Get<PositionComponent>().position;
                    float distSq = (position.position - targetPos).LengthSquared();
                    if (distSq > range.attackRange * range.attackRange)
                        continue;

                    // 🔫 Atacar
                    PerformAttack(self, targeting.targetEntity, damage);

                    // Reiniciar cooldown
                    cooldown.timeLeft = cooldown.maxCooldown;
                }
            }
            private void PerformAttack(Entity self, Entity target, AttackDamageComponent damage)
            {
                // Verificamos que el target sea válido
                if (!target.IsAlive() || target.Has<DeadComponent>())
                    return;

                var selfPos = self.Get<PositionComponent>().position;
                var targetPos = target.Get<PositionComponent>().position;

                // Obtener un proyectil del pool
                Entity projectile = ProjectilePool.Instance.SpawnProjectile(_commandBuffer);
                if (projectile == Entity.Null)
                    return; // pool vacío                

                if (self.Has<BuildingComponent>())
                {
                    var id = self.Get<BuildingComponent>().id;
                    var spriteBase = BuildingManager.Instance.GetData(id).spriteBullet;
                    var sprite = SpriteHelper.CreateSpriteRenderGpuComponent(spriteBase, spriteBase.scale, selfPos, 30);
                    _commandBuffer.Add(projectile,sprite);
                   // GD.Print("Id projectil:" + projectile.Id);
                }

                // Posición inicial del proyectil = posición del atacante
                _commandBuffer.Set(projectile, new PositionComponent
                {
                    position = selfPos,
                    lastPosition = selfPos // opcional, para el sweep si lo usas
                });
                // Calcular dirección solo para proyectil Direct
                Vector2 direction = (targetPos - selfPos).Normalized();
                // Agregar datos de movimiento del proyectil
                _commandBuffer.Set(projectile, new ProjectileComponent
                {
                    source = self,
                    target = target,
                    speed = 7f, // ajustar según arma
                    type = ProjectileTypeShoot.Direct,
                    direction = direction,           // para proyectil recto
                    initialTargetPos = targetPos     // posición original del target
                });

                // Daño que hará al impactar
                _commandBuffer.Set(projectile, new DamageOnHitComponent
                {
                    damage = damage.damage
                });

                // Efectos especiales
                if (self.Has<AttackEffectComponent>())
                {
                    ref var effect = ref self.Get<AttackEffectComponent>();

                    switch (effect.effectType)
                    {
                        case AttackEffectType.Stun:
                            _commandBuffer.Set(projectile, new TakeHitComponent
                            {
                                stunTime = effect.duration
                            });
                            break;

                        case AttackEffectType.None:
                        default:
                            break;
                    }
                }
            }     

            }

        public override void Update(in float t)
        {
            
            World.InlineParallelChunkQuery(in queryAttack, new ChunkJobAttack(commandBuffer, t));
            commandBuffer.Playback(World);
            GD.Print("Total proyectiles:"+ProjectilePool.Instance.GetTotal());
        }
    }
}
