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

[Component]
public struct PositionComponent
{    
    public float x;
    public float y;
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
