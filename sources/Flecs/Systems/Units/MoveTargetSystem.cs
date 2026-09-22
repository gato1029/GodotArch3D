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
          .With<SteeringComponent>() // <-- Añadido
          .With<MoveResolutorComponent>()
          .With<MoveColliderComponent>()
          .Without<StoppedTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var targetArray = it.Field<MoveTargetComponent>(1);
        var stateArray = it.Field<StateComponent>(2);
        var steeringArray = it.Field<SteeringComponent>(3); // <-- Añadido
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
      

            if (resolutor.BlockedTimer>0.5f )
            {
                steering.DesiredDir = Vector2.Zero;
                resolutor.BlockedTimer = 0;
                resolutor.Blocked = true;
                state.stateType = StateType.IDLE;
                it.Entity(i).Remove<MoveTargetComponent>();
                it.Entity(i).Add<StoppedTag>();


                continue;
            }
            Vector2 toTarget = target.Value - pos.position +move.Offset;
            float distSq = toTarget.LengthSquared();

            float umbralLlegada = 0.05f;
       

            if (distSq <= umbralLlegada)//0.05f) // Umbral de llegada
            {
                resolutor.BlockedTimer = 0;
                steering.DesiredDir = Vector2.Zero;
                resolutor.Blocked = true;
                state.stateType = StateType.IDLE;
                it.Entity(i).Remove<MoveTargetComponent>();
                it.Entity(i).Add<StoppedTag>();           
                continue;
            }

            // Solo enviamos la DIRECCIÓN deseada al Steering
            steering.DesiredDir = (toTarget).Normalized();
            state.stateType = StateType.MOVING;
        }
    }
}