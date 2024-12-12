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
using static Godot.TextServer;

[Component]
public struct ColliderDebug
{
    public Rid canvasItemCollider;
    public Rid canvasItemColliderMelle;
    public Rect2 rect;
}
[Component]
public struct DebugEntity
{
    
}
internal class DebugerSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<Collider>().WithNone<ColliderDebug>();
    private QueryDescription queryDebugSprite = new QueryDescription().WithAll<Sprite3D, Transform,Collider>();
    private QueryDescription queryDebugDirectionSprite = new QueryDescription().WithAll<Transform,Direction>();

    private QueryDescription queryDebugMelleCollider = new QueryDescription().WithAll<Position, Direction, MelleCollider>();

    private QueryDescription queryDesactive = new QueryDescription().WithAll<Collider, ColliderDebug>();

    public DebugerSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private struct ChunkJobDebugColliderSprite : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobDebugColliderSprite(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);  
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            ref var pointerCollider = ref chunk.GetFirst<Collider>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                ref Collider c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                
                Transform3D transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
                transform3D = transform3D.Scaled(new Vector3(c.rect.Size.X, c.rect.Size.Y, 1));
                transform3D.Origin = new Vector3(t.transformInternal.Origin.X + c.rectTransform.Position.X, t.transformInternal.Origin.Y + c.rectTransform.Position.Y, t.transformInternal.Origin.Z);
                DebugDraw.Quad(transform3D, 1, Colors.Red, 0.0f); //debug
                DebugDraw.Quad(t.transformInternal, .1f, Colors.Green, 0.0f); //debug          

            }

        }
    }
    private struct ChunkJobDebugDirection : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobDebugDirection(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerTransform = ref chunk.GetFirst<Transform>();
            ref var pointerDirection = ref chunk.GetFirst<Direction>();
           
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);               
                DebugDraw.Arrow(t.transformInternal.Origin, new Vector3(d.value.X,d.value.Y,1),1,Colors.Aqua,0);                  
            }
        }
    }

    private struct ChunkJobDebugMelleCollider : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobDebugMelleCollider(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            ref var pointerMelleCollider = ref chunk.GetFirst<MelleCollider>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Position position = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Direction direction = ref Unsafe.Add(ref pointerDirection, entityIndex);
                ref MelleCollider melleCollider = ref Unsafe.Add(ref pointerMelleCollider, entityIndex);

                ////Vector2 pp = (( melleCollider.collider.rectTransform.Position));
                Vector2 posAtack = position.value + NuevaPosicion( melleCollider.collider.rectTransform.Position, direction.value) ;

                Transform3D transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
                transform3D = transform3D.Scaled(new Vector3(melleCollider.collider.rectTransform.Size.X, melleCollider.collider.rectTransform.Size.Y, 1));
                transform3D.Origin = new Vector3(posAtack.X, posAtack.Y,5);

                DebugDraw.Quad(transform3D, 1, Colors.DarkBlue, 0.0f); //debug       
                
            }
        }

        public Vector2 NuevaPosicion(Vector2 posicionOriginal, Vector2 direccion)
        {
           
            float angle = Mathf.Atan2(direccion.Y, direccion.X);

  
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);

            float x = posicionOriginal.X * cosAngle - posicionOriginal.Y * sinAngle;
            float y = posicionOriginal.X * sinAngle + posicionOriginal.Y * cosAngle;

         
            return new Vector2(x, y);
        }
        public Rect2 RotarRect(Rect2 rect, Vector2 direccion)
        {
          
            float angle = Mathf.Atan2(direccion.Y, direccion.X);

          
            Vector2[] vertices = GetRectVertices(rect);

          
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);

            Vector2[] nuevosVertices = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 p = vertices[i];

                // Aplicar la rotación
                float x = p.X * cosAngle - p.Y * sinAngle;
                float y = p.X * sinAngle + p.Y * cosAngle;


            nuevosVertices[i] = new Vector2(x, y);
            }


            return new Rect2(nuevosVertices[0], nuevosVertices[2] - nuevosVertices[0]);
        }
        private Vector2[] GetRectVertices(Rect2 rect)
        {
            // Calcular los vértices de un Rect2
            Vector2[] vertices = new Vector2[4];
            vertices[0] = rect.Position;
            vertices[1] = rect.Position + new Vector2(rect.Size.X, 0);
            vertices[2] = rect.Position + rect.Size;
            vertices[3] = rect.Position + new Vector2(0, rect.Size.Y);
            return vertices;
        }
    }

    bool DebugActive = false;
    public override void Update(in float t)
    {
        bool debug = ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("debugColliders");
        if (debug)
        {
            DebugActive = !DebugActive;
        }

        if (DebugActive)
        {
            World.InlineParallelChunkQuery(in queryDebugSprite, new ChunkJobDebugColliderSprite(commandBuffer, t));
            World.InlineParallelChunkQuery(in queryDebugDirectionSprite, new ChunkJobDebugDirection(commandBuffer, t));
            World.InlineParallelChunkQuery(in queryDebugMelleCollider, new ChunkJobDebugMelleCollider(commandBuffer, t));
        }

    }

  

}

