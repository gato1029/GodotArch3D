
using Godot;
using GodotEcsArch.sources.components;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class CommonOperations
{
    static RandomNumberGenerator rngInternal = new RandomNumberGenerator();

    private static readonly Random rng = new Random();
    public static Vector2 GetMidPointByDirection(Rect2 rect, AnimationDirection direction)
    {
        Vector2 pos = rect.Position;
        Vector2 size = rect.Size;

        switch (direction)
        {
            case AnimationDirection.UP:
                return new Vector2(pos.X + size.X / 2f, pos.Y);

            case AnimationDirection.DOWN:
                return new Vector2(pos.X + size.X / 2f, pos.Y + size.Y);

            case AnimationDirection.LEFT:
                return new Vector2(pos.X, pos.Y + size.Y / 2f);

            case AnimationDirection.RIGHT:
                return new Vector2(pos.X + size.X, pos.Y + size.Y / 2f);

            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
    public static int GetRandomInt(int min = 1, int max = 1_000_000)
    {
        return rngInternal.RandiRange(min, max);
    }
    public static Vector2 QuantizeDirection(Vector2 direction)
    {
        float quantizedX = 0;
        float quantizedY = 0;

        if (direction.X > 0) quantizedX = 1;
        else if (direction.X < 0) quantizedX = -1;

        if (direction.Y > 0) quantizedY = 1;
        else if (direction.Y < 0) quantizedY = -1;

        return new Vector2(quantizedX, quantizedY);
    }
    public static Vector2 QuantizeDirectionLeftRight(Vector2 direction)
    {
        float quantizedX = 0;
        float quantizedY = 0;

        if (direction.X > 0) quantizedX = 1;
        else if (direction.X < 0) quantizedX = -1;
      
        return new Vector2(quantizedX, quantizedY);
    }
    public static AnimationDirection GetDirectionAnimationLeftRight(Vector2 value)
    {
        // Cuadrante I y IV: X positivo → Derecha
        // Cuadrante II y III: X negativo → Izquierda
        return value.X >= 0 ? AnimationDirection.RIGHT : AnimationDirection.LEFT;
    }
    public static AnimationDirection GetDirectionAnimation(Vector2 value)
    {
        if (Math.Abs(value.X) > Math.Abs(value.Y)) // Predominio en 
        {
            return value.X > 0 ? AnimationDirection.RIGHT : AnimationDirection.LEFT;
        }
        else // Predominio en Y
        {
            return value.Y > 0 ? AnimationDirection.UP : AnimationDirection.DOWN;
        }
    }
    public static AnimationDirection GetDirectionAnimationEight(Vector2 normalized)
    {
        if (normalized == Vector2.Zero)
            return AnimationDirection.RIGHT; // o conserva la última dirección

        float absX = Math.Abs(normalized.X);
        float absY = Math.Abs(normalized.Y);

        // Límite de 22.5° para decidir entre cardinal y diagonal
        const float cardinalThreshold = 2.41421356f; // tan(67.5°)

        if (absX > absY * cardinalThreshold)
            return normalized.X > 0 ? AnimationDirection.RIGHT : AnimationDirection.LEFT;

        if (absY > absX * cardinalThreshold)
            return normalized.Y > 0 ? AnimationDirection.UP : AnimationDirection.DOWN;

        // Diagonales
        if (normalized.X > 0)
            return normalized.Y > 0
                ? AnimationDirection.RIGHTUP
                : AnimationDirection.RIGHTDOWN;

        return normalized.Y > 0
            ? AnimationDirection.LEFTUP
            : AnimationDirection.LEFTDOWN;
    }

    //public static AnimationDirection GetDirectionAnimationEight(Vector2 normalized)
    //{
    //    if (normalized == Vector2.Zero)
    //        return AnimationDirection.RIGHT;

    //    // Obtenemos el ángulo en grados (0 a 360)
    //    float angle = MathF.Atan2(normalized.Y, normalized.X) * (180f / MathF.PI);
    //    if (angle < 0)
    //        angle += 360f;

    //    // Distribución con preferencia cardinal (60° para cardinales, 30° para diagonales):

    //    // Derecha (centrado en 0°) -> 60° de amplitud (de 330° a 30°)
    //    if (angle >= 330f || angle < 30f) return AnimationDirection.RIGHT;

    //    // Diagonal Arriba-Derecha -> 30° de amplitud (de 30° a 60°)
    //    if (angle >= 30f && angle < 60f) return AnimationDirection.RIGHTUP; // Ojo con el eje Y

    //    // Arriba (centrado en 90°) -> 60° de amplitud (de 60° a 120°)
    //    if (angle >= 60f && angle < 120f) return AnimationDirection.UP;

    //    // Diagonal Arriba-Izquierda -> 30° de amplitud (de 120° a 150°)
    //    if (angle >= 120f && angle < 150f) return AnimationDirection.LEFTUP;

    //    // Izquierda (centrado en 180°) -> 60° de amplitud (de 150° a 210°)
    //    if (angle >= 150f && angle < 210f) return AnimationDirection.LEFT;

    //    // Diagonal Abajo-Izquierda -> 30° de amplitud (de 210° a 240°)
    //    if (angle >= 210f && angle < 240f) return AnimationDirection.LEFTDOWN;

    //    // Abajo (centrado en 270°) -> 60° de amplitud (de 240° a 300°)
    //    if (angle >= 240f && angle < 300f) return AnimationDirection.DOWN;

    //    // Diagonal Abajo-Derecha -> 30° de amplitud (de 300° a 330°)
    //    return AnimationDirection.RIGHTDOWN;
    //}

    //public static AnimationDirection GetDirectionAnimationEight(Vector2 normalized)
    //{
    //    if (normalized == Vector2.Zero)
    //        return AnimationDirection.RIGHT;

    //    // Obtenemos el ángulo en radianes y lo pasamos a grados (de -180 a 180)
    //    float angle = MathF.Atan2(normalized.Y, normalized.X) * (180f / MathF.PI);

    //    // Normalizamos los grados de 0 a 360
    //    if (angle < 0)
    //        angle += 360f;

    //    // Dividimos el círculo de 360° en 8 sectores de 45°.
    //    // Desplazamos 22.5° para que los ejes cardinales queden protegidos en el centro de su rango.
    //    if (angle >= 337.5f || angle < 22.5f) return AnimationDirection.RIGHT;
    //    if (angle >= 22.5f && angle < 67.5f) return AnimationDirection.RIGHTUP;    // Ojo con el eje Y invertido
    //    if (angle >= 67.5f && angle < 112.5f) return AnimationDirection.UP;
    //    if (angle >= 112.5f && angle < 157.5f) return AnimationDirection.LEFTUP;
    //    if (angle >= 157.5f && angle < 202.5f) return AnimationDirection.LEFT;
    //    if (angle >= 202.5f && angle < 247.5f) return AnimationDirection.LEFTDOWN;
    //    if (angle >= 247.5f && angle < 292.5f) return AnimationDirection.DOWN;

    //    return AnimationDirection.RIGHTDOWN;
    //}
    public static Vector2 NewPointInCircle(Vector2 origin, float radius)
    {
        Vector2 newPoint;

        do
        {
            float angle = rngInternal.RandfRange(0, Mathf.Pi * 2);

            float distance = Mathf.Sqrt(rngInternal.RandfRange(0, 1)) * radius;

            float x = Mathf.Cos(angle) * distance;
            float y = Mathf.Sin(angle) * distance;
            newPoint = origin + new Vector2(x, y);

        } while (newPoint.DistanceTo(origin) > radius);

        return newPoint; //new Vector2(-159.97408f,1660.1527f);

    }
    public  static Vector2 NewPointInCircle(RandomNumberGenerator rng, Vector2 origin, uint radius)
    {
        Vector2 newPoint;

        do
        {
            float angle = rng.RandfRange(0, Mathf.Pi * 2);

            float distance = Mathf.Sqrt(rng.RandfRange(0, 1)) * radius;

            float x = Mathf.Cos(angle) * distance;
            float y = Mathf.Sin(angle) * distance;
            newPoint = origin + new Vector2(x, y);

        } while (newPoint.DistanceTo(origin) > radius);

        return newPoint; //new Vector2(-159.97408f,1660.1527f);

    }
    public static Vector2 MovementSquare(RandomNumberGenerator rng, Vector2 origin, uint height, uint width)
    {
        float x = rng.Randf() * width;
        float y = rng.Randf() * height;

        Vector2 vector2 = origin + new Vector2(x, y);

        return vector2;
    }

    public static Vector2 SearchNewPosition(AreaMovement am, Position position, RandomNumberGenerator rng)
    {
        Vector2 pointDirection = Vector2.Zero;
        switch (am.type)
        {
            case MovementType.CIRCLE:
                pointDirection = NewPointInCircle(rng, position.value, am.widthRadius);
                break;
            case MovementType.SQUARE:
                pointDirection = MovementSquare(rng, position.value, am.widthRadius, am.height);
                break;
            case MovementType.SQUARE_STATIC:
                pointDirection = MovementSquare(rng, am.origin, am.widthRadius, am.height);
                break;
            case MovementType.CIRCLE_STATIC:
                pointDirection = NewPointInCircle(rng, am.origin, am.widthRadius);
                break;
            default:
                break;
        }
        return pointDirection;
    }


}
