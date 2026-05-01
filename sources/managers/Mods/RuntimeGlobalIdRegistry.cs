using Godot;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Mods;

/// <summary>
/// Registry rápido de IDs runtime para assets lógicos persistentes.
/// ECS guarda runtimeId.
/// Save guarda PersistentAssetRef.
/// </summary>
public class RuntimeGlobalIdRegistry : SingletonBase<RuntimeGlobalIdRegistry>
{
    private int _nextRuntimeId = 1;

    private readonly Dictionary<int, RuntimeAssetEntry> _byRuntimeId = new();
    private readonly Dictionary<PersistentAssetRef, int> _persistentToRuntime = new();

    protected override void Initialize()
    {
        Build();
    }

    // =========================================================
    // BUILD REGISTRY
    // =========================================================

    private void Build()
    {
        RegistrarTodo<ResourceSourceData>(RuntimeAssetType.ResourceSource);
        RegistrarTodo<BuildingData>(RuntimeAssetType.Building);
        RegistrarTodo<BulletData>(RuntimeAssetType.Bullet);
        RegistrarTodo<CharacterModelBaseData>(RuntimeAssetType.Character);

        GD.Print($"RuntimeGlobalIdRegistry cargado -> {_nextRuntimeId - 1} runtime assets.");
    }

    private void RegistrarTodo<T>(RuntimeAssetType assetType) where T : class
    {
        foreach (var item in AtlasModsManager.GetDictionaryAll<T, int>())
        {
            string modName = TableMods.Instance.ObtenerNombre(item.Key);

            foreach (var itemInternal in item.Value)
            {
                var persistent = new PersistentAssetRef(assetType, modName, itemInternal.Key);
                Registrar(persistent, itemInternal.Value);
            }

        }
    }

    private void Registrar(PersistentAssetRef persistentRef, object value)
    {
        if (_persistentToRuntime.ContainsKey(persistentRef))
            return;

        int runtimeId = _nextRuntimeId++;

        _persistentToRuntime[persistentRef] = runtimeId;
        _byRuntimeId[runtimeId] = new RuntimeAssetEntry(runtimeId, persistentRef, value);
    }

    // =========================================================
    // GET RUNTIME ID DESDE MOD+LOCAL
    // =========================================================

    public int GetRuntimeId(RuntimeAssetType assetType, string modName, int localId)
    {
        var persistent = new PersistentAssetRef(assetType, modName, localId);

        if (_persistentToRuntime.TryGetValue(persistent, out int runtimeId))
            return runtimeId;

        return -1;
    }

    // =========================================================
    // GET RUNTIME ID DESDE SAVE REF
    // =========================================================

    public int GetRuntimeId(PersistentAssetRef persistentRef)
    {
        if (_persistentToRuntime.TryGetValue(persistentRef, out int runtimeId))
            return runtimeId;

        return -1;
    }

    // =========================================================
    // GET OBJECT DESDE RUNTIME ID
    // =========================================================

    public T Get<T>(int runtimeId) where T : class
    {
        if (_byRuntimeId.TryGetValue(runtimeId, out var entry))
            return entry.Value as T;

        return null;
    }

    public object Get(int runtimeId)
    {
        if (_byRuntimeId.TryGetValue(runtimeId, out var entry))
            return entry.Value;

        return null;
    }

    public bool TryGet<T>(int runtimeId, out T value) where T : class
    {
        value = Get<T>(runtimeId);
        return value != null;
    }

    // =========================================================
    // GET SAVE REF DESDE RUNTIME ID
    // =========================================================

    public PersistentAssetRef GetPersistentRef(int runtimeId)
    {
        if (_byRuntimeId.TryGetValue(runtimeId, out var entry))
            return entry.PersistentRef;

        return default;
    }

    public bool TryGetPersistentRef(int runtimeId, out PersistentAssetRef persistentRef)
    {
        persistentRef = GetPersistentRef(runtimeId);
        return persistentRef.AssetType != RuntimeAssetType.None;
    }

    // =========================================================
    // VALIDACION
    // =========================================================

    public bool Exists(int runtimeId)
    {
        return _byRuntimeId.ContainsKey(runtimeId);
    }

    public int Count => _nextRuntimeId - 1;
}