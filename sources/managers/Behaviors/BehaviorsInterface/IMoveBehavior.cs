using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.systems;


public interface IMoveBehavior
{
    void Move(Entity entity,IAttackBehavior attackBehavior,  ref StateComponent stateComponent, ref IAController iaController, ref Position position, ref Direction direction, ref Rotation rotation, ref Velocity velocity, ref ColliderSprite collider, RandomNumberGenerator rng, float deltaTime);
}

