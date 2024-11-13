using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;

[Component]
public struct ColliderDebug
{
    public Rid canvasItemCollider;
    public Rid canvasItemColliderMelle;
    public Rect2 rect;
}

internal class DebugerColliderSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<Collider>().WithNone<ColliderDebug>();
    private QueryDescription queryDesactive = new QueryDescription().WithAll<Collider, ColliderDebug>();

    public DebugerColliderSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }


    private readonly struct JobColliderDebug : IForEachWithEntity<Collider>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public JobColliderDebug(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Collider c)
        {
            if (!entity.Has<ColliderDebug>())
            {
                Rid canvasItem = RenderingServer.CanvasItemCreate();
                RenderingServer.CanvasItemSetParent(canvasItem, EcsManager.Instance.CanvasItem);
                drawRectangle(canvasItem, c.rect, Colors.OrangeRed);
                RenderingServer.CanvasItemAddCircle(canvasItem, c.rect.GetCenter(), 5, Colors.Aquamarine);
                                
                if (entity.Has<MelleWeapon>())
                {
                    MelleWeapon ent = entity.Get<MelleWeapon>();
                    Rid canvasItem2 = RenderingServer.CanvasItemCreate();
                    RenderingServer.CanvasItemSetParent(canvasItem2, EcsManager.Instance.CanvasItem);
                    drawRectangle(canvasItem2, ent.rect2, Colors.GreenYellow);
                    RenderingServer.CanvasItemAddCircle(canvasItem2, ent.rect2.GetCenter(), 5, Colors.Aquamarine);
                    _commandBuffer.Add<ColliderDebug>(entity, new ColliderDebug { canvasItemCollider = canvasItem, canvasItemColliderMelle = canvasItem2 });
                }
                else
                {
                    _commandBuffer.Add<ColliderDebug>(entity, new ColliderDebug { canvasItemCollider = canvasItem });
                }
            }            
        }
    }
    private readonly struct JobColliderRemoveDebug : IForEachWithEntity<ColliderDebug>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public JobColliderRemoveDebug(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref ColliderDebug c)
        {
            if (entity.Has<ColliderDebug>())
            {
               _commandBuffer.Remove<ColliderDebug>(entity);
                RenderingServer.CanvasItemClear(c.canvasItemCollider);
                RenderingServer.CanvasItemClear(c.canvasItemColliderMelle);
            }
        }
    }
    private static void drawRectangle(Rid canvasItem, Rect2 rect, Color color)
    {
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position, rect.Position + new Vector2(rect.Size.X, 0), color, 4);
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position + new Vector2(rect.Size.X, 0), rect.Position + rect.Size, color, 4);
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position + rect.Size, rect.Position + new Vector2(0, rect.Size.Y), color, 4);
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position + new Vector2(0, rect.Size.Y), rect.Position, color, 4);
    }

    bool DebugActive = false;
    public override void Update(in float t)
    {
        bool debug = ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("debugColliders");
        if (debug)
        {
            DebugActive = !DebugActive;

            if (DebugActive)
            {
                ActiveDebug();
            }
            else
            {
                var job = new JobColliderRemoveDebug((float)t, commandBuffer);
                World.InlineParallelEntityQuery<JobColliderRemoveDebug, ColliderDebug>(in queryDesactive, ref job);
                commandBuffer.Playback(World);
            }
            
        }
        
    }

    void ActiveDebug()
    {
        World.Query(in query, (Entity entity, ref Collider c) =>
        {
            if (!entity.Has<ColliderDebug>())
            {
                Rid canvasItem = RenderingServer.CanvasItemCreate();
                RenderingServer.CanvasItemSetParent(canvasItem, EcsManager.Instance.CanvasItem);
                drawRectangle(canvasItem, c.rect, Colors.OrangeRed);
                RenderingServer.CanvasItemAddCircle(canvasItem, c.rect.GetCenter(), 5, Colors.Aquamarine);

                if (entity.Has<MelleWeapon>())
                {
                    MelleWeapon ent = entity.Get<MelleWeapon>();
                    Rid canvasItem2 = RenderingServer.CanvasItemCreate();
                    RenderingServer.CanvasItemSetParent(canvasItem2, EcsManager.Instance.CanvasItem);
                    drawRectangle(canvasItem2, ent.rect2, Colors.GreenYellow);
                    RenderingServer.CanvasItemAddCircle(canvasItem2, ent.rect2.GetCenter(), 5, Colors.Aquamarine);
                    entity.Add<ColliderDebug>( new ColliderDebug { canvasItemCollider = canvasItem, canvasItemColliderMelle = canvasItem2 });
                }
                else
                {
                    entity.Add<ColliderDebug>( new ColliderDebug { canvasItemCollider = canvasItem });
                }
            }
        });
        
        
    }
}

