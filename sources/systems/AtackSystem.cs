using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;

using Godot;
using GodotEcsArch.sources.managers.Collision;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[Component]
public struct OrderAtack
{

}

public enum TypeWeapon
{
    SWORD
}

[Component]
public struct MelleWeapon
{
    public Rect2 rect2;
    public Rect2 rect2Transform;
}

[Component]
public struct RelationWeapon
{

}

[Component]
public struct Weapon
{
    
}
[Component]
public struct MelleCollider
{
   public GeometricShape2D shapeCollider;
}

[Component]
public struct WeaponColliderParticular
{
    public GeometricShape2D shapeColliderLeftRight;
    public GeometricShape2D shapeColliderTopDown;
}
internal class AtackSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    
    private QueryDescription query = new QueryDescription().WithAll<Unit, MelleWeapon, Position, Direction, OrderAtack>();
    private QueryDescription queryPending = new QueryDescription().WithAll<Unit, MelleWeapon, PendingAttack>();
    private QueryDescription queryMelleAtack = new QueryDescription().WithAll<UnitController, Position, Velocity, Direction, ColliderSprite, Rotation, MelleCollider>();
    public AtackSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();    
    }

    private struct ChunkJobProcessMelleAttack : IChunkJob
    {
        private readonly CommandBuffer _commandBuffer;
        private readonly float _deltaTime;

        public ChunkJobProcessMelleAttack(CommandBuffer commandBuffer, float deltaTime)
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerVelocity = ref chunk.GetFirst<Velocity>();

            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            ref var pointerRotation = ref chunk.GetFirst<Rotation>();
            ref var pointerCollider = ref chunk.GetFirst<ColliderSprite>();
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
                ref ColliderSprite c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                ref Animation a = ref Unsafe.Add(ref pointerAnimation, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);
                ref UnitController unitController = ref Unsafe.Add(ref pointerUnitController, entityIndex);

                switch (unitController.controllerMode)
                {
                    case ControllerMode.HUMAN:
                        break;
                    case ControllerMode.IA:
                        bool attackPosible = true;

                        if (a.updateAction == AnimationAction.STUN)
                        {
                            attackPosible = false;
                        //    GD.Print("Strun");
                        }
                        if ((a.updateAction == AnimationAction.STUN || a.updateAction == AnimationAction.HIT || a.updateAction == AnimationAction.ATACK) && !a.complete)
                        {
                            attackPosible = false;
                        }
                        else
                        {


                           // GD.Print(a.updateAction);
                            if (a.updateAction == AnimationAction.ATACK && a.complete)
                            {
                                CallAttack(entity, ref p, ref d, ref a);
                                //a.updateAction = AnimationAction.IDLE;
                            }

                            if (attackPosible)
                            {
                                bool flag = EntryRangeAttack(entity, ref p, ref d, ref a);
                                if (flag)
                                {
                                    a.updateAction = AnimationAction.ATACK;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private bool EntryRangeAttack(Entity entity, ref Position p, ref Direction d, ref Animation a)
        {
            //Unit unit = entity.Get<Unit>();
            //MelleCollider melleAtack = entity.Get<MelleCollider>();
            //Vector2 pp = melleAtack.collider.rectTransform.Size / 2 * d.value;
            //Vector2 posAtack = p.value + pp;
            //var result = CollisionManager.Instance.dynamicCollidersEntities.GetPossibleQuadrants(posAtack, 4);
            //if (result != null)
            //{
            //    foreach (var itemDic in result)
            //    {
            //        foreach (var item in itemDic.Value)
            //        {
            //            Entity entB = item.Value;
            //            if (item.Key != entity.Id && unit.team != entB.Get<Unit>().team)
            //            {

            //                var colliderB = entB.Get<ColliderSprite>();
            //                var positionB = entB.Get<Position>().value;

            //                //if (CollisionManager.Instance.CheckAABBCollision2(posAtack, melleAtack.collider, positionB, colliderB))
            //                //{
            //                //    return true;
            //                //}
            //            }
            //        }
            //    }
            //}
            return false;
        }
    
        private void CallAttack(Entity entity, ref Position p, ref Direction d, ref Animation a)
        {

            //Unit unit = entity.Get<Unit>();
            //MelleCollider melleAtack = entity.Get<MelleCollider>();
            //Vector2 pp = melleAtack.collider.rectTransform.Size / 2 * d.value;
            //Vector2 posAtack = p.value + pp;
            //var result = CollisionManager.Instance.dynamicCollidersEntities.GetPossibleQuadrants(posAtack, 4);
            //if (result != null)
            //{               
            //    foreach (var itemDic in result)
            //    {
            //        foreach (var item in itemDic.Value)
            //        {
            //            Entity entB = item.Value;
            //            if (item.Key != entity.Id && unit.team != entB.Get<Unit>().team)
            //            {
                            
            //                var colliderB = entB.Get<ColliderSprite>();
            //                var positionB = entB.Get<Position>().value;

            //                //if (CollisionManager.Instance.CheckAABBCollision2(posAtack, melleAtack.collider, positionB, colliderB))
            //                //{                           
            //                //    ref Animation animationB = ref entB.TryGetRef<Animation>(out bool exist);
            //                //    animationB.updateAction = AnimationAction.HIT;
            //                //    GD.Print("danio enenmi");                                
            //                //}                                                        
            //            }
            //        }
            //    }
            //}
        }
    }

    private struct ChunkJobPendingAtack : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobPendingAtack(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPendingAtack = ref chunk.GetFirst<PendingAttack>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref PendingAttack pa = ref Unsafe.Add(ref pointerPendingAtack, entityIndex);

                pa.entityTarget.Get<Health>().value -= pa.damage;
                Health health = pa.entityTarget.Get<Health>();
                _commandBuffer.Remove<PendingAttack>(entity);
                if (!pa.entityTarget.Has<PendingRemove>())
                {
                    if (health.value <= 0)
                    {
                        _commandBuffer.Add<PendingRemove>(pa.entityTarget);
                    }
                }
            }
        }
    }



    public override void Update(in float delta)
    {



        //World.InlineParallelChunkQuery(in queryMelleAtack, new ChunkJobProcessAtackMelle(commandBuffer,delta));
        //commandBuffer.Playback(World);
        World.InlineParallelChunkQuery(in queryMelleAtack, new ChunkJobProcessMelleAttack(commandBuffer, delta));
        commandBuffer.Playback(World);
    }


}
