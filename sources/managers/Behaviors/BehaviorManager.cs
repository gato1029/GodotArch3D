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
        Dictionary<Type, ICharacterMoveBehavior>  dictionaryMoves = new Dictionary<Type, ICharacterMoveBehavior>();
        Dictionary<Type, ICharacterStateBehavior> dictionaryStates = new Dictionary<Type, ICharacterStateBehavior>();

        Dictionary<Type, IAttackBehavior> dictionaryAttack = new Dictionary<Type, IAttackBehavior>();

        
        protected override void Initialize()
        {
            // aqui inicializaremos todos lo behaviors que seran usados por todos, luego debemos tratar de extenderlo a LUA :)
            RegisterAllAttack();
            RegisterAllMoves();
            RegisterAllStates();
            RegisterParticularBehavior();
        }

       
        protected override void Destroy()
        {

        }
        void RegisterParticularBehavior()
        {
            RegisterBehavior(1, new HumanCharacterBehavior());
        }
        private void RegisterAllStates()
        {
            RegisterStateBehavior<CommonState2D>(new CommonState2D());
        }

       
        void RegisterAllMoves()
        {
            RegisterMoveBehavior<MoveRadiusCharacter2D>(new MoveRadiusCharacter2D());                        
        }
        void RegisterAllAttack()
        {
            RegisterAttackBehavior<MelleAtackHorizontalBehavior>(new MelleAtackHorizontalBehavior());
            RegisterAttackBehavior<MelleAttackBehavior>( new MelleAttackBehavior());
        }
        public ICharacterBehavior GetBehavior(int id)
        {
            if (dictionaryBehaviorsCharacter.ContainsKey(id))
            {
                return dictionaryBehaviorsCharacter[id];
            }
            return null;
        }
        public ICharacterStateBehavior GetStateBehavior<T>()
        {
            var type = typeof(T);
            if (dictionaryStates.TryGetValue(type, out var behavior))
            {
                return behavior;
            }
            throw new KeyNotFoundException($"No se encontró un ICharacterStateBehavior para el tipo {type.FullName}.");
        }
        public ICharacterMoveBehavior GetMoveBehavior<T>()
        {
            var type = typeof(T);
            if (dictionaryMoves.TryGetValue(type, out var moveBehavior))
            {
                return moveBehavior;
            }
            throw new KeyNotFoundException($"No se encontró un IMoveBehavior para el tipo {type.FullName}.");
        }
        public IAttackBehavior GetAttackBehavior<T>()
        {
            var type = typeof(T);
            if (dictionaryAttack.TryGetValue(type, out var attackBehavior))
            {
                return attackBehavior;
            }
            throw new KeyNotFoundException($"No se encontró un IMoveBehavior para el tipo {type.FullName}.");         
        }

        public void RegisterBehavior(int id, ICharacterBehavior behavior)
        {
            if (!dictionaryBehaviorsCharacter.ContainsKey(id))
            {
                dictionaryBehaviorsCharacter[id] = behavior;
            }
   
        }

        public void RegisterMoveBehavior<T>(ICharacterMoveBehavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));

            var type = typeof(T);
            dictionaryMoves.TryAdd(type, behavior);
        }
        public void RegisterStateBehavior<T>(ICharacterStateBehavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));

            var type = typeof(T);
            dictionaryStates.TryAdd(type, behavior);
        }
        public void RegisterAttackBehavior<T>(IAttackBehavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));

            var type = typeof(T);
            dictionaryAttack.TryAdd(type, behavior);
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
            unitB.healthBase -= unitA.damageBase;
            if (unitB.healthBase <= 0)
            {
                unitB.characterStateType = CharacterStateType.DIE;
            }
        }
    }
}
