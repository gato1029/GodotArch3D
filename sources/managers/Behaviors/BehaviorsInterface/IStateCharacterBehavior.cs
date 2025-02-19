using Arch.Buffer;
using Arch.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
public interface IStateCharacterBehavior
{
    public string Name { get; }
    void Controller(Entity entity, ref CharacterAnimationComponent animation, ref CharacterStateComponent stateComponent, ref CommandBuffer commandBuffer);
}
