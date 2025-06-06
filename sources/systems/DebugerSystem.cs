using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;

using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
    private QueryDescription query = new QueryDescription().WithAll<ColliderSprite>().WithNone<ColliderDebug>();
    private QueryDescription queryDebugSprite = new QueryDescription().WithAll<Sprite3D, Transform,ColliderSprite>();
    private QueryDescription queryDebugDirectionSprite = new QueryDescription().WithAll<Transform,Direction>();

    private QueryDescription queryDebugMelleCollider = new QueryDescription().WithAll<Position, Direction, MelleCollider>();


    private QueryDescription queryDirection = new QueryDescription().WithAll<DirectionComponent, PositionComponent>();
    private QueryDescription queryColliderCharacter = new QueryDescription().WithAll<CharacterComponent,CharacterColliderComponent>();

    public DebugerSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private struct ChunkJobDebugColliderCharacter : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobDebugColliderCharacter(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerCharacterComponent = ref chunk.GetFirst<CharacterComponent>();
            ref var pointerPositionComponent = ref chunk.GetFirst<PositionComponent>();
            
            //ref var pointerPosition = ref chunk.GetFirst<Position>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref CharacterComponent characterComponent = ref Unsafe.Add(ref pointerCharacterComponent, entityIndex);
                ref PositionComponent positionComponent = ref Unsafe.Add(ref pointerPositionComponent, entityIndex);

                float scale = characterComponent.CharacterBaseData.scale;
                if (characterComponent.CharacterBaseData.animationCharacterBaseData.collisionBody is Rectangle)
                {
                    Rectangle shape = (Rectangle)characterComponent.CharacterBaseData.animationCharacterBaseData.collisionBody;
                    Transform3D transform3DShape = new Transform3D(Basis.Identity, Vector3.Zero);
                    transform3DShape = transform3DShape.Scaled(new Vector3(shape.Width * scale, shape.Height * scale, 1));
                    transform3DShape.Origin = new Vector3(positionComponent.position.X + shape.OriginCurrent.X * scale, positionComponent.position.Y + shape.OriginCurrent.Y * scale, 1);
                    
                    DebugDraw.Quad(transform3DShape, 1, Colors.Red, 0.0f); 
                }

             
                if (characterComponent.CharacterBaseData.animationCharacterBaseData.collisionMove is Rectangle)
                {
                    Rectangle shape2 = (Rectangle)characterComponent.CharacterBaseData.animationCharacterBaseData.collisionMove;
                    Transform3D transform3DShape2 = new Transform3D(Basis.Identity, Vector3.Zero);
                    transform3DShape2 = transform3DShape2.Scaled(new Vector3(shape2.Width*scale, shape2.Height * scale, 1));
                    transform3DShape2.Origin = new Vector3(positionComponent.position.X + shape2.OriginCurrent.X * scale, positionComponent.position.Y + shape2.OriginCurrent.Y * scale, 1);
                    DebugDraw.Quad(transform3DShape2, 1, Colors.Green, 0.0f); //debug
                }
           
                DebugDraw.Quad(new Vector3(positionComponent.position.X, positionComponent.position.Y, 1), .2f, Colors.DarkOrange, 0.0f); //center          

                if (characterComponent.accessoryArray[0]!=null)
                {
                    var accesoryCollision = characterComponent.accessoryArray[0];
                    if (accesoryCollision.hasBodyAnimation)
                    {
                        for (int i=0; i<= Enum.GetNames(typeof(DirectionAnimationType)).Length; i++)
                        {
                            var animacionData =accesoryCollision.accesoryAnimationBodyData.animationStateData.animationData[i];
                            if (animacionData.hasCollider)
                            {
                                
                                Rectangle shapeWeapon = (Rectangle)animacionData.collider;
                                Transform3D transform3DShape2 = new Transform3D(Basis.Identity, Vector3.Zero);
                                transform3DShape2 = transform3DShape2.Scaled(new Vector3(shapeWeapon.Width * scale, shapeWeapon.Height * scale, 1));
                                transform3DShape2.Origin = new Vector3(positionComponent.position.X + shapeWeapon.OriginCurrent.X * scale, positionComponent.position.Y + shapeWeapon.OriginCurrent.Y * scale, 1);
                                DebugDraw.Quad(transform3DShape2, 1, Colors.Blue, 0.0f); //debug
                            }
                        }
                        
                    }
                    
                }
            }

        }
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
            ref var pointerCollider = ref chunk.GetFirst<ColliderSprite>();
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Transform t = ref Unsafe.Add(ref pointerTransform, entityIndex);
                ref ColliderSprite c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                ref Position position = ref Unsafe.Add(ref pointerPosition, entityIndex);

                if (c.shapeBody is Rectangle)
                {
                    Rectangle shape = (Rectangle)c.shapeBody;
                    Transform3D transform3DShape = new Transform3D(Basis.Identity, Vector3.Zero);
                    transform3DShape = transform3DShape.Scaled(new Vector3(shape.Width, shape.Height, 1));
                    transform3DShape.Origin = new Vector3(position.value.X + shape.OriginCurrent.X, position.value.Y + shape.OriginCurrent.Y, 1);
                    DebugDraw.Quad(transform3DShape, 1, Colors.Red, 0.0f); //debug
                }


                //transform3D.Origin = new Vector3(t.transformInternal.Origin.X + c.rectTransform.Position.X, t.transformInternal.Origin.Y + c.rectTransform.Position.Y, t.transformInternal.Origin.Z);
                if (c.shapeMove is Rectangle)
                {
                    Rectangle shape2 = (Rectangle)c.shapeMove;
                    Transform3D transform3DShape2 = new Transform3D(Basis.Identity, Vector3.Zero);
                    transform3DShape2 = transform3DShape2.Scaled(new Vector3(shape2.Width, shape2.Height, 1));
                    transform3DShape2.Origin = new Vector3(position.value.X + shape2.OriginCurrent.X, position.value.Y + shape2.OriginCurrent.Y, 1);
                    DebugDraw.Quad(transform3DShape2, 1, Colors.Green, 0.0f); //debug
                }
                if (c.shapeMove is Circle)
                {
                    Circle shape2 = (Circle)c.shapeMove;
                    Transform3D transform3DShape2 = new Transform3D(Basis.Identity, Vector3.Zero);
                    transform3DShape2 = transform3DShape2.Scaled(new Vector3(1,1, 1));
                    transform3DShape2.Origin = new Vector3(position.value.X + shape2.OriginCurrent.X, position.value.Y + shape2.OriginCurrent.Y, 1);
                    DebugDraw.Sphere(transform3DShape2, shape2.Radius, Colors.Green, 0.0f); //debug
                }
                DebugDraw.Quad(t.transformInternal, .1f, Colors.BlueViolet, 0.0f); //center          

                //Transform3D transform3DAABB = new Transform3D(Basis.Identity, Vector3.Zero);
                //transform3DAABB = transform3DAABB.Scaled(new Vector3(c.aabbMove.Size.X, c.aabbMove.Size.Y, 1));
                //transform3DAABB.Origin = new Vector3(c.aabbMove.Position.X, c.aabbMove.Position.Y, t.transformInternal.Origin.Z);
                //DebugDraw.Quad(transform3DAABB, 1, Colors.WebGreen, 0.0f); //debug

            }

        }
    }
    private struct ChunkJobDebugDirectionComponent : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobDebugDirectionComponent(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPositionComponent = ref chunk.GetFirst<PositionComponent>();
            ref var pointerDirectionComponent = ref chunk.GetFirst<DirectionComponent>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref PositionComponent t = ref Unsafe.Add(ref pointerPositionComponent, entityIndex);
                ref DirectionComponent d = ref Unsafe.Add(ref pointerDirectionComponent, entityIndex);
                DebugDraw.Arrow(new Vector3(t.position.X, t.position.Y,1), new Vector3(d.value.X, d.value.Y, 1), 1, Colors.Aqua, 0);
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


                if (melleCollider.shapeCollider is Rectangle)
                {
                    Rectangle shape = (Rectangle)melleCollider.shapeCollider;
                    Transform3D transform3DShape = new Transform3D(Basis.Identity, Vector3.Zero);
                    transform3DShape = transform3DShape.Scaled(new Vector3(shape.Width, shape.Height, 1));
                    transform3DShape.Origin = new Vector3(position.value.X + shape.OriginCurrent.X, position.value.Y + shape.OriginCurrent.Y, 1);
                    DebugDraw.Quad(transform3DShape, 1, Colors.DarkBlue, 0.0f); //debug
                }

                //////Vector2 pp = (( melleCollider.collider.rectTransform.Position));
                //Vector2 posAtack = position.value + NuevaPosicion( melleCollider.collider.rectTransform.Position, direction.value) ;

                //Transform3D transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
                //transform3D = transform3D.Scaled(new Vector3(melleCollider.collider.rectTransform.Size.X, melleCollider.collider.rectTransform.Size.Y, 1));
                //transform3D.Origin = new Vector3(posAtack.X, posAtack.Y,5);

                //DebugDraw.Quad(transform3D, 1, Colors.DarkBlue, 0.0f); //debug       

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
        bool debug = Input.IsActionJustPressed("debugColliders");
        if (debug)
        {
            DebugActive = !DebugActive;
            DebugTiles();
        }

        if (DebugActive)
        {
            World.InlineParallelChunkQuery(in queryDebugSprite, new ChunkJobDebugColliderSprite(commandBuffer, t));
            World.InlineParallelChunkQuery(in queryDebugDirectionSprite, new ChunkJobDebugDirection(commandBuffer, t));
            World.InlineParallelChunkQuery(in queryDebugMelleCollider, new ChunkJobDebugMelleCollider(commandBuffer, t));

            World.InlineParallelChunkQuery(in queryColliderCharacter, new ChunkJobDebugColliderCharacter(commandBuffer, t));
            World.InlineParallelChunkQuery(in queryDirection, new ChunkJobDebugDirectionComponent(commandBuffer, t));
            
        }

    }

    private void DebugTiles()
    {
        foreach (var item in CollisionManager.Instance.tileColliders.cellMap)
        {
            foreach (var item2 in item.Value)
            {
                GeometricShape2D collision =item2.Value.collisionBody;
                
                if (collision is Rectangle)
                {
                    int scale = 1;
                    Rectangle shape = (Rectangle)collision;
                    Transform3D transform3DShape = new Transform3D(Basis.Identity, Vector3.Zero);
                    transform3DShape = transform3DShape.Scaled(new Vector3(shape.Width * scale, shape.Height * scale, 1));
                    transform3DShape.Origin = new Vector3(item2.Value.positionCollider.X + shape.OriginCurrent.X * scale, item2.Value.positionCollider.Y + shape.OriginCurrent.Y * scale, 1);

                    DebugDraw.Quad(transform3DShape, 1, Colors.DarkRed, 10.0f);
                }
            }
            
        } 
    }
}

