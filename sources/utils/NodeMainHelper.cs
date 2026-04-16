using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;

public static class NodeMainHelper
{
    public static Node3D node3DMain;
    public static Rid ridWorld3D;
    public static void SetNode3DMain(Node3D node)
    {
        node3DMain = node;
        ridWorld3D = node.GetWorld3D().Scenario;
    }
}
