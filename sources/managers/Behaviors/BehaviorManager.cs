using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors
{
    internal class BehaviorManager : SingletonBase<BehaviorManager>
    {

        protected override void Initialize()
        {
           
        }

        protected override void Destroy()
        {
           
        }

        public void AplyDamage(Entity origin, Entity destiny)
        {
            Unit unitA = origin.Get<Unit>();
            ref Unit unitB = ref destiny.TryGetRef<Unit>(out bool exist);
            ref StateComponent stateComponentB = ref destiny.TryGetRef<StateComponent>(out bool exist2);
            unitB.health -= unitA.damage;

            if (unitB.health<=0)
            {
                stateComponentB.currentType = StateType.DIE;             
            }
        }
    }
}
