using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
internal class DeathCleanupSystem : FlecsSystemBase
{
    protected override bool MultiThreaded => false;
    protected override ulong Phase => flecs.EcsPostUpdate;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RenderGPUComponent>()
        .With<SpatialIDComponent>()
        .With<DeathTimerComponent>()
        .With<CharacterComponent>()   // solo para unidades     
        .With<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var dynGrid = world.State.DynamicHash;

        var gpuArray = it.Field<RenderGPUComponent>(0);
        var spatialArray = it.Field<SpatialIDComponent>(1);
        var timerArray = it.Field<DeathTimerComponent>(2);
        float dt = it.DeltaTime();
        for (int i = 0; i < it.Count(); i++)
        {            
            ref var timer = ref timerArray[i];
            timer.RemainingTime -= dt;
            if (timer.RemainingTime <= 0f)
            {
                var entity = it.Entity(i);
                ref var gpu = ref gpuArray[i];
                ref var spatial = ref spatialArray[i];
                AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
                dynGrid.UnregisterDirect(spatial.Value); // toda unidad deberia tener su idspatial
                             
                world.Tick.TotalUnits--;
                
                if (entity.Has<RvoAgentDebugComponent>())
                {
                    var agentDebug= entity.Get<RvoAgentDebugComponent>();
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeMove);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeBody);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeRadiusAttack);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeRadiusRangeSearch);                    
                }
                if (entity.Has<MeleeAttackComponent>())
                {
                    var melle = entity.Get<MeleeAttackComponent>();
                    ContadoresHelper.Liberar(TipoContador.UnidadesMelle, melle.numberUnitMelle);
                    world.Tick.UpdateGroupCountMelle(ContadoresHelper.ObtenerUltimo(TipoContador.UnidadesMelle));
                }
                if (entity.Has<RangedAttackComponent>())
                {
                    var ranged = entity.Get<RangedAttackComponent>();
                    ContadoresHelper.Liberar(TipoContador.EdificiosUnidadesRango, ranged.NumberUnitRange);
                    world.Tick.UpdateGroupCountRanged(ContadoresHelper.ObtenerUltimo(TipoContador.EdificiosUnidadesRango));
                }
                entity.Destruct();
            }
        }
    }
}

internal class DestroyCleanupSystem : FlecsSystemBase
{
    protected override bool MultiThreaded => false;
    protected override ulong Phase => flecs.EcsPostUpdate;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RenderGPUComponent>()
        .With<SpatialIDComponent>()
        .With<DeathTimerComponent>()
        .With<PositionComponent>()
        .With<BuildingDefinitionComponent>()          
        .With<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var staticGrid = world.State.StaticSpatialBuildings;
        var occupancyMap = world.State.OccupancyMap;
        var spatialEntityMap = world.State.SpatialEntityMap;

        var gpuArray = it.Field<RenderGPUComponent>(0);
        var spatialArray = it.Field<SpatialIDComponent>(1);
        var timerArray = it.Field<DeathTimerComponent>(2);
        var posArray = it.Field<PositionComponent>(3);
        float dt = it.DeltaTime();
        for (int i = 0; i < it.Count(); i++)
        {
            ref var timer = ref timerArray[i];
            timer.RemainingTime -= dt;
            if (timer.RemainingTime <= 0f)
            {
                var entity = it.Entity(i);
                ref var gpu = ref gpuArray[i];
                ref var spatial = ref spatialArray[i];
                ref var  pos = ref posArray[i];
                AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
                staticGrid.FreeCollider(spatial.Value); // todo edificio deberia tener su collider
                
                if (entity.Has<RvoAgentDebugComponent>())
                {
                    var agentDebug = entity.Get<RvoAgentDebugComponent>();
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeMove);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeBody);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeRadiusAttack);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeRadiusRangeSearch);
                }
                if (entity.Has<RangedAttackComponent>())
                {
                    var ranged = entity.Get<RangedAttackComponent>();
                    ContadoresHelper.Liberar(TipoContador.EdificiosUnidadesRango, ranged.NumberUnitRange);
                    world.Tick.UpdateGroupCountRanged(ContadoresHelper.ObtenerUltimo(TipoContador.EdificiosUnidadesRango));
                }
                spatialEntityMap.Remove(entity);
                occupancyMap.ClearByEntity(0, pos.tilePosition.X,pos.tilePosition.Y);
                entity.Destruct();
            }
        }
    }
}