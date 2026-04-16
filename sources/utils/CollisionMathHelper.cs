



using GodotFlecs.sources.Flecs.Components;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.utils;
public static class CollisionMathHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(
        float x1, float y1, ref FastCollider c1,
        float x2, float y2, ref FastCollider c2)
    {
        // Calculamos centros reales sumando el Offset a la posición de la entidad
        float cx1 = x1 + c1.Offset.X;
        float cy1 = y1 + c1.Offset.Y;
        float cx2 = x2 + c2.Offset.X;
        float cy2 = y2 + c2.Offset.Y;

        // Caso 1: Círculo vs Círculo
        if (c1.Shape == ShapeType.Circle && c2.Shape == ShapeType.Circle)
            return CircleToCircle(cx1, cy1, c1.Width, cx2, cy2, c2.Width);

        // Caso 2: Rectángulo vs Rectángulo (AABB)
        if (c1.Shape == ShapeType.Rect && c2.Shape == ShapeType.Rect)
            return RectToRect(cx1, cy1, c1.Width, c1.Height, cx2, cy2, c2.Width, c2.Height);

        // Caso 3: Círculo vs Rectángulo (Híbrido)
        if (c1.Shape == ShapeType.Circle && c2.Shape == ShapeType.Rect)
            return CircleToRect(cx1, cy1, c1.Width, cx2, cy2, c2.Width, c2.Height);

        if (c1.Shape == ShapeType.Rect && c2.Shape == ShapeType.Circle)
            return CircleToRect(cx2, cy2, c2.Width, cx1, cy1, c1.Width, c1.Height);

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PointCheck(
    float px, float py,
    float x2, float y2,
    ref FastCollider c2)
    {
        // Centro real del collider
        float cx = x2 + c2.Offset.X;
        float cy = y2 + c2.Offset.Y;
        if (c2.Shape == ShapeType.Circle)
            return PointToCircle(px, py, cx, cy, c2.Width);

        if (c2.Shape == ShapeType.Rect)
            return PointToRect(px, py, cx, cy, c2.Width, c2.Height);

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointToCircle(
    float px, float py,
    float cx, float cy,
    float radius)
    {
        float dx = px - cx;
        float dy = py - cy;

        return (dx * dx + dy * dy) < (radius * radius);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointToRect(
    float px, float py,
    float rx, float ry,
    float rw, float rh)
    {
        float halfW = rw * 0.5f;
        float halfH = rh * 0.5f;

        return px >= (rx - halfW) &&
               px <= (rx + halfW) &&
               py >= (ry - halfH) &&
               py <= (ry + halfH);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CircleToCircle(float x1, float y1, float r1, float x2, float y2, float r2)
    {
        float dx = x1 - x2;
        float dy = y1 - y2;
        float distSq = (dx * dx) + (dy * dy);
        float radSum = r1 + r2;
        return distSq < (radSum * radSum);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool RectToRect(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
    {
        // Chequeo AABB estándar usando el centro y medias extensiones
        return Math.Abs(x1 - x2) < (w1 + w2)  * 0.5f &&
               Math.Abs(y1 - y2) < (h1 + h2)  * 0.5f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CircleToRect(float cx, float cy, float radius, float rx, float ry, float rw, float rh)
    {
        // 1. Encontrar el punto más cercano del rectángulo al centro del círculo (Clamp)
        float halfW = rw * 0.5f;
        float halfH = rh * 0.5f;

        float closestX = Math.Clamp(cx, rx - halfW, rx + halfW);
        float closestY = Math.Clamp(cy, ry - halfH, ry + halfH);

        // 2. Calcular la distancia entre el círculo y ese punto cercano
        float dx = cx - closestX;
        float dy = cy - closestY;

        // Si la distancia es menor que el radio, hay colisión
        return (dx * dx + dy * dy) < (radius * radius);
    }
}
