
namespace GodotEcsArch.sources.BlackyEngine.GridSparse;

using System.Runtime.CompilerServices;
using Godot;

public class BlackyCombinedGridManager
{
    public readonly BlackyStaticGridLayer StaticLayer;
    public readonly BlackyConcurrentDynamicSparseGrid DynamicLayer;

    public BlackyCombinedGridManager(
        float worldWidth,
        float worldHeight,
        float staticCellSize = 1.0f,
        float dynamicCellSize = 0.5f,
        int maxOccupiedCells = 150000)
    {
        // La capa estática usa celdas más grandes (ej: 1.0f para tiles del mapa)
        StaticLayer = new BlackyStaticGridLayer(worldWidth, worldHeight, staticCellSize);

        // La capa dinámica usa celdas más pequeñas (ej: 0.5f para movimiento preciso de tropas)
        DynamicLayer = new BlackyConcurrentDynamicSparseGrid(worldWidth, worldHeight, dynamicCellSize, maxOccupiedCells);
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