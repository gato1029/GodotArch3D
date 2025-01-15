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

    public static void CreateUnitSprite()
    {
        ArrayMesh quadMesh = MeshCreator.CreateSquareMesh(32, 32, new Vector2(32,32), new Vector3(0, 0, 0));

        ResourceSaver.Save(quadMesh, "res://Quad_32_32.tres");
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

