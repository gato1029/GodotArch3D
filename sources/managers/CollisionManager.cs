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
using static Godot.TextServer;

[Component]
public struct ColliderSprite
{

}
[Component]
public struct Collider
{
    public Rect2 rect;
    public Rect2 rectTransform; 
    public bool aplyRotation;
}
internal class CollisionManager : BaseSystem<World, float>
{
   public static SpatialHashMap<Entity> dynamicCollidersEntities;
    public static SpatialHashMap<Entity> MoveCollidersEntities;

    private CommandBuffer commandBuffer;
    private QueryDescription queryDynamicSprite = new QueryDescription().WithAll<Position, Direction, Collider>();
    public CollisionManager(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
        dynamicCollidersEntities = new SpatialHashMap<Entity>(256, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
        MoveCollidersEntities = new SpatialHashMap<Entity>(256, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
    }

    private struct ChunkJobUpdateCollider : IChunkJob
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public ChunkJobUpdateCollider(CommandBuffer commandBuffer, float deltaTime) : this()
        {
            _commandBuffer = commandBuffer;
            _deltaTime = deltaTime;
        }

        public void Execute(ref Chunk chunk)
        {
            ref var pointerEntity = ref chunk.Entity(0);
            ref var pointerPosition = ref chunk.GetFirst<Position>();

            foreach (var entityIndex in chunk)
            {
                ref Entity entity = ref Unsafe.Add(ref pointerEntity, entityIndex);
                ref Position p = ref Unsafe.Add(ref pointerPosition, entityIndex);
                dynamicCollidersEntities.AddUpdateItem(p.value,in entity);
            }
        }
    }
    private readonly struct JobUpdateCollider : IForEachWithEntity<Position, Direction>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public JobUpdateCollider(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity, ref Position p, ref Direction dir)
        {
            dynamicCollidersEntities.AddUpdateItem(p.value, entity);
        }
    }



    public override void Update(in float t)
    {



        var job = new JobUpdateCollider((float)t, commandBuffer);
        World.InlineEntityQuery<JobUpdateCollider, Position, Direction>(in queryDynamicSprite, ref job);

        //World.InlineParallelChunkQuery(in queryDynamicSprite, new ChunkJobUpdateCollider(commandBuffer, t));
        //commandBuffer.Playback(World);

    }

    public static bool CheckAABBCollision(Vector2 positionA, Vector2 sizeA, Vector2 directionA, Vector2 positionB, Vector2 sizeB, Vector2 directionB)
    {
        //Inicializo values sin rotacion
        Vector2 topLeftA = positionA - sizeA / 2; ;
        Vector2 topLeftB = positionB - sizeB / 2;

        if (directionA != Vector2.Zero)
        {
            topLeftA = positionA - CalculateSizeBasedOnDirection(sizeA, directionA) / 2;
        }

        if (directionB != Vector2.Zero)
        {
            topLeftB = positionA - CalculateSizeBasedOnDirection(sizeB, directionB) / 2;
        }

        // Verifica si hay intersección
        return (topLeftA.X + sizeA.X >= topLeftB.X &&
                topLeftA.X <= topLeftB.X + sizeB.X &&
                topLeftA.Y + sizeA.Y >= topLeftB.Y &&
                topLeftA.Y <= topLeftB.Y + sizeB.Y);
    }

    public static bool CheckAABBCollision(Vector2 positionA, Rect2 rectA,  Vector2 positionB, Rect2 rectB)
    {
        
        Rect2 adjustedRectA = new Rect2(positionA + rectA.Position, rectA.Size);
        Rect2 adjustedRectB = new Rect2(positionB + rectB.Position, rectB.Size);

       
        return (adjustedRectA.Position.X + adjustedRectA.Size.X >= adjustedRectB.Position.X &&  
                adjustedRectA.Position.X <= adjustedRectB.Position.X + adjustedRectB.Size.X &&  
                adjustedRectA.Position.Y + adjustedRectA.Size.Y >= adjustedRectB.Position.Y  &&  
                adjustedRectA.Position.Y <= adjustedRectB.Position.Y + adjustedRectB.Size.Y);   

    }

    public static Vector2 CalculateSizeBasedOnDirection(Vector2 originalSize, Vector2 direction)
    {
        ////Vector2 normalizedDirection = direction.Normalized();

        float width = originalSize.X;
        float height = originalSize.Y;

        // Se establece un umbral para determinar si la dirección es principalmente horizontal o vertical
        float threshold = 0.707f; // Aproximadamente 45 grados en términos de vector

        // Determinar el ancho y el alto basado en la dirección
        if (Math.Abs(direction.X) > threshold) // Direcciones horizontales
        {
            width = originalSize.X;  // Mantiene el ancho original
            height = 0;               // Altura se convierte en cero
        }
        else if (Math.Abs(direction.Y) > threshold) // Direcciones verticales
        {
            width = 0;                // Ancho se convierte en cero
            height = originalSize.Y;  // Mantiene la altura original
        }
        else // Direcciones diagonales
        {
            // Puedes elegir mantener ambas dimensiones o una combinación
            width = originalSize.X;   // Mantiene el ancho original
            height = originalSize.Y;  // Mantiene la altura original
        }

        return new Vector2(width, height);
    }
}

