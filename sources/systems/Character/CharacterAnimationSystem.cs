using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
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
    private QueryDescription query = new QueryDescription().WithAll<CharacterAnimationComponent, CharacterComponent,DirectionComponent, RenderGPUComponent>().WithNone<RenderGPULinkedComponent>();
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

               
                if (characterAnimationComponent.lastStateAnimation != stateAnimation)
                {
                    characterAnimationComponent.TimeSinceLastFrame = 0f;
                    characterAnimationComponent.animationComplete = false;
                    characterAnimationComponent.currentFrameIndex = 0;
                    characterAnimationComponent.active = true;
                    characterAnimationComponent.lastStateAnimation = stateAnimation;
                    characterAnimationComponent.frameDuration = animationData.frameDuration + characterComponent.speedAtackBase;                     
                }

                characterAnimationComponent.TimeSinceLastFrame += _deltaTime;
                if (characterAnimationComponent.TimeSinceLastFrame >= characterAnimationComponent.frameDuration && characterAnimationComponent.active)
                {
                    characterAnimationComponent.TimeSinceLastFrame = 0;
                    if (characterAnimationComponent.currentFrameIndex >= (animationData.frameDataArray.Length))
                    {
                        if (animationData.loop)
                        {
                            characterAnimationComponent.currentframeData.R = animationData.frameDataArray[0].x; // Reinicia al frame inicial
                            characterAnimationComponent.currentframeData.G = animationData.frameDataArray[0].y; // Reinicia al frame inicial
                            characterAnimationComponent.currentframeData.B = animationData.frameDataArray[0].widhtFormat; // Reinicia al frame inicial
                            characterAnimationComponent.currentframeData.A = animationData.frameDataArray[0].heightFormat; // Reinicia al frame inicial

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
                        characterAnimationComponent.currentframeData.B = animationData.frameDataArray[characterAnimationComponent.currentFrameIndex].widhtFormat; // Reinicia al frame inicial
                        characterAnimationComponent.currentframeData.A = animationData.frameDataArray[characterAnimationComponent.currentFrameIndex].heightFormat; // Reinicia al frame inicial
             
                        characterAnimationComponent.currentFrameIndex++;
                    }
                    RenderingServer.MultimeshInstanceSetCustomData(renderGPUComponent.rid, renderGPUComponent.instance, characterAnimationComponent.currentframeData);

                    
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
                
                ref CharacterAtackComponent characterAtackComponent = ref entity.Get<CharacterAtackComponent>();

                
                if (animComp.lastStateAnimation != stateAnimation)
                {
                    animComp.TimeSinceLastFrame = 0f;
                    animComp.animationComplete = false;
                    animComp.currentFrameIndex = 0;
                    animComp.active = true;
                    animComp.lastStateAnimation = stateAnimation;
                    animComp.frameDuration =  animData.frameDuration;
                    if (stateAnimation == 2)
                        animComp.frameDuration = charComp.speedAtackBase +animData.frameDuration;
                }

                animComp.TimeSinceLastFrame = animComp.TimeSinceLastFrame +_deltaTime;
                
                if (animComp.TimeSinceLastFrame >= animComp.frameDuration && animComp.active)
                {
                   
                    animComp.TimeSinceLastFrame = 0;

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
                        var linkedWeapon = renderLinked.instancedLinked[0];
                        Color colorWeapon = new Color(-1, -1, -1, -1);
                        if (characterAtackComponent.isAttack)
                        {                 
                            int frameWeapon = animComp.currentFrameIndex;                        
                            var animDataWeapon = charComp.accessoryArray[0].accesoryAnimationBodyData.animationStateData.animationData[(int)dirComp.animationDirection];
                            var frame = animDataWeapon.frameDataArray[frameWeapon];
                            colorWeapon = new Color(frame.x, frame.y, frame.widht, frame.height);
                                                    
                        }                                               
                        RenderingServer.MultimeshInstanceSetCustomData(linkedWeapon.rid, linkedWeapon.instance, colorWeapon);
                        RenderMainAndAccessories(animComp, renderGPU, renderLinked, charComp, stateAnimation);
                        animComp.currentFrameIndex++;
                                    
                    }             
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
            for (int i = 1; i < charComp.accessoryArray.Length; i++)
            {
                var item = charComp.accessoryArray[i];
                if (item == null) continue;

                var animData = item.accesoryAnimationBodyData.animationStateData.animationData[(int)dirComp.animationDirection];
                var frame = animData.frameDataArray[frameIndex];
                animComp.currentframeDataAccesorys[i] = new Color(frame.x, frame.y, frame.widhtFormat, frame.heightFormat);
            }
        }

        private static void RenderMainAndAccessories(CharacterAnimationComponent animComp, RenderGPUComponent renderGPU, RenderGPULinkedComponent renderLinked, CharacterComponent charComp, int stateAnimation)
        {
            RenderingServer.MultimeshInstanceSetCustomData(renderGPU.rid, renderGPU.instance, animComp.currentframeData);
                        
            //for (int i = 1; i < animComp.currentframeDataAccesorys.Length; i++)
            //{
            //    if (charComp.accessoryArray[i]==null)
            //    {
            //        continue;
            //    }
            //    Color color1 = animComp.currentframeDataAccesorys[i];
               

            //    var linked = renderLinked.instancedLinked[i];
            //    RenderingServer.MultimeshInstanceSetCustomData(linked.rid, linked.instance, color1);
            //}
        }
    }

  
    public override void Update(in float t)
    {

        World.InlineParallelChunkQuery(in query, new ChunkJobAnimationCharacter(commandBuffer, t));
        World.InlineParallelChunkQuery(in queryLinkedGpu, new ChunkJobAnimationAvatarCharacter(commandBuffer, t));
    }
}
