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
    ATACANDO_CUERPO = 2,
    RECIBE_DANIO = 3,
    STUNEADO = 4,
    MUERTO = 5,    
    ATACANDO_DISTANCIA = 6,
    ATACANDO_DISTANCIA_MAGIA = 7
}



public record struct DeathTimerComponent
(
   float RemainingTime
);

public record struct DamagePendingComponent
(
    int Amount,
    Entity Source
);


public record struct AmountComponent
(
   int value
);

public record struct HealthBarComponent
(
     Rid MultiMeshRid,
     int InstanceId,
     bool IsVisible,       // Bandera para mostrar/ocultar fácilmente
     float HeightOffset   // A qué altura flota sobre la unidad
);


public record struct HealthComponent
(
    int MaxHealth, 
    int value
);


public record struct AttackPendingComponent
(
    bool Active,
    Entity Target,
    bool isUnit,
    Vector2 targetPosition,
    float impactThereshold
);


public record struct AttackPendingRelation
(     
);



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


public record struct RangedAttackComponent
(
    ushort idMod,
    int idProjectile,
    int Damage,
    float Range,           // Rango de ataque (ej. 1.5m)
    float Cooldown,        // Tiempo entre ataques
    float Timer,           // Temporizador interno
    bool Homing,           // misil auto dirigido
    float SpeedProjectile,
    int NumberUnitRange
);

public record struct WeaponComponent
(
    int idWeapon,
    bool isRanged
);

public record struct MeleeAttackComponent
(
    int Damage,
    float RangeAttack,           // Rango de base
    Vector2 OffSetRange,         // Offset de ataque
    float Cooldown,              // Cooldown entre ataques
    float Timer,                 // Temporizador de cooldown
    float AnimDuration,          // ⏱️ Duración total de la animación de ataque
    float AnimTimer,             // ⏱️ Temporizador interno de la animación actual
    int numberUnitMelle
);


public record struct HumanAttackComponent
(       
   int Damage,
   float Range,           // Rango de ataque melee (ej. 1.5m)
   float CooldownMelle,   // Tiempo entre ataques
   float CooldownRanged,  // Tiempo entre ataques a distancia
   float Timer            // Temporizador interno
);


public record struct EnemySearchComponent(
    float Radius,           // Radio de búsqueda
    float Interval,         // Cada cuánto buscar (en segundos)
    float Timer,            // Temporizador interno
    int StaggerIndex,       // Índice secuencial para el staggering
    bool EnableChase        // ¿Esta entidad persigue al objetivo si sale del radio inicial?
);


public record struct TeamComponent
(
    ushort TeamId            // Ej: 0 = player, 1 = enemigos
);


public record struct SelectedTag();


public record struct AttackingPathFollowerTag();



public record struct PersistEntityTag();

public record struct RvoDeltaTag();


public record struct ActiveProjectileTag();


public record struct ProjectileInitializedTag();




public record struct StaticTag();


public record struct StaticRenderTag();


public record struct DestroyRequestTag();


public record struct ChaseTargetTag();



public record struct DeadTag();

public struct AttackPendingTag { }


public record struct UseRvoTag();   // unidades controladas por RVO

public record struct UseBoidTag();  // unidades con avoidance simple



public record struct SpriteSimpleAnimationTag();


public record struct RenderDisabledTag();




public record struct DirtyTileSpriteTextureRenderTag();


public record struct DirtyTransformTag();


public record struct UnitDefinitionComponent
(
    ushort idTemplate // id Template de la unidad, para buscar en la base de datos
);


public record struct BuildingDefinitionComponent
(
    ushort idTemplate, // id Template del edificio
    int idSpriteTemplateNormal, // id template del sprite    
    int idSpriteConstruccion,
    int idSpriteDestruccion 
);


public record struct ResourceDefinitionComponent
(
    ushort idTemplate, // id Template del recurso
    int idSpriteTemplate // id template del sprite    
);


public record struct IdGenericComponent
(
    long id,
    EntityType entityType
);


public record struct RenderFrameDataComponent
{
    public Color uvMap;
}


public record struct RenderLayerListComponent
{
    public AnimationComponent[] Animations;
    public RenderGPUComponent[] GPUData;
    public RenderFrameDataComponent[] Frames;
    public RenderTransformComponent[] Transforms;
}


public record struct AnimationSimpleComponent
(
   int idSprite,      
   int currentFrameIndex,
   float TimeSinceLastFrame,
   float frameDuration,
   bool animationComplete,
   bool active,
   bool visible
);



public record struct AnimationComponent
(
   long idSpriteOrAnimation,
   EntityType entityType,
   AnimationType animationType,
   AnimationType lastAnimationType,
   int currentFrameIndex,
   float TimeSinceLastFrame,
   float frameDuration,
   bool animationComplete,
   bool active,
   bool visible
);


public record struct TileSpriteComponent
(
    long idTileSprite
);



public record struct LayerRenderComponent
(
    int layerRender       
);

public record struct RenderSelectionGPUComponent
(
    Rid rid,
    int instance,    
    int layerTextureMaterial,
    int layerRender,
    Vector2 offset
);

