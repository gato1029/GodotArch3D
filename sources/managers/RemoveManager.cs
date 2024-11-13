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


    internal class RemoveManager : BaseSystem<World, float>
    {
        private CommandBuffer commandBuffer;
    private QueryDescription queryRemoveUnit = new QueryDescription().WithAll<Unit, PendingRemove>();
    public RemoveManager(World world) : base(world)
        {
            commandBuffer = new CommandBuffer();
        }
    private readonly struct ProcessJobPendingRemove : IForEachWithEntity<Sprite>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly World _world;

        public ProcessJobPendingRemove(float deltaTime, CommandBuffer commandBuffer, World world)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;
            _world = world;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Sprite s)
        {            
            if (entity.Has<ColliderDebug>())
            {
                RenderingServer.CanvasItemClear(entity.Get<ColliderDebug>().canvasItemCollider);
                RenderingServer.CanvasItemClear(entity.Get<ColliderDebug>().canvasItemColliderMelle);
            }
            RenderingServer.CanvasItemClear(s.CanvasItem);
            CollisionManager.dynamicCollidersEntities.RemoveItem(entity.Reference());
            DebugerManager.entities.Remove(entity.Id);
            _world.Destroy(entity);
            
        }

    }
    public override void AfterUpdate(in float t)
    {
        var job = new ProcessJobPendingRemove((float)t, commandBuffer, World);
        World.InlineEntityQuery<ProcessJobPendingRemove, Sprite>(in queryRemoveUnit, ref job);
        commandBuffer.Playback(World);
    }
}
   
