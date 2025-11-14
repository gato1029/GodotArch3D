using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;

namespace GodotEcsArch.sources.systems
{
    internal class CollisionSystem : BaseSystem<World, float>
    {


        private CommandBuffer commandBuffer;
        private QueryDescription queryDynamicSprite = new QueryDescription().WithAll<Position, Direction, ColliderSprite>();
        private QueryDescription queryCharacter = new QueryDescription().WithAll<PositionComponent, ColliderComponent,CharacterComponent>().WithNone<BuildingComponent>();
        public CollisionSystem(World world,CommandBuffer commandBuffer) : base(world)
        {
            this.commandBuffer = commandBuffer;
     
        }

        private struct ChunkJobUpdateCollider : IChunkJob
        {
            private readonly float _deltaTime;
            private readonly CommandBuffer _commandBuffer;

            public ChunkJobUpdateCollider(CommandBuffer commandBuffer, float deltaTime) : this()
            {
                _commandBuffer = commandBuffer;
                _deltaTime = deltaTime;
            }

            public void Execute(ref Chunk chunk)
            {
                ref var pointerEntity = ref chunk.Entity(0);
                ref var pointerPosition = ref chunk.GetFirst<Position>();
                ref var pointerCollider = ref chunk.GetFirst<ColliderSprite>();

                foreach (var entityIndex in chunk)
                {
                    ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                    ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                    ref ColliderSprite collider = ref Unsafe.Add(ref pointerCollider, entityIndex);
                                                  
                    CollisionManager.Instance.dynamicCollidersEntities.AddUpdateItem(p.value, in entity);
                }
            }
        }
        private readonly struct JobUpdateCollider : IForEachWithEntity<Position, Direction, ColliderSprite>
        {
            private readonly float _deltaTime;
            private readonly CommandBuffer _commandBuffer;

            public JobUpdateCollider(float deltaTime, CommandBuffer commandBuffer)
            {
                _deltaTime = deltaTime;
                _commandBuffer = commandBuffer;

            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(Entity entity, ref Position p, ref Direction dir,ref ColliderSprite collider)
            {
                CollisionManager.Instance.dynamicCollidersEntities.AddUpdateItem(p.value, entity);
      
            }
        }

        private readonly struct UpdateColliderCharacter : IForEachWithEntity<PositionComponent,ColliderComponent,CharacterComponent>
        {
            private readonly float _deltaTime;
            private readonly CommandBuffer _commandBuffer;
            private readonly int _batchIndex;
            private readonly int _numBatches;

            public UpdateColliderCharacter(float deltaTime, CommandBuffer commandBuffer, int batchIndex, int numBatches)
            {
                _deltaTime = deltaTime;
                _commandBuffer = commandBuffer;
                _batchIndex = batchIndex;
                _numBatches = numBatches;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(Entity entity, ref PositionComponent p, ref ColliderComponent c, ref CharacterComponent cp)
            {
                if (entity.IsAlive())
                {
                    if (entity.Has<BuildingComponent>())
                    {
                      
                        GameLog.LogCat("collision character incorrecta con build");
                    }
                    if ((c.idCollider % _numBatches) == _batchIndex)
                    {

                        CollisionManager.Instance.characterCollidersEntities
                                              .UpdateColliderPosition(c.idCollider, p.position);
                    }
                }
            }
        }
        private int batchIndex = 0;
        private float batchTimer = 0f;
        private const float batchInterval = 0.02f; // cada 20ms rota batch
        private const int TargetUpdatesPerFrame = 600; // cuántos colliders máximo por frame

        private int NumBatches = 1;
        public override void Update(in float t)
        {
            // 🔹 calcular cuántos batches necesito
            int totalColliders = CollisionManager.Instance.characterCollidersEntities.Count;
            NumBatches = Math.Max(1, (int)Math.Ceiling(totalColliders / (float)TargetUpdatesPerFrame));
            
            // 🔹 avanzar el índice de batch con deltaTime
            batchTimer += t;
            if (batchTimer >= batchInterval)
            {
                batchTimer -= batchInterval;
                batchIndex = (batchIndex + 1) % NumBatches;
            }

            var job = new UpdateColliderCharacter(t, commandBuffer, batchIndex, NumBatches);
            World.InlineEntityQuery<UpdateColliderCharacter, PositionComponent, ColliderComponent, CharacterComponent>(
                in queryCharacter, ref job
            );


        
        }
    }
}
