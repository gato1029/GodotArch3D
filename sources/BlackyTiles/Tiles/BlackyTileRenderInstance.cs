
using Flecs.NET.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Tiles;
public class BlackyTileRenderInstance
{
    public int height;
    public int layer;
    public int x;
    public int y;

    public Rid rid;
    public int Instance;
    public int MaterialId;
    public Entity EntityReference;
    public bool HasEntityReference;
    public BlackyTileRenderInstance(Rid rid, int instance, int materialId, int height, int layer, int x, int y)
    {
        this.rid = rid;
        Instance = instance;
        MaterialId = materialId;
        HasEntityReference = false;

        this.height = height;
        this.layer = layer;
        this.x = x;
        this.y = y;
    }
    public bool IsDestroyed { get; private set; }

    public void MarkDestroyed()
    {
        IsDestroyed = true;
    }
    public void SetEntityReference(Entity entity)
    {
        if (IsDestroyed)
            return;

        EntityReference = entity;
        HasEntityReference = true;

    }
    public void DestroyEntity()
    {
        if (!HasEntityReference)
            return;

        if (EntityReference.IsAlive())
        {
            
            EntityReference.Destruct();
        }

        HasEntityReference = false;
        EntityReference = default;
    }
}