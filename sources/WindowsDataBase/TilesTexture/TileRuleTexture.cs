using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;

/// <summary>
/// Estados lógicos para la comparación de vecinos.
/// Ocupan 2 bits (0-3).
/// </summary>
public enum NeighborCondition : byte
{
    Ignore = 0,    // 00: No importa qué haya.
    Empty = 1,     // 01: La celda debe tener ID 0.
    Filled = 2,    // 10: Cualquier ID mayor a 0.
    Specific = 3   // 11: Un ID exacto (definido en SpecificIDs).
}

/// <summary>
/// Configuración de entrada para una celda del patrón.
/// </summary>
public struct ConditionSlot
{
    public NeighborCondition Condition;
    public int TargetID; // Solo se usa si Condition es Specific.
}
public class TileRuleTexture
{

    // Propiedades de identificación y dimensiones
    public string Name { get; set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int AnchorX { get; private set; } // origen x
    public int AnchorY { get; private set; } // origen y

    // El Sello (Output): -1 = Mantener, 0 = Borrar, >0 = Nuevo ID
    public int[,] Output { get; private set; }

    // Máscaras de Bits (Soportan hasta 5x5 celdas usando 2 bits c/u)
    public long FilterMask { get; private set; }
    public long ValueMask { get; private set; }

    // Optimización para IDs específicos
    private int[] _specificIDs;
    private bool _hasSpecificRequirements;

    public TileRuleTexture(int width, int height, int anchorX, int anchorY, string name = "New Rule")
    {
        if (width * height > 32)
            throw new ArgumentException("El tamaño máximo soportado por la máscara de 2 bits es de 32 celdas (ej. 5x5).");

        Name = name;
        Width = width;
        Height = height;
        AnchorX = anchorX;
        AnchorY = anchorY;

        Output = new int[width, height];
        _specificIDs = new int[width * height];

        // Inicialización por defecto
        for (int i = 0; i < _specificIDs.Length; i++) _specificIDs[i] = -1;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                Output[x, y] = -1;
    }

    /// <summary>
    /// Compila el patrón de entrada en máscaras binarias de alto rendimiento.
    /// </summary>
    public void Precompute(ConditionSlot[,] inputPattern)
    {
        FilterMask = 0;
        ValueMask = 0;
        _hasSpecificRequirements = false;
        int bitOffset = 0;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var slot = inputPattern[x, y];
                int index = y * Width + x;

                if (slot.Condition != NeighborCondition.Ignore)
                {
                    // Activamos el filtro para estos 2 bits
                    FilterMask |= (0x3L << bitOffset);

                    // Guardamos el valor de la condición (01, 10 o 11)
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
                    int lx = i % Width;
                    int ly = i / Width;

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

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int action = Output[x, y];
                if (action == -1) continue; // Mantener original

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



