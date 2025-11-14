using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Components
{
    [RegisterComponentFlecs]
    public struct DamageEvent
    {
        public Entity Source;   // Quién causa el daño
        public Entity Target;   // A quién se aplica
        public int Amount;      // Cuánto daño
    }
    public enum EntityType
    {
        PERSONAJE = 0,
        EDIFICIO = 1,
        RECURSO = 2,
        ACCESORIO = 3,
        TERRENO = 4,
        TILESPRITE = 5,
    }


    [RegisterComponentFlecs]
    public record struct DeathTimerComponent
    (
       float RemainingTime
    );
    [RegisterComponentFlecs]
    public record struct DamagePendingComponent
    (
        int Amount,
        Entity Source
    );

    [RegisterComponentFlecs]
    public record struct HealthComponent
    (
       int value
    );

    [RegisterComponentFlecs]
    public record struct AttackPendingComponent
    (
        bool Active,        
        Entity Target
    );

    [RegisterComponentFlecs]
    public record struct AttackPendingRelation
    (     
    );


    [RegisterComponentFlecs]
    public record struct ProjectileComponent
    {
        public Entity Owner;     // Quién lo disparó (unidad o torre)
        public Entity Target;    // A quién apunta (puede ser null si es direccional)
        public float Speed;      // Velocidad de movimiento
        public Vector2 Direction; // Dirección de vuelo (normalizada)
        public int Damage;     // Daño que inflige al impactar
        public int idProjectile;
        public bool Homing;
        public float LifeTime;
        public Vector2 TargetPosition;  // posición inicial del objetivo
    }

    [RegisterComponentFlecs]
    public record struct RangedAttackComponent
    (
        int idProjectile,
        int Damage,
        float Range,           // Rango de ataque (ej. 1.5m)
        float Cooldown,        // Tiempo entre ataques
        float Timer,           // Temporizador interno
        bool Homing,
        float SpeedProjectile
    );

    [RegisterComponentFlecs]
    public record struct MeleeAttackComponent
    (
        int Damage,
        float Range,           // Rango de ataque melee (ej. 1.5m)
        float Cooldown,        // Tiempo entre ataques
        float Timer           // Temporizador interno
    );

    [RegisterComponentFlecs]
    public record struct HumanAttackComponent
    (
       int Damage,
       float Range,           // Rango de ataque melee (ej. 1.5m)
       float CooldownMelle,   // Tiempo entre ataques
       float CooldownRanged,  // Tiempo entre ataques a distancia
       float Timer            // Temporizador interno
    );

    [RegisterComponentFlecs]
    public record struct EnemySearchComponent
    (
        float Radius,           // Radio de búsqueda
        float Interval,         // Cada cuánto buscar (en segundos)
        float Timer            // Temporizador interno
    );

    [RegisterComponentFlecs]
    public record struct TeamComponent
    (
        int TeamId            // Ej: 0 = player, 1 = enemigos
    );

    [RegisterComponentFlecs]
    public record struct RvoDeltaTag();

    [RegisterComponentFlecs]
    public record struct StaticRenderTag();

    [RegisterComponentFlecs]
    public record struct DestroyRequestTag();

    [RegisterComponentFlecs]
    public record struct DeadTag();

    [RegisterComponentFlecs]
    public record struct UseRvoTag();   // unidades controladas por RVO
    [RegisterComponentFlecs]
    public record struct UseBoidTag();  // unidades con avoidance simple


    [RegisterComponentFlecs]
    public record struct TileSpriteAnimationTag();  


    [RegisterComponentFlecs]
    public record struct IdGenericComponent
    (
        long id,
        EntityType entityType
    );

    [RegisterComponentFlecs]
    public record struct RenderFrameDataComponent
    {
        public Color uvMap;
    }

    [RegisterComponentFlecs]
    public record struct RenderLayerListComponent
    {
        public AnimationComponent[] Animations;
        public RenderGPUComponent[] GPUData;
        public RenderFrameDataComponent[] Frames;
        public RenderTransformComponent[] Transforms;
    }
   

    [RegisterComponentFlecs]
    public record struct AnimationComponent
    (
       long idSpriteOrAnimation,
       EntityType entityType,
       int stateAnimation,
       int lastStateAnimation,
       int currentFrameIndex,
       float TimeSinceLastFrame,
       float frameDuration,
       bool animationComplete,
       bool active,
       bool visible
    );

    [RegisterComponentFlecs]
    public record struct TileSpriteComponent
    (
        long idTileSprite
    );
    

    [RegisterComponentFlecs]
    public record struct LayerRenderComponent
    (
        int layerRender       
    );
    [RegisterComponentFlecs]
    public record struct RenderGPUComponent
    (      
        Rid rid,
        int instance,        
        int idMaterial,
        int layerTextureMaterial,
        int layerRender,
        float zOrdering,
        float scale,
        Vector2 originOffset       
    );
    [RegisterComponentFlecs]
    public record struct RenderTransformComponent
    (
        Transform3D transform
    ); 


    public record struct AnimationExtraData
    (
        Rid Rid,
        int instance,
        Color frameData
    );

    public record struct AnimationCustomCharacterComponent
    (
         AnimationExtraData[] animationExtraComponent
    );

    [RegisterComponentFlecs]
    public struct PlayerInputComponent
    {
        public Vector2 moveDirection;
        public bool attackPressed;
        public bool attackReleased;
        public bool isAttack;
    }

    [RegisterComponentFlecs]
    public record struct CharacterComponent
    {
        public CharacterStateType characterStateType;
        public CharacterBehaviorType characterBehaviorType;                   
    }

    [RegisterComponentFlecs]
    public record struct BuildingComponent
    {
        public Rect2 colliderLocal;        
    }

    [RegisterComponentFlecs]
    public record struct VelocityComponent(Vector2 Value, float MaxSpeed, Vector2 prefVel);

    [RegisterComponentFlecs]
    public record struct MoveTargetComponent(
        Vector2 Value
        );

    [RegisterComponentFlecs]
    public record struct MoveResolutorComponent(
        bool Blocked,     // Flag que indica si el agente está bloqueado
        float BlockedTimer, // tiempo acumulado sin moverse
        Vector2 positionFuture
    );

    [RegisterComponentFlecs]
    public record struct RvoAgentIdComponent(
        int Id,
        float radius
        );


    [RegisterComponentFlecs]
    public record struct RvoAgentDebugComponent(
        int idShapeRadius,
        int idShapeBody,
        int idShapeRadiusAttack
        );

    [RegisterComponentFlecs]
    public record struct PositionComponent
    (
         Vector2 position,
         Vector2I tilePosition

    );
    [RegisterComponentFlecs]
    public record struct DirectionComponent
    (
        Vector2 value,
        Vector2 normalized,
        AnimationDirection animationDirection
    );
    [RegisterComponentFlecs]
    public record struct ColliderComponent
    (
        int idCollider,
        Rect2 aabb,
        Vector2 offset,
        Rect2 aabbMove,
        Vector2 offsetMove
    );

    internal class ComponentsFlecs
    {
    }
}