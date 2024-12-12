using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;



public enum MovementType
{
    CIRCLE,SQUARE,CIRCLE_STATIC,SQUARE_STATIC
}
[Component]
public struct AreaMovement
{
    public uint widthRadius;
    public uint height;
    public Vector2 origin;
    public MovementType type;
}
[Component]
public struct TargetMovement
{
    public bool arrive;
    public Vector2 value;    
}
[Component]
public struct Direction
{
    public Vector2 value;
    public DirectionAnimation directionAnimation;
}
[Component]
public struct Velocity
{
    public float value;
}



internal class MovementSystem: BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription queryBack = new QueryDescription().WithAll<UnitController, Position,Velocity,Direction,Collider, Rotation,RefreshPositionAlways>();
    private QueryDescription query = new QueryDescription().WithAll<StateComponent, Position, Velocity, Direction, Collider, Rotation, RefreshPositionAlways, TargetMovement>();
    public MovementSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private struct ChunkJobMovementIA : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly RandomNumberGenerator rng;
        public ChunkJobMovementIA(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            rng = new RandomNumberGenerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerVelocity = ref chunk.GetFirst<Velocity>();
            
            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            ref var pointerRotation = ref chunk.GetFirst<Rotation>();
            ref var pointerCollider = ref chunk.GetFirst<Collider>();
            ref var pointerAnimation = ref chunk.GetFirst<Animation>();
            ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();
            ref var pointerUnitController = ref chunk.GetFirst<UnitController>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Velocity v = ref Unsafe.Add(ref pointerVelocity, entityIndex);
           
                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);
                ref Rotation r = ref Unsafe.Add(ref pointerRotation, entityIndex);
                ref Collider c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                ref Animation a = ref Unsafe.Add(ref pointerAnimation, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);
                ref UnitController unitController = ref Unsafe.Add(ref pointerUnitController, entityIndex);


                switch (unitController.controllerMode)
                {
                    case ControllerMode.HUMAN:
                        break;
                    case ControllerMode.IA:
                        bool walkPosible = true;

                        if ((a.updateAction == AnimationAction.STUN || a.updateAction == AnimationAction.HIT || a.updateAction == AnimationAction.ATACK) && !a.complete)
                        {
                            walkPosible = false;
                        }
                        else
                        {
                            if (walkPosible)
                            {
                                //a.updateAction = AnimationAction.IDLE;
                                CallIAMovement(entity, ref a, ref unitController.iaController, ref p, ref d, ref r, ref v, ref c, rng, _deltaTime);
                            }
                        }
                                               
                        break;
                    default:
                        break;
                }            
            }
        }

        private void CallIAMovement(Entity entity, ref Animation animation,ref IAController iaController, ref Position position, ref Direction direction, ref Rotation rotation, ref Velocity velocity, ref Collider collider, RandomNumberGenerator rng, float deltaTime)
        {
            if (iaController.targetMovement.arrive)
            {
                //search new position to go
                Vector2 newpoint = CommonOperations.SearchNewPosition(iaController.areaMovement, position, rng);
                Vector2 targetDirection = (newpoint - position.value).Normalized();
                direction.value = targetDirection;
                rotation.value = Mathf.RadToDeg(targetDirection.Angle());
                iaController.targetMovement.value = newpoint;
                iaController.targetMovement.arrive = false;
                //animation.updateAction = AnimationAction.IDLE;
            }
            else
            {
                Vector2 movement = direction.value * velocity.value * deltaTime;
                Vector2 movementNext = position.value + movement;

                var entityInternal = collider.rect.Size;
                var resultList = CollisionManager.Instance.dynamicCollidersEntities.GetPossibleQuadrants(movementNext, 4);
                bool existCollision = false;
                foreach (var itemMap in resultList)
                {
                    foreach (var item in itemMap.Value)
                    {
                        if (item.Key != entity.Id)
                        {
                            Entity entB = item.Value;
                            var colliderB = entB.Get<Collider>();
                            var entityExternal = entB.Get<Collider>().rect;
                            var entityExternalPos = entB.Get<Position>().value;
                            if (CollisionManager.Instance.CheckAABBCollision(movementNext, collider, entityExternalPos, colliderB))
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
                // to do radio busqueda para atacar
                // 1. si esta en radio la posicion de objetivo se actualiza
                // 2. verifica el radio de ataque del arma, si esta dentro del ataque cambia comportamiento a ataque

                if (!existCollision)
                {

                    float distanceToTarget = (iaController.targetMovement.value - position.value).Length();
                    float movementDistance = velocity.value * deltaTime;
                    if (movementDistance >= distanceToTarget)
                    {
                        iaController.targetMovement.arrive = true;
                        position.value = iaController.targetMovement.value;
                      
                    }
                    else
                    {
                        animation.updateAction = AnimationAction.WALK;
                        position.value += movement;
                    }
                }
                else
                {
                    if ((animation.updateAction == AnimationAction.STUN || animation.updateAction == AnimationAction.HIT || animation.updateAction == AnimationAction.ATACK) && animation.complete)
                    {
                        animation.updateAction = AnimationAction.IDLE;
                        
                    }
                    
                }
            }
        }
    }

    private struct ChunkJobMovement : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly RandomNumberGenerator rng;
        public ChunkJobMovement(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            rng = new RandomNumberGenerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerVelocity = ref chunk.GetFirst<Velocity>();
            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            ref var pointerRotation = ref chunk.GetFirst<Rotation>();
            ref var pointerCollider = ref chunk.GetFirst<Collider>();                        
            ref var pointerTargetMovement = ref chunk.GetFirst<TargetMovement>();
            ref var pointerStateComponent = ref chunk.GetFirst<StateComponent>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Velocity v = ref Unsafe.Add(ref pointerVelocity, entityIndex);

                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);
                ref Rotation r = ref Unsafe.Add(ref pointerRotation, entityIndex);
                ref Collider c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                ref StateComponent stateComponent = ref Unsafe.Add(ref pointerStateComponent, entityIndex);

                ref TargetMovement tm = ref Unsafe.Add(ref pointerTargetMovement, entityIndex);


                Vector2 movement = d.value * v.value * _deltaTime;
                Vector2 movementNext = p.value + movement;
               
                var resultList = CollisionManager.Instance.dynamicCollidersEntities.GetPossibleQuadrants(movementNext, 4);
                bool existCollision = false;
                foreach (var itemMap in resultList)
                {
                    foreach (var item in itemMap.Value)
                    {
                        if (item.Key != entity.Id)
                        {
                            Entity entB = item.Value;
                            var colliderB = entB.Get<Collider>();
                            var entityExternal = entB.Get<Collider>().rect;
                            var entityExternalPos = entB.Get<Position>().value;
                            var direction = entB.Get<Direction>().value;
                            if (CollisionManager.Instance.CheckAABBCollision(movementNext, c, entityExternalPos, colliderB))
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
         
                    // 5. Verificar si la entidad ha llegado al objetivo
                    float distanceToTarget = (tm.value - p.value).Length();
                    float movementDistance = v.value * _deltaTime;
                    if (movementDistance >= distanceToTarget)
                    {
                        p.value = tm.value;
                        _commandBuffer.Remove<TargetMovement>(entity);
                        stateComponent.currentType = StateType.IDLE;
                    }
                    else
                    {                      
                        p.value += movement;                                               
                    }
                }
                else
                {                  
                    stateComponent.currentType = StateType.IDLE;
                }
             

            }
        }
    }

    public override void Update(in float t)
    {
        
        World.InlineParallelChunkQuery(in query, new ChunkJobMovement(commandBuffer, t));
        commandBuffer.Playback(World);

    }
}

