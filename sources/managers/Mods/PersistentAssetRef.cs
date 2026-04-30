using System;

namespace GodotEcsArch.sources.managers.Mods;

public enum RuntimeAssetType : byte
{
    None = 0,
    ResourceSource = 1,
    Building = 2,
    Bullet = 3,
    Character = 4
}
/// <summary>
/// Referencia persistente estable para cualquier dato de mod.
/// Esto SI se puede guardar en save.
/// </summary>


[Serializable]
public struct PersistentAssetRef : IEquatable<PersistentAssetRef>
{
    public RuntimeAssetType AssetType;
    public string ModName;
    public int LocalId;

    public PersistentAssetRef(RuntimeAssetType assetType, string modName, int localId)
    {
        AssetType = assetType;
        ModName = modName;
        LocalId = localId;
    }

    public bool Equals(PersistentAssetRef other)
    {
        return AssetType == other.AssetType &&
               ModName == other.ModName &&
               LocalId == other.LocalId;
    }

    public override bool Equals(object obj)
    {
        return obj is PersistentAssetRef other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((byte)AssetType, ModName, LocalId);
    }

    public override string ToString()
    {
        return $"{AssetType}|{ModName}|{LocalId}";
    }
}