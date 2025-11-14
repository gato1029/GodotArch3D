using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems.Character;
internal class CharacterStateSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;

    private QueryDescription query = new QueryDescription()
        .WithAll<CharacterComponent, CharacterAnimationComponent>();

    public CharacterStateSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private struct ChunkJobState : IChunkJob
    {
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobState(CommandBuffer commandBuffer)
        {
            _commandBuffer = commandBuffer;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerChar = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerAnim = ref chunk.GetFirst<CharacterAnimationComponent>();

            foreach (var i in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, i);
                ref CharacterComponent character = ref Unsafe.Add(ref pointerChar, i);
                ref CharacterAnimationComponent animation = ref Unsafe.Add(ref pointerAnim, i);

                var rules = CharacterStateConfig.GetRules(character.behaviorType);

                // 🔹 1. Transición de estado si la animación terminó
                if (animation.animationComplete)
                {
                    //var newState = rules.OnAnimationComplete?.Invoke(character, animation);
                    //if (newState.HasValue && newState.Value != character.characterStateType)
                    //{
                    //    character.characterStateType = newState.Value;
                    //}


                }

                // 🔹 2. Seleccionar animación según el estado
                if (rules.StateToAnimation.TryGetValue(character.characterStateType, out int newAnim))
                {
                    if (newAnim != animation.stateAnimation)
                    {
                        animation.lastStateAnimation = animation.stateAnimation;
                        animation.stateAnimation = newAnim;
                        animation.currentFrameIndex = 0;       // reset animación
                        animation.TimeSinceLastFrame = 0f;     // reset timer
                        animation.animationComplete = false;   // empezar de nuevo
                    }
                }
            }
        }
    }

    public override void Update(in float t)
    {
        World.InlineParallelChunkQuery(in query, new ChunkJobState(commandBuffer));
        commandBuffer.Playback(World);
    }
}