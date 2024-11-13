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



public enum MovementType
{
    CIRCLE,SQUARE,CIRCLE_STATIC,SQUARE_STATIC
}
[Component]
public struct AreaMovement
{
    public uint value;
    public uint value2;
    public Vector2 origin;
    public MovementType type;
}
[Component]
public struct TargetMovement
{
    public Vector2 value;    
}
[Component]
public struct Direction
{
    public Vector2 value;
}
[Component]
public struct Velocity
{
    public float value;
}


[Component]
public struct HumanController
{

}

[Component]
public struct IAController
{ 
}
[Component]
public struct SelectedController
{

}
[Component]
public struct ThirdPersonController
{

}
internal class MovementSystem: BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription queryIA = new QueryDescription().WithAll<IAController,Position,Velocity, TargetMovement,Direction,Collider, Rotation>();
    private QueryDescription queryHuman = new QueryDescription().WithAll<HumanController, Position, Velocity,  Direction, Collider, Rotation>();
    public MovementSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private struct ChunkJobMovementIA : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobMovementIA(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerVelocity = ref chunk.GetFirst<Velocity>();
            ref var pointerTargetMovement = ref chunk.GetFirst<TargetMovement>();
            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            ref var pointerCollider = ref chunk.GetFirst<Collider>();
         
           
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Velocity v = ref Unsafe.Add(ref pointerVelocity, entityIndex);
                ref TargetMovement tm = ref Unsafe.Add(ref pointerTargetMovement, entityIndex);
                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);
                ref Collider c = ref Unsafe.Add(ref pointerCollider, entityIndex);
               

             
                Vector2 movement = d.value * v.value * _deltaTime;

             
                Vector2 movementNext = p.value + movement;

                var entityInternal = c.rect.Size;
                var resultList = CollisionManager.dynamicCollidersEntities.GetPossibleQuadrants(movementNext, entityInternal.X);
                bool existCollision = false;
                foreach (var itemMap in resultList)
                {
                    foreach (var item in itemMap.Value)
                    {
                        if (item.Key != entity.Id)
                        {
                            Entity entB = item.Value;
                            var entityExternal = entB.Get<Collider>().rect;
                            var entityExternalPos = entB.Get<Position>().value;
                            var direction = entB.Get<Direction>().value;
                            if (CollisionManager.CheckAABBCollision(movementNext, c.rect, entityExternalPos, entityExternal))
                            {
                                existCollision = true;
                                break;
                            }
                        }
                    }
                    if (existCollision)
                    {
                        break;
                    }
                }

                if (!existCollision)
                {
                    // 4. Actualizar la posición de la entidad
                  

                    // 5. Verificar si la entidad ha llegado al objetivo
                    float distanceToTarget = (tm.value - p.value).Length();
                    float movementDistance = v.value * _deltaTime;
                    if (movementDistance >= distanceToTarget)
                    {
                        p.value = tm.value;
                        _commandBuffer.Remove<TargetMovement>(entity);
                        _commandBuffer.Add<SearchTargetMovement>(entity);
                    }
                    else
                    {
                        p.value += movement;
                    }
                }

            }
        }
    }

    private readonly struct ProcessJobHuman : IForEachWithEntity<Position, Velocity, Direction, Rotation, Collider>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly InputHandler _inputHandler;

        public ProcessJobHuman(float deltaTime, CommandBuffer commandBuffer, InputHandler inputHandler)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;
            _inputHandler = inputHandler;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Position p, ref Velocity v, ref Direction d, ref Rotation r, ref Collider c)
        {
            Vector2 moveDirection = Vector2.Zero;

            if (_inputHandler.IsActionActive("move_up"))
                moveDirection.Y -= 1;
            if (_inputHandler.IsActionActive("move_down"))
                moveDirection.Y += 1;
            if (_inputHandler.IsActionActive("move_left"))
                moveDirection.X -= 1;
            if (_inputHandler.IsActionActive("move_right"))
                moveDirection.X += 1;
            if (moveDirection != Vector2.Zero)
            {
                moveDirection = moveDirection.Normalized();
                d.value = moveDirection;

                if (!entity.Has<PendingTransform>() && r.value != Mathf.RadToDeg(d.value.Angle()))
                {
                    r.value = Mathf.RadToDeg(d.value.Angle());
                    _commandBuffer.Add<PendingTransform>(entity);
                }

                Vector2 movement = d.value * v.value * _deltaTime;
                Vector2 movementNext = p.value + movement;                
                var resultList = CollisionManager.dynamicCollidersEntities.GetPossibleQuadrants(movementNext, 128);
                bool existCollision = false;
                foreach (var itemMap in resultList)
                {
                    foreach (var item in itemMap.Value)
                    {
                        if (item.Key != entity.Id)
                        {
                            Entity entB = item.Value;

                            var entityExternal = entB.Get<Collider>().rectTransform;
                            var entityExternalPos = entB.Get<Position>().value;

                            if (CollisionManager.CheckAABBCollision(movementNext, c.rectTransform, entityExternalPos, entityExternal))
                            {
                                existCollision = true;
                                break;
                            }
                        }
                    }

                }
                if (!existCollision)
                {
                    p.value += movement;
                }


            }
            if (_inputHandler.IsActionActive("attack"))
            {
                if (!entity.Has<OrderAtack>())
                {
                    _commandBuffer.Add<OrderAtack>(entity);
                }
            }
        }
    }
    public override void Update(in float t)
    {
        
        World.InlineParallelChunkQuery(in queryIA, new ChunkJobMovementIA(commandBuffer, t));
        commandBuffer.Playback(World);

        var job = new ProcessJobHuman(TimeGodot.Delta, commandBuffer, ServiceLocator.Instance.GetService<InputHandler>());
        World.InlineEntityQuery<ProcessJobHuman, Position, Velocity, Direction, Rotation, Collider>(in queryHuman, ref job);
        commandBuffer.Playback(World);
    }
}

