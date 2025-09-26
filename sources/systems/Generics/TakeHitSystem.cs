using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Combat
{
    internal class TakeHitSystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;

        private QueryDescription queryTakeHit = new QueryDescription()
            .WithAll<TakeHitComponent, CharacterComponent>();

        public TakeHitSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }

        private struct ChunkJobTakeHit : IChunkJob
        {
            private readonly CommandBuffer _commandBuffer;

            public ChunkJobTakeHit(CommandBuffer commandBuffer) : this()
            {
                _commandBuffer = commandBuffer;
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);
                ref var pointerCharacter = ref chunk.GetFirst<CharacterComponent>();
                ref var pointerTakeHit = ref chunk.GetFirst<TakeHitComponent>();

                foreach (var i in chunk)
                {
                    ref Entity entity = ref Unsafe.Add(ref pointerEntity, i);
                    ref CharacterComponent character = ref Unsafe.Add(ref pointerCharacter, i);
                    ref TakeHitComponent takeHit = ref Unsafe.Add(ref pointerTakeHit, i);

                    // Cambiar estado a animación de recibir golpe
                    character.characterStateType = CharacterStateType.TAKE_HIT;
                    character.hitStunTimer = takeHit.stunTime;
                    //GD.Print($"Entidad {entity.Id} entra en estado TAKE_HIT por {takeHit.stunTime}s");

                    // Eliminar el evento después de procesarlo (one-shot)
                    _commandBuffer.Remove<TakeHitComponent>(entity);
                }
            }
        }

        public override void Update(in float t)
        {
            World.InlineParallelChunkQuery(in queryTakeHit, new ChunkJobTakeHit(commandBuffer));
            commandBuffer.Playback(World);
        }
    }
}
