using Godot;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Globals;
internal static class GlobalData
{
    public static int numBatchColliders = 0;
    public static int batchIndexColliders = 0;

    public static int numBatchMoveColliders = 0;
    public static int batchIndexMoveColliders = 0;

    public static Queue<(Vector2I pos, int id)> PendingBuildingQueue = new();
    public static Queue<(Vector3 pos, TileSpriteData)> PendingTileSprite = new();

    public static Queue<DamageEvent> EventsDamage = new();
}
