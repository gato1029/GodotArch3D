using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.systems;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Behaviors.Attack
{
    internal class MelleAttackBehavior : IAttackBehavior
    {
        public bool AttackPosible(Entity entity, ref Position position, ref Direction direction)
        {
            Unit unit = entity.Get<Unit>();
            MelleCollider melleAtack = entity.Get<MelleCollider>();
            Vector2 pp = melleAtack.shapeCollider.OriginCurrent;
            Vector2 posAtack = position.value + pp;

            Rectangle rectangle = (Rectangle)melleAtack.shapeCollider;
            Rect2 aabb = new Rect2(posAtack, rectangle.Width, rectangle.Height);
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

                                return true;
                            }

                        }
                    }
                }
            }            
            return false;
        }

        public void Attack(Entity entity, ref Position position, ref Direction direction)
        {
            Unit unit = entity.Get<Unit>();
            MelleCollider melleAtack = entity.Get<MelleCollider>();
            Vector2 pp = melleAtack.shapeCollider.OriginCurrent;
            Vector2 posAtack = position.value + pp;

            Rectangle rectangle = (Rectangle)melleAtack.shapeCollider;

            Rect2 aabb = new Rect2(posAtack, rectangle.Width, rectangle.Height);
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

                                stateComponentB.currentType = StateType.TAKE_STUN;
                                BehaviorManager.Instance.AplyDamage(entity, entB);

                            }

                        }
                    }
                }
            }           
        }

        void IAttackBehavior.AttackDirection(Entity entity, ref Position position, ref Direction direction)
        {
            throw new System.NotImplementedException();
        }
    }
}
