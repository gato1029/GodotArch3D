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

namespace GodotEcsArch.sources.systems.Combat;

public class MeleeAttackSystem : BaseSystem<World, float>
{
    private QueryDescription queryAttack = new QueryDescription()
        .WithAll<MeleeAttackComponent, MeleeTargetCandidateComponent, PositionComponent, CharacterComponent>();

    private CommandBuffer commandBuffer;
    private World sharedWorld;

    public MeleeAttackSystem(World world, CommandBuffer sharedCommandBuffer) : base(world) { 
        sharedWorld = world;
        commandBuffer = sharedCommandBuffer;
    }

    private struct ChunkJobMeleeAttack : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobMeleeAttack(float deltaTime, CommandBuffer commandBuffer) : this()
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerAttack = ref chunk.GetFirst<MeleeAttackComponent>();
            ref var pointerCandidate = ref chunk.GetFirst<MeleeTargetCandidateComponent>();
            ref var pointerPos = ref chunk.GetFirst<PositionComponent>();
            ref var pointerChar = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerAnim = ref chunk.GetFirst<CharacterAnimationComponent>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref chunk.Entity(entityIndex);
                if (!entity.IsAlive())
                {
                    continue;
                }
                ref MeleeAttackComponent attack = ref Unsafe.Add(ref pointerAttack, entityIndex);
                ref MeleeTargetCandidateComponent candidate = ref Unsafe.Add(ref pointerCandidate, entityIndex);
                ref PositionComponent pos = ref Unsafe.Add(ref pointerPos, entityIndex);
                ref CharacterComponent character = ref Unsafe.Add(ref pointerChar, entityIndex);
                ref CharacterAnimationComponent anim = ref Unsafe.Add(ref pointerAnim, entityIndex);
                // 🛑 Validación: si el enemigo objetivo ya no es válido
                if (entity.Has<DeadComponent>() || entity.Has<PendingDestroyComponent>())
                {
                    continue;
                }
                if (!entity.Has<MeleeTargetCandidateComponent>())
                {
                    continue;
                }
                if (!candidate.target.IsAlive()) { continue; }

                if ( candidate.target.Has<DeadComponent>() || entity.Has<PendingDestroyComponent>())
                {
                    //if (candidate.target.Has<CharacterComponent>())
                    //{

                    //    _commandBuffer.Add<DeadComponent>(candidate.target);
                    //}
                    //if (candidate.target.Has<BuildingComponent>())
                    //{
                    //    _commandBuffer.Add<PendingDestroyComponent>(candidate.target);
                    //}
                    character.characterStateType = CharacterStateType.IDLE;
                    _commandBuffer.Remove<MeleeTargetCandidateComponent>(entity);
                    continue;
                }
                //if (candidate.timeToLive > 0f)
                //{
                //    candidate.timeToLive -= _deltaTime;
                //}
                //else
                //{
                //    // Tiempo expirado → limpiar target
                //    character.characterStateType = CharacterStateType.IDLE;
                //    _commandBuffer.Remove<MeleeTargetCandidateComponent>(entity);
                //    continue;
                //}
                // Si está fuera de rango, limpiar y permitir que Targeting busque de nuevo

                Vector2 positionTarget = candidate.target.Get<PositionComponent>().position;

                if (pos.position.DistanceTo(positionTarget) > attack.attackRange * 1)
                {
                    character.characterStateType = CharacterStateType.IDLE;
                    _commandBuffer.Remove<MeleeTargetCandidateComponent>(entity);
                    continue;
                }

                
                float distance = pos.position.DistanceTo(positionTarget);
                float ran = attack.attackRange*2;
                // ⚔️ Si está en rango y listo para atacar
                if (distance <= ran)
                {// 🔹 Esperar a que termine la animación antes de ejecutar ataque
                    character.characterStateType = CharacterStateType.ATTACK;
                    if (anim.animationComplete)
                    {
                       

                        ApplyDamage(entity, candidate.target, attack.damage);
                        character.characterStateType = CharacterStateType.IDLE;
                    }
                    
                }
             
                    
                
            }
        }

        private void ApplyDamage(Entity attacker, Entity target, int damage)
        {
            // Ejemplo simple: aplicar daño al target
            if (target.Has<HealthComponent>())
            {
                ref var health = ref target.Get<HealthComponent>();
                health.current -= damage;
                if (target.Has<CharacterComponent>())
                {
                    _commandBuffer.Add(target, new TakeHitComponent { stunTime = 0.1f });                 
                }
                if (health.current<=0)
                {
                    if (target.Has<CharacterComponent>())
                    {
                        
                        _commandBuffer.Add<DeadComponent>(target);
                    }
                    if (target.Has<BuildingComponent>())
                    {
                        _commandBuffer.Add<PendingDestroyComponent>(target);
                    }
                }
               
            }
           
        }
    }

    public override void Update(in float deltaTime)
    {
        using (new ProfileScope("MeleeAttackSystem"))
        {
            sharedWorld.InlineParallelChunkQuery(in queryAttack,
                new ChunkJobMeleeAttack(deltaTime, commandBuffer));
            //commandBuffer.Playback(World);
        }
    }
}
