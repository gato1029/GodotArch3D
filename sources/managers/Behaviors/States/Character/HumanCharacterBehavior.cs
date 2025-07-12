using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Accesories;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.systems;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Behaviors.States.Character;
public class HumanCharacterBehavior : ICharacterBehavior
{
    public string Name => "Human";

 
    public void ControllerState(Entity entity, ref CharacterComponent characterComponent, ref CharacterAnimationComponent animation, ref CharacterBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer)
    {
        ref CharacterAtackComponent characterAtackComponent = ref entity.Get<CharacterAtackComponent>();
        switch (characterComponent.characterStateType)
        {
            
            case CharacterStateType.IDLE:
                 animation.stateAnimation = 0;
                break;
            case CharacterStateType.MOVING:
                 animation.stateAnimation = 1;            
                break;
            case CharacterStateType.ATTACK:
                characterComponent.speedAtackBase =-0.08f;
                animation.stateAnimation = 2;                                
                if (animation.animationComplete)
                {
                    characterComponent.characterStateType = CharacterStateType.EXECUTE_ATTACK;          
                    
                }
         
                break;
            case CharacterStateType.EXECUTE_ATTACK:
                characterComponent.characterStateType = CharacterStateType.IDLE;                
                break;
            
            case CharacterStateType.TAKE_HIT:
                
                animation.stateAnimation = 4;
                if (animation.animationComplete)
                {
                    characterComponent.characterStateType = CharacterStateType.IDLE;

                }
                break;
            case CharacterStateType.TAKE_STUN:
                break;
            case CharacterStateType.DIE:
                animation.stateAnimation = 3;
                if (animation.animationComplete)
                {
                    if (!entity.Has<PendingRemove>())
                    {
                        commandBuffer.Add<PendingRemove>(in entity);
                    }
                }
                break;
        }
    }

