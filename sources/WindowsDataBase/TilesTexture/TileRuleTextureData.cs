using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;

namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;

/// <summary>
/// Estados lógicos para la comparación de vecinos.
/// Ocupan 2 bits (0-3).
/// </summary>
public enum NeighborCondition : byte
{
    Ignore = 0, // 00: No importa qué haya.
    Empty = 1, // 01: La celda debe tener ID 0.
    Filled = 2, // 10: Cualquier ID mayor a 0.
    Specific = 3, // 11: Un ID exacto (definido en SpecificIDs).
}

/// <summary>
/// Configuración de entrada para una celda del patrón.
/// </summary>
public struct ConditionSlot
{
    public NeighborCondition Condition;
    public int TargetID; // Solo se usa si Condition es Specific.
}

public class TileRuleTextureData
{
    // Propiedades de identificación y dimensiones
    public string Name { get; set; }
    public int Rows { get; private set; }
    public int Columns { get; private set; }
    public int AnchorX { get; private set; } // origen x
    public int AnchorY { get; private set; } // origen y

    private ConditionSlot[,] _inputPattern;

    // El Sello (Output): -1 = Mantener, -2 = Borrar, >=0 = Nuevo ID
    public int[,] Output { get; private set; }

    // Máscaras de Bits (Soportan hasta 5x5 celdas usando 2 bits c/u)
    public long FilterMask { get; private set; }
    public long ValueMask { get; private set; }

    // Optimización para IDs específicos
    private int[] _specificIDs;
    private bool _hasSpecificRequirements;

    public TileRuleTextureData()
    {
        Name = "NewRule";
    }

