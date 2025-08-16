
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

        

    public static Rect2 CalculateTransformedRect(Rect2 originalRect, float rotation)
    {
        Transform2D transformMatrix = new Transform2D(rotation, Vector2.Zero);

        Vector2 topLeft = originalRect.Position;
        Vector2 topRight = originalRect.Position + new Vector2(originalRect.Size.X, 0);
        Vector2 bottomLeft = originalRect.Position + new Vector2(0, originalRect.Size.Y);
        Vector2 bottomRight = originalRect.Position + originalRect.Size;


        Vector2 rotatedTopLeft = transformMatrix.BasisXform(topLeft);
        Vector2 rotatedTopRight = transformMatrix.BasisXform(topRight);
        Vector2 rotatedBottomLeft = transformMatrix.BasisXform(bottomLeft);
        Vector2 rotatedBottomRight = transformMatrix.BasisXform(bottomRight);


        float minX = Math.Min(Math.Min(rotatedTopLeft.X, rotatedTopRight.X), Math.Min(rotatedBottomLeft.X, rotatedBottomRight.X));
        float maxX = Math.Max(Math.Max(rotatedTopLeft.X, rotatedTopRight.X), Math.Max(rotatedBottomLeft.X, rotatedBottomRight.X));
        float minY = Math.Min(Math.Min(rotatedTopLeft.Y, rotatedTopRight.Y), Math.Min(rotatedBottomLeft.Y, rotatedBottomRight.Y));
        float maxY = Math.Max(Math.Max(rotatedTopLeft.Y, rotatedTopRight.Y), Math.Max(rotatedBottomLeft.Y, rotatedBottomRight.Y));


        Vector2 newSize = new Vector2((int)(maxX - minX), (int)(maxY - minY));
        Vector2 newPosition = new Vector2((int)minX, (int)minY);

        return new Rect2(newPosition, newSize);
    }
}
