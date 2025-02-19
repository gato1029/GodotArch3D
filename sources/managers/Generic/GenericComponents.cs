using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;

namespace GodotEcsArch.sources.managers.Generic;

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
public struct PositionComponent
{    
    public float x;
    public float y;
}
[Component]
public struct DirectionComponent
{
    public AnimationDirection animationDirection;
}

[Component]
public struct RenderGPUComponent
{
    public Rid rid;
    public int instance;
    public int layerRender;
    public Transform3D transform;
}

internal class GenericComponents
{
}
