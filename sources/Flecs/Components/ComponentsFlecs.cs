using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Components;



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


public enum AnimationType
{
    NINGUNA = -1,
    PARADO = 0,
    CAMINANDO = 1,
    ATACANDO = 2,
    RECIBE_DANIO = 3,
    STUNEADO = 4,
    MUERTO = 5,
    ARMA_ATACANDO = 6
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
public record struct WeaponComponent
(
    int idWeapon,

    bool isRanged,
    Dictionary< AnimationDirection,GeometricShape2D> collidersDirection
    
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
public record struct StaticTag();

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
public record struct RenderDisabledTag();

[RegisterComponentFlecs]
public record struct TileTextureTag();

[RegisterComponentFlecs]
public record struct DirtyTileRenderTag();

[RegisterComponentFlecs]
public record struct DirtyTransformTag();


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
   AnimationType stateAnimation,
   AnimationType lastStateAnimation,
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
public record struct RenderInstanceComponent
(
    Rid rid,
    int instance,
    int materialId,        
    bool isActive
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
public record struct VelocityComponent(Vector2 currentVel, float MaxSpeed, Vector2 desiredVel);

[RegisterComponentFlecs]
public record struct MoveTargetComponent(
    Vector2 Value
    );

[RegisterComponentFlecs]
public record struct MoveResolutorComponent(
    bool Blocked,     // Flag que indica si el agente está bloqueado
    float BlockedTimer, // tiempo acumulado sin moverse
    Vector2 ResolveDir,
    float PenetrationMagnitude, float SeparationForceMagnitude
    // <-- Nuevo campo
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
public struct SpatialComponent
{
    public Vector2I Chunk;
    public int IndexInBucket;
}
[RegisterComponentFlecs]
public record struct PositionComponent
(         
     Vector2 position,
     Vector2I tilePosition,
     int height
);
[RegisterComponentFlecs]
public record struct DirectionComponent
(
    Vector2 value,
    Vector2 normalized,
    DirectionAnimationType directionType,
    AnimationDirection animationDirection
);
[RegisterComponentFlecs]
public record struct ColliderComponent
(
    int idCollider,
    Rect2 aabb,
    Vector2 offset,
    Rect2 aabbMove,
    Vector2 offsetMove,
    int idDebug
);
[RegisterComponentFlecs]
public record struct ColliderDebugComponent
(
     List<int> idShapes
);



// El componente que se añade a cualquier entidad que necesite colisiones
[RegisterComponentFlecs]
public record struct SpatialIDComponent
(
     int Value,     // ID compacto para el array
     uint Layer,    // Qué SOY (ej: Soy una BALA)
     uint Mask     // Con qué CHOCO (ej: Choco con ENEMIGOS y MUROS)
);
public enum SlopeType : byte
{    
    BottomLeft,  // Triángulo 1
    TopRight,    // Triángulo 2
    TopLeft,     // Triángulo 3
    BottomRight  // Triángulo 4
}

public enum ShapeType : byte
{
    Rect,
    Circle,    
    Slope // triangulos
}
[RegisterComponentFlecs]
public record struct FastColliderComponent
(
     ShapeType Shape,
     float Width,
     float Height,
     float OffsetX,
     float OffsetY
);

[RegisterComponentFlecs]
public record struct MovementColliderRefComponent(Entity Value);
[RegisterComponentFlecs]
public record struct BodyColliderRefComponent ( Entity Value );
[RegisterComponentFlecs]
public record struct SensorColliderRefComponent ( Entity Value );


[RegisterComponentFlecs]
public record struct UnitTag();


[RegisterComponentFlecs]
public record struct SleepTag();

[RegisterComponentFlecs]
public record struct PlayerTag();

[RegisterComponentFlecs]
public record struct StuckComponent
(
    Vector2 LastPosition,
    float StuckTime,
     bool IsBlocked 
);


[RegisterComponentFlecs]
public record struct SteeringComponent
(
    float SeparationRadius,
    float SeparationWeight,
    Vector2 DesiredDir
);

[RegisterComponentFlecs]
public record struct SteeringStateComponent
(
    Vector2 LastDir
);
[RegisterComponentFlecs]
public record struct MoveColliderComponent
(
    float Radius, // Para colisiones circulares, que usara el steerring
    Vector2 Offset,
    float AvoidanceRadius
);
[RegisterComponentFlecs]
public struct FastCollider
{
    public SlopeType Slope { get; set; } // Solo se usa si Shape == Slope
    public ShapeType Shape { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public Vector2 Offset { get; set; }
}

[RegisterComponentFlecs]
public record struct BodyColliderComponent
(
    FastCollider[] Shapes
);

// structs Basicos


internal class ComponentsFlecs
{
}