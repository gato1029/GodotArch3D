using Godot;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Procedural.Resources;

public readonly struct BlackyResourceGenerationData
{
    public readonly ushort ResourceId;
    public readonly ResourceSourceType ResourceType;
    public readonly Vector2I PositionTileWorld;

    public BlackyResourceGenerationData(
        ushort resourceId,
        ResourceSourceType resourceType,
        Vector2I positionTileWorld)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
        PositionTileWorld = positionTileWorld;
    }
}