using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;

namespace GodotEcsArch.sources.components;

public enum AnimationDirection
{
    LEFT,
    RIGHT,
    UP,
    DOWN,
}
public enum CardinalDirection
{
    LEFT,
    RIGHT,
    UP,
    DOWN,
}

[Component]
public struct VelocityComponent
{
    public float velocity;
}

[Component]
public struct PositionComponent
{
    public Vector2 position;
}
[Component]
public struct DirectionComponent
{
    public Vector2 value;
    public Vector2 normalized;
    public AnimationDirection animationDirection;
}

[Component]
public struct RenderGPUComponent
{
    public Rid rid;
    public int instance;
    public int layerRender;
    public float zOrdering;
    public Transform3D transform;    
}

[Component]
public struct RenderGPULinkedComponent
{
    public GpuInstance[] instancedLinked;
}

public struct GpuInstance
{
    public Rid rid;
    public int instance;
}

internal class GenericComponents
{
}
