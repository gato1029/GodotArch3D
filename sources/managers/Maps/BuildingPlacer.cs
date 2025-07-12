using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using System.Collections.Generic;
using System.Linq;

public static class BuildingPlacer
{
    // Devuelve las posiciones del mapa que ocupará el edificio
    public static List<(int x, int y)> GetOccupiedMapPositions(
        int mapX, int mapY, BuildingPosition buildingPosition)
    {
        var positions = new List<(int x, int y)>();

        foreach (var tile in buildingPosition.buildingTilePositions)
        {
            int offsetX = tile.X - buildingPosition.centerX;
            int offsetY = tile.Y - buildingPosition.centerY;

            int posX = mapX + offsetX;
            int posY = mapY + offsetY;

            positions.Add((posX, posY));
        }

        return positions;
    }

    // Verifica si las posiciones ocupadas están todas dentro de los límites del mapa y son válidas
    public static bool CanPlaceBuilding(
        int[,] mapGrid,
        int mapX,
        int mapY,
        BuildingPosition buildingPosition)
    {
        int mapWidth = mapGrid.GetLength(0);
        int mapHeight = mapGrid.GetLength(1);

        var positions = GetOccupiedMapPositions(mapX, mapY, buildingPosition);

        foreach (var (x, y) in positions)
        {
            // Verifica si está dentro del mapa
            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                return false;

            // Verifica si está libre (ajusta la lógica según cómo representes el mapa)
            if (mapGrid[x, y] != 0) // 0 = libre
                return false;
        }

        return true;
    }
}
