using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using GodotEcsArch.sources.managers.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;


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
        private QueryDescription query = new QueryDescription().WithAll<MainCharacter, StateComponent, Position, Sprite3D, RefreshPositionAlways, Animation, Direction, Rotation, Velocity, Collider>();


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
                ref var pointerCollider = ref chunk.GetFirst<Collider>();
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
                    ref Collider c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                    ref StateComponent stateComponent = ref Unsafe.Add(ref pointerStateComponent, entityIndex);

                    Vector2 moveDirection = Vector2.Zero;
                    bool attack = false;
                    if (Input.IsActionPressed("move_up"))
                    { moveDirection.Y += 1; }
                    if (Input.IsActionPressed("move_down"))
                    { moveDirection.Y -= 1; }
                    if (Input.IsActionPressed("move_left"))
                    { moveDirection.X -= 1; }
                    if (Input.IsActionPressed("move_right"))
                    { moveDirection.X += 1; }
                    if (Input.IsActionPressed("attack"))
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

            private void CallMove(Entity entity, Vector2 moveDirection, ref StateComponent stateComponent, ref Direction d,ref Rotation r, ref Animation a, ref Velocity v, ref Position p, ref Collider c)
            {
               
                d.value = moveDirection;
                d.directionAnimation = CommonOperations.GetDirectionAnimation(d.value);

                if ( r.value != Mathf.RadToDeg(d.value.Angle()))
                {
                    
                    r.value = Mathf.RadToDeg(d.value.Angle());
                    float angle = Mathf.Atan2(moveDirection.Y, moveDirection.X);
                    ref MelleCollider melleAtack = ref entity.TryGetRef<MelleCollider>(out bool exist);
                    //melleAtack.collider.rectTransform= RotateRect2(melleAtack.collider.rect, moveDirection);
                    //GD.Print(melleAtack.collider.rectTransform);
                    //GD.Print(Mathf.RadToDeg(angle));
                    //GD.Print(r.value);
                    //GD.Print(Mathf.RadToDeg(dd));
                }
                
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
                            var entityExternal = entB.Get<Collider>().rectTransform;
                            var entityExternalPos = entB.Get<Position>().value;

                            if (CollisionManager.Instance.CheckAABBCollision(movementNext, c, entityExternalPos, colliderB))
                            {
                                existCollision = true;
                                break;
                            }
                        }
                    }

                }
                if (!existCollision)
                {
                    stateComponent.currentType = StateType.MOVING;                    
                    p.value += movement;
                }
                else
                {
                    stateComponent.currentType = StateType.IDLE;
                }
            }


            public static Rect2 RotateRect2(Rect2 originalRect, Vector2 direction)
            {


                float angle = Mathf.Atan2(direction.Y, direction.X );


                Vector2 center = originalRect.Position + originalRect.Size / 2;
                float cosAngle = Mathf.Cos(angle);
                float sinAngle = Mathf.Sin(angle);

                float newX = center.X * cosAngle - center.Y * sinAngle;
                float newY = center.X * sinAngle + center.Y * cosAngle;

                Vector2 newPosition = new Vector2(newX, newY) - originalRect.Size / 2;
               
                Rect2 nuevoRect = new Rect2(newPosition, originalRect.Size);

                return nuevoRect;
            }

            private void CallAttack(Entity entity, ref Position p, ref Direction d)
            {
                if (entity.Has<MelleCollider>())
                {
                    Unit unit = entity.Get<Unit>();
                    MelleCollider melleAtack = entity.Get<MelleCollider>();
                    Vector2 pp = melleAtack.collider.rectTransform.Position;
                    Vector2 posAtack = (p.value + pp) ;
                    var result = CollisionManager.Instance.dynamicCollidersEntities.GetPossibleQuadrants(posAtack, 4);
                    if (result != null)
                    {
                        foreach (var itemDic in result)
                        {
                            foreach (var item in itemDic.Value)
                            {
                                Entity entB = item.Value;
                                if (item.Key != entity.Id && unit.team != entB.Get<Unit>().team)
                                {
                                    var colliderB = entB.Get<Collider>();
                                    var positionB = entB.Get<Position>().value;

                                    if (CollisionManager.Instance.CheckAABBCollision2(posAtack, melleAtack.collider, positionB, colliderB))
                                    {
                                        ref StateComponent stateComponentB = ref entB.TryGetRef<StateComponent>(out bool exist);

                                        stateComponentB.currentType = StateType.TAKE_STUN;
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
            if (RenderWindowGui.Instance.IsActive)
            {
                World.InlineParallelChunkQuery(in query, new JobCharacter(commandBuffer, t));
                commandBuffer.Playback(World);
            }
        }
    }
}
