using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[Component]
public struct SearchTargetMovement
{
}
internal class SearchMovementTargetSystem : BaseSystem<World, float>
{

    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription().WithAll<Unit, IAController, SearchTargetMovement, AreaMovement>();
    public SearchMovementTargetSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }


    private struct ChunkJob : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;
        private readonly RandomNumberGenerator rng;
        public ChunkJob(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
            rng = new RandomNumberGenerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerAreaMovement = ref chunk.GetFirst<AreaMovement>();
            ref var pointerPosition = ref chunk.GetFirst<Position>();
            ref var pointerRotation = ref chunk.GetFirst<Rotation>();
            ref var pointerDirection = ref chunk.GetFirst<Direction>();
            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref AreaMovement am = ref Unsafe.Add(ref pointerAreaMovement, entityIndex);

                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                ref Rotation r = ref Unsafe.Add(ref pointerRotation, entityIndex);
                ref Direction d = ref Unsafe.Add(ref pointerDirection, entityIndex);
                Vector2 pointDirection = Vector2.Zero;
                switch (am.type)
                {
                    case MovementType.CIRCLE:
                        pointDirection = CommonOperations.NewPointInCircle(rng, p.value, am.widthRadius);
                        break;
                    case MovementType.SQUARE:
                        pointDirection = CommonOperations.MovementSquare(rng, p.value, am.widthRadius, am.height);
                        break;
                    case MovementType.SQUARE_STATIC:
                        pointDirection = CommonOperations.MovementSquare(rng, am.origin, am.widthRadius, am.height);
                        break;
                    case MovementType.CIRCLE_STATIC:
                        pointDirection = CommonOperations.NewPointInCircle(rng, am.origin, am.widthRadius);
                        break;
                    default:
                        break;
                }


                if (!entity.Has<TargetMovement>())
                {
                    if (!entity.Has<PendingTransform>())
                    {

                        Vector2 targetDirection = (pointDirection - p.value).Normalized();
                        d.value = targetDirection;
                        r.value = Mathf.RadToDeg(targetDirection.Angle());
                        _commandBuffer.Add<PendingTransform>(entity);
                    }
                    _commandBuffer.Add<TargetMovement>(entity, new TargetMovement { value = pointDirection });
                    _commandBuffer.Remove<SearchTargetMovement>(entity);
                }

            }
        }
    }
    public override void Update(in float t)
    {
        World.InlineParallelChunkQuery(in query, new ChunkJob(commandBuffer, t));
        commandBuffer.Playback(World);
    }
  
}
