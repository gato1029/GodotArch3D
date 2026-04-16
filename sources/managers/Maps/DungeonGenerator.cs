
using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using LiteDB;
using ProtoBuf;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


public enum RoomShape
{
    Rectangle,
    Circle,
    Cross
}
public enum TileTypeDungeon
{
    Floor,
    Wall,
    Door,
    AccesoSalidaMazmorra,
    SiguienteNivelMazmorra,
    BaldosaPiso,
    DetallePiso,
}
public class DungeonGenerator
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    private ArrayView2D<TileTypeDungeon> map;
    private List<Rectangle> rooms;
    private List<Point> doors;

    public DungeonGenerator(int width, int height)
    {
        Width = width;
        Height = height;
        map = new ArrayView2D<TileTypeDungeon>(width, height);
        rooms = new List<Rectangle>();
        doors = new List<Point>();

        // Inicializar todo en suelo
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                map[x, y] = TileTypeDungeon.Floor;
    }
    public void PlaceDungeonAccessPoints(int minDistanceFromWall = 10)
    {
        Random rand = new Random();

        // --- Colocar AccesoSalidaMazmorra ---
        // Debe estar en un piso pero fuera de las salas
        bool placedExit = false;
        while (!placedExit)
        {
            int x = rand.Next(minDistanceFromWall, Width - minDistanceFromWall);
            int y = rand.Next(minDistanceFromWall, Height - minDistanceFromWall);

            // No puede estar en un muro ni en una sala
            if (map[x, y] == TileTypeDungeon.Floor)
            {
                bool insideRoom = false;
                foreach (var r in rooms)
                {
                    if (r.Contains(new Point(x, y))) { insideRoom = true; break; }
                }

                if (!insideRoom)
                {
                    map[x, y] = TileTypeDungeon.AccesoSalidaMazmorra;
                    placedExit = true;
                }
            }
        }

        // --- Colocar SiguienteNivelMazmorra ---
        // Puede estar en cualquier piso (incluso dentro de salas)
        bool placedNext = false;
        while (!placedNext)
        {
            int x = rand.Next(minDistanceFromWall, Width - minDistanceFromWall);
            int y = rand.Next(minDistanceFromWall, Height - minDistanceFromWall);

            if (map[x, y] == TileTypeDungeon.Floor)
            {
                map[x, y] = TileTypeDungeon.SiguienteNivelMazmorra;
                placedNext = true;
            }
        }
    }
    public int CalculateMinimumRooms(int minRoomSize, int maxRoomSize)
    {
        // Área promedio de una sala
        double avgRoomSize = (minRoomSize + maxRoomSize) / 2.0;
        double avgRoomArea = avgRoomSize * avgRoomSize;

        // Área efectiva de la mazmorra (considerando borde de 2 muros)
        double effectiveWidth = Width - 4;
        double effectiveHeight = Height - 4;
        double dungeonArea = effectiveWidth * effectiveHeight;

        // Número mínimo de salas aproximado
        int numRoomsMin = (int)Math.Ceiling(dungeonArea / avgRoomArea);

        return numRoomsMin;
    }
    public (int minSize, int maxSize) CalculateRoomSizeRange(double minPercent = 0.05, double maxPercent = 0.15, int absoluteMin = 20)
    {
        // Usamos la dimensión más pequeña del mapa como referencia
        int baseDimension = Math.Min(Width, Height);

        // Calculamos los tamaños como porcentaje del mapa
        int minSize = (int)(baseDimension * minPercent);
        int maxSize = (int)(baseDimension * maxPercent);

        // Aseguramos un mínimo absoluto
        if (minSize < absoluteMin) minSize = absoluteMin;
        if (maxSize < minSize + 5) maxSize = minSize + 5;

        return (minSize, maxSize);
    }

    public void GenerateRandomFilled(int minSeparation = 5, int maxSeparation = 15)
    {
        // Para salas grandes
        var (minLarge, maxLarge) = CalculateRoomSizeRange(0.08, 0.20, 30);

        // Para salas pequeñas (un poco más chicas pero nunca menores a 25x25)
        var (minSmall, maxSmall) = CalculateRoomSizeRange(0.01, 0.10, 15);

        int largeRooms = CalculateMinimumRooms(minLarge, maxLarge);
        int smallRooms = CalculateMinimumRooms(minSmall, maxSmall);
        int wallThickness = 4;
        Random rand = new Random();
        rooms.Clear();
        doors.Clear();

        // Inicializar mapa con borde
        int wallBorder = 4;
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                map[x, y] = (x < wallBorder || y < wallBorder || x >= Width - wallBorder || y >= Height - wallBorder)
                            ? TileTypeDungeon.Wall : TileTypeDungeon.Floor;

        // --- Función auxiliar para verificar separaciones ---
        bool IsValidRoom(Rectangle newRoom)
        {
            bool closeEnough = false;

            foreach (var r in rooms)
            {
                // Expandido con minSeparation → si toca, es inválido
                var minExpanded = new Rectangle(
                    r.MinExtentX - minSeparation,
                    r.MinExtentY - minSeparation,
                    r.Width + minSeparation * 2,
                    r.Height + minSeparation * 2);

                if (newRoom.Intersects(minExpanded))
                    return false; // ❌ demasiado cerca

                // Expandido con maxSeparation → si toca, entonces está dentro del rango
                var maxExpanded = new Rectangle(
                    r.MinExtentX - maxSeparation,
                    r.MinExtentY - maxSeparation,
                    r.Width + maxSeparation * 2,
                    r.Height + maxSeparation * 2);

                if (newRoom.Intersects(maxExpanded))
                    closeEnough = true;
            }

            // ✅ debe estar lo bastante cerca de al menos una sala
            return closeEnough || rooms.Count == 0; // la primera sala se acepta siempre
        }


        // --- Primera pasada: salas grandes ---
        int attempts = 0;
        int maxAttempts = largeRooms * 20;

        while (rooms.Count < largeRooms && attempts < maxAttempts)
        {
            attempts++;

            int roomWidth = rand.Next(minLarge, maxLarge);
            int roomHeight = rand.Next(minLarge, maxLarge);

            int x = rand.Next(wallBorder + minSeparation, Width - wallBorder - roomWidth - minSeparation);
            int y = rand.Next(wallBorder + minSeparation, Height - wallBorder - roomHeight - minSeparation);

            var newRoom = new Rectangle(x, y, roomWidth, roomHeight);

            if (!IsValidRoom(newRoom)) continue;

            rooms.Add(newRoom);
            CarveRoom(newRoom, RoomShape.Rectangle, wallThickness);
            EnsureRoomConnection(newRoom, wallThickness);
        }

        // --- Segunda pasada: salas pequeñas ---
        attempts = 0;
        maxAttempts = smallRooms * 50;
        int placedSmallRooms = 0;

        while (placedSmallRooms < smallRooms && attempts < maxAttempts)
        {
            attempts++;

            int roomWidth = rand.Next(minSmall, maxSmall);
            int roomHeight = rand.Next(minSmall, maxSmall);

            int x = rand.Next(wallBorder + minSeparation, Width - wallBorder - roomWidth - minSeparation);
            int y = rand.Next(wallBorder + minSeparation, Height - wallBorder - roomHeight - minSeparation);

            var newRoom = new Rectangle(x, y, roomWidth, roomHeight);

            if (!IsValidRoom(newRoom)) continue;

            rooms.Add(newRoom);
            CarveRoom(newRoom, RoomShape.Rectangle, wallThickness);
            EnsureRoomConnection(newRoom, wallThickness);
            placedSmallRooms++;
        }

        PlaceDungeonAccessPoints();
        AddRandomBaldosaInRooms(0.9);
        AddRandomBaldosaOutsideRooms(0.2);
    }

    public void AddRandomBaldosaInRooms(double probability = 0.05)
    {
        Random rand = new Random();

        foreach (var room in rooms)
        {
            for (int y = room.MinExtentY + 4; y < room.MaxExtentY - 6; y++)
            {
                for (int x = room.MinExtentX + 4; x < room.MaxExtentX - 4; x++)
                {
                    // Solo sobre el suelo
                    if (map[x, y] == TileTypeDungeon.Floor)
                    {
                        if (rand.NextDouble() < probability)
                        {
                            // Aquí defines el tile aleatorio que quieres poner
                            // Ejemplo: colocar una baldosa distinta
                            map[x, y] = TileTypeDungeon.BaldosaPiso;
                        }
                    }
                }
            }
        }
    }
    public void AddRandomBaldosaOutsideRooms(double probability = 0.02)
    {
        Random rand = new Random();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Verifica que sea suelo
                if (map[x, y] != TileTypeDungeon.Floor)
                    continue;

                // Verifica si está dentro de una sala
                bool insideRoom = false;
                foreach (var room in rooms)
                {
                    if (room.Contains( new Point( x, y)))
                    {
                        insideRoom = true;
                        break;
                    }
                }
                if (insideRoom) continue;

                // Probabilidad de colocar baldosa
                if (rand.NextDouble() < probability)
                {
                    map[x, y] = TileTypeDungeon.DetallePiso;
                }
            }
        }
    }
    public void Generate(int minRoomSize = 30, int maxRoomSize = 50)
    {
        int wallThickness = 4; // muro interno de cada sala
        Random rand = new Random();
        rooms.Clear();
        doors.Clear();

        // Inicializar mapa: borde de 2 muros, resto suelo
        for (int x = 0; x < Width; x++)
            for (int yy = 0; yy < Height; yy++)
            {
                if (x < 2 || yy < 2 || x >= Width - 2 || yy >= Height - 2)
                    map[x, yy] = TileTypeDungeon.Wall; // borde externo
                else
                    map[x, yy] = TileTypeDungeon.Floor;
            }

        int y = 2; // empezar después del borde
        bool leftToRight = true;

        while (y + minRoomSize + wallThickness < Height - 2)
        {
            int x = leftToRight ? 2 : Width - 2;
            if (!leftToRight) x -= maxRoomSize;

            while (true)
            {
                int roomWidth = rand.Next(minRoomSize, maxRoomSize);
                int roomHeight = rand.Next(minRoomSize, maxRoomSize);

                // Verificar que cabe horizontalmente
                if (leftToRight && x + roomWidth > Width - 2) break;
                if (!leftToRight && x < 2) break;

                if (y + roomHeight > Height - 2) break;

                var newRoom = leftToRight
                    ? new Rectangle(x, y, roomWidth, roomHeight)
                    : new Rectangle(x, y, roomWidth, roomHeight);

                rooms.Add(newRoom);

                RoomShape shape = (RoomShape)rand.Next(2);
                CarveRoom(newRoom, shape, wallThickness);
                EnsureRoomConnection(newRoom, wallThickness);

                //ConnectWithNeighbors(newRoom, wallThickness);

                if (leftToRight)
                    x += roomWidth; // no separación entre salas
                else
                    x -= roomWidth;
            }

            y += maxRoomSize; // pasar a siguiente fila
            leftToRight = !leftToRight;
        }
    }
    public void EnsureRoomConnection(Rectangle room, int wallThickness = 2)
    {
        Random rand = new Random();
        int floorGap = 4; // doble suelo para atravesar muro

        // Elegir aleatoriamente un lado de la sala: 0=top,1=bottom,2=left,3=right
        int side = rand.Next(0, 4);

        switch (side)
        {
            case 0: // bottom
                int xTop = rand.Next(room.MinExtentX + wallThickness, room.MaxExtentX - wallThickness);
                map[xTop, room.MinExtentY] = TileTypeDungeon.Door; // puerta en el muro externo superior
                doors.Add(new Point(xTop, room.MinExtentY));
                break;

            case 1: // top
                int xBottom = rand.Next(room.MinExtentX + wallThickness, room.MaxExtentX - wallThickness - floorGap);
                for (int x = xBottom; x < xBottom + floorGap; x++)
                    for (int y = room.MaxExtentY - floorGap - 2; y < room.MaxExtentY; y++)
                        map[x, y] = TileTypeDungeon.BaldosaPiso;
                break;

            case 2: // left
                int yLeft = rand.Next(room.MinExtentY + wallThickness, room.MaxExtentY - wallThickness - floorGap);
                for (int x = room.MinExtentX; x < room.MinExtentX + floorGap; x++)
                    for (int y = yLeft; y < yLeft + floorGap; y++)
                        map[x, y] = TileTypeDungeon.BaldosaPiso;
                break;

            case 3: // right
                int yRight = rand.Next(room.MinExtentY + wallThickness, room.MaxExtentY - wallThickness - floorGap);
                for (int x = room.MaxExtentX - floorGap; x < room.MaxExtentX; x++)
                    for (int y = yRight; y < yRight + floorGap; y++)
                        map[x, y] = TileTypeDungeon.BaldosaPiso;
                break;
        }
    }

    private void CarveRoom(Rectangle room, RoomShape shape, int wallThickness)
    {
        switch (shape)
        {
            case RoomShape.Rectangle:
                for (int x = room.MinExtentX; x < room.MaxExtentX; x++)
                    for (int y = room.MinExtentY; y < room.MaxExtentY; y++)
                        map[x, y] = (x < room.MinExtentX + wallThickness || x >= room.MaxExtentX - wallThickness ||
                                      y < room.MinExtentY + wallThickness || y >= room.MaxExtentY - wallThickness)
                                     ? TileTypeDungeon.Wall
                                     : TileTypeDungeon.Floor;
                break;

            case RoomShape.Circle:
                var center = room.Center;
                int radiusX = (room.Width / 2) - wallThickness;
                int radiusY = (room.Height / 2) - wallThickness;
                for (int x = room.MinExtentX; x < room.MaxExtentX; x++)
                    for (int y = room.MinExtentY; y < room.MaxExtentY; y++)
                    {
                        int dx = (x - center.X) * (x - center.X);
                        int dy = (y - center.Y) * (y - center.Y);
                        if ((dx / (radiusX * radiusX + 1.0)) + (dy / (radiusY * radiusY + 1.0)) <= 1.0)
                            map[x, y] = TileTypeDungeon.Floor;
                        else
                            map[x, y] = TileTypeDungeon.Wall;
                    }
                break;

            case RoomShape.Cross:
                int midX = room.Center.X;
                int midY = room.Center.Y;

                for (int x = room.MinExtentX; x < room.MaxExtentX; x++)
                    for (int y = room.MinExtentY; y < room.MaxExtentY; y++)
                        map[x, y] = TileTypeDungeon.Wall;

                // Horizontal
                for (int x = room.MinExtentX + wallThickness; x < room.MaxExtentX - wallThickness; x++)
                    for (int y = midY - 1; y <= midY + 1; y++)
                        map[x, y] = TileTypeDungeon.Floor;

                // Vertical
                for (int y = room.MinExtentY + wallThickness; y < room.MaxExtentY - wallThickness; y++)
                    for (int x = midX - 1; x <= midX + 1; x++)
                        map[x, y] = TileTypeDungeon.Floor;
                break;
        }
    }


    public void ExportToTextFile(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    switch (map[x, y])
                    {
                        case TileTypeDungeon.Floor: writer.Write('.'); break;
                        case TileTypeDungeon.Wall: writer.Write('#'); break;
                        case TileTypeDungeon.Door: writer.Write('D'); break;
                        case TileTypeDungeon.AccesoSalidaMazmorra:
                            writer.Write('A');  break;
                        case TileTypeDungeon.SiguienteNivelMazmorra:
                            writer.Write('S'); break;                            
                    }
                }
                writer.WriteLine();
            }
        }
    }
    public void ExportInGame(MapTerrain terrainMap, TerrainCategoryType terrainCategoryType)
    {
        //Random rand = new Random();
        
        
        //// 1. Cargar tabla de equivalencias TerrainType -> idData
        //BsonExpression bsonExpression = BsonExpression.Create(
        //    "category = @0 and isRule = @1",
        //    terrainCategoryType.ToString(),
        //    true
        //);
        //var result = DataBaseManager.Instance.FindAllFilter<TerrainData>(bsonExpression);

        //List<TerrainData> listMosaico = ElementosTerrain(terrainCategoryType, TerrainType.Mosaico);
        //List<TerrainData> listDetallePiso = ElementosTerrain(terrainCategoryType, TerrainType.PisoDetalle);

        //Dictionary<TerrainType, int> tableData = result.ToDictionary(r => r.terrainType, r => r.id);
        //int offsetX = Width / 2;
        //int offsetY = Height / 2;

        //int idPiso = tableData[TerrainType.PisoBase];
        //for (int y = 0; y < Height; y++)
        //{
        //    for (int x = 0; x < Width; x++)
        //    {
        //        Vector2I pos = new Vector2I(x - offsetX, y - offsetY);
        //        switch (map[x, y])
        //        {
                    
        //            case TileTypeDungeon.Floor:
                        
        //                terrainMap.AddUpdateTile(pos, idPiso, 0);
        //                break;
        //            case TileTypeDungeon.Wall:
        //                int idMuro = tableData[TerrainType.Muro];
        //                terrainMap.AddUpdateTile(pos, idMuro);                        
        //                terrainMap.AddUpdateTile(pos, idPiso, 0);
        //                break;
        //            case TileTypeDungeon.BaldosaPiso:
        //                int idMosaico = listMosaico[rand.Next(0, listMosaico.Count - 1)].id;
        //                terrainMap.AddUpdateTileSimple(pos, idMosaico);
        //                terrainMap.AddUpdateTile(pos, idPiso, 0);
        //                break;
        //            case TileTypeDungeon.DetallePiso:
        //                int DetallePiso = listDetallePiso[rand.Next(0, listDetallePiso.Count - 1)].id;
        //                terrainMap.AddUpdateTileSimple(pos, DetallePiso);
        //                terrainMap.AddUpdateTile(pos, idPiso, 0);
        //                break;
        //        }
        //    }
        
        //}
    }

    private List<TerrainData> ElementosTerrain(TerrainCategoryType terrainCategoryType, TerrainType terrainType)
    {
        // 1. Cargar tabla de equivalencias TerrainType -> idData
        BsonExpression bsonExpression = BsonExpression.Create(
            "category = @0 and terrainType = @1",
            terrainCategoryType.ToString(),
            terrainType.ToString()
        );
        var result = DataBaseManager.Instance.FindAllFilter<TerrainData>(bsonExpression);
        return result;
    }
}
