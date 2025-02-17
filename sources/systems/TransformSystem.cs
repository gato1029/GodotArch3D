using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Godot;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using GOD = Godot;

[Component]
public struct PendingTransform
{    

}

[Component]
public struct RefreshPositionOnce
{

}
[Component]
public struct RefreshScale
{

}
[Component]
public struct RefreshRotation
{

}

[Component]
public struct RefreshPositionAlways
{

}
[Component]
public struct RefreshScaleAlways
{

}
[Component]
public struct RefreshRotationAlways
{

}


[Component]
public struct Transform
{
    public Transform2D value;
    public Transform3D transformInternal;
}

[Component]
public struct Position
{
    public Vector2 value;   
}
[Component]
public struct Scale
{
    public Vector2 value;
}
[Component]
public struct Rotation
{
    public float value;
}

internal class TransformSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription queryPositionAlways = new QueryDescription().WithAll<Transform, Position, Sprite3D, RefreshPositionAlways>().WithNone<ColliderSprite>();
    private QueryDescription queryPositionOnce = new QueryDescription().WithAll<Transform, Position, Sprite3D, RefreshPositionOnce>().WithNone<ColliderSprite>();
    private QueryDescription queryPositionAlwaysCollider = new QueryDescription().WithAll<Transform, Position, Sprite3D, RefreshPositionAlways,ColliderSprite>();
    private QueryDescription queryPositionOnceCollider = new QueryDescription().WithAll<Transform, Position, Sprite3D, RefreshPositionOnce, ColliderSprite>();



    public TransformSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }
    private struct ChunkJobPositionAlways : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobPositionAlways(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();            
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);
                
                t.transformInternal.Origin = new Vector3(p.value.X , p.value.Y , (p.value.Y * CommonAtributes.LAYER_MULTIPLICATOR) + s.layer);                      

                if (s.idInstance==-1)
                {
                    RenderingServer.InstanceSetTransform(s.idRid, t.transformInternal);
                }
                else
                {
                    RenderingServer.MultimeshInstanceSetTransform(s.idRid, s.idInstance, t.transformInternal);
                }
                
            }

        }
    }

    private struct ChunkJobPositionAlwaysCollider : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobPositionAlwaysCollider(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();
            ref var pointerCollider = ref chunk.GetFirst<ColliderSprite>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);
                ref ColliderSprite colliderSprite = ref Unsafe.Add(ref pointerCollider, entityIndex);
                t.transformInternal.Origin = new Vector3(p.value.X, p.value.Y, ((p.value.Y + colliderSprite.shapeMove.OriginCurrent.Y) * -0.05f) + s.layer);                
                if (s.idInstance == -1)
                {
                    RenderingServer.InstanceSetTransform(s.idRid, t.transformInternal);
                }
                else
                {
                    RenderingServer.MultimeshInstanceSetTransform(s.idRid, s.idInstance, t.transformInternal);
                }

            }

        }
    }
    private struct ChunkJobPositionOnceCollider : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobPositionOnceCollider(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();
            ref var pointerCollider = ref chunk.GetFirst<ColliderSprite>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);
                ref ColliderSprite colliderSprite = ref Unsafe.Add(ref pointerCollider, entityIndex);

                t.transformInternal.Origin = new Vector3(p.value.X, p.value.Y, ((p.value.Y + colliderSprite.shapeMove.OriginCurrent.Y) * -0.05f) + s.layer);
                if (s.idInstance == -1)
                {
                    RenderingServer.InstanceSetTransform(s.idRid, t.transformInternal);
                }
                else
                {
                    RenderingServer.MultimeshInstanceSetTransform(s.idRid, s.idInstance, t.transformInternal);
                }
                if (entity.Has<RefreshPositionOnce>())
                {
                    _commandBuffer.Remove<RefreshPositionOnce>(entity);
                }
            }

        }
    }
    private struct ChunkJobPositionOnce : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobPositionOnce(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);            
                t.transformInternal.Origin =new Vector3(p.value.X ,p.value.Y, (p.value.Y * -0.05f)+s.layer);
                if (s.idInstance == -1)
                {
                    RenderingServer.InstanceSetTransform(s.idRid, t.transformInternal);
                }
                else
                {
                    RenderingServer.MultimeshInstanceSetTransform(s.idRid, s.idInstance, t.transformInternal);
                }
                if (entity.Has<RefreshPositionOnce>())
                {
                    _commandBuffer.Remove<RefreshPositionOnce>(entity);
                }
            }

        }
    }


   
    public override void Update(in float t)
    {              
        World.InlineParallelChunkQuery(in queryPositionAlways, new ChunkJobPositionAlways(commandBuffer, t));
        commandBuffer.Playback(World);
        World.InlineParallelChunkQuery(in queryPositionAlwaysCollider, new ChunkJobPositionAlwaysCollider(commandBuffer, t));
        commandBuffer.Playback(World);
        World.InlineParallelChunkQuery(in queryPositionOnce, new ChunkJobPositionOnce(commandBuffer, t));
        commandBuffer.Playback(World);
        World.InlineParallelChunkQuery(in queryPositionOnceCollider, new ChunkJobPositionOnceCollider(commandBuffer, t));
        commandBuffer.Playback(World);
    }
}

