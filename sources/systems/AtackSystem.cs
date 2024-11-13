using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;

using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;

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

internal class AtackUnitSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    
    private QueryDescription queryMelleAtack = new QueryDescription().WithAll<Unit, MelleWeapon, Position, Direction, OrderAtack>();
    private QueryDescription queryPending = new QueryDescription().WithAll<Unit, MelleWeapon, PendingAttack>();
    public AtackUnitSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();    
    }

    private struct ChunkJobProcessAtackMelle : IChunkJob
    {
        private readonly CommandBuffer _commandBuffer;
        private readonly float _deltaTime;

        public ChunkJobProcessAtackMelle(CommandBuffer commandBuffer, float deltaTime)
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerMelleWeapon = ref chunk.GetFirst<MelleWeapon>();
            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            ref var pointerDamage = ref chunk.GetFirst<Damage>();
            ref var pointerUnit = ref chunk.GetFirst<Unit>();
            ref var pointerFrecuencyAttack = ref chunk.GetFirst<FrecuencyAttack>();            
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Position pos = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref MelleWeapon mw = ref Unsafe.Add(ref pointerMelleWeapon, entityIndex);
                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);
                ref Damage da = ref Unsafe.Add(ref pointerDamage, entityIndex);
                ref Unit u = ref Unsafe.Add(ref pointerUnit, entityIndex);
                ref FrecuencyAttack fa = ref Unsafe.Add(ref pointerFrecuencyAttack, entityIndex);

                fa.timeAccumulator += _deltaTime;
                if (fa.timeAccumulator >= fa.value)
                {
                    fa.timeAccumulator = 0;
                    Vector2 pp = mw.rect2Transform.Size / 2 * d.value;
                    Vector2 posAtack = pos.value + pp;
                    var result = CollisionManager.dynamicCollidersEntities.GetPossibleQuadrants(posAtack, 128);
                    if (result != null)
                    {
                        foreach (var itemDic in result)
                        {
                            foreach (var item in itemDic.Value)
                            {
                                Entity entB = item.Value;
                                if (item.Key != entity.Id && u.team != entB.Get<Unit>().team)
                                {                                    
                                    if (!entity.Has<PendingAttack>())
                                    {

                                        var entityExternal = entB.Get<Collider>().rectTransform;
                                        var entityExternalPos = entB.Get<Position>().value;

                                        if (CollisionManager.CheckAABBCollision(pos.value, mw.rect2Transform, entityExternalPos, entityExternal))
                                        {
                                            _commandBuffer.Add<PendingAttack>(in entity,new PendingAttack { entityTarget = entB, damage = da.value });
                                        }
                                    }

                                }
                            }
                        }
                    }
                    if (entity.Has<HumanController>())
                    {
                        _commandBuffer.Remove<OrderAtack>(in entity);
                    }
                }
            }
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

    public override void Update(in float tick)
    {
        float delta = TimeGodot.Delta;


        World.InlineParallelChunkQuery(in queryMelleAtack, new ChunkJobProcessAtackMelle(commandBuffer,delta));
        commandBuffer.Playback(World);
        World.InlineParallelChunkQuery(in queryPending, new ChunkJobPendingAtack(commandBuffer,delta));

        commandBuffer.Playback(World);
    }


}
