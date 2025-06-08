using Arch.Buffer;
using Arch.Core;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
public interface ICharacterStateBehavior
{
    void ControllerState(Entity entity, ref CharacterComponent characterComponent, ref CharacterAnimationComponent animation, ref CharacterCommonBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer);
}
