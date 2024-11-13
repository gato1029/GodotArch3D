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
using static System.Net.Mime.MediaTypeNames;


internal class UnitManager
{
    public static void CreateUnit(Vector2 position, Vector2? targetMoved = null, bool debugBorder = false)
    {
        Vector2 target = targetMoved ?? Vector2.Zero;

        Texture2D text = ResourceLoader.Load<Texture2D>("res://resources/cdemo.png");
        Texture2D text2 = ResourceLoader.Load<Texture2D>("res://resources/cat.png");
        Rid texture = text.GetRid();
        Rect2 rect = new Rect2(-(text.GetSize()) / 2, text.GetSize());
        if ((target == Vector2.Zero))
        {
            texture= text2.GetRid();
            rect = new Rect2(-(text2.GetSize()) / 2, text2.GetSize());
        }
       

        Rid canvasItem = RenderingServer.CanvasItemCreate();

        RenderingServer.CanvasItemSetParent(canvasItem, EcsManager.Instance.CanvasItem);
        Entity entity = EcsManager.Instance.World.Create();
        
        
        
        entity.Add<Sprite>(new Sprite { CanvasItem = canvasItem, Texture = texture, rect = rect });
        entity.Add<IsRender>();
        entity.Add<Position>(new Position { value = position });

        entity.Add<Direction>();
        entity.Add<Velocity>(new Velocity { value = 200 });
        
        entity.Add<Melee>();
       
        entity.Add<Damage>(new Damage { value = 10 });
        
        entity.Add<Rotation>(new Rotation {value =0 });
        entity.Add<FrecuencyAttack>(new FrecuencyAttack { value = 0.1f });

        Vector2 sizeCollider = new Vector2(64, 128);
        Rect2 rectCollider = new Rect2(-(sizeCollider) / 2 + new Vector2(0, 0), sizeCollider);

        Vector2 sizeColliderAtack = new Vector2(128, 256);
        //Vector2 offset = new Vector2(96, 0);
        Rect2 rectColliderAtack = new Rect2((-sizeColliderAtack / 2) + new Vector2(64, 0), sizeColliderAtack);
        entity.Add<MelleWeapon>(new MelleWeapon { rect2 = rectColliderAtack, rect2Transform = rectColliderAtack });

        entity.Add<Weapon>();
        //entity.Add<ColliderMelleAtack>(new ColliderMelleAtack { offset = offset, rect = rectColliderAtack });
        if ((target == Vector2.Zero))
        {                                               
            entity.Add<Collider>(new Collider { rect = rectCollider, rectTransform = rectCollider,aplyRotation =true });
            entity.Add<HumanController>();
            entity.Add<Health>(new Health { value = 100000 });
            entity.Add<OrderAtack>();
            entity.Add<Unit>(new Unit { team = 1 });            
        }
        else
        {
            entity.Add<Collider>(new Collider { rect = rectCollider, rectTransform = rectCollider, aplyRotation = true });
            entity.Add<SearchTargetMovement>();
            entity.Add<Health>(new Health { value = 100 });
            entity.Add<OrderAtack>();
            entity.Add<IAController>();
            entity.Add<AreaMovement>(new AreaMovement { type = MovementType.CIRCLE_STATIC, origin = position, value = 250});
            entity.Add<Unit>(new Unit { team = 2 });
        }
        
        Transform2D transform2D = new Transform2D(0, position);
        RenderingServer.CanvasItemAddTextureRect(canvasItem, rect, texture);
        entity.Add<Transform>( new Transform { value = transform2D });

        RenderingServer.CanvasItemSetSortChildrenByY(canvasItem, true);
        RenderingServer.CanvasItemSetTransform(canvasItem, transform2D);

        CollisionManager.dynamicCollidersEntities.AddUpdateItem(position, entity.Reference());

        if (!(target == Vector2.Zero))
        {
            //CreateDebug(entity, 250, position);
        }
        //CreateDebug(entity, rectColliderAtack,offset);
    }

    private static void CreateDebug(Entity entity, int radio, Vector2 position)
    {
        Rid canvasItem = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(canvasItem, EcsManager.Instance.CanvasItem);
        RenderingServer.CanvasItemAddCircle(canvasItem, position, radio, new Color(30,30,30,0.1f));

    }
        private static void CreateDebug(Entity entity, Rect2 rect, Vector2 offset)
    {
        Rid canvasItem = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(canvasItem, EcsManager.Instance.CanvasItem);                       
        entity.Add<Debug>( new Debug {  CanvasItem= canvasItem, rect =rect, offset = offset });       
        drawRectangle(canvasItem, rect,Colors.PaleVioletRed);
        RenderingServer.CanvasItemAddCircle(canvasItem, rect.GetCenter(), 5, Colors.Aquamarine);
    }
    private static void drawRectangle(Rid canvasItem, Rect2 rect, Color color)
    {
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position, rect.Position + new Vector2(rect.Size.X, 0), color, 2);
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position + new Vector2(rect.Size.X, 0), rect.Position + rect.Size, color, 2);
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position + rect.Size, rect.Position + new Vector2(0, rect.Size.Y), color, 2);
        RenderingServer.CanvasItemAddLine(canvasItem, rect.Position + new Vector2(0, rect.Size.Y), rect.Position, color, 2);
    }
}

