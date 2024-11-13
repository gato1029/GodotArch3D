using Arch.Core;
using Arch.LowLevel;
using Arch.LowLevel.Jagged;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;


internal class SpatialHashMap<TItem> where TItem : struct
{
   
    private readonly float cellSize;   
    public readonly Dictionary<int, Dictionary<int, TItem>> cellMap;
    private readonly Dictionary<int, int> mapItems;

    private readonly Func<TItem, int> getPositionItem;

    private int offsetCantor = 10000;
    private int count = 0;
    public SpatialHashMap(float cellSize, Func<TItem, int> getPositionItem)
    {
        this.cellSize = cellSize;
        this.getPositionItem = getPositionItem;
        this.cellMap = new Dictionary<int, Dictionary<int, TItem>>();
        this.mapItems = new Dictionary<int, int>();
    }

    public int CantorPairing(int X, int Y)
    {
        // Ajusta x e y para que sean no negativos
        int x = X + offsetCantor;
        int y = Y + offsetCantor;
   
        return (x + y) * (x + y + 1) / 2 + y;
    }

    private int GetCellIndex(Vector2 position)
    {
        int cellX = (int)(position.X / cellSize);
        int cellY = (int)(position.Y / cellSize);
        return CantorPairing(cellX,cellY); 
    }

   
    public void AddItem(Vector2 positionCell,TItem item)
    {        
        int cellIndex = GetCellIndex(positionCell);

        if (!cellMap.ContainsKey(cellIndex))
        {
            cellMap[cellIndex] = new Dictionary<int, TItem>();
        }
       
        int itemId = getPositionItem(item);
        cellMap[cellIndex][itemId] = item;

        mapItems.Add(itemId, cellIndex);
    }
    public void AddUpdateItem(Vector2 positionCell, in TItem item)
    {
        int itemId = getPositionItem(item);
        int cellIndex = GetCellIndex(positionCell);
        if (mapItems.ContainsKey(itemId))
        {
            int cellIndexPast = mapItems[itemId];
            if (cellIndex != cellIndexPast)
            {
                cellMap[cellIndexPast].Remove(itemId);
                if (!cellMap.ContainsKey(cellIndex))
                {
                    cellMap[cellIndex] = new Dictionary<int, TItem>();
                }
                cellMap[cellIndex][itemId] = item;
                mapItems[itemId] = cellIndex;
            }            
        }
        else
        {         
            if (!cellMap.ContainsKey(cellIndex))
            {
                cellMap[cellIndex] = new Dictionary<int, TItem>();
            }
            cellMap[cellIndex][itemId] = item;
            mapItems.Add(itemId, cellIndex);
            count++;
        }
      
    }
    public void RemoveItem(Vector2 positionCell, TItem item)
    {
     
        int cellIndex = GetCellIndex(positionCell);
        int itemId = getPositionItem(item);

        if (cellMap.ContainsKey(cellIndex))
        {
            cellMap[cellIndex].Remove(itemId);
            mapItems.Remove(itemId);
            count--;         
        }
    }
    public void RemoveItem(TItem item)
    {       
        int itemId = getPositionItem(item);
        int cellIndex = mapItems[itemId];

        if (cellMap.ContainsKey(cellIndex))
        {
            cellMap[cellIndex].Remove(itemId);
            mapItems.Remove(itemId);
            count--;
        }
    }

    public Dictionary<int, TItem> QueryPosition(Vector2 positionCell) 
    {
        int cellIndex = GetCellIndex(positionCell);
        if (cellMap.ContainsKey(cellIndex))
        {
            return cellMap[cellIndex];
        }
        return null;
    }

    public Dictionary<int, TItem> QueryDirection(Vector2 position, Vector2 direction, float distance)
    {
        Vector2 newPosition = position + direction * distance;
        int cellIndex = GetCellIndex(newPosition);
        if (cellMap.ContainsKey(cellIndex))
        {
            return cellMap[cellIndex];
        }
        return null;
    }

    public Dictionary<int,Dictionary<int, TItem>> GetPossibleQuadrants(Vector2 position,  float distance)
    {
        Dictionary<int,Dictionary<int, TItem>> quadrants = new Dictionary<int,Dictionary<int, TItem>>();      
        Vector2[] offsets = new Vector2[]
            {
            new Vector2(distance, 0),    // Derecha
            new Vector2(-distance, 0),   // Izquierda
            new Vector2(0, distance),    // Arriba
            new Vector2(0, -distance),   // Abajo
            new Vector2(distance, distance),    // Diagonal superior derecha
            new Vector2(-distance, distance),   // Diagonal superior izquierda
            new Vector2(distance, -distance),   // Diagonal inferior derecha
            new Vector2(-distance, -distance),    // Diagonal inferior izquierda
            new Vector2(0, 0)    // misma posicion
            };

        // Agregar los índices de las celdas correspondientes, excluyendo la celda original
        foreach (var offset in offsets)
        {
            Vector2 newPosition = position + offset;
            int cellIndex = GetCellIndex(newPosition);
            if (!quadrants.ContainsKey(cellIndex) &&cellMap.ContainsKey(cellIndex)) 
            {
                quadrants.Add(cellIndex,cellMap[cellIndex]);
            }
        }

        return quadrants;
    }

    public Dictionary<int, Dictionary<int, TItem>> GetPossibleQuadrants(Vector2 position, Vector2 size)
    {
        Dictionary<int, Dictionary<int, TItem>> quadrants = new Dictionary<int, Dictionary<int, TItem>>();
        Vector2[] offsets = new Vector2[]
            {
            new Vector2(size.X, 0),    // Derecha
            new Vector2(-size.X, 0),   // Izquierda
            new Vector2(0, size.Y),    // Arriba
            new Vector2(0, -size.Y),   // Abajo
            new Vector2(size.X, size.Y),    // Diagonal superior derecha
            new Vector2(-size.X, size.Y),   // Diagonal superior izquierda
            new Vector2(size.X, -size.Y),   // Diagonal inferior derecha
            new Vector2(-size.X, -size.Y),    // Diagonal inferior izquierda
            new Vector2(0, 0)    // misma posicion
            };

        // Agregar los índices de las celdas correspondientes, excluyendo la celda original
        foreach (var offset in offsets)
        {
            Vector2 newPosition = position + offset;
            int cellIndex = GetCellIndex(newPosition);
            if (!quadrants.ContainsKey(cellIndex) && cellMap.ContainsKey(cellIndex))
            {
                quadrants.Add(cellIndex, cellMap[cellIndex]);
            }
        }

        return quadrants;
    }
    public Dictionary<int, Dictionary<int, TItem>> GetPossibleQuadrantsInRect(Vector2 position, Rect2 rect)
    {
       
        Dictionary<int, Dictionary<int, TItem>> quadrants = new Dictionary<int, Dictionary<int, TItem>>();

     
        Vector2 rectStart = rect.Position + position; 
        Vector2 rectEnd = rectStart + rect.Size; 

        
        int startX = (int)Math.Floor(rectStart.X / cellSize);
        int startY = (int)Math.Floor(rectStart.Y / cellSize);
        int endX = (int)Math.Floor(rectEnd.X / cellSize);
        int endY = (int)Math.Floor(rectEnd.Y / cellSize);

       
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                
                int cellIndex = CantorPairing(x, y);

           
                if (cellMap.ContainsKey(cellIndex))
                {
                    if (!quadrants.ContainsKey(cellIndex))
                    {
                        quadrants.Add(cellIndex, cellMap[cellIndex]);
                    }
                }
            }
        }

        return quadrants;
    }
}
