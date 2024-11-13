using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Godot;

public enum TypeUnit
{
    MELLE
}
[Component]
public struct Unit
{
    public string name;
    public TypeUnit unitType;
    public uint team;
    public uint health;
    public uint barrier;
    public uint shield;
    public uint resistenceFire;
    public uint resistenceWater;
    public uint resistenceEarth;
    public uint resistenceWind;
}

[Component]
public struct Caster
{

}


[Component]
public struct Melee
{

}


[Component]
public struct Ranged
{

}

[Component]
public struct ColliderMelleAtack
{
    public Rect2 rect;
    public Vector2 offset;
    public Rect2 rectTransform;
}

[Component]
public struct RangeAttack
{
    public int area;
}

[Component]
public struct FrecuencyAttack
{
    public float value;
    public float timeAccumulator;
}

[Component]
public struct PendingAttack
{
    public Entity entityTarget;
    public int damage;
}
[Component]
public struct Health
{
    public int value;
}

[Component]
public struct Damage
{
    public int value;
}
[Component]
public struct PendingRemove
{
}

internal class UnitsComponents
{
}
