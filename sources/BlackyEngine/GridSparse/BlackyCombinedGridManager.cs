
namespace GodotEcsArch.sources.BlackyEngine.GridSparse;

using System.Runtime.CompilerServices;
using Godot;

public class BlackyCombinedGridManager
{
    public readonly BlackyStaticGridLayer StaticLayer;
    public readonly BlackyConcurrentDynamicSparseGrid DynamicLayer;

    public BlackyCombinedGridManager(
        float worldWidthStatic,
        float worldHeightStatic,
        float worldWidthDynamic,
        float worldHeightDynamic,
        float staticCellSize = 16f,
        int dynamicCellSize = 16,
        int maxOccupiedCells = 150000)
    {
        // La capa estática usa celdas más grandes (ej: 1.0f para tiles del mapa)
        StaticLayer = new BlackyStaticGridLayer(worldWidthStatic, worldHeightStatic, staticCellSize);

        // La capa dinámica usa celdas más pequeñas (ej: 0.5f para movimiento preciso de tropas)
        DynamicLayer = new BlackyConcurrentDynamicSparseGrid(worldWidthDynamic, worldHeightDynamic, dynamicCellSize, maxOccupiedCells);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked(in Vector2 worldPos)
    {
        // 1. Evalúa según la resolución de la capa estática (1.0f)
        if (StaticLayer.IsBlocked(worldPos))
            return true;

        // 2. Evalúa según la resolución de la capa dinámica (0.5f)
        return DynamicLayer.IsBlocked(worldPos);
    }
}