public record struct RenderGPUComponent
(      
    // ojo idMaterial ya no se usa
    int gpuindex,
    Rid rid, // se debe eliminar si funciona
    int instance,        
    int idMaterial, // no se usa
    int layerTextureMaterial,
    int layerRender,
    float depthOffset,
    float scale,
    Vector2 originOffset       
);


public record struct RenderInstanceComponent
(
    Rid rid,
    int instance,
    int materialId,        
    bool isActive
);

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


public struct PlayerInputComponent
{
    public Vector2 moveDirection;
    public bool attackPressed;
    public bool attackReleased;
    public bool isAttack;
}


public record struct StateComponent
{
    public StateType lastStateType;
    public StateType stateType;
    public BehaviorType behaviorType;                   
}


public record struct BuildingComponent
{
    public Rect2 colliderLocal;        
}


public record struct VelocityComponent(Vector2 currentVel, float MaxSpeed, Vector2 desiredVel,float Acceleration );


public record struct MoveTargetComponent(
    Vector2 Value
    );




public record struct MoveResolutorComponent(
    bool Blocked,     // Flag que indica si el agente está bloqueado
    float BlockedTimer, // tiempo acumulado sin moverse
    Vector2 ResolveDir,
    float PenetrationMagnitude, 
    float SeparationForceMagnitude,
    float StuckTimer,
    float RetryTimer,        // NUEVO: cooldown entre intentos de avance
    int ConsecutiveBlocks,
    int LastConeIndex,
    float AvoidanceTimer// NUEVO: cuántas veces seguidas no pudo avanzar
);


public record struct RvoAgentIdComponent(
    int Id,
    float radius
    );



public record struct RvoAgentDebugComponent(
    int idShapeMove,
    int idShapeBody,
    int idShapeRadiusAttack,
    int idShapeRadiusRangeSearch
    );

public struct SpatialComponent
{
    public Vector2I Chunk;
    public int IndexInBucket;
}

public record struct PositionComponent
(         
     Vector2 position,
     Vector2I tilePosition,
     int height
);
public struct ProjectilePositionComponent
{
    public Vector2 Position;
}

public struct ProjectileVelocityComponent
{
    public Vector2 Velocity;
}

public struct ProjectileTargetComponent
{
    public ushort idMod;
    public int idProjectile;
    public Entity Target;
    public int Damage;
    public Vector2 Origin;
    public Vector2 Destination;
    public float TotalDistance;
    public float impactThereshold;
}


public record struct DirectionComponent
(
    Vector2 value,
    Vector2 normalized,
    DirectionAnimationType directionType,
    AnimationDirection animationDirection
);

public record struct ColliderComponent
(
    int idCollider,
    Rect2 aabb,
    Vector2 offset,
    Rect2 aabbMove,
    Vector2 offsetMove,
    int idDebug
);

public record struct ColliderDebugComponent
(
     List<int> idShapes
);



public record struct TileSpriteTextureComponent
(
    int idSpriteInternal
);

// El componente que se añade a cualquier entidad que necesite colisiones

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

public record struct FastColliderComponent
(
     ShapeType Shape,
     float Width,
     float Height,
     float OffsetX,
     float OffsetY
);


public record struct MovementColliderRefComponent(Entity Value);

public record struct BodyColliderRefComponent ( Entity Value );

public record struct SensorColliderRefComponent ( Entity Value );



public record struct UnitTag();



public record struct StoppedTag();


public record struct PlayerTag();


public record struct StuckComponent
(
    Vector2 LastPosition,
    float StuckTime,
     bool IsBlocked 
);
// En el ENEMIGO/TARGET: qué slots están ocupados

public struct AttackSlotsComponent
{
    public uint OccupiedMask; // hasta 32 slots, uno por bit
}
// En el ATACANTE: qué slot tiene reservado y contra quién

public struct AttackSlotComponent
{
    public Entity Target;
    public int SlotIndex;
}


public record struct PathReferenceComponent
(
    int PathId,       // El ID de la ruta en el BlackyPathRegistryManager
    int CurrentIndex, // El índice del siguiente punto a visitar
    Vector2 FormationOffset
);


public record struct SteeringComponent
(
    float SeparationRadius,
    float SeparationWeight,
    Vector2 DesiredDir,
    Vector2 TargetDir
);


public record struct SteeringStateComponent
(
    Vector2 LastDir
);

public record struct MovementMassiveTag
();


public record struct FlowFieldFollowerComponent
(
    int FlowFieldId,
    Vector2 TargetDir,
    byte Status, 
    Vector2 GoalPos,   // meta en coordenadas del mundo
    bool Counted      // true si ya cuenta en SettledCount del campo
);


public record struct MoveColliderComponent
(
    float Radius, // Para colisiones circulares, que usara el steerring
    Vector2 Offset,
    float AvoidanceRadius
);

public struct FastCollider
{
    public SlopeType Slope { get; set; } // Solo se usa si Shape == Slope
    public ShapeType Shape { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public Vector2 Offset { get; set; }
}


public record struct BodyColliderComponent
(
    FastCollider[] Shapes
);

// structs Basicos


internal class ComponentsFlecs
{
}