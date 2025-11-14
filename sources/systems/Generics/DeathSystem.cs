using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System.Runtime.CompilerServices;

internal class DeathSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private World sharedWorld;
    private QueryDescription queryDead = new QueryDescription()
        .WithAll<DeadComponent, CharacterComponent>().WithNone<PendingDestroyComponent>();

    public DeathSystem(World world, CommandBuffer sharedCommandBuffer) : base(world)
    {
        sharedWorld = world;
        commandBuffer = sharedCommandBuffer; 
    }

    private struct ChunkJobDeath : IChunkJob
    {
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobDeath(CommandBuffer commandBuffer) : this()
        {
            _commandBuffer = commandBuffer;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerCharacter = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerAnim = ref chunk.GetFirst<CharacterAnimationComponent>();

            foreach (var i in chunk)
            {                
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, i);
                if (!entity.IsAlive())
                {
                    continue;
                }
                ref CharacterComponent character = ref Unsafe.Add(ref pointerCharacter, i);
                ref CharacterAnimationComponent anim = ref Unsafe.Add(ref pointerAnim, i);

                // 🔹 Cambiar estado a DIE si aún no lo está
                if (character.characterStateType != CharacterStateType.DIE)
                {
                    character.characterStateType = CharacterStateType.DIE;
                    //anim.stateAnimation = 3; // asumiendo que 3 es animación de morir
                    //anim.currentFrameIndex = 0;
                    //anim.TimeSinceLastFrame = 0f;
                    //anim.animationComplete = false;
                }

                // 🔹 Cuando la animación de muerte termine → marcar PendingRemove
                if (anim.animationComplete)
                {
                    if (!entity.Has<PendingDestroyComponent>())
                    {
                        _commandBuffer.Add<PendingDestroyComponent>(entity);
                    }
                }
            }
        }
    }

    public override void Update(in float t)
    {
        sharedWorld.InlineParallelChunkQuery(in queryDead, new ChunkJobDeath(commandBuffer));
        //commandBuffer.Playback(World);
    }
}
