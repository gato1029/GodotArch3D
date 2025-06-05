using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
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
    private QueryDescription queryLinkedGpu = new QueryDescription().WithAll<CharacterAnimationComponent, CharacterComponent, DirectionComponent, RenderGPUComponent, RenderGPULinkedComponent>();

    public CharacterAnimationSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }
    private struct ChunkJobAnimationCharacter : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobAnimationCharacter(CommandBuffer commandBuffer, float deltaTime) : this()
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
                AnimationStateData animationStateData = characterComponent.CharacterBaseData.animationCharacterBaseData.animationDataArray[stateAnimation];
                AnimationData animationData = animationStateData.animationData[(int)directionComponent.animationDirection];

                float frameDuration = characterAnimationComponent.frameDuration + animationData.frameDuration;


                characterAnimationComponent.TimeSinceLastFrame += _deltaTime;
                if (characterAnimationComponent.TimeSinceLastFrame >= frameDuration && characterAnimationComponent.active)
                {
                    if (characterAnimationComponent.currentFrameIndex >= (animationData.frameDataArray.Length))
                    {
                        if (animationData.loop)
                        {
                            characterAnimationComponent.currentframeData.R = animationData.frameDataArray[0].x; // Reinicia al frame inicial
                            characterAnimationComponent.currentframeData.G = animationData.frameDataArray[0].y; // Reinicia al frame inicial
                            characterAnimationComponent.currentframeData.B = animationData.frameDataArray[0].widht; // Reinicia al frame inicial
                            characterAnimationComponent.currentframeData.A = animationData.frameDataArray[0].height; // Reinicia al frame inicial

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
                        characterAnimationComponent.currentframeData.R = animationData.frameDataArray[characterAnimationComponent.currentFrameIndex].x; // Reinicia al frame inicial
                        characterAnimationComponent.currentframeData.G = animationData.frameDataArray[characterAnimationComponent.currentFrameIndex].y; // Reinicia al frame inicial
                        characterAnimationComponent.currentframeData.B = animationData.frameDataArray[characterAnimationComponent.currentFrameIndex].widht; // Reinicia al frame inicial
                        characterAnimationComponent.currentframeData.A = animationData.frameDataArray[characterAnimationComponent.currentFrameIndex].height; // Reinicia al frame inicial
             
                        characterAnimationComponent.currentFrameIndex++;
                    }

                    //int activeMirror = 0;
                    //if (animationData.mirrorHorizontal && directionComponent.animationDirection == AnimationDirection.LEFT)
                    //{
                    //    activeMirror = -1;
                    //}
                    RenderingServer.MultimeshInstanceSetCustomData(renderGPUComponent.rid, renderGPUComponent.instance, characterAnimationComponent.currentframeData);

                    characterAnimationComponent.TimeSinceLastFrame -= frameDuration;
                }

            }

        }
    }

    private struct ChunkJobAnimationAvatarCharacter : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobAnimationAvatarCharacter(CommandBuffer commandBuffer, float deltaTime) : this()
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
            ref var pointerRenderGPULinkedComponent = ref chunk.GetFirst<RenderGPULinkedComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref CharacterAnimationComponent animComp = ref Unsafe.Add(ref pointerCharacterAnimationComponent, entityIndex);
                ref CharacterComponent charComp = ref Unsafe.Add(ref pointerCharacterComponent, entityIndex);
                ref DirectionComponent dirComp = ref Unsafe.Add(ref pointerDirectionComponent, entityIndex);
                ref RenderGPUComponent renderGPU = ref Unsafe.Add(ref pointerRenderGPUComponent, entityIndex);
                ref RenderGPULinkedComponent renderLinked = ref Unsafe.Add(ref pointerRenderGPULinkedComponent, entityIndex);

                int stateAnimation = animComp.stateAnimation;
                AnimationStateData stateData = charComp.CharacterBaseData.animationCharacterBaseData.animationDataArray[stateAnimation];
                AnimationData animData = stateData.animationData[(int)dirComp.animationDirection];

                if (animComp.lastStateAnimation != stateAnimation)
                {
                    animComp.TimeSinceLastFrame = 0f;
                    animComp.animationComplete = false;
                    animComp.currentFrameIndex = 0;
                    animComp.active = true;
                    animComp.lastStateAnimation = stateAnimation;
                    animComp.frameDuration = animData.frameDuration;
                    if (stateAnimation == 2)
                        animComp.frameDuration += charComp.speedAtackBase;
                }

                animComp.TimeSinceLastFrame += _deltaTime;

                if (animComp.TimeSinceLastFrame >= animComp.frameDuration && animComp.active)
                {
                    animComp.TimeSinceLastFrame -= animComp.frameDuration;

                    if (animComp.currentFrameIndex >= animData.frameDataArray.Length)
                    {
                        if (animData.loop)
                        {
                            animComp.currentFrameIndex = 0;
                            animComp.animationComplete = true;
                            animComp.active = true;
                        }
                        else
                        {
                            animComp.animationComplete = true;
                            animComp.active = false;
                        }
                    }

                    if (animComp.active)
                    {
                        SetFrameData(ref animComp, animData, animComp.currentFrameIndex);
                        if (stateAnimation == 2)
                            SetAccessoryFrameData(ref animComp, charComp, dirComp, animComp.currentFrameIndex);
                        animComp.currentFrameIndex++;
                    }

                    RenderMainAndAccessories(animComp, renderGPU, renderLinked, stateAnimation);
                }
            }
        }

        private static void SetFrameData(ref CharacterAnimationComponent animComp, AnimationData animData, int frameIndex)
        {
            var frame = animData.frameDataArray[frameIndex];
            animComp.currentframeData = new Color(frame.x, frame.y, frame.widht, frame.height);
        }

        private static void SetAccessoryFrameData(ref CharacterAnimationComponent animComp, CharacterComponent charComp, DirectionComponent dirComp, int frameIndex)
        {
            for (int i = 0; i < charComp.accessoryArray.Length; i++)
            {
                var item = charComp.accessoryArray[i];
                if (item == null) continue;

                var animData = item.accesoryAnimationBodyData.animationStateData.animationData[(int)dirComp.animationDirection];
                var frame = animData.frameDataArray[frameIndex];
                animComp.currentframeDataAccesorys[i] = new Color(frame.x, frame.y, frame.widht, frame.height);
            }
        }

        private static void RenderMainAndAccessories(CharacterAnimationComponent animComp, RenderGPUComponent renderGPU, RenderGPULinkedComponent renderLinked, int stateAnimation)
        {
            RenderingServer.MultimeshInstanceSetCustomData(renderGPU.rid, renderGPU.instance, animComp.currentframeData);

            for (int i = 0; i < animComp.currentframeDataAccesorys.Length; i++)
            {
                var color = stateAnimation == 2 ? animComp.currentframeDataAccesorys[i] : new Color(-1, -1, -1, -1);
                var linked = renderLinked.instancedLinked[i];
                RenderingServer.MultimeshInstanceSetCustomData(linked.rid, linked.instance, color);
            }
        }
    }

  
    public override void Update(in float t)
    {

        World.InlineParallelChunkQuery(in query, new ChunkJobAnimationCharacter(commandBuffer, t));
        World.InlineParallelChunkQuery(in query, new ChunkJobAnimationAvatarCharacter(commandBuffer, t));
    }
}
