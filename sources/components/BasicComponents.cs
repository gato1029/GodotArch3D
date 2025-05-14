using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;

internal class BasicComponents
{
}

public enum MovementType
{
    CIRCLE, SQUARE, CIRCLE_STATIC, SQUARE_STATIC
}
[Component]
public struct AreaMovement
{
    public uint widthRadius;
    public uint height;
    public Vector2 origin;
    public MovementType type;
}
[Component]
public struct TargetMovement
{
    public bool arrive;
    public Vector2 value;
}
[Component]
public struct Direction
{
    public Vector2 value;
    public Vector2 normalized;
    public AnimationDirection directionAnimation;
}
[Component]
public struct Velocity
{
    public float value;
}