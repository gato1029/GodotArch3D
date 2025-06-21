using Godot;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.serializer.Data;
[ProtoContract]
public class TileTerrainSer
{
    [ProtoMember(1)] public int idTerrain;
    [ProtoMember(2)] public Vector2I tilePositionChunk;
    [ProtoMember(3)] public Vector2 positionCollider;
}
