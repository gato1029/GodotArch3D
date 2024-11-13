using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[Component]
public struct PendingTransform
{    
}
[Component]
public struct Transform
{
    public Transform2D value;
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
    private QueryDescription queryPosition = new QueryDescription().WithAll<Transform, Position>();

    private QueryDescription queryRotationSprite = new QueryDescription().WithAll<PendingTransform, Transform, Rotation, Sprite>();
    private QueryDescription queryRotationCollider = new QueryDescription().WithAll<PendingTransform, Transform, Rotation>();

    private QueryDescription queryScale = new QueryDescription().WithAll<PendingTransform, Transform, Scale>();

    public TransformSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }


    private readonly struct JobPosition : IForEachWithEntity<Transform, Position>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public JobPosition(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Transform t, ref Position p)
        {
            t.value.Origin = p.value;
        }
    }

    private readonly struct JobRotation : IForEachWithEntity<Transform, Rotation, Collider>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public JobRotation(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Transform t, ref Rotation r, ref Collider c)
        {
            float gradRad = Mathf.DegToRad(r.value);
            if (entity.Has<MelleWeapon>())
            {
                entity.Get<MelleWeapon>().rect2Transform = CalculateTransformedRect(entity.Get<MelleWeapon>().rect2, gradRad);                
            }
            if (c.aplyRotation)
            {
                c.rectTransform = CalculateTransformedRect(c.rect, gradRad);
            }
            if (entity.Has<PendingTransform>())
            {
                _commandBuffer.Remove<PendingTransform>(entity);
            }
        }

    }

    private struct ChunkJobRotation : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobRotation(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            
            ref var pointerRotation = ref chunk.GetFirst<Rotation>();
            ref var pointerCollider = ref chunk.GetFirst<Collider>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Rotation r = ref Unsafe.Add(ref pointerRotation, entityIndex);
                ref Collider c = ref Unsafe.Add(ref pointerCollider, entityIndex);

                float gradRad = Mathf.DegToRad(r.value);
                if (entity.Has<MelleWeapon>())
                {
                    entity.Get<MelleWeapon>().rect2Transform = CalculateTransformedRect(entity.Get<MelleWeapon>().rect2, gradRad);
                }
                if (c.aplyRotation)
                {
                    c.rectTransform = CalculateTransformedRect(c.rect, gradRad);
                }
                if (entity.Has<PendingTransform>())
                {
                    _commandBuffer.Remove<PendingTransform>(entity);
                }
            }
        }
    }

    private static Rect2 CalculateTransformedRect(Rect2 originalRect,  float rotation)
    {
        Transform2D transformMatrix = new Transform2D(rotation, Vector2.Zero);

        Vector2 topLeft = originalRect.Position;
        Vector2 topRight = originalRect.Position + new Vector2(originalRect.Size.X, 0);
        Vector2 bottomLeft = originalRect.Position + new Vector2(0, originalRect.Size.Y);
        Vector2 bottomRight = originalRect.Position + originalRect.Size;

        
        Vector2 rotatedTopLeft = transformMatrix.BasisXform(topLeft);
        Vector2 rotatedTopRight = transformMatrix.BasisXform(topRight);
        Vector2 rotatedBottomLeft = transformMatrix.BasisXform(bottomLeft);
        Vector2 rotatedBottomRight = transformMatrix.BasisXform(bottomRight);


        float minX = Math.Min(Math.Min(rotatedTopLeft.X, rotatedTopRight.X), Math.Min(rotatedBottomLeft.X, rotatedBottomRight.X));
        float maxX = Math.Max(Math.Max(rotatedTopLeft.X, rotatedTopRight.X), Math.Max(rotatedBottomLeft.X, rotatedBottomRight.X));
        float minY = Math.Min(Math.Min(rotatedTopLeft.Y, rotatedTopRight.Y), Math.Min(rotatedBottomLeft.Y, rotatedBottomRight.Y));
        float maxY = Math.Max(Math.Max(rotatedTopLeft.Y, rotatedTopRight.Y), Math.Max(rotatedBottomLeft.Y, rotatedBottomRight.Y));

        
        Vector2 newSize = new Vector2((int)(maxX - minX), (int)(maxY - minY));
        Vector2 newPosition = new Vector2((int)minX, (int)minY);

        return new Rect2(newPosition, newSize);
    }
    public override void Update(in float t)
    {
        World.InlineParallelChunkQuery(in queryRotationCollider, new ChunkJobRotation(commandBuffer, t));
        commandBuffer.Playback(World);      
    }
}

