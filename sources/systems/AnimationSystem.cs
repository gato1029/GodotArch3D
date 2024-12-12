using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



internal class AnimationSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription queryAnimation = new QueryDescription().WithAll<Animation,Sprite3D,Transform,Direction>();


    public AnimationSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }
    private struct ChunkJobAnimation : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobAnimation(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerSprite = ref chunk.GetFirst<Sprite3D>();            
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            ref var pointerAnimation = ref chunk.GetFirst<Animation>();
            ref var pointerDirection = ref chunk.GetFirst<Direction>();


            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);

                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite, entityIndex);
                ref Animation animation = ref Unsafe.Add(ref pointerAnimation, entityIndex);
                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);

                animation.TimeSinceLastFrame += _deltaTime;
                if (animation.TimeSinceLastFrame >= animation.TimePerFrame  )
                {
                    

                    //animation.StartFrame + animation.TotalFrames
                    if (animation.CurrentFrame >= (animation.frameAnimation.startFrame + animation.frameAnimation.totalFrames))
                    {
                        if (animation.loop)
                        {
                            animation.CurrentFrame = animation.frameAnimation.startFrame; // Reinicia al frame inicial
                            animation.complete = true;
                        }
                        else
                        {
                            animation.complete = true;
                        }
                    }
                    else
                    {
                        animation.CurrentFrame++;
                    }
                
                   
                    int flipX = 0;
                    if (animation.horizontalOrientation!= 0 && Math.Sign(d.value.X)!= animation.horizontalOrientation)
                    {
                        flipX = 1;
                    }
                    RenderingServer.MultimeshInstanceSetCustomData(s.idRid, s.idInstance, new Color(animation.CurrentFrame, flipX, 0, 0));
                    animation.TimeSinceLastFrame -= animation.TimePerFrame;
                }

            }
        }
    }
    public override void Update(in float t)
    {

        World.InlineParallelChunkQuery(in queryAnimation, new ChunkJobAnimation(commandBuffer, t));
        commandBuffer.Playback(World);
    }
}

