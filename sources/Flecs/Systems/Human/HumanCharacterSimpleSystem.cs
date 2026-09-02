using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Human;

internal class HumanCharacterSimpleSystem : FlecsSystemBase
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
            .With<TeamComponent>()            
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
        var teamArray = it.Field<TeamComponent>(6);        
        var weaponArray = it.Field<WeaponComponent>(7);
        var steeringArray = it.Field<SteeringComponent>(8);
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
            // ref var age = ref ageArray[i];

            ref var steeringData = ref steeringArray[i];
            bool isAtack = false;
            if (weapon.isRanged==false)
            {
                Entity ent = it.Entity(i);
                ref MeleeAttackComponent attackMelle = ref ent.GetMut<MeleeAttackComponent>();
                isAtack =HandleAttackMelle(ent, ref player, ref chara, ref attackMelle, ref pos, ref dir, team, it.DeltaTime());
            }
            else
            {
                // aqui ira el rango
            }

            if (!isAtack)
            {
                chara.characterStateType = CharacterStateType.IDLE;
                HandleMovement(ref player, ref chara, ref pos, ref dir, ref vel, ref moveRes, it.DeltaTime(), ref steeringData);
            }            
        }
    }
    private bool HandleAttackMelle(Entity entity,
    ref PlayerInputComponent input,
    ref GodotFlecs.sources.Flecs.Components.CharacterComponent character,
    ref MeleeAttackComponent attack,
    ref PositionComponent position,
    ref DirectionComponent direction,            
    TeamComponent teamComponent,
    float deltaTime)
    {


        // --- Si está en cooldown, esperamos ---
        if (attack.Timer > 0f)
        {
            attack.Timer -= deltaTime;
            return false;
        }
        if (character.characterStateType == CharacterStateType.EXECUTE_ATTACK)
        {   
            attack.Timer = attack.Cooldown; // inicia cooldown            
            GD.Print("Aplicar daño");
            input.isAttack = false;
            return true;
        }
        // --- Inicio del ataque ---
        if (input.attackPressed && character.characterStateType != CharacterStateType.EXECUTE_ATTACK)
        {            
            input.isAttack = true;
            character.characterStateType = CharacterStateType.ATTACK;
            return true;
        }
        
        // --- Durante el ataque ---
        if (input.isAttack)
        {
            //// Si la animación de ataque termina, volvemos al idle
            //if (animation.animationComplete)
            //{
            //    input.isAttack = false;
            //    character.characterStateType = CharacterStateType.IDLE;
            ////    animationWeapon.visible = false;
           
            //    //ExecuteAttack(entity, position, attack, weapon, teamComponent, direction, colliderComponent);
            //}
        }
        return false;
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
