using System;
using Godot;

namespace GodotEcsArch.sources.managers.GpuDriven;

public class SimpleAnimCatalog
{
    private readonly AnimationLUTManager _lutManager;

    private int[] _lookupTable;
    private int _currentCapacity;

    public SimpleAnimCatalog(int initialCapacity = 256)
    {
        _lutManager = new AnimationLUTManager();

        _currentCapacity = initialCapacity;
        _lookupTable = new int[initialCapacity];

        Array.Fill(_lookupTable, -1);
    }
    /// <summary>
    /// Obtiene la duración total de un ciclo de animación en segundos en tiempo O(1).
    /// Útil para eventos
    /// </summary>
    public float GetAnimationDuration(int unitTypeId)
    {
        // Obtenemos el ID global en la LUT (respetando los fallbacks a DOWN si no existe)
        int lutId = GetLutIndex(unitTypeId);

        // Consultamos la definición original a través del LUTManager
        if (_lutManager.TryGetClip(lutId, out var clipDef))
        {
            return clipDef.CycleDuration;
        }

        return 0.0f; // Retorna 0 si el clip no está registrado
    }
    public void RegisterAnimation(
        int animationId,
        AnimClipDefinition clipDef)
    {
        if (animationId < 0)
            return;

        EnsureCapacity(animationId + 1);

        int globalLutId = _lutManager.RegisterClip(clipDef);

        _lookupTable[animationId] = globalLutId;
    }

    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _currentCapacity)
            return;

        int newCapacity = Math.Max(
            _currentCapacity * 2,
            requiredCapacity
        );

        int oldSize = _lookupTable.Length;

        Array.Resize(ref _lookupTable, newCapacity);

        for (int i = oldSize; i < newCapacity; i++)
            _lookupTable[i] = -1;

        _currentCapacity = newCapacity;
    }

    public int GetLutIndex(int animationId)
    {
        if ((uint)animationId >= (uint)_lookupTable.Length)
            return 0;

        int lutId = _lookupTable[animationId];

        return lutId != -1 ? lutId : 0;
    }

    public void BuildAndApply(ShaderMaterial animatedMaterial)
    {
        _lutManager.BuildAndApply(animatedMaterial);
    }
}