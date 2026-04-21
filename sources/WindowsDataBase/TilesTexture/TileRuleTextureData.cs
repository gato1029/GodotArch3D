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
    public NeighborCondition Condition { get; set; }
    public int TargetID { get; set; } // Solo se usa si Condition es Specific.
}

public class TileRuleTextureData
{
    // Propiedades de identificación y dimensiones
    public string Name { get; set; }
    public int Rows { get; set; }
    public int Columns { get; set; }
    public int AnchorX { get; set; }
    public int AnchorY { get; set; }

    public ConditionSlot[] _inputPattern { get; set; }

    // -1 = Mantener, -2 = Borrar, >=0 = Nuevo ID
    public int[] Output { get; set; }

    // Máscaras binarias
    public long FilterMask { get; set; }
    public long ValueMask { get; set; }

    // IDs específicos
    public int[] _specificIDs { get; set; }
    public bool _hasSpecificRequirements { get; set; }

    public TileRuleTextureData()
    {
        AnchorX = -1;
        AnchorY = -1;
        Name = "NewRule";
    }

    private int ToIndex(int x, int y) => y * Rows + x;

    public void ConfigureDimensions(int width, int height)
    {
        ResetPrecompute();

        if (width * height > 32)
            throw new ArgumentException("El tamaño máximo soportado es 32 celdas.");

        Rows = width;
        Columns = height;

        int size = width * height;

        Output = new int[size];
        _inputPattern = new ConditionSlot[size];
        _specificIDs = new int[size];

        for (int i = 0; i < size; i++)
        {
            Output[i] = -1;
            _specificIDs[i] = -1;

            _inputPattern[i] = new ConditionSlot
            {
                Condition = NeighborCondition.Ignore,
                TargetID = -1
            };
        }

        Precompute();
    }

    public void SetConditionByIndex(int index, NeighborCondition condition, int targetID = -1)
    {
        if (_inputPattern == null)
            throw new InvalidOperationException("InputPattern no inicializado.");

        if (index < 0 || index >= _inputPattern.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        _inputPattern[index] = new ConditionSlot
        {
            Condition = condition,
            TargetID = targetID
        };

        Precompute();
    }

    public ConditionSlot GetConditionByIndex(int index)
    {
        if (_inputPattern == null)
            throw new InvalidOperationException("InputPattern no inicializado.");

        if (index < 0 || index >= _inputPattern.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _inputPattern[index];
    }

    public int GetOutputByIndex(int index)
    {
        if (Output == null)
            throw new InvalidOperationException("Output no inicializado.");

        if (index < 0 || index >= Output.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        return Output[index];
    }

    public void SetOutputByIndex(int index, int value)
    {
        if (Output == null)
            throw new InvalidOperationException("Output no inicializado.");

        if (index < 0 || index >= Output.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        Output[index] = value;
    }

    public void ClearOutput(int defaultValue = -1)
    {
        if (Output == null)
            throw new InvalidOperationException("Output no inicializado.");

        for (int i = 0; i < Output.Length; i++)
            Output[i] = defaultValue;
    }

    public void SetOutput(int x, int y, int value)
    {
        if (Output == null)
            throw new InvalidOperationException("Output no inicializado.");

        int index = ToIndex(x, y);

        if (index < 0 || index >= Output.Length)
            throw new ArgumentOutOfRangeException($"Posición fuera de rango: ({x},{y})");

        Output[index] = value;
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
            throw new InvalidOperationException("Dimensiones no configuradas.");

        if (index < 0 || index >= Rows * Columns)
            throw new ArgumentOutOfRangeException(nameof(index));

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

    public void Precompute()
    {
        FilterMask = 0;
        ValueMask = 0;
        _hasSpecificRequirements = false;

        int bitOffset = 0;

        for (int i = 0; i < _inputPattern.Length; i++)
        {
            var slot = _inputPattern[i];

            if (slot.Condition != NeighborCondition.Ignore)
            {
                FilterMask |= (0x3L << bitOffset);
                ValueMask |= ((long)slot.Condition << bitOffset);

                if (slot.Condition == NeighborCondition.Specific)
                {
                    _specificIDs[i] = slot.TargetID;
                    _hasSpecificRequirements = true;
                }
            }

            bitOffset += 2;
        }
    }

    public bool IsMatch(long mapMask, int worldX, int worldY, IReadOnlyTileMap map)
    {
        if ((mapMask & FilterMask) != ValueMask)
            return false;

        if (_hasSpecificRequirements)
        {
            for (int i = 0; i < _specificIDs.Length; i++)
            {
                int requiredID = _specificIDs[i];
                if (requiredID != -1)
                {
                    int lx = i % Rows;
                    int ly = i / Rows;

                    int targetX = worldX - AnchorX + lx;
                    int targetY = worldY - AnchorY + ly;

                    if (map.GetLogicID(targetX, targetY) != requiredID)
                        return false;
                }
            }
        }

        return true;
    }

    public void Apply(int worldX, int worldY, ITileMapActions map)
    {
        int startX = worldX - AnchorX;
        int startY = worldY - AnchorY;

        for (int i = 0; i < Output.Length; i++)
        {
            int action = Output[i];
            if (action == -1)
                continue;

            int x = i % Rows;
            int y = i / Rows;

            int targetX = startX + x;
            int targetY = startY + y;

            if (map.IsInside(targetX, targetY))
            {
                map.SetVisualTile(targetX, targetY, action);
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
