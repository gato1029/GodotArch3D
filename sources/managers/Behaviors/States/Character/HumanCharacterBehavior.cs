using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Behaviors.BehaviorsInterface;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
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
        switch (characterBehaviorComponent.characterStateType)
        {
            
            case CharacterStateType.IDLE:
                 animation.stateAnimation = 0;              
                break;
            case CharacterStateType.MOVING:
                 animation.stateAnimation = 1;                     
                break;
            case CharacterStateType.ATTACK:
                if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("attack"))
                {
                    characterComponent.speedAtackBase = -0.05f;
                    animation.stateAnimation = 2;
                    
                    if (animation.animationComplete)
                    {
                        characterBehaviorComponent.characterStateType = CharacterStateType.EXECUTE_ATTACK;
                    }
                }
                else
                {
                    animation.TimeSinceLastFrame = 0;
                    characterBehaviorComponent.characterStateType = CharacterStateType.IDLE;
                }                                                                     
                break;
            case CharacterStateType.EXECUTE_ATTACK:
                characterBehaviorComponent.characterStateType = CharacterStateType.IDLE;                
                break;
            
            case CharacterStateType.TAKE_HIT:
                break;
            case CharacterStateType.TAKE_STUN:
                break;
            case CharacterStateType.DIE:
                break;
        }
    }

    public void ControllerBehavior(Entity entity, ref CharacterComponent characterComponent, ref CharacterBehaviorComponent characterBehaviorComponent, ref CommandBuffer commandBuffer, float delta)
    {
        ref PositionComponent positionComponent = ref entity.Get<PositionComponent>();
        ref DirectionComponent directionComponent = ref entity.Get<DirectionComponent>();
        ref VelocityComponent  velocityComponent = ref entity.Get<VelocityComponent>();

        CharacterStateType stateCharacter = characterBehaviorComponent.characterStateType;


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
        if (ServiceLocator.Instance.GetService<InputHandler>().IsActionActive("attack"))
        { attack = true; }
                 

        if (stateCharacter == CharacterStateType.EXECUTE_ATTACK)
        {
            Atack(ref characterBehaviorComponent);
        }

        if ((stateCharacter == CharacterStateType.IDLE || stateCharacter == CharacterStateType.MOVING) && attack)
        {
            characterBehaviorComponent.characterStateType = CharacterStateType.ATTACK;           
            stateCharacter = CharacterStateType.ATTACK;
           
        }

        if ((stateCharacter == CharacterStateType.IDLE || stateCharacter == CharacterStateType.MOVING) && moveDirection == Vector2.Zero)
        {
            characterBehaviorComponent.characterStateType = CharacterStateType.IDLE;
          
        }

        if ((stateCharacter == CharacterStateType.IDLE || stateCharacter == CharacterStateType.MOVING) && moveDirection != Vector2.Zero)
        {            
            moveDirection = moveDirection.Normalized();
           Move(entity, moveDirection,characterComponent, ref characterBehaviorComponent, ref positionComponent,ref directionComponent, velocityComponent, delta);         
        }
    }

    private void Atack(ref CharacterBehaviorComponent characterBehaviorComponent)
    {
        GD.Print("Ejecutar ataque");
        characterBehaviorComponent.characterStateType = CharacterStateType.IDLE;
    }

    private void Move(Entity entity, Vector2 moveDirection, CharacterComponent characterComponent, ref CharacterBehaviorComponent characterBehaviorComponent, ref PositionComponent positionComponent,ref DirectionComponent directionComponent, VelocityComponent velocityComponent, float delta)
    {
        AnimationCharacterBaseData characterData = characterComponent.CharacterBaseData.animationCharacterBaseData;
        GeometricShape2D collisionMove = characterData.collisionMove.Multiplicity(characterComponent.CharacterBaseData.scale);


        if (directionComponent.value != moveDirection)
        {
            directionComponent.animationDirection = CommonOperations.GetDirectionAnimation(moveDirection);
            directionComponent.value = moveDirection;
            // r.value = Mathf.RadToDeg(d.value.Angle());
            directionComponent.normalized = new Vector2(Math.Sign(moveDirection.X), Math.Sign(moveDirection.Y));
            

            // luego ponerlo en una solo funcion

            //melleAtack.shapeCollider = characterWeapon.shapeColliderLeftRight;

            //if (d.directionAnimation == AnimationDirection.UP || d.directionAnimation == AnimationDirection.DOWN)
            //{
            //    melleAtack.shapeCollider = characterWeapon.shapeColliderTopDown;
            //}

            //Rectangle rectangle = (Rectangle)melleAtack.shapeCollider;

            //rectangle.DirectionTo(moveDirection.X, moveDirection.Y);

            //Vector2 vector2 = Collision2D.RotatePosition(rectangle.OriginRelative, d.normalized);
            //rectangle.OriginCurrent = vector2;

            //ref Direction directionCharacter = ref entityCharacter.Get<Direction>();

            //directionCharacter.value = d.value;
            //directionCharacter.normalized = d.normalized;
            //directionCharacter.directionAnimation = d.directionAnimation;
        }


        Vector2 movement = directionComponent.value * velocityComponent.velocity * delta;

        Vector2 movementNext = positionComponent.position + movement + collisionMove.OriginCurrent;

        bool existCollision = false;

        Rect2 aabb = new Rect2(movementNext, collisionMove.GetSizeQuad() * 2);
        Dictionary<int, Dictionary<int, Entity>> data = CollisionManager.Instance.characterCollidersEntities.QueryAABB(aabb);
        if (data != null)
        {
            foreach (var item in data.Values)
            {
                foreach (var itemInternal in item)
                {
                    if (itemInternal.Value.Id != entity.Id)
                    {
                        CharacterComponent characterComponentB = itemInternal.Value.Get<CharacterComponent>();
                        AnimationCharacterBaseData characterB = characterComponentB.CharacterBaseData.animationCharacterBaseData;
                        GeometricShape2D colliderB =  characterB.collisionMove.Multiplicity(characterComponentB.CharacterBaseData.scale);
                        var positionB = itemInternal.Value.Get<PositionComponent>().position + colliderB.OriginCurrent;

                        if (Collision2D.Collides(collisionMove, colliderB, movementNext, positionB))
                        {
                            existCollision = true;
                            break;
                        }
                    }
                }
                if (existCollision)
                {
                    break;
                }
            }
        }

        if (!existCollision)
        {
            Dictionary<int, Dictionary<int, TileDataGame>> dataTile = CollisionManager.Instance.tileColliders.QueryAABB(aabb);
            if (dataTile != null)
            {
                foreach (var item in dataTile.Values)
                {
                    foreach (var itemInternal in item)
                    {

                        GeometricShape2D colliderB = itemInternal.Value.collisionBody;
                        var positionB = itemInternal.Value.positionCollider + itemInternal.Value.collisionBody.OriginCurrent;
                        if (Collision2D.Collides(collisionMove, colliderB, movementNext, positionB))
                        {
                            existCollision = true;
                            break;
                        }

                    }
                    if (existCollision)
                    {
                        break;
                    }
                }
            }
        }



        if (!existCollision)
        {
            characterBehaviorComponent.characterStateType = CharacterStateType.MOVING;
            positionComponent.position += movement;           
        }
        else
        {
            characterBehaviorComponent.characterStateType = CharacterStateType.IDLE;
        }

    }

    bool AtackIsPosible()
    {
        return false; 
    }
}
