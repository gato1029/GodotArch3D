using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;



namespace GodotFlecs.sources.Flecs.Systems.Units;

public class MoveTargetSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveTargetComponent>()
          .With<StateComponent>()
          .With<SteeringComponent>()
          .With<MoveResolutorComponent>()
          .With<MoveColliderComponent>()
          .Without<StoppedTag>()
          .Without<DeadTag>()
          .Without<FlowFieldFollowerComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var targetArray = it.Field<MoveTargetComponent>(1);
        var stateArray = it.Field<StateComponent>(2);
        var steeringArray = it.Field<SteeringComponent>(3);
        var resolutorArray = it.Field<MoveResolutorComponent>(4);
        var moveArray = it.Field<MoveColliderComponent>(5);


        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var target = ref targetArray[i];
            ref var state = ref stateArray[i];
            ref var steering = ref steeringArray[i];
            ref var resolutor = ref resolutorArray[i];
            ref var move = ref moveArray[i];

            Vector2 toTarget = target.Value - pos.position;
            float dist = toTarget.Length();

            // -------------------------------------------------
            // Umbral de llegada relativo al tamaño físico de la
            // unidad, no un punto matemático exacto — si el
            // waypoint cae sobre (o cerca de) otro collider, es
            // imposible ocupar el punto exacto, y antes nunca
            // se consideraba "llegada".
            // -------------------------------------------------
            float arrivalThreshold = move.Radius;// + 0.15f;

            // -------------------------------------------------
            // Llegada forzada: si llevamos varios intentos
            // fallidos de GridSteeringSystem (ConsecutiveBlocks)
            // MIENTRAS ya estamos razonablemente cerca del target,
            // nos damos por "llegados" igual — evita el loop
            // circular perpetuo cuando el punto exacto está
            // ocupado por alguien que no se va a mover.
            // -------------------------------------------------
            bool closeEnoughToForceArrival = dist <= arrivalThreshold * 2.5f;// && resolutor.ConsecutiveBlocks >= 3;

            if (dist <= arrivalThreshold || closeEnoughToForceArrival)
            {
                resolutor.BlockedTimer = 0;
                resolutor.AvoidanceTimer = 0;
                resolutor.LastConeIndex = 0;
                resolutor.ConsecutiveBlocks = 0;
                steering.DesiredDir = Vector2.Zero;
                steering.TargetDir = Vector2.Zero;
                resolutor.Blocked = false;
                state.stateType = StateType.IDLE;

                it.Entity(i).Remove<MoveTargetComponent>();
                it.Entity(i).Add<StoppedTag>();
                continue;
            }

            steering.TargetDir = toTarget.Normalized();
            state.stateType = StateType.MOVING;
        }
    }
}