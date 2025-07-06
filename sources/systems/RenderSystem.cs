using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[Component]
public struct Debug
{
    public Rid CanvasItem;
    public Rect2 rect;
    public Vector2 offset;
}


[Component]
public struct IsRender
{

}

[Component]
public struct Sprite
{
    public Rid CanvasItem;
    public Rid Texture;
    public Rid Material;
    public Rect2 rect;
    public Rect2 rectTransform;
}



internal class RenderSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
   
    private QueryDescription queryRender = new QueryDescription().WithAll<PositionComponent,RenderGPUComponent>().WithNone<RenderGPULinkedComponent>();
    private QueryDescription queryRenderLinked = new QueryDescription().WithAll<CharacterComponent, PositionComponent, RenderGPUComponent,RenderGPULinkedComponent>();

    public RenderSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();        
    }

    private struct ChunkJobRender : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobRender(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerPositionComponent = ref chunk.GetFirst<PositionComponent>();
            ref var pointerRenderComponent = ref chunk.GetFirst<RenderGPUComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref PositionComponent   positionComponent = ref Unsafe.Add(ref pointerPositionComponent, entityIndex);
                ref RenderGPUComponent  renderComponent = ref Unsafe.Add(ref pointerRenderComponent, entityIndex);
                float renderZ = ((positionComponent.position.Y + renderComponent.originOffset.Y + renderComponent.zOrdering) * CommonAtributes.LAYER_MULTIPLICATOR) + renderComponent.layerRender;
                renderComponent.transform.Origin = new Vector3(positionComponent.position.X, positionComponent.position.Y, renderZ );                
                RenderingServer.MultimeshInstanceSetTransform(renderComponent.rid, renderComponent.instance, renderComponent.transform);       
                
            }

        }
    }

    private struct ChunkJobRenderLinked : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobRenderLinked(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerCharacterComponent = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerPositionComponent = ref chunk.GetFirst<PositionComponent>();
            ref var pointerRenderComponent = ref chunk.GetFirst<RenderGPUComponent>();
            ref var pointerRenderLinkedComponent = ref chunk.GetFirst<RenderGPULinkedComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref PositionComponent positionComponent = ref Unsafe.Add(ref pointerPositionComponent, entityIndex);
                ref RenderGPUComponent renderComponent = ref Unsafe.Add(ref pointerRenderComponent, entityIndex);
                ref CharacterComponent characterComponent = ref Unsafe.Add(ref pointerCharacterComponent, entityIndex);
                float renderZ = ((positionComponent.position.Y +renderComponent.originOffset.Y + renderComponent.zOrdering) * CommonAtributes.LAYER_MULTIPLICATOR) + renderComponent.layerRender ;
                
                renderComponent.transform.Origin = new Vector3(positionComponent.position.X, positionComponent.position.Y, renderZ);
                RenderingServer.MultimeshInstanceSetTransform(renderComponent.rid, renderComponent.instance, renderComponent.transform);

                //if (characterComponent.activeTool)
                //{
                //    GpuInstance item = pointerRenderLinkedComponent.instancedLinked[0];
                //    RenderingServer.MultimeshInstanceSetTransform(item.rid, item.instance, renderComponent.transform);
                //}
             
                for (int i = 0; i < pointerRenderLinkedComponent.instancedLinked.Length; i++)
                {
                    GpuInstance item = pointerRenderLinkedComponent.instancedLinked[i];
                    if (item.rid.IsValid)
                    {
                        RenderingServer.MultimeshInstanceSetTransform(item.rid, item.instance, renderComponent.transform);
                    }                    
                }

            }

        }
    }
    public override void Update(in float t)
    {
        using (new ProfileScope("Render System"))
        {
            World.InlineParallelChunkQuery(in queryRender, new ChunkJobRender(commandBuffer, t));
            World.InlineParallelChunkQuery(in queryRenderLinked, new ChunkJobRenderLinked(commandBuffer, t));
        }
        
    }

    public override void AfterUpdate(in float t)
    {
     
        
    }
}

