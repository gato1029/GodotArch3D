using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


    internal class RemoveSystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;
        private QueryDescription queryRemove = new QueryDescription().WithAll<PendingRemove,Sprite3D>();
        private QueryDescription queryRemoveGPU = new QueryDescription().WithAll<PendingRemove, RenderGPUComponent, CharacterComponent>();
    public RemoveSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }
    private struct ChunkJobRemoveGpu : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly World _world;

        public ChunkJobRemoveGpu(CommandBuffer commandBuffer, float deltaTime, World world) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            _world = world;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerRenderGPUComponent = ref chunk.GetFirst<RenderGPUComponent>();
            ref var pointerCharacterComponent = ref chunk.GetFirst<CharacterComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref RenderGPUComponent renderGpuComponent = ref Unsafe.Add(ref pointerRenderGPUComponent, entityIndex);
                ref CharacterComponent characterComponent = ref Unsafe.Add(ref pointerCharacterComponent, entityIndex);      
                CharacterCreatorManager.Instance.RemoveCharacter(entity, characterComponent, renderGpuComponent.rid,renderGpuComponent.instance);
                _world.Destroy(entity);
            }
        }
    }
    private struct ChunkJob : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly World _world;

        public ChunkJob(CommandBuffer commandBuffer, float deltaTime, World world) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            _world = world;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerSprite = ref chunk.GetFirst<Sprite3D>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite, entityIndex);

                if (s.idInstance == -1)
                {
                    RenderingServer.FreeRid(s.idRid);                    
                }
                else
                {
                    if (s.spriteAnimation==0)
                    { // stack free posiciones
                        SpriteManager.Instance.SpriteMultimeshStore.AddFreeInstance(s.spriteAnimation, s.idInstance);
                    }
                    else
                    {
                        SpriteManager.Instance.SpriteAnimation.AddFreeInstance(s.spriteAnimation, s.idInstance);
                    }

                    RenderingServer.MultimeshInstanceSetCustomData(s.idRid, s.idInstance, new Color(-1, -1, -1, -1));                    
                }

                CollisionManager.Instance.dynamicCollidersEntities.RemoveItem(entity.Reference());                
                _world.Destroy(entity);
            }
        }
    }
    
    public override void AfterUpdate(in float t)
    {
        World.InlineParallelChunkQuery(in queryRemove, new ChunkJob(commandBuffer, t,World));
        World.InlineParallelChunkQuery(in queryRemoveGPU, new ChunkJobRemoveGpu(commandBuffer, t, World));
        commandBuffer.Playback(World);
    }
}
   
