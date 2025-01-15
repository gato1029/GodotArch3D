using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Godot;
using GodotEcsArch.sources.managers.Collision;

namespace GodotEcsArch.sources.components
{
    internal class CharacterComponents
    {
    }

    [Component]
    public struct CharacterWeapon
    {
        public int id;
        public Vector2 offset;
        public string pathTexture;
        public int damage;

        public GeometricShape2D shapeColliderLeftRight;
        public GeometricShape2D shapeColliderTopDown;
    }
}
