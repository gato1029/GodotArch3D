using Godot;
using ProtoBuf;

namespace GodotEcsArch.sources.managers.Serializer.Data;

[ProtoContract]
public class TileEntry<T>
{
    [ProtoMember(1)] public ProtoVector2I Position { get; set; }
    [ProtoMember(2)] public T Value { get; set; }

    public TileEntry() { }

    public TileEntry(int x, int y, T value)
    {
        Position = new Vector2I(x, y);
        Value = value;
    }
}
