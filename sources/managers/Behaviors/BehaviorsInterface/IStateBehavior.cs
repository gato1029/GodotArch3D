using Arch.Buffer;
using Arch.Core;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface
{
    public interface IStateBehavior
    {
        public string Name { get; }    
        void StateController(Entity entity, ref Animation animation, ref StateComponent stateComponent, ref CommandBuffer commandBuffer);
    }
}
