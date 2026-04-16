
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


using System.Runtime.InteropServices;

namespace GodotEcsArch.sources.managers.Collision;


[StructLayout(LayoutKind.Sequential)]
public struct TerrainColliderData
{
    // Qué forma tiene (Círculo o Rectángulo)
    public ShapeType Shape;

    // Dimensiones (Radio para Círculo, Ancho/Alto para Rectángulo)
    public float Width;
    public float Height;

    // Desplazamiento relativo al CENTRO del Tile (64x64)
    // Ejemplo: OffsetY = -32 situaría la colisión en la parte superior del tile
    public float OffsetX;
    public float OffsetY;

    // Capa de colisión (Muro, Agua, Daño, etc.) 
    // Usamos el sistema de Bits (CollisionConfig) que definimos antes
    public uint Layer;

    // EL PUNTERO LÓGICO: 
    // Si un tile tiene más de una colisión (ej. una valla y un poste), 
    // este ID apunta a la siguiente "receta" en la TerrainCollisionLibrary.
    // Se usa -1 para indicar que es la última (o única) forma del tile.
    public int NextIndex;

    /// <summary>
    /// Constructor de conveniencia para crear recetas rápidas.
    /// </summary>
    public TerrainColliderData(ShapeType shape, float w, float h, float ox = 0, float oy = 0, uint layer = 1, int next = -1)
    {
        Shape = shape;
        Width = w;
        Height = h;
        OffsetX = ox;
        OffsetY = oy;
        Layer = layer;
        NextIndex = next;
    }
    public bool Equals(TerrainColliderData other)
    {
        return Shape == other.Shape &&
               Width == other.Width &&
               Height == other.Height &&
               OffsetX == other.OffsetX &&
               OffsetY == other.OffsetY &&
               Layer == other.Layer;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Shape, Width, Height, OffsetX, OffsetY, Layer);
    }
}
public struct ColliderChainKey : IEquatable<ColliderChainKey>
{
    private readonly TerrainColliderData[] _data;
    private readonly int _hash;

    public ColliderChainKey(TerrainColliderData[] data)
    {
        _data = data;

        HashCode hc = new HashCode();
        for (int i = 0; i < data.Length; i++)
            hc.Add(data[i]);

        _hash = hc.ToHashCode();
    }

    public bool Equals(ColliderChainKey other)
    {
        if (_data.Length != other._data.Length)
            return false;

        for (int i = 0; i < _data.Length; i++)
        {
            if (!_data[i].Equals(other._data[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode() => _hash;
}
public static class TerrainCollisionLibrary
{
    private const int MAX_TEMPLATES = 2048;

    private static readonly TerrainColliderData[] _storage = new TerrainColliderData[MAX_TEMPLATES];
    private static int _nextId = 1;

    // 🔹 Cache simple
    private static readonly Dictionary<TerrainColliderData, ushort> _simpleCache = new();

    // 🔹 Cache compleja
    private static readonly Dictionary<ColliderChainKey, ushort> _complexCache = new();

    static TerrainCollisionLibrary()
    {
        for (int i = 0; i < MAX_TEMPLATES; i++)
            _storage[i].NextIndex = -1;
    }

    // =============================
    // 🔹 SIMPLE
    // =============================
    public static ushort Add(ShapeType shape, float w, float h, float ox = 0, float oy = 0, uint layer = 1)
    {
        var data = new TerrainColliderData(shape, w, h, ox, oy, layer, -1);

        if (_simpleCache.TryGetValue(data, out ushort existing))
            return existing;

        int id = _nextId++;
        _storage[id] = data;

        _simpleCache[data] = (ushort)id;

        return (ushort)id;
    }

    // =============================
    // 🔹 COMPLEJO (cacheado)
    // =============================
    public static ushort AddComplex(params TerrainColliderData[] shapes)
    {
        var key = new ColliderChainKey(shapes);

        if (_complexCache.TryGetValue(key, out ushort existing))
            return existing;

        int firstId = -1;
        int lastId = -1;

        for (int i = 0; i < shapes.Length; i++)
        {
            int currentId = _nextId++;

            _storage[currentId] = shapes[i];
            _storage[currentId].NextIndex = -1;

            if (firstId == -1)
                firstId = currentId;

            if (lastId != -1)
                _storage[lastId].NextIndex = currentId;

            lastId = currentId;
        }

        _complexCache[key] = (ushort)firstId;

        return (ushort)firstId;
    }

    // =============================
    // 🔹 DESDE SHAPES (Godot)
    // =============================
    public static ushort AddComplexTemplate(GeometricShape2D[] collisions)
    {
        var list = new TerrainColliderData[collisions.Length];

        for (int i = 0; i < collisions.Length; i++)
        {
            var item = collisions[i];

            switch (item)
            {
                case Circle c:
                    list[i] = new TerrainColliderData(
                        ShapeType.Circle,
                        c.Radius,
                        0,
                        c.OriginRelative.X,
                        c.OriginRelative.Y,
                        CollisionConfig.TypeWall
                    );
                    break;

                case Rectangle r:
                    list[i] = new TerrainColliderData(
                        ShapeType.Rect,
                        r.Width,
                        r.Height,
                        r.OriginRelative.X,
                        r.OriginRelative.Y,
                        CollisionConfig.TypeWall
                    );
                    break;

                default:
                    throw new NotSupportedException($"Shape no soportado: {item.GetType()}");
            }
        }

        return AddComplex(list);
    }

    // =============================
    // 🔹 GET (CRÍTICO - ultra rápido)
    // =============================
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly TerrainColliderData Get(int id)
    {
        return ref _storage[id];
    }
}