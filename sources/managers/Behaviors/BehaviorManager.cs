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
        Dictionary<int, ICharacterMoveBehavior>  dictionaryMoves = new Dictionary<int, ICharacterMoveBehavior>();
        Dictionary<int, ICharacterStateBehavior> dictionaryStates = new Dictionary<int, ICharacterStateBehavior>();

        Dictionary<int, ICharacterAttackBehavior> dictionaryAttack = new Dictionary<int, ICharacterAttackBehavior>();

        
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
            RegisterStateBehavior(1,new CommonState2D());
        }

       
        void RegisterAllMoves()
        {
            RegisterMoveBehavior(1,new MoveRadiusCharacter2D());                        
        }
        void RegisterAllAttack()
        {
            RegisterAttackBehavior(1,new MelleAtack2D());
            //RegisterAttackBehavior<MelleAttackBehavior>( new MelleAttackBehavior());
        }
        public ICharacterBehavior GetBehavior(int id)
        {
            if (dictionaryBehaviorsCharacter.ContainsKey(id))
            {
                return dictionaryBehaviorsCharacter[id];
            }
            return null;
        }
        public ICharacterStateBehavior GetStateBehavior(int id)
        {
            
            if (dictionaryStates.TryGetValue(id, out var behavior))
            {
                return behavior;
            }
            throw new KeyNotFoundException($"No se encontró un ICharacterStateBehavior para el tipo id."+ id);
        }

        public ICharacterMoveBehavior GetMoveBehavior(int id)
        {            
            if (dictionaryMoves.TryGetValue(id, out var moveBehavior))
            {
                return moveBehavior;
            }
            throw new KeyNotFoundException($"No se encontró un IMoveBehavior para el id."+ id);
        }
        public ICharacterAttackBehavior GetAttackBehavior(int id)
        {
          
            if (dictionaryAttack.TryGetValue(id, out var attackBehavior))
            {
                return attackBehavior;
            }
            throw new KeyNotFoundException($"No se encontró un IMoveBehavior para el tipo."+id);         
        }

        public void RegisterBehavior(int id, ICharacterBehavior behavior)
        {
            if (!dictionaryBehaviorsCharacter.ContainsKey(id))
            {
                dictionaryBehaviorsCharacter[id] = behavior;
            }
   
        }

        public void RegisterMoveBehavior(int id, ICharacterMoveBehavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));          
            dictionaryMoves.TryAdd(id, behavior);
        }
        public void RegisterStateBehavior(int id,ICharacterStateBehavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));            
            dictionaryStates.TryAdd(id, behavior);
        }
        public void RegisterAttackBehavior(int id, ICharacterAttackBehavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));            
            dictionaryAttack.TryAdd(id, behavior);
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
