using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
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
        public CollisionSystem(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
     
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



        public override void Update(in float t)
        {
            //World.InlineParallelChunkQuery(in queryDynamicSprite, new ChunkJobUpdateCollider(commandBuffer, t));


            var job = new JobUpdateCollider((float)t, commandBuffer);
            World.InlineEntityQuery<JobUpdateCollider, Position, Direction, ColliderSprite>(in queryDynamicSprite, ref job);
            //CollisionManager.Instance.worldPhysic.Step(1/60);
            bool debug = Input.IsActionJustPressed("debugGridCollider");
            if (debug)
            {
                CollisionManager.Instance.dynamicCollidersEntities.DrawGrid(Colors.WebPurple);
            }
        }
    }
}
