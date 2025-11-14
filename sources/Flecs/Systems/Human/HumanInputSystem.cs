using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Human;
internal class HumanInputSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PlayerInputComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var playerArray = it.Field<PlayerInputComponent>(0);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var player = ref playerArray[i];
            Vector2 moveDirection = Vector2.Zero;

            if (Input.IsActionPressed("move_up"))
                moveDirection.Y += 1;
            if (Input.IsActionPressed("move_down"))
                moveDirection.Y -= 1;
            if (Input.IsActionPressed("move_left"))
                moveDirection.X -= 1;
            if (Input.IsActionPressed("move_right"))
                moveDirection.X += 1;

            player.moveDirection = moveDirection.Normalized();

            // ataque
            player.attackPressed = Input.IsActionPressed("attack");
            player.attackReleased = Input.IsActionJustReleased("attack");
        }
    }
}