    public void ConfigureDimensions(int width, int height)
    {
        ResetPrecompute();
        if (width * height > 32)
            throw new ArgumentException("El tamaño máximo soportado es 32 celdas (ej. 5x5).");

        Rows = width;
        Columns = height;

        // Re-crear estructuras internas
        Output = new int[width, height];
        _specificIDs = new int[width * height];

        // Inicialización por defecto
        for (int i = 0; i < _specificIDs.Length; i++)
            _specificIDs[i] = -1;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            Output[x, y] = -1;
        _inputPattern = new ConditionSlot[width, height];

        // Inicialización por defecto
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _inputPattern[x, y] = new ConditionSlot
                {
                    Condition = NeighborCondition.Ignore,
                    TargetID = -1,
                };
            }
        }
        Precompute();
    }

    public void SetConditionByIndex(int index, NeighborCondition condition, int targetID = -1)
    {
        if (_inputPattern == null)
            throw new InvalidOperationException(
                "InputPattern no inicializado. Llama a ConfigureDimensions."
            );

        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

        int x = index % Rows;
        int y = index / Rows;

        _inputPattern[x, y] = new ConditionSlot { Condition = condition, TargetID = targetID };
        Precompute();
    }

    public ConditionSlot GetConditionByIndex(int index)
    {
        if (_inputPattern == null)
            throw new InvalidOperationException("InputPattern no inicializado.");

        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

        int x = index % Rows;
        int y = index / Rows;

        return _inputPattern[x, y];
    }

    public int GetOutputByIndex(int index)
    {
        if (Output == null)
            throw new InvalidOperationException("Output no está inicializado.");

        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

        int x = index % Rows;
        int y = index / Rows;

        return Output[x, y];
    }

    public void SetOutputByIndex(int index, int value)
    {
        if (Output == null)
            throw new InvalidOperationException(
                "Output no está inicializado. Llama primero a ConfigureDimensions."
            );

        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index), $"Índice fuera de rango: {index}");

        int x = index % Rows;
        int y = index / Rows;

        Output[x, y] = value;
    }

    public void ClearOutput(int defaultValue = -1)
    {
        if (Output == null)
            throw new InvalidOperationException("Output no está inicializado.");

        for (int y = 0; y < Columns; y++)
        {
            for (int x = 0; x < Rows; x++)
            {
                Output[x, y] = defaultValue;
            }
        }
    }

    public void SetOutput(int x, int y, int value)
    {
        if (Output == null)
            throw new InvalidOperationException(
                "Output no está inicializado. Llama primero a ConfigureDimensions."
            );

        if (x < 0 || x >= Rows || y < 0 || y >= Columns)
            throw new ArgumentOutOfRangeException($"Posición fuera de rango: ({x},{y})");

        Output[x, y] = value;
    }

    public void SetAnchor(int anchorX, int anchorY)
    {
        AnchorX = anchorX;
        AnchorY = anchorY;
    }

    public bool IsAnchor(int index)
    {
        if (Rows <= 0 || Columns <= 0)
            throw new InvalidOperationException("Dimensiones no configuradas.");

        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

        int x = index % Rows;
        int y = index / Rows;

        return x == AnchorX && y == AnchorY;
    }

    public void SetSize(int width, int height)
    {
        ConfigureDimensions(width, height);
    }

    public void SetAnchorByIndex(int index)
    {
        if (Rows <= 0 || Columns <= 0)
            throw new InvalidOperationException(
                "Dimensiones no configuradas. Llama a ConfigureDimensions."
            );

        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index), $"Índice fuera de rango: {index}");

        AnchorX = index % Rows;
        AnchorY = index / Rows;
    }

    public (int x, int y) IndexToCoords(int index)
    {
        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

        return (index % Rows, index / Rows);
    }

    public void ResetPrecompute()
    {
        FilterMask = 0;
        ValueMask = 0;
        _hasSpecificRequirements = false;
    }

    /// <summary>
    /// Compila el patrón de entrada en máscaras binarias de alto rendimiento.
    /// </summary>
    public void Precompute()
    {
        FilterMask = 0;
        ValueMask = 0;
        _hasSpecificRequirements = false;

        int bitOffset = 0;

        for (int y = 0; y < Columns; y++)
        {
            for (int x = 0; x < Rows; x++)
            {
                var slot = _inputPattern[x, y];
                int index = y * Rows + x;

                if (slot.Condition != NeighborCondition.Ignore)
                {
                    FilterMask |= (0x3L << bitOffset);
                    ValueMask |= ((long)slot.Condition << bitOffset);

                    if (slot.Condition == NeighborCondition.Specific)
                    {
                        _specificIDs[index] = slot.TargetID;
                        _hasSpecificRequirements = true;
                    }
                }

                bitOffset += 2;
            }
        }
    }

    /// <summary>
    /// Valida si el área del mapa coincide con esta regla.
    /// </summary>
    public bool IsMatch(long mapMask, int worldX, int worldY, IReadOnlyTileMap map)
    {
        // Paso 1: Comparación binaria (O(1)). El 99% de las veces termina aquí.
        if ((mapMask & FilterMask) != ValueMask)
            return false;

        // Paso 2: Validación de IDs específicos (solo si existen).
        if (_hasSpecificRequirements)
        {
            for (int i = 0; i < _specificIDs.Length; i++)
            {
                int requiredID = _specificIDs[i];
                if (requiredID != -1)
                {
                    int lx = i % Rows;
                    int ly = i / Rows;

                    // Posición relativa al mundo considerando el ancla
                    int targetX = worldX - AnchorX + lx;
                    int targetY = worldY - AnchorY + ly;

                    if (map.GetLogicID(targetX, targetY) != requiredID)
                        return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Aplica los cambios visuales definidos en el Output de la regla.
    /// </summary>
    public void Apply(int worldX, int worldY, ITileMapActions map)
    {
        int startX = worldX - AnchorX;
        int startY = worldY - AnchorY;

        for (int y = 0; y < Columns; y++)
        {
            for (int x = 0; x < Rows; x++)
            {
                int action = Output[x, y];
                if (action == -1)
                    continue; // Mantener original

                int targetX = startX + x;
                int targetY = startY + y;

                if (map.IsInside(targetX, targetY))
                {
                    map.SetVisualTile(targetX, targetY, action);
                }
            }
        }
    }
}

// Interfaces para desacoplar la regla de la implementación del mapa
public interface IReadOnlyTileMap
{
    int GetLogicID(int x, int y);
}

public interface ITileMapActions
{
    bool IsInside(int x, int y);
    void SetVisualTile(int x, int y, int tileID);
}
