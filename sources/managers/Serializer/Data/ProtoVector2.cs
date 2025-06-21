using Godot;
using ProtoBuf;

namespace GodotEcsArch.sources.managers.Serializer.Data;


[ProtoContract]
public struct ProtoVector2
{
    [ProtoMember(1)] public float X { get; set; }
    [ProtoMember(2)] public float Y { get; set; }

    public ProtoVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public ProtoVector2(Vector2 godotVector)
    {
        X = godotVector.X;
        Y = godotVector.Y;
    }

    public Vector2 ToGodot() => new Vector2(X, Y);

    public static implicit operator ProtoVector2(Vector2 v) => new ProtoVector2(v);
    public static implicit operator Vector2(ProtoVector2 pv) => pv.ToGodot();
}
