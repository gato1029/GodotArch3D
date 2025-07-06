using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems;
public class AnimationTileSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<TileAnimation>();

    public AnimationTileSystem(World world) : base(world)
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

            ref var pointerTileAnimation = ref chunk.GetFirst<TileAnimation>();
          
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref TileAnimation tileAnimation = ref Unsafe.Add(ref pointerTileAnimation, entityIndex);
                var tileDataAnimation = TilesManager.Instance.GetTileDataAnimation(tileAnimation.TileAnimateData.id);
                tileAnimation.TimeSinceLastFrame += _deltaTime;
                if (tileAnimation.TimeSinceLastFrame >= tileAnimation.TileAnimateData.frameDuration && tileAnimation.loop)
                {       
                    if (tileAnimation.currentFrameIndex >= (tileAnimation.TileAnimateData.idFrames.Length))
                    {
                        if (tileAnimation.loop)
                        {
                            tileAnimation.frameRender.R = tileDataAnimation.framesArray[0].x;
                            tileAnimation.frameRender.G = tileDataAnimation.framesArray[0].y;
                            tileAnimation.frameRender.B = tileDataAnimation.framesArray[0].widht;
                            tileAnimation.frameRender.A = tileDataAnimation.framesArray[0].height;

                            //tileAnimation.currentFrame = tileAnimation.TileAnimateData.idFrames[0]; // Reinicia al frame inicial
                            tileAnimation.complete = true;
                            tileAnimation.currentFrameIndex = 0;
                        }
                        else
                        {
                            tileAnimation.complete = true;
                            tileAnimation.loop = false;
                        }
                    }
                    else
                    {
                        tileAnimation.frameRender.R = tileDataAnimation.framesArray[tileAnimation.currentFrameIndex].x;
                        tileAnimation.frameRender.G = tileDataAnimation.framesArray[tileAnimation.currentFrameIndex].y;
                        tileAnimation.frameRender.B = tileDataAnimation.framesArray[tileAnimation.currentFrameIndex].widht;
                        tileAnimation.frameRender.A = tileDataAnimation.framesArray[tileAnimation.currentFrameIndex].height;

                      //  tileAnimation.currentFrame = tileAnimation.TileAnimateData.idFrames[tileAnimation.currentFrameIndex];
                        tileAnimation.currentFrameIndex++;
                    }                                                      
                    RenderingServer.MultimeshInstanceSetCustomData(tileAnimation.rid, tileAnimation.instance, tileAnimation.frameRender);
                    
                    tileAnimation.TimeSinceLastFrame -= tileAnimation.TileAnimateData.frameDuration;
                }

            }

        }
    }

    public override void Update(in float t)
    {

        World.InlineParallelChunkQuery(in query, new ChunkJobAnimationTile(commandBuffer, t));
        //commandBuffer.Playback(World);
    }

}
