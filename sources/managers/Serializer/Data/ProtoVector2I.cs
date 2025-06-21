using Godot;
using ProtoBuf;

namespace GodotEcsArch.sources.managers.Serializer.Data;

[ProtoContract]
public struct ProtoVector2I
{
    [ProtoMember(1)] public int X { get; set; }
    [ProtoMember(2)] public int Y { get; set; }

    public ProtoVector2I(int x, int y)
    {
        X = x;
        Y = y;
    }

    public ProtoVector2I(Vector2I v)
    {
        X = v.X;
        Y = v.Y;
    }

    public Vector2I ToGodot() => new Vector2I(X, Y);

    public static implicit operator ProtoVector2I(Vector2I v) => new ProtoVector2I(v);
    public static implicit operator Vector2I(ProtoVector2I pv) => pv.ToGodot();
}
