using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.systems;

namespace GodotEcsArch.sources.managers.Behaviors.Attack
{
    internal class MelleAttackBehavior : IAttackBehavior
    {
        public bool AttackPosible(Entity entity, ref Position position, ref Direction direction)
        {
            Unit unit = entity.Get<Unit>();
            MelleCollider melleAtack = entity.Get<MelleCollider>();
            Vector2 pp = melleAtack.collider.rectTransform.Size / 2 * direction.value;
            Vector2 posAtack = position.value + pp;
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
            Vector2 pp = melleAtack.collider.rectTransform.Size / 2 * direction.value;
            Vector2 posAtack = position.value + pp;
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
                                stateComponentB.currentType = StateType.TAKE_HIT;
                            }
                        }
                    }
                }
            }
        }
    }
}
