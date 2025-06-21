using Arch.Buffer;
using Arch.Core;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
public interface ICharacterAttackBehavior
{
    void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterCommonBehaviorComponent characterCommonBehaviorComponent, ref CommandBuffer commandBuffer, float delta);
}
