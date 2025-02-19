using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Generic;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems.Character;
internal class CharacterAnimationSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<CharacterAnimationComponent, CharacterComponent,DirectionComponent, RenderGPUComponent>();

    public CharacterAnimationSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }
    private struct ChunkJobAnimationTile : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobAnimationTile(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerCharacterAnimationComponent = ref chunk.GetFirst<CharacterAnimationComponent>();
            ref var pointerCharacterComponent = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerDirectionComponent = ref chunk.GetFirst<DirectionComponent>();
            ref var pointerRenderGPUComponent = ref chunk.GetFirst<RenderGPUComponent>();


            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref CharacterAnimationComponent characterAnimationComponent = ref Unsafe.Add(ref pointerCharacterAnimationComponent, entityIndex);
                ref CharacterComponent characterComponent = ref Unsafe.Add(ref pointerCharacterComponent, entityIndex);
                ref DirectionComponent directionComponent = ref Unsafe.Add(ref pointerDirectionComponent, entityIndex);
                ref RenderGPUComponent renderGPUComponent = ref Unsafe.Add(ref pointerRenderGPUComponent, entityIndex);

                int stateAnimation = characterAnimationComponent.stateAnimation;
                AnimationStateData animationStateData = characterComponent.CharacterBaseData.animationDataArray[stateAnimation];
                AnimationData animationData = animationStateData.animationData[(int)directionComponent.animationDirection];

                characterAnimationComponent.TimeSinceLastFrame += _deltaTime;
                if (characterAnimationComponent.TimeSinceLastFrame >= characterAnimationComponent.frameDuration && characterAnimationComponent.active)
                {
                    if (characterAnimationComponent.currentFrameIndex >= (animationData.idFrames.Length))
                    {
                        if (animationStateData.loop)
                        {
                            characterAnimationComponent.currentFrame = animationData.idFrames[0]; // Reinicia al frame inicial
                            characterAnimationComponent.animationComplete = true;
                            characterAnimationComponent.currentFrameIndex = 0;
                            characterAnimationComponent.active = true;
                        }
                        else
                        {
                            characterAnimationComponent.animationComplete = true;
                            characterAnimationComponent.active = false;
                        }
                    }
                    else
                    {
                        characterAnimationComponent.currentFrame = animationData.idFrames[characterAnimationComponent.currentFrameIndex];
                        characterAnimationComponent.currentFrameIndex++;
                    }

                    int activeMirror = 0;
                    if (animationStateData.mirrorHorizontal && directionComponent.animationDirection == AnimationDirection.LEFT)
                    {
                        activeMirror = -1;
                    }
                    RenderingServer.MultimeshInstanceSetCustomData(renderGPUComponent.rid, renderGPUComponent.instance, new Color(characterAnimationComponent.currentFrame, activeMirror, 0, 0));
                    characterAnimationComponent.TimeSinceLastFrame -= characterAnimationComponent.frameDuration;
                }

            }

        }
    }

    public override void Update(in float t)
    {

        World.InlineParallelChunkQuery(in query, new ChunkJobAnimationTile(commandBuffer, t));
     
    }
}
