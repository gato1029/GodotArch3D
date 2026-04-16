using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Systems;
using RVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;
using static Flecs.NET.Core.Ecs.Units;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.Flecs.Systems.Human;
internal class HumanCharacterSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PlayerInputComponent>()
            .With<GodotFlecs.sources.Flecs.Components.CharacterComponent>()
            .With<PositionComponent>()
            .With<DirectionComponent>()
            .With<VelocityComponent>()
            .With<MoveResolutorComponent>()
            .With<RenderLayerListComponent>()
            .With<ColliderComponent>()
            .With<TeamComponent>()
            .With<MeleeAttackComponent>()
            .With<WeaponComponent>()
            .With<SteeringComponent>()
            .With<UseBoidTag>();
            
    }

    protected override void OnIter(Iter it)
    {
        var playerArray = it.Field<PlayerInputComponent>(0);
        var charArray = it.Field<GodotFlecs.sources.Flecs.Components.CharacterComponent>(1);
        var posArray = it.Field<PositionComponent>(2);
        var dirArray = it.Field<DirectionComponent>(3);
        var velArray = it.Field<VelocityComponent>(4);
        var moveResArray = it.Field<MoveResolutorComponent>(5);
        var renderLayerArray = it.Field<RenderLayerListComponent>(6);
        var colliderArray = it.Field<ColliderComponent>(7);
        var teamArray = it.Field<TeamComponent>(8);
        var attackArray = it.Field<MeleeAttackComponent>(9);
        var weaponArray = it.Field<WeaponComponent>(10);
        var steeringArray = it.Field<SteeringComponent>(11);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var player = ref playerArray[i];
            ref var chara = ref charArray[i];
            ref var pos = ref posArray[i];
            ref var dir = ref dirArray[i];
            ref var vel = ref velArray[i];
            ref var moveRes = ref moveResArray[i];
            ref var renderLayer = ref renderLayerArray[i];            
            ref var collider = ref colliderArray[i];
            ref var team = ref teamArray[i];
            ref var attackMelle = ref attackArray[i];
            ref var weapon = ref weaponArray[i];
            ref var steering = ref steeringArray[i];
            // ref var age = ref ageArray[i];

            ref var animation = ref renderLayer.Animations[0];
            ref var animationWeapon = ref renderLayer.Animations[1];
            ref var frameWeapon = ref renderLayer.Frames[1];
            ref var steeringData = ref steeringArray[i];

            HandleAttackMelle(it.Entity(i), ref player, ref chara, ref attackMelle, ref pos, ref dir, ref animation, ref animationWeapon,ref frameWeapon, ref weapon, collider, team,  it.DeltaTime());
            HandleMovement(ref player, ref chara, ref pos, ref dir, ref vel,ref moveRes,it.DeltaTime(), ref steeringData);
        }
    }
    private void HandleAttackMelle(Entity entity,
     ref PlayerInputComponent input,
     ref GodotFlecs.sources.Flecs.Components.CharacterComponent character,
     ref MeleeAttackComponent attack,
     ref PositionComponent position,
     ref DirectionComponent direction,
     ref AnimationComponent animation,
     ref AnimationComponent animationWeapon,
     ref RenderFrameDataComponent frameWeapon,
     ref WeaponComponent weapon,
     ColliderComponent colliderComponent,
     TeamComponent teamComponent,
     float deltaTime)
    {
        if (character.characterStateType == CharacterStateType.TAKE_HIT)
        {
            animationWeapon.visible = false;
            input.isAttack = false;
            return;
        }
        // --- Reinicio si se suelta el botón ---
        if (input.attackReleased)
        {
            animationWeapon.visible = false;
            character.characterStateType = CharacterStateType.IDLE;
            input.isAttack = false;
            return;
        }

        // --- Si está en cooldown, esperamos ---
        if (attack.Timer > 0f)
        {
            attack.Timer -= deltaTime;
            return;
        }
        
        // --- Inicio del ataque ---
        if (input.attackPressed && !input.isAttack)
        {
            // Reset de animaciones
            animation.animationComplete = false;
            animation.currentFrameIndex = 0;
            animation.TimeSinceLastFrame = 0f;

            animationWeapon.animationComplete = false;
            animationWeapon.currentFrameIndex = 0;
            animationWeapon.TimeSinceLastFrame = 0f;

            // Cargar primer frame del arma
            var animationData = AnimationCache.GetAnimation(
                animationWeapon.idSpriteOrAnimation,
                animationWeapon.entityType,
                animationWeapon.stateAnimation,
                direction
            );

            
            frameWeapon.uvMap = animationData.uvFramesArray[0];

            // Mostrar arma y marcar ataque activo
            animationWeapon.visible = true;
            input.isAttack = true;
            character.characterStateType = CharacterStateType.ATTACK;
            
        }

        // --- Durante el ataque ---
        if (input.isAttack)
        {
            // Si la animación de ataque termina, volvemos al idle
            if (animation.animationComplete)
            {
                input.isAttack = false;
                character.characterStateType = CharacterStateType.IDLE;
                animationWeapon.visible = false;
                attack.Timer = attack.Cooldown; // inicia cooldown
                ExecuteAttack(entity, position, attack, weapon, teamComponent,direction, colliderComponent);
            }
        }
    }

    private void ExecuteAttack(Entity entity, PositionComponent pos, MeleeAttackComponent melle, WeaponComponent weapon, TeamComponent team, DirectionComponent direction, ColliderComponent col)
    {

        var collider = weapon.collidersDirection[direction.animationDirection];

        Godot.Vector2 posf = pos.position - (collider.GetSizeQuad() / 2) + collider.OriginCurrent;
        var rectangle = new Rect2(posf, collider.GetSizeQuad());

        var nearby = CollisionManager.Instance.characterEntitiesFlecs.QueryAABBBrute(rectangle,col.idCollider);

        //var nearby = CollisionManager.Instance.characterEntitiesFlecs.QueryCirclePoints(pos.position, melle.Range, col.idCollider);
        foreach (var target in nearby)
        {

            if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
            {
                continue; // mismo equipo, ignorar
            }
            else
            {
                if (target.Owner != default && target.Owner.IsAlive() && !target.Owner.Has<DeadTag>())
                {
                    GlobalData.EventsDamage.Enqueue(new DamageEvent
                    {
                        Source = entity,
                        Target = target.Owner,
                        Amount = melle.Damage
                    });
                    GD.Print("Aplicando danio");
                }
            }
        }

      
        nearby = CollisionManager.Instance.BuildingsCollidersFlecs.QueryAABBBInCirclePoints(pos.position, melle.Range, 0, 1);

        foreach (var target in nearby)
        {
            if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
            {
                continue; // mismo equipo, ignorar
            }
            else
            {
                if (target.Owner != default && target.Owner.IsAlive() && !target.Owner.Has<DestroyRequestTag>())
                {

                    melle.Timer = melle.Cooldown; // resetear cooldown

                }
            }
        }
    }
    private void HandleMovement(
       ref PlayerInputComponent input,
       ref GodotFlecs.sources.Flecs.Components.CharacterComponent character,
       ref PositionComponent position,
       ref DirectionComponent direction, // opcional (puedes quitarlo)
       ref VelocityComponent velocity,
       ref MoveResolutorComponent moveRes,
       float deltaTime,
       ref SteeringComponent steeringData)
    {
        // 1. Si hay input y no estamos bloqueados, mandamos la intención al steering
        if (input.moveDirection != Vector2.Zero && character.characterStateType != CharacterStateType.BLOCKED)
        {
            Vector2 moveDir = input.moveDirection.Normalized();

            // ✅ AHORA: Solo informamos al steering. 
            // Ya no tocamos velocity.prefVel aquí, lo hará el SteeringSystem.
            steeringData.DesiredDir = moveDir;

            character.characterStateType = CharacterStateType.MOVING;
            moveRes.Blocked = false;
        }
        else
        {
            // 2. Si no hay input o estamos bloqueados, apagamos la intención
            steeringData.DesiredDir = Vector2.Zero;

            // Limpiamos la velocidad si el SteeringSystem no está corriendo o para frenado inmediato
            if (character.characterStateType == CharacterStateType.MOVING)
            {
                character.characterStateType = CharacterStateType.IDLE;
                // Opcional: velocity.prefVel = Vector2.Zero; 
                // Aunque el SteeringSystem lo hará solo al ver DesiredDir en Zero.
            }
        }
    }
    }
