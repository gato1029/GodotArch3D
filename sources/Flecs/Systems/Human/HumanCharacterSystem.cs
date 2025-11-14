using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
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
            .With<HumanAttackComponent>()
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
        var attackArray = it.Field<HumanAttackComponent>(7);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var player = ref playerArray[i];
            ref var chara = ref charArray[i];
            ref var pos = ref posArray[i];
            ref var dir = ref dirArray[i];
            ref var vel = ref velArray[i];
            ref var moveRes = ref moveResArray[i];
            ref var renderLayer = ref renderLayerArray[i];
            ref var attack = ref attackArray[i];
            // ref var age = ref ageArray[i];

            ref var animation = ref renderLayer.Animations[0];
            ref var animationWeapon = ref renderLayer.Animations[1];
            ref var frameWeapon = ref renderLayer.Frames[1];
            
            HandleAttack(it.Entity(i), ref player, ref chara, ref attack, ref pos, ref dir, ref animation, ref animationWeapon,ref frameWeapon,  it.DeltaTime());
            HandleMovement(ref player, ref chara, ref pos, ref dir, ref vel,ref moveRes,it.DeltaTime());
        }
    }
    private void HandleAttack(Entity entity,
     ref PlayerInputComponent input,
     ref GodotFlecs.sources.Flecs.Components.CharacterComponent character,
     ref HumanAttackComponent attack,
     ref PositionComponent position,
     ref DirectionComponent direction,
     ref AnimationComponent animation,
     ref AnimationComponent animationWeapon,
     ref RenderFrameDataComponent frameWeapon,
     float deltaTime)
    {
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

            var frameData = animationData.frameDataArray[0];
            frameWeapon.uvMap = new Godot.Color(
                frameData.xFormat,
                frameData.yFormat,
                frameData.widhtFormat,
                frameData.heightFormat
            );

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
                attack.Timer = attack.CooldownMelle; // inicia cooldown
            }
        }
    }

    private void HandleMovement(
        ref PlayerInputComponent input,
        ref GodotFlecs.sources.Flecs.Components.CharacterComponent character,
        ref PositionComponent position,
        ref DirectionComponent direction,
        ref VelocityComponent velocity,
        ref MoveResolutorComponent moveRes,        
        float deltaTime)
    {
        CharacterStateType state = character.characterStateType;

        if (input.moveDirection != Vector2.Zero &&
            (state == CharacterStateType.IDLE ||
             state == CharacterStateType.MOVING ||
             state == CharacterStateType.BLOCKED))
        {
            if (state != CharacterStateType.BLOCKED)
            {
                Vector2 moveDir = input.moveDirection.Normalized();

                // actualizar dirección
                direction.animationDirection = CommonOperations.GetDirectionAnimation(moveDir);
                direction.value = moveDir;
                direction.normalized = new Vector2(Math.Sign(moveDir.X), Math.Sign(moveDir.Y));

                velocity.prefVel = moveDir * velocity.MaxSpeed;
                //Simulator.Instance.setAgentMaxSpeed(age.Id, velocity.MaxSpeed);
                //Simulator.Instance.setAgentPrefVelocity(age.Id, velocity.prefVel);

                moveRes.Blocked = false;
                //// movimiento
                //Vector2 newPosition = position.position + moveDir * velocity.Value * deltaTime;
                //position.positionFuture = newPosition;
                character.characterStateType = CharacterStateType.MOVING;
            }
            else
            {
                //velocity.prefVel = Vector2.Zero;
                character.characterStateType = CharacterStateType.IDLE;
                //Simulator.Instance.setAgentMaxSpeed(age.Id, 0);
            }
        }
        else
        {
            if (state == CharacterStateType.MOVING)
            {
               // velocity.prefVel = Vector2.Zero;
                character.characterStateType = CharacterStateType.IDLE;
                //Simulator.Instance.setAgentMaxSpeed(age.Id, 0);
            }
        }
    }
}
