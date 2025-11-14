using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Generics
{
    /// <summary>
    /// Sistema encargado de procesar entidades marcadas con PendingDestroyComponent.
    /// - Libera el SpriteRenderGPUComponent si existe.
    /// - Luego destruye la entidad de forma segura en el main thread.
    /// </summary>
    public class PendingDestroySystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;
        private World sharedWorld;
        private QueryDescription query = new QueryDescription()
            .WithAll<PendingDestroyComponent>();

        public PendingDestroySystem(World world, CommandBuffer sharedCommandBuffer) : base(world) 
        {
            commandBuffer = sharedCommandBuffer;
            sharedWorld = world; 
        }

        private readonly struct CleanupJob : IForEachWithEntity<PendingDestroyComponent>
        {
            private readonly World _world;
            private readonly CommandBuffer _commandBuffer;

            public CleanupJob(World world)
            {
                _world = world;
            }

            public CleanupJob(World world, CommandBuffer commandBuffer) : this(world)
            {
                _commandBuffer = commandBuffer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(Entity entity, ref PendingDestroyComponent pending)
            {
                if (entity.IsAlive())
                {
                    // 1. Liberar GPU si tiene sprite
                    if (entity.Has<SpriteRenderGPUComponent>())
                    {
                        ref var sprite = ref entity.Get<SpriteRenderGPUComponent>();
                        MultimeshManager.Instance.FreeInstance(sprite.rid, sprite.instance, sprite.idMaterial);
                        RenderingServer.MultimeshInstanceSetCustomData(sprite.rid, sprite.instance, new Color(-1, -1, -1, -1));
                    }
                    if (entity.Has<RenderGPUComponent>())
                    {
                        ref var sprite = ref entity.Get<RenderGPUComponent>();
                        MultimeshManager.Instance.FreeInstance(sprite.rid, sprite.instance, sprite.idMaterial);
                        RenderingServer.MultimeshInstanceSetCustomData(sprite.rid, sprite.instance, new Color(-1, -1, -1, -1));
                    }

                    if (entity.Has<CharacterComponent>())
                    {
                        CollisionManager.Instance.characterCollidersEntities.RemoveCollider(entity.Get<ColliderComponent>().idCollider);
                    }
                    if (entity.Has<BuildingComponent>())
                    {
                        CollisionManager.Instance.BuildingsColliders.RemoveCollider(entity.Get<ColliderComponent>().idCollider);
                        var vec = new Vector2I(entity.Get<TilePositionComponent>().x, entity.Get<TilePositionComponent>().y);
                        MapManagerEditor.Instance.CurrentMapLevelData.mapBuildings.RemoveTile(vec);
                    }
                    // 2. Destruir entidad
                    _commandBuffer.Destroy(entity);
                }
                
            }
        }

        public override void Update(in float t)
        {
            var job = new CleanupJob(World,commandBuffer);
            sharedWorld.InlineEntityQuery<CleanupJob, PendingDestroyComponent>(in query, ref job);
            //commandBuffer.Playback(World);
        }
    }
}
