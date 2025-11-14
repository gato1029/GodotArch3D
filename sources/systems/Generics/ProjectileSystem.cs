using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Combat
{
    internal class ProjectileSystem : BaseSystem<World, float>
    {
        private CommandBuffer sharedCommandBuffer;
        private CommandBuffer internalCommandBuffer;
        private World sharedWorld;
        private QueryDescription queryProjectiles = new QueryDescription()
            .WithAll<ProjectileComponent, PositionComponent>();

        public ProjectileSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
        {
            sharedWorld = world;
            internalCommandBuffer = new CommandBuffer();
            this.sharedCommandBuffer = sharedCommandBuffer;
        }

        private struct ChunkJobProjectile : IChunkJob
        {
            private  CommandBuffer _commandBuffer;
            private readonly float _deltaTime;

            public ChunkJobProjectile(CommandBuffer commandBuffer, float deltaTime) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);
                ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
                ref var pointerProj = ref chunk.GetFirst<ProjectileComponent>();
                ref var pointerActiveProj = ref chunk.GetFirst<ActiveProjectileComponent>();
                foreach (var i in chunk)
                {
                    ref Entity entity = ref Unsafe.Add(ref pointerEntity, i);
                    ref PositionComponent position = ref Unsafe.Add(ref pointerPos, i);
                    ref ProjectileComponent proj = ref Unsafe.Add(ref pointerProj, i);
                    ref ActiveProjectileComponent projAct = ref Unsafe.Add(ref pointerActiveProj, i);

                    if (!proj.target.IsAlive() || proj.target.Has<DeadComponent>() || proj.target.Has<PendingDestroyComponent>())
                    {

                        _commandBuffer.Add<PendingDestroyComponent>(in entity);
                        continue;
                    }
                    position.lastPosition = position.position;

                    switch (proj.type)
                    {
                        case ProjectileTypeShoot.Direct:
                            UpdateStraightProjectile(ref entity, ref position, ref proj);
                            break;
                        case ProjectileTypeShoot.Homing:
                            UpdateHomingProjectile(ref entity, ref position, ref proj);
                            break;
                        case ProjectileTypeShoot.Physics:
                            UpdatePhysicsProjectile(ref entity, ref position, ref proj);
                            break;
                    }
                }
            }

            private void UpdateDirectProjectile(ref Entity projectile, ref PositionComponent position, ref ProjectileComponent proj)
            {
                // Si el target murió o ya no está vivo → destruir proyectil
                if (!proj.target.IsAlive() || proj.target.Has<DeadComponent>())
                {
                    _commandBuffer.Add<PendingDestroyComponent>(projectile);
                    return;
                }

                Vector2 targetPos = proj.target.Get<PositionComponent>().position;

                // Dirección hacia el target
                Vector2 dir = (targetPos - position.position).Normalized();
                position.position += dir * proj.speed * _deltaTime;
                
                // Comprobación de impacto
                float distSq = (position.position - targetPos).LengthSquared();
                if (distSq < .01f) // umbral de impacto (4px de radio)
                {
                    GD.Print("Posicion proyectil:" + position.position);
                    OnHit(projectile,ref proj.target);
                  //  FreeProjectileGPU(projectile);
                    _commandBuffer.Add<PendingDestroyComponent>(projectile);
                }
            }
            private void UpdateStraightProjectile(ref Entity projectile, ref PositionComponent position, ref ProjectileComponent proj)
            {
                // mover en la dirección fija
                position.position += proj.direction * proj.speed * _deltaTime;

                // distancia al punto inicial del target
                float distSq = (position.position - proj.initialTargetPos).LengthSquared();                
                if (distSq < 0.01f)// llegó al punto donde estaba el target
                {
                    // calcular distancia real al target actual
                    if (proj.target.IsAlive())
                    {
                        Vector2 targetPos = proj.target.Get<PositionComponent>().position;
                        float distSqToTarget = (position.position - targetPos).LengthSquared();

                        float impactRadius = .2f; // ej. 4px de radio
                        if (distSqToTarget <= impactRadius)
                        {
                            OnHit(projectile,ref proj.target); // aplicar daño
                        }
                    }

                    // Devolver proyectil al pool en vez de destruirlo
                    //ProjectilePool.Instance.ReturnProjectile(projectile,_commandBuffer);
                    //// liberar GPU siempre, independientemente de si hubo impacto
                    //FreeProjectileGPU(projectile);
                    //// destruir proyectil
                    _commandBuffer.Add<PendingDestroyComponent>(projectile);
                }
            }

            private void UpdateHomingProjectile(ref Entity projectile, ref PositionComponent position, ref ProjectileComponent proj)
            {
        
                // recalcular dirección hacia el target
                Vector2 targetPos = proj.target.Get<PositionComponent>().position;
                Vector2 dir = (targetPos - position.position).Normalized();
                position.position += dir * proj.speed * _deltaTime;

                // comprobar impacto
                float distSq = (position.position - targetPos).LengthSquared();
                if (distSq < 0.01f)
                {
                    OnHit(projectile,ref proj.target);
                    // Devolver proyectil al pool en vez de destruirlo
                    _commandBuffer.Add<PendingDestroyComponent>(projectile);
                    //ProjectilePool.Instance.ReturnProjectile(projectile, _commandBuffer);
                }
            }
            private void UpdatePhysicsProjectile(ref Entity projectile, ref PositionComponent position, ref ProjectileComponent proj)
            {
                position.position += proj.direction.Normalized() * proj.speed * _deltaTime;

                // 🚧 Aquí más adelante se puede conectar con un sistema de colisiones físicas
            }

            private void OnHit(Entity projectile,ref Entity target)
            {
                if (target.Has<BuildingComponent>())
                {
                    GameLog.LogCat("error en proyectil");
                }
                // Aplicar daño
                if (projectile.Has<DamageOnHitComponent>() && target.IsAlive())
                {
                    ref var damage = ref projectile.Get<DamageOnHitComponent>();
                    if (target.Has<HealthComponent>())
                    {
                        ref var health = ref target.Get<HealthComponent>();
                        health.current -= damage.damage;

                        if (health.current <= 0)
                            _commandBuffer.Add<DeadComponent>(target);
                        else
                        {
                            // Aplicar efectos (ej. Stun)
                            if (projectile.Has<TakeHitComponent>() && target.IsAlive())
                            {
                                 var hit =  projectile.Get<TakeHitComponent>();
                                _commandBuffer.Add(target, new TakeHitComponent { stunTime = hit.stunTime });
                            }
                        }
                    }
                }

              
            }
            private void FreeProjectileGPU(Entity projectile)
            {
                if (projectile.Has<SpriteRenderGPUComponent>())
                {
                    ref var sprite = ref projectile.Get<SpriteRenderGPUComponent>();
                    // Aquí llamas a tu función que libera la textura en GPU
                    MultimeshManager.Instance.FreeInstance(sprite.rid, sprite.instance, sprite.idMaterial);
                    RenderingServer.MultimeshInstanceSetCustomData(sprite.rid, sprite.instance, new Color(-1, -1, -1, -1));
                }

                // Si también usas RenderGPUComponent:
                //if (projectile.Has<RenderGPUComponent>())
                //{
                //    ref var render = ref projectile.Get<RenderGPUComponent>();
                //    RenderingServer.FreeRid(render.rid);
                //}
            }
        }

        public override void Update(in float t)
        {
            sharedWorld.InlineParallelChunkQuery(in queryProjectiles, new ChunkJobProjectile(internalCommandBuffer, t));
            internalCommandBuffer.Playback(sharedWorld);

            //ProjectilePool.Instance.Update(t);
        }
    }
}
