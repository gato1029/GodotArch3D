using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;

using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Tilemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;





namespace GodotEcsArch.sources.systems
{
    [Component]
    public struct MainCharacter
    {

    }
    [Component]
    public struct ActionUnit
    {
        public bool isAtack;
        public bool isTakeDamage;
        public bool isWalking;
    }

    internal class MainCharacterSystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;
        private QueryDescription query = new QueryDescription().WithAll<MainCharacter, StateComponent, Position, Sprite3D, RefreshPositionAlways, Animation, Direction, Rotation, Velocity, ColliderSprite>();


        public MainCharacterSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }
        private struct JobCharacter : IChunkJob
        {
            private readonly float _deltaTime;
            private readonly CommandBuffer _commandBuffer;
            private readonly RandomNumberGenerator rng;
            public JobCharacter(CommandBuffer commandBuffer, float deltaTime) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
                rng = new RandomNumberGenerator();
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);
                
                ref var pointerPosition = ref chunk.GetFirst<Position>();
                ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();
                ref var pointerAnimation = ref chunk.GetFirst<Animation>();
                ref var pointerDirection = ref chunk.GetFirst<Direction>();
                ref var pointerRotation = ref chunk.GetFirst<Rotation>();
                ref var pointerVelocity = ref chunk.GetFirst<Velocity>();
                ref var pointerCollider = ref chunk.GetFirst<ColliderSprite>();
                ref var pointerStateComponent = ref chunk.GetFirst<StateComponent>();
                foreach (var entityIndex in chunk)
                {
                    ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                    ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                    ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);
                    ref Animation a = ref Unsafe.Add(ref pointerAnimation, entityIndex);
                    ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);
                    ref Rotation r = ref Unsafe.Add(ref pointerRotation, entityIndex);
                    ref Velocity v = ref Unsafe.Add(ref pointerVelocity, entityIndex);
                    ref ColliderSprite c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                    ref StateComponent stateComponent = ref Unsafe.Add(ref pointerStateComponent, entityIndex);

                    Vector2 moveDirection = Vector2.Zero;
                    bool attack = false;
                    if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_up"))
                    { moveDirection.Y += 1; }
                    if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_down"))
                    { moveDirection.Y -= 1; }
                    if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_left"))
                    { moveDirection.X -= 1; }
                    if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_right"))
                    { moveDirection.X += 1; }
                    if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("attack"))
                    { attack = true; }

                    if (stateComponent.currentType == StateType.EXECUTE_ATTACK)
                    {
                        CallAttack(entity, ref p, ref d);
                    }

                    if ((stateComponent.currentType == StateType.IDLE || stateComponent.currentType == StateType.MOVING) &&  attack)
                    {
                        stateComponent.currentType = StateType.ATTACK;
                    }

                    if ((stateComponent.currentType == StateType.IDLE || stateComponent.currentType == StateType.MOVING) && moveDirection != Vector2.Zero)
                    {                      
                        moveDirection = moveDirection.Normalized();
                        CallMove(entity, moveDirection, ref stateComponent, ref d, ref r, ref a, ref v, ref p, ref c);                   
                    }

                    if ((stateComponent.currentType == StateType.IDLE || stateComponent.currentType == StateType.MOVING) && moveDirection == Vector2.Zero)
                    {
                        stateComponent.currentType =StateType.IDLE;
                    }
                }

            }

            private void CallMove(Entity entity, Vector2 moveDirection, ref StateComponent stateComponent, ref Direction d,ref Rotation r, ref Animation a, ref Velocity v, ref Position p, ref ColliderSprite c)
            {
                Entity entityCharacter = default;
                ref Relationship<CharacterWeapon> parentOfRelation = ref entity.GetRelationships<CharacterWeapon>();
                
                foreach (var child in parentOfRelation)
                {
                    entityCharacter = child.Key;
                }
                CharacterWeapon characterWeapon = parentOfRelation.Get(entityCharacter);
                ref Position positionCharacter = ref entityCharacter.Get<Position>();

                if ( d.value != moveDirection)
                {
                    d.directionAnimation = CommonOperations.GetDirectionAnimation(moveDirection);
                    d.value = moveDirection;
                    r.value = Mathf.RadToDeg(d.value.Angle());
                    d.normalized = new Vector2(Math.Sign(moveDirection.X), Math.Sign(moveDirection.Y));
                    ref MelleCollider melleAtack = ref entity.TryGetRef<MelleCollider>(out bool exist);

                    // luego ponerlo en una solo funcion

                    melleAtack.shapeCollider = characterWeapon.shapeColliderLeftRight;

                    if (d.directionAnimation == AnimationDirection.UP || d.directionAnimation == AnimationDirection.DOWN)
                    {
                        melleAtack.shapeCollider = characterWeapon.shapeColliderTopDown; 
                    }
                    
                    Rectangle rectangle = (Rectangle)melleAtack.shapeCollider; 

                    //rectangle.DirectionTo(moveDirection.X, moveDirection.Y);
                    
                    Vector2 vector2 = Collision2D.RotatePosition(rectangle.OriginRelative, d.normalized);
                    rectangle.OriginCurrent = vector2;

                    ref Direction directionCharacter = ref entityCharacter.Get<Direction>();

                    directionCharacter.value = d.value;
                    directionCharacter.normalized = d.normalized;
                    directionCharacter.directionAnimation = d.directionAnimation;
                }
                
                Vector2 movement = d.value * v.value * _deltaTime;
                Vector2 movementNext = p.value + movement + c.shapeMove.OriginCurrent;

                
                bool existCollision = false;

                Rect2 aabb = new Rect2(movementNext, c.shapeMove.GetSizeQuad()*2);
                Dictionary<int, Dictionary<int, Entity>> data = CollisionManager.Instance.dynamicCollidersEntities.QueryAABB(aabb);
                if (data != null)
                {
                    foreach (var item in data.Values)
                    {
                        foreach (var itemInternal in item)
                        {
                            if (itemInternal.Value.Id != entity.Id)
                            {
                                ColliderSprite colliderB = itemInternal.Value.Get<ColliderSprite>();
                                var positionB = itemInternal.Value.Get<Position>().value + colliderB.shapeMove.OriginCurrent;                                
                                if (Collision2D.Collides(c.shapeMove,colliderB.shapeMove,movementNext,positionB))
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
                }

                if (!existCollision)
                {
                    Dictionary<int, Dictionary<int, IDataTile>> dataTile = CollisionManager.Instance.tileColliders.QueryAABB(aabb);
                    if (dataTile != null)
                    {
                        foreach (var item in dataTile.Values)
                        {
                            foreach (var itemInternal in item)
                            {

                                //GeometricShape2D colliderB = itemInternal.Value.collisionBody;
                                //var positionB = itemInternal.Value.positionCollider + itemInternal.Value.collisionBody.OriginCurrent;
                                //if (Collision2D.Collides(c.shapeMove, colliderB, movementNext, positionB))
                                //{
                                //    existCollision = true;
                                //    break;
                                //}

                            }
                            if (existCollision)
                            {
                                break;
                            }
                        }
                    }
                }

                if (!existCollision)
                {
                    stateComponent.currentType = StateType.MOVING;                    
                    p.value += movement;
                    positionCharacter.value = p.value;
                }
                else
                {
                    stateComponent.currentType = StateType.IDLE;                   
                }
            }
       
            private void CallAttack(Entity entity, ref Position p, ref Direction d)
            {
                if (entity.Has<MelleCollider>())
                {
                    Unit unit = entity.Get<Unit>();
                    MelleCollider melleAtack = entity.Get<MelleCollider>();

                    Rectangle rectangle = (Rectangle)melleAtack.shapeCollider;

                    Vector2 pp = melleAtack.shapeCollider.OriginCurrent;
                    Vector2 posAtack = (p.value + pp) ;

                   

                    Rect2 aabb = new Rect2(posAtack, rectangle.Width,rectangle.Height);
                    Dictionary<int, Dictionary<int, Entity>> data = CollisionManager.Instance.dynamicCollidersEntities.QueryAABB(aabb);
                    if (data != null)
                    {
                        foreach (var item in data.Values)
                        {
                            foreach (var itemInternal in item)
                            {
                                Entity entB = itemInternal.Value;
                                if (itemInternal.Value.Id != entity.Id && unit.team != entB.Get<Unit>().team)
                                {
                                    var colliderB = itemInternal.Value.Get<ColliderSprite>();
                                    var positionB = itemInternal.Value.Get<Position>().value;
                                    if (Collision2D.Collides(rectangle, colliderB.shapeBody, posAtack, positionB))
                                    {
                                        ref StateComponent stateComponentB = ref entB.TryGetRef<StateComponent>(out bool exist);

                                        stateComponentB.currentType = StateType.TAKE_HIT;
                                        BehaviorManager.Instance.AplyDamage(entity, entB);
                                       
                                    }

                                }
                            }                         
                        }
                    }                  
                }
              
            }
        }

        public override void Update(in float t)
        {
            //if (RenderWindowGui.Instance.IsActive)
            {
                World.InlineParallelChunkQuery(in query, new JobCharacter(commandBuffer, t));
                commandBuffer.Playback(World);
            }
        }
    }
}