    public void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer, float delta)
    {
        ref PositionComponent positionComponent = ref entity.Get<PositionComponent>();
        ref DirectionComponent directionComponent = ref entity.Get<DirectionComponent>();
        ref VelocityComponent  velocityComponent = ref entity.Get<VelocityComponent>();
        ref CharacterAtackComponent characterAtackComponent = ref entity.Get<CharacterAtackComponent>();

        CharacterStateType stateCharacter = characterComponent.characterStateType;


        Vector2 moveDirection = Vector2.Zero;
        bool attack = false;
        if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_up"))
        { moveDirection.Y += 1; }
        if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_down"))
        { moveDirection.Y -= 1; }
        if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_left"))
        { moveDirection.X -= 1; }
        if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("move_right"))
        { moveDirection.X += 1; }
        //if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("attack"))
        //{ attack = true; }

        if (Input.IsActionPressed("attack"))
        {
            characterAtackComponent.isAttack = true;
            characterComponent.characterStateType = CharacterStateType.ATTACK;
            attack = true;
        }
        if (Input.IsActionJustReleased("attack"))
        {
            characterComponent.characterStateType = CharacterStateType.IDLE;
        
            characterAtackComponent.isAttack = false;
            attack = false;
        }
        if (attack!=true)
        {
           

            //if ((stateCharacter == CharacterStateType.IDLE || stateCharacter == CharacterStateType.MOVING) && attack)
            //{
            //    characterComponent.characterStateType = CharacterStateType.ATTACK;
            //    stateCharacter = CharacterStateType.ATTACK;
            //    //characterAtackComponent.isAttack = true;
            //}

            if ((stateCharacter == CharacterStateType.IDLE || stateCharacter == CharacterStateType.MOVING) && moveDirection == Vector2.Zero)
            {
                characterComponent.characterStateType = CharacterStateType.IDLE;

            }

            if ((stateCharacter == CharacterStateType.IDLE || stateCharacter == CharacterStateType.MOVING ) && moveDirection != Vector2.Zero)
            {
                moveDirection = moveDirection.Normalized();
                Move(entity, moveDirection, ref characterComponent, ref characterBehaviorComponent, ref positionComponent, ref directionComponent, velocityComponent, delta);
            }
        }
        else
        {
            if (stateCharacter == CharacterStateType.EXECUTE_ATTACK)
            {
                GD.Print("ataque");
                Atack(entity, characterComponent, positionComponent, directionComponent);
                characterComponent.characterStateType = CharacterStateType.IDLE;
            }
        }
          
        
     
    }

    private void Atack(Entity entity, CharacterComponent characterComponent, PositionComponent positionComponent, DirectionComponent directionComponent)
    {
        var dataAccesory = AccesoryManager.Instance.GetAccesory(characterComponent.accessoryArray[0]);
        var animacionData = dataAccesory.accesoryAnimationBodyData.animationStateData.animationData[(int)directionComponent.animationDirection];
        if (animacionData.hasCollider)
        {
            var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
            GeometricShape2D collision = animacionData.collider.Multiplicity(dataCharacterModel.scale);
            
            Vector2 positionRelative = positionComponent.position + collision.OriginCurrent;

            Rect2 aabb = new Rect2(positionRelative, collision.GetSizeQuad() * 2);
            Dictionary<int, Dictionary<int, Entity>> data = CollisionManager.Instance.characterCollidersEntities.QueryAABB(aabb);
            if (data != null)
            {
                foreach (var item in data.Values)
                {
                    foreach (var itemInternal in item)
                    {
                        Entity entityObjetive = itemInternal.Value;
                        if (entityObjetive.Id != entity.Id)
                        {
                            
                            ref CharacterComponent characterComponentB =ref itemInternal.Value.TryGetRef<CharacterComponent>(out bool exist);
                            var dataCharacterModelB = CharacterModelManager.Instance.GetCharacterModel(characterComponentB.idCharacterBaseData);

                            AnimationCharacterBaseData characterB = dataCharacterModelB.animationCharacterBaseData;
                            GeometricShape2D colliderB = characterB.collisionBody.Multiplicity(dataCharacterModelB.scale);
                            var positionB = itemInternal.Value.Get<PositionComponent>().position + colliderB.OriginCurrent;

                            if (Collision2D.Collides(collision, colliderB, positionRelative, positionB))
                            {                              
                                characterComponentB.characterStateType = CharacterStateType.TAKE_HIT;
                                BehaviorManager.Instance.AplyDamageCharacter(entity, entityObjetive);                                
                            }
                        }
                    }
                
                }
            }

        }

    }

    private void Move(Entity entity, Vector2 moveDirection,ref CharacterComponent characterComponent, ref CharacterBehaviorComponent characterBehaviorComponent, ref PositionComponent positionComponent,ref DirectionComponent directionComponent, VelocityComponent velocityComponent, float delta)
    {
        var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(characterComponent.idCharacterBaseData);
        AnimationCharacterBaseData characterData = dataCharacterModel.animationCharacterBaseData;
        GeometricShape2D collisionMove = characterData.collisionMove.Multiplicity(dataCharacterModel.scale);


        if (directionComponent.value != moveDirection)
        {
            directionComponent.animationDirection = CommonOperations.GetDirectionAnimation(moveDirection);
            directionComponent.value = moveDirection;            
            directionComponent.normalized = new Vector2(Math.Sign(moveDirection.X), Math.Sign(moveDirection.Y));            
        }


        Vector2 movement = directionComponent.value * velocityComponent.velocity * delta;

        Vector2 movementNext = positionComponent.position + movement + collisionMove.OriginCurrent;

        bool existCollision = CollisionManager.CheckAnyCollision(entity, movementNext, collisionMove);
       
        if (!existCollision)
        {
            characterComponent.characterStateType = CharacterStateType.MOVING;
            positionComponent.position += movement;           
        }
        else
        {
            characterComponent.characterStateType = CharacterStateType.IDLE;
        }

    }

    bool AtackIsPosible()
    {
        return false; 
    }
}
