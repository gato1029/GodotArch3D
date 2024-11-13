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
                        pointDirection = MovementCircle(rng, p.value, am.value);
                        break;
                    case MovementType.SQUARE:
                        pointDirection = MovementSquare(rng, p.value, am.value, am.value2);
                        break;
                    case MovementType.SQUARE_STATIC:
                        pointDirection = MovementSquare(rng, am.origin, am.value, am.value2);
                        break;
                    case MovementType.CIRCLE_STATIC:
                        pointDirection = MovementCircle(rng, am.origin, am.value);
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
    static Vector2 MovementCircle(RandomNumberGenerator rng, Vector2 origin, uint radius)
    {
        Vector2 newPoint;

        do
        {
        float angle = rng.RandfRange(0, Mathf.Pi * 2);

        float distance = Mathf.Sqrt(rng.RandfRange(0, 1)) * radius;

        float x = Mathf.Cos(angle) * distance;
        float y = Mathf.Sin(angle) * distance;
         newPoint = origin + new Vector2(x, y);

     } while (newPoint.DistanceTo(origin) > radius);

        return newPoint; //new Vector2(-159.97408f,1660.1527f);

    }
    static Vector2 MovementSquare(RandomNumberGenerator rng, Vector2 origin, uint height, uint width)
    {
        float x = rng.Randf() * width; 
        float y = rng.Randf() * height; 

        Vector2 vector2 = origin + new Vector2(x, y);

        return vector2;

    }
}
