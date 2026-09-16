
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Systems;
using System;

namespace GodotEcsArch.sources.Flecs.Systems.Human;

internal class HumanCharacterSimpleSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PlayerInputComponent>()
            .With<GodotFlecs.sources.Flecs.Components.StateComponent>()
            .With<PositionComponent>()
            .With<DirectionComponent>()
            .With<VelocityComponent>()
            .With<MoveResolutorComponent>()
            .With<TeamComponent>()
            .With<WeaponComponent>()
            .With<SteeringComponent>()
            .With<SpatialIDComponent>()
            .With<UseBoidTag>()
            .Without<DeadTag>();

    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var dynGrid = world.State.DynamicHash;

        var playerArray = it.Field<PlayerInputComponent>(0);
        var charArray = it.Field<GodotFlecs.sources.Flecs.Components.StateComponent>(1);
        var posArray = it.Field<PositionComponent>(2);
        var dirArray = it.Field<DirectionComponent>(3);
        var velArray = it.Field<VelocityComponent>(4);
        var moveResArray = it.Field<MoveResolutorComponent>(5);                
        var teamArray = it.Field<TeamComponent>(6);        
        var weaponArray = it.Field<WeaponComponent>(7);
        var steeringArray = it.Field<SteeringComponent>(8);
        var spatial = it.Field<SpatialIDComponent>(9);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var player = ref playerArray[i];
            ref var chara = ref charArray[i];
            ref var pos = ref posArray[i];
            ref var dir = ref dirArray[i];
            ref var vel = ref velArray[i];
            ref var moveRes = ref moveResArray[i];                        
            ref var team = ref teamArray[i];            
            ref var weapon = ref weaponArray[i];
            ref var steering = ref steeringArray[i];
            ref var spatialId = ref spatial[i];
            
            ref var steeringData = ref steeringArray[i];
            bool isAtack = false;
            if (weapon.isRanged==false)
            {
                Entity ent = it.Entity(i);
                ref MeleeAttackComponent attackMelle = ref ent.GetMut<MeleeAttackComponent>();
                isAtack =HandleAttackMelle(ent, ref player, ref chara, ref attackMelle, ref pos, ref dir, team, it.DeltaTime(),dynGrid, spatialId);
            }
            else
            {
                // aqui ira el rango
            }

            if (!isAtack && chara.stateType != StateType.DIE)
            {
                chara.stateType = StateType.IDLE;
                HandleMovement(ref player, ref chara, ref pos, ref dir, ref vel, ref moveRes, it.DeltaTime(), ref steeringData);
            }            
        }
    }
    private bool HandleAttackMelle(Entity entity,
    ref PlayerInputComponent input,
    ref GodotFlecs.sources.Flecs.Components.StateComponent character,
    ref MeleeAttackComponent attack,
    ref PositionComponent position,
    ref DirectionComponent direction,            
    TeamComponent teamComponent,
    float deltaTime,
    BlackyEngine.Spatial.FastSpatialHash dynGrid,
    SpatialIDComponent spatialId)
    {


        // --- Si está en cooldown, esperamos ---
        if (attack.Timer > 0f)
        {
            attack.Timer -= deltaTime;
            return false;
        }
        if (character.stateType == StateType.EXECUTE_ATTACK)
        {   
            attack.Timer = attack.Cooldown; // inicia cooldown
            ExecuteAttack(entity, position, attack, teamComponent, direction,dynGrid, spatialId);            
            input.isAttack = false;
            return true;
        }
        // --- Inicio del ataque ---
        if (input.attackPressed && character.stateType != StateType.EXECUTE_ATTACK)
        {            
            input.isAttack = true;
            character.stateType = StateType.ATTACK;
            return true;
        }
      
        return false;
    }

    private void ExecuteAttack(Entity entity, PositionComponent pos, MeleeAttackComponent melle, TeamComponent team, DirectionComponent direction, BlackyEngine.Spatial.FastSpatialHash dynGrid, SpatialIDComponent spatialId)
    {
        Span<int> neighbors = stackalloc int[8];

        Vector2 originAttackCenter = pos.position+ melle.OffSetRange;
        // 🔥 SOLO UNA QUERY (optimización crítica)
        int count = dynGrid.QueryNodesBounded(
            originAttackCenter.X,
            originAttackCenter.Y,
            melle.RangeAttack,
            neighbors
        );

        for (int ii = 0; ii < count; ii++)
        {
            var targetEntity = dynGrid.GetEntity(neighbors[ii]);

            if (spatialId.Value == neighbors[ii]) continue; // no nos atacamos a nosotros mismos

            var otherTeam = targetEntity.Get<TeamComponent>();
            if (team.TeamId == otherTeam.TeamId) continue; // mismo equipo, ignorar

            if (targetEntity.IsAlive() && !targetEntity.Has<DeadTag>())
            {
                var targetPosition =targetEntity.Get<PositionComponent>();
                ushort idTemplate= targetEntity.Get<UnitDefinitionComponent>().idTemplate;
                var template = BlackyPalletesPersistence.characterPalette.GetData(idTemplate);

            

                foreach (var item in template.bodyColliders)
                {
                    FastCollider fast = item;
                    if (CollisionMathHelper.CheckAttackHalfCircle(
                        originAttackCenter.X,
                        originAttackCenter.Y,
                        direction.normalized.X,
                        direction.normalized.Y,
                        melle.RangeAttack,
                        targetPosition.position.X,
                        targetPosition.position.Y,
                        ref fast
                    ))
                    {
                        GlobalData.EventsDamage.Enqueue(new DamageEvent
                        {
                            Source = entity,
                            Target = targetEntity,
                            Amount = melle.Damage
                        });
                        GD.Print("Aplicando danio");
                        break;                        
                    }
                }

          
                            
            }
        }

      


        //nearby = CollisionManager.Instance.BuildingsCollidersFlecs.QueryAABBBInCirclePoints(pos.position, melle.Range, 0, 1);

        //foreach (var target in nearby)
        //{
        //    if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
        //    {
        //        continue; // mismo equipo, ignorar
        //    }
        //    else
        //    {
        //        if (target.Owner != default && target.Owner.IsAlive() && !target.Owner.Has<DestroyRequestTag>())
        //        {

        //            melle.Timer = melle.Cooldown; // resetear cooldown

        //        }
        //    }
        //}
    }

    public static bool IsInsideAttackHalfCircle(
    Vector2 attackerPosition,
    Vector2 targetPosition,
    Vector2 lookDirection, // Debe estar normalizado
    float attackRadius)
    {
        Vector2 toTarget = targetPosition - attackerPosition;

        // 1. Dentro del círculo (sin Sqrt: más eficiente)
           if (toTarget.LengthSquared() > attackRadius * attackRadius)
            return false;

        // 2. En el semicírculo que mira el personaje
        return toTarget.Dot(lookDirection) >= 0;
    }
    private void HandleMovement(
      ref PlayerInputComponent input,
      ref GodotFlecs.sources.Flecs.Components.StateComponent character,
      ref PositionComponent position,
      ref DirectionComponent direction, // opcional (puedes quitarlo)
      ref VelocityComponent velocity,
      ref MoveResolutorComponent moveRes,
      float deltaTime,
      ref SteeringComponent steeringData)
    {
        // 1. Si hay input y no estamos bloqueados, mandamos la intención al steering
        if (input.moveDirection != Vector2.Zero && character.stateType != StateType.BLOCKED)
        {
            Vector2 moveDir = input.moveDirection.Normalized();

            // ✅ AHORA: Solo informamos al steering. 
            // Ya no tocamos velocity.prefVel aquí, lo hará el SteeringSystem.
            steeringData.DesiredDir = moveDir;

            character.stateType = StateType.MOVING;
            moveRes.Blocked = false;
        }
        else
        {
            // 2. Si no hay input o estamos bloqueados, apagamos la intención
            steeringData.DesiredDir = Vector2.Zero;

            // Limpiamos la velocidad si el SteeringSystem no está corriendo o para frenado inmediato
            if (character.stateType == StateType.MOVING)
            {
                character.stateType = StateType.IDLE;
                // Opcional: velocity.prefVel = Vector2.Zero; 
                // Aunque el SteeringSystem lo hará solo al ver DesiredDir en Zero.
            }
        }
    }
}
