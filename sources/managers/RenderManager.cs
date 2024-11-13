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


internal class RenderManager : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription queryDebug = new QueryDescription().WithAll<Debug, Position>();
    private QueryDescription queryRender = new QueryDescription().WithAll<Sprite,Position, Transform>();
    private QueryDescription queryDebugCollider = new QueryDescription().WithAll<ColliderDebug, Position, Transform>();
    
    public RenderManager(World world) : base(world)
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

            ref var pointerSprite = ref chunk.GetFirst<Sprite>();
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Sprite s = ref Unsafe.Add(ref pointerSprite, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                t.value.Origin = p.value;
                RenderingServer.CanvasItemSetTransform(s.CanvasItem, t.value);

            }
        }
    }
    private readonly struct RenderJob : IForEachWithEntity< Sprite, Position, Transform>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public RenderJob(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Sprite sp, ref Position pos, ref Transform t)
        {
            t.value.Origin = pos.value;                     
            RenderingServer.CanvasItemSetTransform(sp.CanvasItem, t.value);
            
        }
    }

    private readonly struct RenderColliderDebugJob : IForEachWithEntity<ColliderDebug, Position, Transform, Rotation, Collider>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public RenderColliderDebugJob(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref ColliderDebug c, ref Position pos, ref Transform t, ref Rotation r, ref Collider col)
        {
            float gradRad = Mathf.DegToRad(r.value);
            Transform2D transform = new Transform2D(gradRad, pos.value);
            RenderingServer.CanvasItemSetTransform(c.canvasItemColliderMelle, transform);
            if (col.aplyRotation)
            {
                RenderingServer.CanvasItemSetTransform(c.canvasItemCollider, transform);
            }
            else
            {
                RenderingServer.CanvasItemSetTransform(c.canvasItemCollider, t.value);
            }
            
        }

    }


    private readonly struct RenderJobDebug : IForEachWithEntity<Debug, Position, Direction>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public RenderJobDebug(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Debug sp, ref Position pos, ref Direction d)
        {
            Vector2 posNew = pos.value + GetRotatedPointByDirection(sp.offset, Vector2.Zero, d.value);
            
            Transform2D transform = 
                new Transform2D(d.value.Angle(), posNew);
            //transform = transform.RotatedLocal(Mathf.Pi / 2);
            RenderingServer.CanvasItemSetTransform(sp.CanvasItem, transform);
            //GD.Print("rect:"+sp.rect.Position);
            //GD.Print("rect center:" + sp.rect.GetCenter());
            //GD.Print("posicion transform:" + transform.Origin);
            //GD.Print("posicion pos:" + pos.value);
         
        }

    }

    public static Vector2 GetRotatedPointByDirection(Vector2 point, Vector2 origin, Vector2 direction)
    {
        // Normalizar la dirección para obtener un vector unitario
        direction = direction.Normalized();

        // Calcular el vector perpendicular para el eje de la rotación
        Vector2 perpDirection = new Vector2(-direction.Y, direction.X);

        // Mover el punto al origen (trasladar)
        Vector2 relativePoint = point - origin;

        // Aplicar la rotación usando la dirección y su perpendicular
        Vector2 rotatedPoint = origin + (relativePoint.X * direction + relativePoint.Y * perpDirection);

        return rotatedPoint;
    }

    public override void Update(in float t)
    {
        //var job = new RenderJob((float)t, commandBuffer);
        //World.InlineEntityQuery<RenderJob,Sprite, Position,Transform>(in queryRender, ref job);

        World.InlineParallelChunkQuery(in queryRender, new ChunkJobRender(commandBuffer, t));

        //var jobDebug = new RenderJobDebug((float)t, commandBuffer);
        //World.InlineEntityQuery<RenderJobDebug, Debug, Position, Direction>(in queryDebug, ref jobDebug);

        var jobDebugCollider = new RenderColliderDebugJob((float)t, commandBuffer);
        World.InlineEntityQuery<RenderColliderDebugJob, ColliderDebug, Position, Transform, Rotation, Collider>(in queryDebugCollider, ref jobDebugCollider);
    }

    public override void AfterUpdate(in float t)
    {
     
        
    }
}

