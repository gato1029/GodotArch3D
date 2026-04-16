using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyTiles;
using GodotFlecs.sources.Flecs.Components;

namespace GodotFlecs.sources.Flecs.Systems.Generic;

public class RegisterFastHashSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate; // Justo después de Clear
    protected override bool MultiThreaded => false; // Escritura en Hash no es Thread-Safe

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<SpatialIDComponent>()
          .With<UnitTag>()
          .Without<SleepTag>();// solo dinámicos que se mueven, no los que están quietos (aunque esto se podría cambiar según necesidades)
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;

        var posArray = it.Field<PositionComponent>(0);        
        var sidArray = it.Field<SpatialIDComponent>(1);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var p = ref posArray[i];            
            ref var sid = ref sidArray[i];

            //Calculamos posición real del colisionador
            float actualX = p.position.X; //+ col.OffsetX;
            float actualY = p.position.Y;// + col.OffsetY;
            
            //Registramos en el hash dinámico
            blackyWorld.DynamicHash.UpdatePosition(sid.Value, actualX, actualY);
        }
    }
}
