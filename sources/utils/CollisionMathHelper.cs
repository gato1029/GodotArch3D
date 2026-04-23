



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

        // Caso 4: Rectángulo vs Slope
        if (c1.Shape == ShapeType.Rect && c2.Shape == ShapeType.Slope)
            return RectToSlope(cx1, cy1, c1.Width, c1.Height, cx2, cy2, c2.Width, c2.Height, c2.Slope);

        if (c1.Shape == ShapeType.Slope && c2.Shape == ShapeType.Rect)
            return RectToSlope(cx2, cy2, c2.Width, c2.Height, cx1, cy1, c1.Width, c1.Height, c1.Slope);
        // Caso 5: Círculo vs Slope
        if (c1.Shape == ShapeType.Circle && c2.Shape == ShapeType.Slope)
            return CircleToSlope(cx1, cy1, c1.Width, cx2, cy2, c2.Width, c2.Height, c2.Slope);

        if (c1.Shape == ShapeType.Slope && c2.Shape == ShapeType.Circle)
            return CircleToSlope(cx2, cy2, c2.Width, cx1, cy1, c1.Width, c1.Height, c1.Slope);


        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool RectToSlope(float rx, float ry, float rw, float rh, float sx, float sy, float sw, float sh, SlopeType type)
    {
        // 1. AABB rápido: ¿Se tocan siquiera los cuadros?
        if (Math.Abs(rx - sx) >= (rw + sw) * 0.5f || Math.Abs(ry - sy) >= (rh + sh) * 0.5f)
            return false;

        // 2. Dependiendo del tipo de triángulo, chequeamos la esquina más crítica
        // Para suelos (Bottom), chequeamos las esquinas de abajo del rect.
        // Para techos (Top), chequeamos las esquinas de arriba.
        return type switch
        {
            SlopeType.BottomLeft => PointToSlope(rx - rw * 0.5f, ry + rh * 0.5f, sx, sy, sw, sh, type),
            SlopeType.BottomRight => PointToSlope(rx + rw * 0.5f, ry + rh * 0.5f, sx, sy, sw, sh, type),
            SlopeType.TopLeft => PointToSlope(rx - rw * 0.5f, ry - rh * 0.5f, sx, sy, sw, sh, type),
            SlopeType.TopRight => PointToSlope(rx + rw * 0.5f, ry - rh * 0.5f, sx, sy, sw, sh, type),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PointToSlope(float px, float py, float sx, float sy, float sw, float sh, SlopeType type)
    {
        float hw = sw * 0.5f;
        float hh = sh * 0.5f;

        // Coordenadas normalizadas 0.0 a 1.0 dentro del Tile
        float tx = (px - (sx - hw)) / sw;
        float ty = (py - (sy - hh)) / sh;

        // Evitar errores de precisión fuera del tile
        tx = Math.Clamp(tx, 0f, 1f);
        ty = Math.Clamp(ty, 0f, 1f);

        return type switch
        {
            SlopeType.BottomLeft => ty >= (1.0f - tx), // Triángulo 1
            SlopeType.TopRight => ty <= (1.0f - tx), // Triángulo 2
            SlopeType.TopLeft => ty <= tx,          // Triángulo 3
            SlopeType.BottomRight => ty >= tx,          // Triángulo 4
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CircleToSlope(float cx, float cy, float cradius, float sx, float sy, float sw, float sh, SlopeType type)
    {
        // 1. AABB rápido (Caja del círculo vs Caja del Tile)
        if (Math.Abs(cx - sx) >= (cradius + sw * 0.5f) || Math.Abs(cy - sy) >= (cradius + sh * 0.5f))
            return false;

        // 2. Si el centro del círculo ya está dentro de la masa sólida, hay colisión
        if (PointToSlope(cx, cy, sx, sy, sw, sh, type)) return true;

        // 3. Chequeo de proximidad a la diagonal
        // Calculamos la posición relativa del centro (0 a 1)
        float tx = (cx - (sx - sw * 0.5f)) / sw;
        tx = Math.Clamp(tx, 0f, 1f);

        // Calculamos la Y de la línea en esa X
        float lineYLocal = type switch
        {
            SlopeType.BottomLeft or SlopeType.TopRight => 1.0f - tx,
            SlopeType.TopLeft or SlopeType.BottomRight => tx,
            _ => 0f
        };

        float lineYWorld = (sy + sh * 0.5f) - (lineYLocal * sh);

        // 4. Distancia vertical a la diagonal
        float distY = Math.Abs(cy - lineYWorld);

        // Si la distancia vertical es menor que el radio, hay colisión aproximada
        // (Para mayor precisión en ángulos agudos se usa la distancia perpendicular, 
        // pero para tiles de 45° esto es perfecto y mucho más rápido).
        return distY < cradius;
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
