using Arch.Buffer;
using Arch.Core;
using Arch.System;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System.Runtime.CompilerServices;

internal class DeathSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;

    private QueryDescription queryDead = new QueryDescription()
        .WithAll<DeadComponent, CharacterComponent>();

    public DeathSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
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

            foreach (var i in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, i);
                ref CharacterComponent character = ref Unsafe.Add(ref pointerCharacter, i);

                // Cambiar estado a DIE
                character.characterStateType = CharacterStateType.DIE;

                // Destruir entidad después de animación
                // (aquí podrías usar un temporizador o esperar evento de animación)
               // _commandBuffer.Destroy(entity);
            }
        }
    }

    public override void Update(in float t)
    {
        World.InlineParallelChunkQuery(in queryDead, new ChunkJobDeath(commandBuffer));
        commandBuffer.Playback(World);
    }
}
