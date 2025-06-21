using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Behaviors.Move
{
    internal class DefaultMove : IMoveBehavior
    {       
        public void Move(Entity entity, IAttackBehavior attackBehavior, ref StateComponent stateComponent, ref IAController iaController, ref Position position, ref Direction direction, ref Rotation rotation, ref Velocity velocity, ref ColliderSprite collider,RandomNumberGenerator rng, float deltaTime)
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
                direction.normalized = new Vector2(Math.Sign(targetDirection.X), Math.Sign(targetDirection.Y));
                attackBehavior.AttackDirection(entity, ref position, ref direction);
                // CollisionUpdate
                //direction.normalized = CommonOperations.QuantizeDirectionLeftRight(targetDirection);         
                //ref MelleCollider melleAtack = ref entity.TryGetRef<MelleCollider>(out bool exist);
                //Rectangle rectangle = (Rectangle)melleAtack.shapeCollider;
                //rectangle.DirectionTo(direction.normalized.X, direction.normalized.Y);
                //Vector2 vector2 = Collision2D.RotatePosition(rectangle.OriginRelative, Vector2.Zero, direction.normalized.Angle());
                //rectangle.OriginCurrent = vector2;         
            }
            else
            {
                MoveInternal(entity, ref direction, ref velocity, ref position, ref collider, ref iaController, ref stateComponent, deltaTime);
            }                                    
        }
       
        private void MoveInternal(Entity entity, ref Direction direction, ref Velocity velocity, ref Position position, ref ColliderSprite collider, ref IAController iaController, ref StateComponent stateComponent, float deltaTime)
        {
            Vector2 movement = direction.value * velocity.value * deltaTime;
            Vector2 movementNext = position.value + movement + collider.shapeMove.OriginCurrent;


            bool existCollision = false;


           
           

            Rect2 aabb = new Rect2(movementNext, collider.shapeMove.GetSizeQuad()*2);
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
                            if (Collision2D.Collides(collider.shapeMove, colliderB.shapeMove, movementNext, positionB))
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

            //
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
                            //if (Collision2D.Collides(collider.shapeMove, colliderB, movementNext, positionB))
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
                    stateComponent.currentType = StateType.IDLE;
                }
                else
                {
                    stateComponent.currentType = StateType.MOVING;
                    position.value += movement;
                }
            }
            else
            {
                stateComponent.currentType = StateType.IDLE;
            }
        }
    }


}
