using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Behaviors.Attack;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Behaviors.Move;
using GodotEcsArch.sources.managers.Behaviors.States;
using GodotEcsArch.sources.managers.Behaviors.States.Character;
using GodotEcsArch.sources.managers.Characters;
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
        Dictionary<int, ICharacterBehavior> dictionaryBehaviorsCharacter = new Dictionary<int, ICharacterBehavior>();
        Dictionary<int, IMoveBehavior>  dictionaryMoves = new Dictionary<int, IMoveBehavior>();
        Dictionary<int, IAttackBehavior> dictionaryAttack = new Dictionary<int, IAttackBehavior>();

        
        protected override void Initialize()
        {
            // aqui inicializaremos todos lo behaviors que seran usados por todos, luego debemos tratar de extenderlo a LUA :)
            RegisterAllAttack();
            RegisterAllMoves();
            RegisterAllStates();
        }

        protected override void Destroy()
        {
           
        }

        void RegisterAllStates()
        {
            RegisterBehavior(1, new HumanCharacterBehavior());
         
        }
        void RegisterAllMoves()
        {
            RegisterMoveBehavior(1, new DefaultMove());                        
        }
        void RegisterAllAttack()
        {
            RegisterAttackBehavior(1, new MelleAtackHorizontalBehavior());
            RegisterAttackBehavior(2, new MelleAttackBehavior());
        }
        public ICharacterBehavior GetBehavior(int id)
        {
            if (dictionaryBehaviorsCharacter.ContainsKey(id))
            {
                return dictionaryBehaviorsCharacter[id];
            }
            return null;
        }
        public IMoveBehavior GetMoveBehavior(int id)
        {
            if (dictionaryMoves.ContainsKey(id))
            {
                return dictionaryMoves[id];
            }
            return null;
        }
        public IAttackBehavior GetAttackBehavior(int id)
        {
            if (dictionaryAttack.ContainsKey(id))
            {
                return dictionaryAttack[id];
            }
            return null;
        }

        public void RegisterBehavior(int id, ICharacterBehavior behavior)
        {
            if (!dictionaryBehaviorsCharacter.ContainsKey(id))
            {
                dictionaryBehaviorsCharacter[id] = behavior;
            }
   
        }

        public void RegisterMoveBehavior(int id, IMoveBehavior behavior)
        {
            if (!dictionaryMoves.ContainsKey(id))
            {
                dictionaryMoves[id] = behavior;
            }
           
        }

        public void RegisterAttackBehavior(int id, IAttackBehavior behavior)
        {
            if (!dictionaryAttack.ContainsKey(id))
            {
                dictionaryAttack[id] = behavior;
            }
          
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

        public void AplyDamageCharacter(Entity origin, Entity destiny)
        {
            CharacterComponent unitA = origin.Get<CharacterComponent>();
            ref CharacterComponent unitB = ref destiny.TryGetRef<CharacterComponent>(out bool exist);
            ref CharacterBehaviorComponent characterBehaviorComponentB = ref destiny.TryGetRef<CharacterBehaviorComponent>(out bool exist2);
            unitB.healthBase -= unitA.damageBase;

            if (unitB.healthBase <= 0)
            {
                characterBehaviorComponentB.characterStateType = CharacterStateType.DIE;
            }
        }
    }
}
