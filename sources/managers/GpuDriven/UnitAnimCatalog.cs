using System;
using Godot;
using GodotEcsArch.sources.components;
using GodotFlecs.sources.Flecs.Components;

namespace GodotEcsArch.sources.managers.GpuDriven;

/// <summary>
/// Catálogo ultra-rápido de animaciones con acceso O(1) basado en un arreglo plano en RAM.
/// Soporta expansión automática de memoria si se supera la cantidad de tipos de unidades iniciales.
/// </summary>
public class UnitAnimCatalog
{
    private const int TOTAL_TYPES = 8;  // Cantidad de elementos en AnimationType (0 a 7)
    private const int TOTAL_DIRS = 8;   // Cantidad de elementos en AnimationDirection (0 a 7)
    private const int ANIMS_PER_UNIT = TOTAL_TYPES * TOTAL_DIRS; // 64 combinaciones por tipo de unidad

    private readonly AnimationLUTManager _lutManager;
    private int _currentCapacityUnits;
    
    // Arreglo plano auto-expandible [CapacidadUnidades * 64 combinaciones]
    private int[] _lutLookupTable;
    private int _currentGlobalLutIndex = 0;

    public UnitAnimCatalog(int initialMaxUnitTypes = 256)
    {
        _lutManager = new AnimationLUTManager();

        _currentCapacityUnits = initialMaxUnitTypes;

        _lutLookupTable = new int[
            _currentCapacityUnits * ANIMS_PER_UNIT
        ];

        Array.Fill(_lutLookupTable, -1);
    }
    /// <summary>
    /// Obtiene la duración total de un ciclo de animación en segundos en tiempo O(1).
    /// Útil para sistemas de combate, sincronización de golpes o eventos de gameplay.
    /// </summary>
    public float GetAnimationDuration(int unitTypeId, AnimationType animType, AnimationDirection direction)
    {
        // Obtenemos el ID global en la LUT (respetando los fallbacks a DOWN si no existe)
        int lutId = GetLutIndex(unitTypeId, animType, direction);

        // Consultamos la definición original a través del LUTManager
        if (_lutManager.TryGetClip(lutId, out var clipDef))
        {
            return clipDef.CycleDuration;
        }

        return 0.0f; // Retorna 0 si el clip no está registrado
    }
    /// <summary>
    /// Registra una animación asociándole una ranura única en la LUT de VRAM.
    /// Si el unitTypeId supera la capacidad actual, expande automáticamente el arreglo en RAM.
    /// </summary>
    public void RegisterAnimation(int unitTypeId, AnimationType animType, AnimationDirection direction, AnimClipDefinition clipDef)
    {
        if (unitTypeId < 0) return;
        if (animType == AnimationType.NINGUNA) return;

        // 🚀 Si el ID de unidad sobrepasa la capacidad actual, redimensionamos automáticamente el array
        if (unitTypeId >= _currentCapacityUnits)
        {
            EnsureCapacity(unitTypeId + 1);
        }

        // Cálculo del índice plano directo O(1)
        int index = (unitTypeId * ANIMS_PER_UNIT) + ((int)animType * TOTAL_DIRS) + (int)direction;

        int globalLutId = _lutManager.RegisterClip(clipDef);
        _lutLookupTable[index] = globalLutId;
    }

    /// <summary>
    /// Expande la memoria del arreglo plano conservando los datos registrados previamente.
    /// </summary>
    private void EnsureCapacity(int requiredUnits)
    {
        int newCapacity = Math.Max(_currentCapacityUnits * 2, requiredUnits);
        int oldSize = _lutLookupTable.Length;
        int newSize = newCapacity * ANIMS_PER_UNIT;

        Array.Resize(ref _lutLookupTable, newSize);

        // Inicializar únicamente los nuevos slots agregados con -1
        for (int i = oldSize; i < newSize; i++)
        {
            _lutLookupTable[i] = -1;
        }

        GD.Print($"ℹ️ [UnitAnimCatalog] Capacidad ampliada automáticamente de {_currentCapacityUnits} a {newCapacity} tipos de unidades.");
        _currentCapacityUnits = newCapacity;
    }

    /// <summary>
    /// Consulta instantánea O(1) en tiempo de ejecución sin diccionarios ni asignaciones GC.
    /// </summary>
    public int GetLutIndex(int unitTypeId, AnimationType animType, AnimationDirection direction)
    {
        if (animType == AnimationType.NINGUNA) return 0;

        int index = (unitTypeId * ANIMS_PER_UNIT) + ((int)animType * TOTAL_DIRS) + (int)direction;

        if (index >= 0 && index < _lutLookupTable.Length)
        {
            int lutId = _lutLookupTable[index];
            if (lutId != -1) return lutId;

            // Fallback rápido a dirección DOWN
            int fallbackIndex = (unitTypeId * ANIMS_PER_UNIT) + ((int)animType * TOTAL_DIRS) + (int)AnimationDirection.DOWN;
            if (fallbackIndex >= 0 && fallbackIndex < _lutLookupTable.Length)
            {
                int fallbackLutId = _lutLookupTable[fallbackIndex];
                if (fallbackLutId != -1) return fallbackLutId;
            }
        }

        return 0; // Retorna slot 0 por defecto si la combinación no está registrada
    }


    
    /// <summary>
    /// Compila la mini-textura flotante LUT y la inyecta al ShaderMaterial animado.
    /// </summary>
    public void BuildAndApply(ShaderMaterial animatedMaterial)
    {
        _lutManager.BuildAndApply(animatedMaterial);
        GD.Print($"✅ [UnitAnimCatalog] Catálogo compilado con éxito. Total clips registrados en VRAM: {_currentGlobalLutIndex}");
    }
}