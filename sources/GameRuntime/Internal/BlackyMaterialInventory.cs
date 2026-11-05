using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.GameRuntime.Internal;

public class BlackyMaterialInventory
{
    private Dictionary<long, int> materials = new();

    public IReadOnlyDictionary<long, int> Materials => materials;

    public event Action<long, int> OnMaterialChanged;
    public event Action OnInventoryChanged;

    public int GetAmount(long idMaterial)
    {
        return materials.TryGetValue(idMaterial, out int amount)
            ? amount
            : 0;
    }

    public void AddMaterial(long idMaterial, int amount)
    {
        if (amount <= 0)
            return;

        if (materials.ContainsKey(idMaterial))
            materials[idMaterial] += amount;
        else
            materials.Add(idMaterial, amount);

        OnMaterialChanged?.Invoke(idMaterial, materials[idMaterial]);
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveMaterial(long idMaterial, int amount)
    {
        if (amount <= 0)
            return false;

        if (!materials.TryGetValue(idMaterial, out int currentAmount))
            return false;

        if (currentAmount < amount)
            return false;

        int newAmount = currentAmount - amount;

        if (newAmount == 0)
            materials.Remove(idMaterial);
        else
            materials[idMaterial] = newAmount;

        OnMaterialChanged?.Invoke(idMaterial, newAmount);
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool HasMaterial(long idMaterial, int amount)
    {
        return GetAmount(idMaterial) >= amount;
    }

    public void Clear()
    {
        var ids = materials.Keys.ToArray();

        materials.Clear();

        foreach (long id in ids)
            OnMaterialChanged?.Invoke(id, 0);

        OnInventoryChanged?.Invoke();
    }
}