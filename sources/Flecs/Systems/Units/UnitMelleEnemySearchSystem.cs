
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace GodotFlecs.sources.Flecs.Systems.Units;
public class UnitMelleEnemySearchSystem: FlecsSystemBase
{
    // busca enemigos de las unidades que son melle, osea ataque cuerpo a cuerpo
    // encuentra objetivo y automaticamente ya asigna destino y objetivo
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;    
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<SpatialIDComponent>()
          .With<TeamComponent>()
          .With<StateComponent>()
          .With<EnemySearchComponent>()
          .With<MoveResolutorComponent>()
          .With<MeleeAttackComponent>()
          .With<DirectionComponent>()
          .With<StoppedTag>()
          //.Without<MoveTargetComponent>()
          .Without<PlayerInputComponent>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>()
          .Without<AttackPendingTag>();
    }
    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var sim = world.Simulation.Tick;
        if (sim == null || sim.TickCount == 0) return;

        var dynGrid = world.State.DynamicHash;
        var staticGrid = world.State.StaticSpatialBuildings;

        var posArray = it.Field<PositionComponent>(0);
        var spatialArray = it.Field<SpatialIDComponent>(1);
        var teamArray = it.Field<TeamComponent>(2);
        var charArray = it.Field<StateComponent>(3);
        var searchArray = it.Field<EnemySearchComponent>(4);        
        var moveResolutorArray = it.Field<MoveResolutorComponent>(5);
        var melleArray = it.Field<MeleeAttackComponent>(6);
        var dirArray = it.Field<DirectionComponent>(7);

        Span<int> neighbors = stackalloc int[8];

        float deltaTime = sim.FixedDelta; // 🔥 Usar solo el delta del tick actual, no el acumulado histórico
        int mask = sim.GetGroupMaskMelle();
        int frame = sim.FrameIndex & mask;

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var spatial = ref spatialArray[i];
            ref var team = ref teamArray[i];
            ref var search = ref searchArray[i];
            ref var moveResolutor = ref moveResolutorArray[i];
            ref var melle = ref melleArray[i];
            ref var dir = ref dirArray[i];
            var e = it.Entity(i);

            // 🔥 acumular tiempo correctamente
            search.Timer += deltaTime;

            if (search.Timer >= search.Interval)
            {
                // 🔥 cuántas veces "debió" ejecutarse
                int times = (int)(search.Timer / search.Interval);

                // 🔥 conservar excedente (CLAVE)
                search.Timer -= times * search.Interval;
                // 🔥 👇 AQUI VA EL STAGGERING 👇
                int group = melle.numberUnitMelle & mask;
                if (group != frame) continue; 

                // 🔥 SOLO UNA QUERY (optimización crítica)
                int count = dynGrid.QueryNodesBoundedClosestLayersFiltered(
                    pos.position.X,
                    pos.position.Y,
                    search.Radius,
                    team.TeamId,
                    neighbors
                );
                bool existTarget = false;
                Vector2 targetPos = Vector2.Zero;
                Entity targetEntity = default;
                bool istargetUnit = false;
                ushort idTarget = 0;
                
                for (int ii = 0; ii < count; ii++)
                {
                    targetEntity = dynGrid.GetEntity(neighbors[ii]);

                    if (spatial.Value == neighbors[ii]) continue;

                    //var otherTeam = targetEntity.Get<TeamComponent>();
                    //if (team.TeamId == otherTeam.TeamId) continue; // el query ya hace este filtro

                    if (targetEntity.IsAlive() && !targetEntity.Has<DeadTag>())
                    {
                        targetPos = targetEntity.Get<PositionComponent>().position;
                        float rangeSqr = search.Radius * search.Radius;
                        float distSqr = pos.position.DistanceSquaredTo(targetPos);

                        if (distSqr > rangeSqr) continue; // Fuera del rango de melee, buscar siguiente
                        idTarget = targetEntity.Get<UnitDefinitionComponent>().idTemplate;
                        
                        existTarget = true;
                        istargetUnit = true;
                        break;
                    }
                }
                if (!existTarget)
                {
                    int radius = (int)MathF.Ceiling((search.Radius * 2f) / staticGrid._cellSize);
                    // aqui buscar edificos
                    foreach (var id in staticGrid.QueryNearbyUnique(pos.position.X, pos.position.Y, radius,team.TeamId))
                    {
                        if (staticGrid.TryGetValue(id, out Entity otherEntity))
                        {
                            if (!otherEntity.IsAlive()) continue;
                            targetEntity = otherEntity;
                            TileSpriteData sprite = null;

                            if (otherEntity.Has<BuildingDefinitionComponent>())
                            {
                                int idTemplate = otherEntity.Get<BuildingDefinitionComponent>().idSpriteTemplateNormal; // 🔹 para asegurar que es una entidad con collider
                                var template = AtlasModsManager.TryGetTileSprite(idTemplate, out sprite);
                            }
                            var posOther = otherEntity.Get<PositionComponent>();

                            targetPos = otherEntity.Get<PositionComponent>().position;
                            float rangeSqr = search.Radius * search.Radius;
                            float distSqr = pos.position.DistanceSquaredTo(targetPos);

                            if (distSqr > rangeSqr) continue; // Fuera del rango de melee, buscar siguiente
                            idTarget = targetEntity.Get<BuildingDefinitionComponent>().idTemplate;

                            existTarget = true;
                            break;
                        }
                    }
                }
       
                if (existTarget)
                {
                    //Vector2 toTarget = targetPos - pos.position + melle.OffSetRange;
                    //float distSq = toTarget.LengthSquared();
                    //float umbralLlegada = melle.RangeAttack;
                    //if (distSq <= umbralLlegada)//0.05f) // Umbral de llegada
                    //{
                    //    //ataca directamente
                    //    e.Set(new AttackPendingComponent(true, targetEntity, istargetUnit, targetPos));
                    //    e.Add<AttackPendingTag>();
                    //}
                    bool inRange = false;
                    if (istargetUnit)
                    {
                        inRange = CheckCollisionWithTargetUnit(pos.position+melle.OffSetRange,dir.normalized, melle.RangeAttack, targetPos, idTarget);
                    }
                    else
                    {
                        inRange= CheckCollisionWithTargetBuild(pos.position+melle.OffSetRange, dir.normalized, melle.RangeAttack, targetPos, idTarget);
                    }
                    if (inRange)
                    {
                        //ataca directamente
                            e.Set(new AttackPendingComponent(true, targetEntity, istargetUnit, targetPos,0));
                            e.Add<AttackPendingTag>();
                    }
                    else
                    {
                        moveResolutor.Blocked = false;
                        moveResolutor.BlockedTimer = 0f;
                        e.Set(new MoveTargetComponent(targetPos));
                        e.Remove<StoppedTag>();
                        e.Set(new AttackPendingComponent(true, targetEntity, istargetUnit, targetPos,0));
                    }
                    
                }
            
            }
        }
    }

    private bool CheckCollisionWithTargetBuild(Vector2 origin, Vector2 dirNormalized, float range, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.buildingPalette.GetData(templateId);
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.CheckAttackHalfCircle(
                origin.X, origin.Y,
                dirNormalized.X, dirNormalized.Y,
                range,
                targetPos.X, targetPos.Y,
                ref fast
            ))
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckCollisionWithTargetUnit(Vector2 origin, Vector2 dirNormalized, float range, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.characterPalette.GetData(templateId);
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.CheckAttackHalfCircle(
                origin.X, origin.Y,
                dirNormalized.X, dirNormalized.Y,
                range,
                targetPos.X, targetPos.Y,
                ref fast
            ))
            {
                return true;
            }
        }
        return false;
    }
}
