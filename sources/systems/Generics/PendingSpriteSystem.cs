using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.utils;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Rendering
{
    /// <summary>
    /// Sistema encargado de procesar los PendingSpriteComponent y
    /// convertirlos en SpriteRenderGPUComponent de forma segura (main thread).
    /// </summary>
    public class PendingSpriteSystem : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;
        private World sharedWorld;
        private QueryDescription query = new QueryDescription()
            .WithAll<PendingSpriteComponent, PositionComponent>();

        public PendingSpriteSystem(World world, CommandBuffer sharedCommandBuffer) : base(world) {

            sharedWorld = world;
            commandBuffer = sharedCommandBuffer;
        }

        private readonly struct CreateSpriteJob : IForEachWithEntity<PendingSpriteComponent, PositionComponent>
        {
            private readonly CommandBuffer _commandBuffer;

            public CreateSpriteJob(CommandBuffer commandBuffer)
            {
                _commandBuffer = commandBuffer;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Update(Entity entity, ref PendingSpriteComponent pending, ref PositionComponent pos)
            {
                // Crear el sprite en main thread
                var sprite = SpriteHelper.CreateSpriteRenderGpuComponent(
                    pending.spriteData,
                    pending.spriteData.scale,
                    pos.position,
                    pending.zIndex
                );

                // Agregar el sprite real a la entidad
                _commandBuffer.Add(entity, sprite);

                // Eliminar el componente temporal
                _commandBuffer.Remove<PendingSpriteComponent>(in entity);
            }
        }

        public override void Update(in float t)
        {
            var job = new CreateSpriteJob(commandBuffer);
            sharedWorld.InlineEntityQuery<CreateSpriteJob, PendingSpriteComponent, PositionComponent>(in query, ref job);
            //commandBuffer.Playback(World);
        }
    }
}
