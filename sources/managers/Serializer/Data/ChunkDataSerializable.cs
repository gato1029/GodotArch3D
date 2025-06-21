using ProtoBuf;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Serializer.Data;

[ProtoContract]
public class ChunkDataSerializable<T>
{    
    [ProtoMember(1)] public ProtoVector2 PositionChunk { get; set; }
    [ProtoMember(2)] public ProtoVector2I Size { get; set; }
    [ProtoMember(3)] public List<TileEntry<T>> Tiles { get; set; } = new();
}