using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Behaviors;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using Microsoft.CodeAnalysis;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.systems.Player;

public class HumanCharacterSystem : BaseSystem<World, float>
{
    private QueryDescription query = new QueryDescription()
        .WithAll<PlayerInputComponent, CharacterComponent, PositionComponent, DirectionComponent, VelocityComponent, CharacterAtackComponent>();

    private CommandBuffer commandBuffer;

    public HumanCharacterSystem(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
    }

    private readonly struct UpdateHuman : IForEachWithEntity<
        PlayerInputComponent,
        CharacterComponent,
        PositionComponent,
        DirectionComponent,
        VelocityComponent,
        CharacterAtackComponent,CharacterAnimationComponent>
    {
        private readonly float _deltaTime;
        private readonly CommandBuffer _commandBuffer;

        public UpdateHuman(float deltaTime, CommandBuffer commandBuffer)
        {
            _deltaTime = deltaTime;
            _commandBuffer = commandBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(Entity entity,
            ref PlayerInputComponent input,
            ref CharacterComponent character,
            ref PositionComponent position,
            ref DirectionComponent direction,
            ref VelocityComponent velocity,
            ref CharacterAtackComponent attackComp,ref CharacterAnimationComponent animComp)
        {
            HandleAttack(entity, ref input, ref character, ref position, ref direction, ref attackComp,ref animComp,_deltaTime);
            HandleMovement(ref input, ref character, ref position, ref direction, ref velocity);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleAttack(Entity entity,
     ref PlayerInputComponent input,
     ref CharacterComponent character,
     ref PositionComponent position,
     ref DirectionComponent direction,
     ref CharacterAtackComponent attackComp,
     ref CharacterAnimationComponent animComp,
     float deltaTime)
        {
            if (input.attackReleased)
            {
                character.characterStateType = CharacterStateType.IDLE;
                attackComp.isAttack = false;
            }       
            // --- Inicio del ataque ---
            if (input.attackPressed)
            {
                attackComp.isAttack = true;
                character.speedAtackBase = -0.05f;
                character.characterStateType = CharacterStateType.ATTACK;

            }
            // --- Durante ataque ---
            if (attackComp.isAttack)
            {              
                if (animComp.animationComplete)
                {
                    
                    //GD.Print("ataca");
                   
                    ApplyAttackDamage(entity, ref character, ref position, ref direction);
                    
                }
            }
        }
        private void ApplyAttackDamage(Entity entity,
    ref CharacterComponent character,
    ref PositionComponent position,
    ref DirectionComponent direction)
        {
            var dataAccesory = AccesoryManager.Instance.GetAccesory(character.accessoryArray[0]);
            var animacionData = dataAccesory.accesoryAnimationBodyData.animationStateData.animationData[(int)direction.animationDirection];

            if (!animacionData.hasCollider)
                return;

            ColliderComponent colliderComponent = entity.Get<ColliderComponent>();
            TeamComponent team = entity.Get<TeamComponent>();

            var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(character.idCharacterBaseData);
            GeometricShape2D collision = animacionData.collider.Multiplicity(dataCharacterModel.scale);

            Vector2 positionRelative = position.position + collision.OriginCurrent;
            Rect2 aabb = new Rect2(positionRelative - (collision.GetSizeQuad() / 2), collision.GetSizeQuad());

            var dataQuery = CollisionManager.Instance.characterCollidersEntities.GetCollidingOwnersInAABBExternal(
                dataCharacterModel.collisionBody,
                aabb,
                colliderComponent.idCollider
            );

            foreach (var item in dataQuery)
            {
                TeamComponent teamB = item.Get<TeamComponent>();
                if (item.Id != entity.Id && teamB.team != team.team)
                {
                    ref CharacterComponent characterB = ref item.TryGetRef<CharacterComponent>(out bool exists);
                    if (!exists) continue;

                    var dataCharacterModelB = CharacterModelManager.Instance.GetCharacterModel(characterB.idCharacterBaseData);
                    GeometricShape2D colliderB = dataCharacterModelB.collisionBody;
                    var positionB = item.Get<PositionComponent>().position + colliderB.OriginCurrent;

                    if (Collision2D.Collides(collision, colliderB, positionRelative, positionB))
                    {
                        _commandBuffer.Add(item, new TakeHitComponent
                        {
                            stunTime = 0.25f
                        });
                        //characterB.characterStateType = CharacterStateType.TAKE_HIT;
                        AplyDamageCharacter(entity, item);
                    }
                }
            }
        }
        public void AplyDamageCharacter(Entity origin, Entity destiny)
        {
            CharacterComponent unitA = origin.Get<CharacterComponent>();
            ref CharacterComponent unitB = ref destiny.TryGetRef<CharacterComponent>(out bool exist);            
            unitB.healthBase -= unitA.damageBase;
            if (unitB.healthBase <= 0)
            {
                _commandBuffer.Add<DeadComponent>(destiny);
               
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleMovement(
            ref PlayerInputComponent input,
            ref CharacterComponent character,
            ref PositionComponent position,
            ref DirectionComponent direction,
            ref VelocityComponent velocity)
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

                    // movimiento
                    Vector2 newPosition = position.position+ moveDir * velocity.velocity * _deltaTime;
                    position.positionFuture = newPosition;
                    character.characterStateType = CharacterStateType.MOVING;
                }
                else
                {
                    character.characterStateType = CharacterStateType.IDLE;
                }
            }
            else
            {
                if (state == CharacterStateType.MOVING)
                {
                    character.characterStateType = CharacterStateType.IDLE;
                }
            }
        }
    }

    public override void Update(in float t)
    {
        var job = new UpdateHuman(t, commandBuffer);
        World.InlineEntityQuery<
            UpdateHuman,
            PlayerInputComponent,
            CharacterComponent,
            PositionComponent,
            DirectionComponent,
            VelocityComponent,
            CharacterAtackComponent,CharacterAnimationComponent>(in query, ref job);

        commandBuffer.Playback(World);
    }
}
