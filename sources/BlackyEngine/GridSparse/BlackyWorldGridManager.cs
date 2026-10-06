using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace GodotEcsArch.sources.BlackyEngine.GridSparse;



/*
 * (-2048, -2048)                 (2048, -2048)    (3072, -2048)
         +------------------------------+---------------+
         |                              |    SLOT 0     |
         |                              |  (1024x1024)  |
         |        MAIN WORLD            +---------------+ (2048, -1024)
         |       (4096 x 4096)          |    SLOT 1     |
         |                              |  (1024x1024)  |
         |                              +---------------+ (2048, 0)
         |                              |    ESPACIO    |
         |                              |   LIBRE /     |
         |                              |  RESERVADO    |
         +------------------------------+---------------+
   (-2048, 2048)                  (2048, 2048)     (3072, 2048)
 */
public class BlackyWorldGridManager
{

    // Instancia interna del combinador
    public readonly BlackyCombinedGridManager Grid;
    public ConcurrentDictionary<Vector2I, int> ClaimedCellsThisFrame = new();
    // =========================================================================
    // CONSTANTES Y CONFIGURACIÓN DEL ATLAS
    // =========================================================================
    public static readonly Vector2 MaxMainWorldSize = new Vector2(4096f, 4096f);
    public static readonly Vector2 MaxMiniWorldSize = new Vector2(1024f, 1024f);
  
    public const float AtlasWidth = 6144f ;
    public const float AtlasHeight= 4096f ;

    public static readonly Vector2 MainWorldMin = new Vector2(-2048f, -2048f);
    public static readonly Vector2 Slot0Min = new Vector2(2048f, -2048f);
    public static readonly Vector2 Slot1Min = new Vector2(2048f, -1024f);

    private readonly bool[] _slotOccupied = new bool[2];

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================
    public BlackyWorldGridManager(
        float staticCellSize = 16.0f,
        int dynamicCellSize = 16,
        int maxOccupiedDynamicCells = 150000)
    {
        // El Atlas completo mide 6144 x 4096 unidades
        Grid = new BlackyCombinedGridManager(
            worldWidthStatic: AtlasWidth,
            worldHeightStatic: AtlasHeight,
            worldWidthDynamic: AtlasWidth,
            worldHeightDynamic: AtlasHeight,
            staticCellSize: staticCellSize,
            dynamicCellSize: dynamicCellSize,
            maxOccupiedCells: maxOccupiedDynamicCells
        );
    }

    // =========================================================================
    // 1. GESTIÓN DE UNIDADES Y CAPA DINÁMICA (Frame a Frame)
    // =========================================================================

    /// 
    /// Limpia las posiciones dinámicas del frame anterior. Llamar al inicio de _PhysicsProcess.
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BeginDynamicFrame()
    {
        ClaimedCellsThisFrame.Clear();
        Grid.DynamicLayer.ClearOccupiedOnly();
    }

    /// 
    /// Registra la posición de una unidad individual en el Atlas.
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RegisterUnit(in Vector2 globalAtlasPos, float radius = 0.5f)
    {
        if (radius <= 0.5f)
        {
            Grid.DynamicLayer.OccupyCell(globalAtlasPos);
        }
        else
        {
            Grid.DynamicLayer.OccupyArea(globalAtlasPos, radius);
        }
    }

    // =========================================================================
    // 2. GESTIÓN DEL MUNDO PRINCIPAL (Capa Estática)
    // =========================================================================

    public void LoadMainWorld(byte[,] obstacleMatrix)
    {
        Grid.StaticLayer.ClearRegion(MainWorldMin, MaxMainWorldSize);
        Grid.StaticLayer.LoadRegionData(MainWorldMin, obstacleMatrix);
    }

    public void PlaceStaticObjectInMainWorld(Vector2 localCenterPos, Vector2 worldSize, byte obstacleType = 1)
    {
        Vector2 atlasPos = MainWorldLocalToAtlasPos(localCenterPos);
        Grid.StaticLayer.PlaceStaticObject(atlasPos, worldSize, obstacleType);
    }

    public void RemoveStaticObjectFromMainWorld(Vector2 localCenterPos, Vector2 worldSize)
    {
        Vector2 atlasPos = MainWorldLocalToAtlasPos(localCenterPos);
        Grid.StaticLayer.PlaceStaticObject(atlasPos, worldSize, obstacleType: 0);
    }

    // =========================================================================
    // 3. GESTIÓN DE MINI MUNDOS / SLOTS (Capa Estática)
    // =========================================================================

    public void LoadMiniWorld(int slotIndex, byte[,] obstacleMatrix)
    {
        Vector2 slotOffset = GetSlotOffset(slotIndex);
        Grid.StaticLayer.ClearRegion(slotOffset, MaxMiniWorldSize);
        Grid.StaticLayer.LoadRegionData(slotOffset, obstacleMatrix);
        _slotOccupied[slotIndex] = true;
    }

    public void UnloadMiniWorld(int slotIndex)
    {
        Vector2 slotOffset = GetSlotOffset(slotIndex);
        Grid.StaticLayer.ClearRegion(slotOffset, MaxMiniWorldSize);
        _slotOccupied[slotIndex] = false;
    }

    public void PlaceStaticObjectInMiniWorld(int slotIndex, Vector2 localCenterPos, Vector2 worldSize, byte obstacleType = 1)
    {
        Vector2 atlasPos = MiniWorldLocalToAtlasPos(slotIndex, localCenterPos);
        Grid.StaticLayer.PlaceStaticObject(atlasPos, worldSize, obstacleType);
    }

    public void RemoveStaticObjectFromMiniWorld(int slotIndex, Vector2 localCenterPos, Vector2 worldSize)
    {
        Vector2 atlasPos = MiniWorldLocalToAtlasPos(slotIndex, localCenterPos);
        Grid.StaticLayer.PlaceStaticObject(atlasPos, worldSize, obstacleType: 0);
    }

    public bool IsSlotOccupied(int slotIndex) => _slotOccupied[slotIndex];

    // =========================================================================
    // 4. CONSULTAS DE COLISIÓN Y NAVEGACIÓN
    // =========================================================================

    /// 
    /// Consulta combinada: Retorna verdadero si la posición está bloqueada por muro (1.0f) O unidad (0.5f).
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked(in Vector2 globalAtlasPos)
    {
        return Grid.IsBlocked(globalAtlasPos);
    }

    /// 
    /// Consulta si solo la capa estática (muros/terreno) está bloqueada.
    /// 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsStaticBlocked(in Vector2 globalAtlasPos)
    {
        return Grid.StaticLayer.IsBlocked(globalAtlasPos);
    }

    // =========================================================================
    // 5. CONVERSIÓN DE COORDENADAS (Local ◄► Atlas)
    // =========================================================================

    public Vector2 MainWorldLocalToAtlasPos(Vector2 localPos) => MainWorldMin + localPos;
    public Vector2 AtlasToMainWorldLocalPos(Vector2 atlasPos) => atlasPos - MainWorldMin;

    public Vector2 MiniWorldLocalToAtlasPos(int slotIndex, Vector2 localPos) => GetSlotOffset(slotIndex) + localPos;
    public Vector2 AtlasToMiniWorldLocalPos(int slotIndex, Vector2 atlasPos) => atlasPos - GetSlotOffset(slotIndex);

    public Vector2 GetSlotOffset(int slotIndex) => slotIndex switch
    {
        0 => Slot0Min,
        1 => Slot1Min,
        _ => throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot inválido (Solo 0 y 1).")
    };
}