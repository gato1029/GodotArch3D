using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


public enum BehaviorState
{
    ATACK,
    MOVE, 
}
public enum OperationState
{
    NONE,
    SEARCH_PATH,    
}
public enum ControllerMode
{
    HUMAN,
    IA
}
[Component]
public struct BehaviorCharacter
{
    public IAttackBehavior attackBehavior;
    public IMoveBehavior moveBehavior;
    public IStateBehavior stateBehavior;
}

[Component]
public struct HumanController
{
    public TargetMovement targetMovement;
}

[Component]
public struct IAController
{    
    
    public AreaMovement areaMovement;
    public TargetMovement targetMovement;
}

[Component]
public struct UnitController
{
    public ControllerMode controllerMode;
    public IAController iaController;
}
internal class BehaviorCharacterSystem : BaseSystem<World, float>
{
    private CommandBuffer commandBuffer;
    private QueryDescription queryBehaviorIA = new QueryDescription().WithAll<BehaviorCharacter, Position, Sprite3D, UnitController, Direction, Rotation,Velocity,ColliderSprite>();
    private QueryDescription queryBehaviorHuman = new QueryDescription().WithAll<BehaviorCharacter, Position, Sprite3D, RefreshPositionAlways, HumanController, Animation, Direction, Rotation, Velocity, ColliderSprite>();

    public BehaviorCharacterSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private struct JobBehaviorIA : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly RandomNumberGenerator rng;
        public JobBehaviorIA(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            rng = new RandomNumberGenerator();
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);

            ref var pointerBehaviorCharacter = ref chunk.GetFirst<BehaviorCharacter>();
            ref var pointerPosition = ref chunk.GetFirst<Position>();            
            ref var pointerSprite3D = ref chunk.GetFirst<Sprite3D>();
            ref var pointerUnitController = ref chunk.GetFirst<UnitController>();

            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            ref var pointerRotation = ref chunk.GetFirst<Rotation>();
            ref var pointerVelocity = ref chunk.GetFirst<Velocity>();
            ref var pointerCollider = ref chunk.GetFirst<ColliderSprite>();
            ref var pointerStateComponent = ref chunk.GetFirst<StateComponent>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Sprite3D s = ref Unsafe.Add(ref pointerSprite3D, entityIndex);
                ref BehaviorCharacter bc = ref Unsafe.Add(ref pointerBehaviorCharacter, entityIndex);
                ref UnitController ia = ref Unsafe.Add(ref pointerUnitController, entityIndex);
              
                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);
                ref Rotation r = ref Unsafe.Add(ref pointerRotation, entityIndex);
                ref Velocity v = ref Unsafe.Add(ref pointerVelocity, entityIndex);
                ref ColliderSprite c = ref Unsafe.Add(ref pointerCollider, entityIndex);
                ref StateComponent stateComponent = ref Unsafe.Add(ref pointerStateComponent, entityIndex);

                if (stateComponent.currentType == StateType.EXECUTE_ATTACK)
                {
                    bc.attackBehavior.Attack(entity, ref p, ref d);
                }
       
                if ((stateComponent.currentType == StateType.IDLE || stateComponent.currentType == StateType.MOVING) )
                {
                    if (bc.attackBehavior.AttackPosible(entity, ref p, ref d))
                    {
                        stateComponent.currentType = StateType.ATTACK;
                    }
                    else
                    {
                        bc.moveBehavior.Move(entity,bc.attackBehavior, ref stateComponent, ref ia.iaController, ref p, ref d, ref r, ref v, ref c, rng, _deltaTime);
                    }

                }             
            }

        }       
    }

    public override void Update(in float t)
    {
        World.InlineParallelChunkQuery(in queryBehaviorIA, new JobBehaviorIA(commandBuffer, t));
        commandBuffer.Playback(World);  
    }
}

