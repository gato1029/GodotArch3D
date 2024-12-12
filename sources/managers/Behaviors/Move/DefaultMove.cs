using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.systems;
using static Godot.TextServer;


namespace GodotEcsArch.sources.managers.Behaviors.Move
{
    internal class DefaultMove : IMoveBehavior
    {       
        public void Move(Entity entity, ref StateComponent stateComponent, ref IAController iaController, ref Position position, ref Direction direction, ref Rotation rotation, ref Velocity velocity, ref Collider collider,RandomNumberGenerator rng, float deltaTime)
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
            }
            else
            {
                MoveInternal(entity, ref direction, ref velocity, ref position, ref collider, ref iaController, ref stateComponent, deltaTime);
            }                                    
        }
       
        private void MoveInternal(Entity entity, ref Direction direction, ref Velocity velocity, ref Position position, ref Collider collider, ref IAController iaController, ref StateComponent stateComponent, float deltaTime)
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
