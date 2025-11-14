using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using Schedulers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.systems.Player;
public class PlayerInputSystem : BaseSystem<World, float>
{
    private QueryDescription query = new QueryDescription().WithAll<PlayerInputComponent>();    
    private CommandBuffer commandBuffer;
    public PlayerInputSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private readonly struct UpdateInput : IForEachWithEntity<PlayerInputComponent>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public UpdateInput(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref PlayerInputComponent inputComponent)
        {

            Vector2 moveDirection = Vector2.Zero;

            if (Input.IsActionPressed("move_up"))
                moveDirection.Y += 1;
            if (Input.IsActionPressed("move_down"))
                moveDirection.Y -= 1;
            if (Input.IsActionPressed("move_left"))
                moveDirection.X -= 1;
            if (Input.IsActionPressed("move_right"))
                moveDirection.X += 1;

            inputComponent.moveDirection = moveDirection.Normalized();

            // ataque
            inputComponent.attackPressed = Input.IsActionPressed("attack");
            inputComponent.attackReleased = Input.IsActionJustReleased("attack");
        }
    }

    public override void Update(in float t)
    {
        var job = new UpdateInput(t, commandBuffer);
        World.InlineEntityQuery<UpdateInput,PlayerInputComponent>(in query, ref job);
    }
    
}