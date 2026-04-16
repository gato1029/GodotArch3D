using Godot;
using System;

namespace GodotEcsArch.sources.utils;


public static class Vector2Extensions
{
    // --- BASICS ---

    public static float LengthSquared(this Vector2 v)
    {
        return v.X * v.X + v.Y * v.Y;
    }

    public static float DotProduct(this Vector2 a, Vector2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    // --- NORMALIZATION ---

    public static Vector2 NormalizedSafe(this Vector2 v)
    {
        float len = v.Length();
        if (len == 0f)
            return Vector2.Zero;

        return v / len;
    }

    // --- DIRECTION ---

    /// <summary>
    /// Dirección normalizada desde pb hacia pa
    /// </summary>
    public static Vector2 DirectionNormalized(Vector2 pa, Vector2 pb)
    {
        Vector2 dir = pa - pb;
        float len = dir.Length();

        if (len == 0f)
            return Vector2.Zero;

        return dir / len;
    }

    // --- NORMALS ---
    // Asume espacio de coordenadas Godot (Y+ hacia abajo)

    public static Vector2 LeftNormal(this Vector2 v)
    {
        return new Vector2(v.Y, -v.X);
    }

    public static Vector2 RightNormal(this Vector2 v)
    {
        return new Vector2(-v.Y, v.X);
    }

    // --- ADD SCALED ---

    public static Vector2 AddScaled(this Vector2 v, Vector2 other, float scale)
    {
        return v + other * scale;
    }

    public static Vector2 AddScaled(this Vector2 v, params (Vector2 vec, float scale)[] values)
    {
        foreach (var (vec, scale) in values)
            v += vec * scale;

        return v;
    }

    // --- COMPARISONS ---

    public static bool LessThanX(this Vector2 a, Vector2 b)
    {
        return a.X < b.X;
    }

    public static bool LessThanY(this Vector2 a, Vector2 b)
    {
        return a.Y < b.Y;
    }
}
