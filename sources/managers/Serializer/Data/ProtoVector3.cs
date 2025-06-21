using Godot;
using ProtoBuf;

namespace GodotEcsArch.sources.managers.Serializer.Data;

[ProtoContract]
public struct ProtoVector3
{
    [ProtoMember(1)] public float X { get; set; }
    [ProtoMember(2)] public float Y { get; set; }
    [ProtoMember(3)] public float Z { get; set; }

    public ProtoVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    // Conversiones
    public static implicit operator ProtoVector3(Vector3 v) => new ProtoVector3(v.X, v.Y, v.Z);
    public static implicit operator Vector3(ProtoVector3 v) => new Vector3(v.X, v.Y, v.Z);
}