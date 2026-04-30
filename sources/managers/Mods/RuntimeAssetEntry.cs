namespace GodotEcsArch.sources.managers.Mods;

public struct RuntimeAssetEntry
{
    public int RuntimeId;
    public PersistentAssetRef PersistentRef;
    public object Value;

    public RuntimeAssetEntry(int runtimeId, PersistentAssetRef persistentRef, object value)
    {
        RuntimeId = runtimeId;
        PersistentRef = persistentRef;
        Value = value;
    }
}