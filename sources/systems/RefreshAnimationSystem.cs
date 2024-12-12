using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems
{
    internal class RefreshAnimationSystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;
        private QueryDescription queryRefresh = new QueryDescription().WithAll<Sprite3D, Animation, Direction>();

        public RefreshAnimationSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }
        private struct JobRefreshAnimation : IChunkJob
        {
            private readonly float _deltaTime;
            private readonly CommandBuffer _commandBuffer;
            private readonly RandomNumberGenerator rng;
            public JobRefreshAnimation(CommandBuffer commandBuffer, float deltaTime) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
                rng = new RandomNumberGenerator();
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);

     
                ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();
             
                ref var pointerAnimation = ref chunk.GetFirst<Animation>();
                ref var pointerDirection = ref chunk.GetFirst<Direction>();
             
                foreach (var entityIndex in chunk)
                {
                    ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);            
                    ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);         
                    ref Animation a = ref Unsafe.Add(ref pointerAnimation, entityIndex);
                    ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);

                    if (a.updateAction!= a.currentAction )
                    {
                        a.currentAction = a.updateAction;
                        AnimationIndividual animationIndividual = SpriteManager.Instance.GetAnimation(s.idSpriteOrAnimation);
                        a.frameAnimation = animationIndividual.GetFrame(a.updateAction, (int)d.directionAnimation);
                        a.CurrentFrame   = a.frameAnimation.startFrame;
                        a.currentAction  = a.updateAction;
                        a.loop           = animationIndividual.GetTypeAnimation(a.updateAction).Looping;
                        a.complete = false;
                    }
                    else
                    {
                        if (a.complete && a.loop)
                        {
                            a.complete = false;                         
                        }
                    }
                }

            }


        }
        public override void Update(in float t)
        {
            World.InlineParallelChunkQuery(in queryRefresh, new JobRefreshAnimation(commandBuffer, t));
            commandBuffer.Playback(World);
        }
    }
}
