using Arch.Buffer;
using Arch.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
public  interface ICharacterBehavior
{
    public string Name { get; }
    void ControllerState(Entity entity, ref CharacterComponent characterComponent, ref CharacterAnimationComponent animation, ref CharacterBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer);
    void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterBehaviorComponent characterBehaviorComponent , ref CommandBuffer commandBuffer, float delta);
}
