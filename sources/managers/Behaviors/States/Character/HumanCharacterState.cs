using Arch.Buffer;
using Arch.Core;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.States.Character;
public class HumanCharacterState : IStateCharacterBehavior
{
    public string Name => "Human";

    public void Controller(Entity entity, ref CharacterAnimationComponent animation, ref CharacterStateComponent stateComponent, ref CommandBuffer commandBuffer)
    {
        int state = stateComponent.currentState;
        switch (state)
        {
            case 0:  // idle
                animation.stateAnimation = 0;                
            break;
            default:
                break;
        }
    }
}
