using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.components;

public enum AnimationDirection
{
    LEFT,
    RIGHT,
    UP,
    DOWN,
    LEFTUP,
    RIGHTUP,
    LEFTDOWN,
    RIGHTDOWN,

}
public enum CardinalDirection
{
    LEFT,
    RIGHT,
    UP,
    DOWN,
}

[Component]
public struct VelocityComponent
{
    public float velocity;
}

[Component]
public struct TilePositionComponent
{
    public int x;
    public int y;
}

[Component]
public struct PositionComponent
{
    public Vector2 position; 
    public Vector2 lastPosition; // 💡 nuevo campo para el sweep
    public Vector2 positionFuture;    
}
[Component]
public struct DirectionComponent
{
    public Vector2 value;
    public Vector2 normalized;
    public AnimationDirection animationDirection;
}
[Component]
public struct ColliderComponent
{
    public int idCollider;
    public Rect2 aabb;
    public Vector2 position;
}
[Component]
public struct PendingSpriteComponent
{
    public SpriteData spriteData;   // Referencia a los datos base (ej: textura, material, scale, etc.)
    public Vector2 position;        // Posición inicial donde debe instanciarse
    public int zIndex;            // Orden de render (opcional)
}

[Component]
public struct RenderGPUComponent
{
    public Rid rid;
    public int instance;
    public int idMaterial;
    public int layerRender;
    public float zOrdering;
    public Vector2 originOffset;
    public Transform3D transform;    
}

[Component]
public struct RenderGPULinkedComponent
{
    public GpuInstance[] instancedLinked;
}

public struct GpuInstance
{
    public Rid rid;
    public int instance;
}
[Component]
public struct SpriteRenderGPUComponent
{
    public Rid rid;
    public int instance;
    public int layerRender;
    public int arrayPositiontexture;
    public int idMaterial;
    public float zOrdering;
    public Vector2 originOffset;
    public float scale;
    public Color uvMap;
    public Transform3D transform;
}
[Component]
public struct TargetPositionComponent
{
    public Vector2 targetPosition;
    public float arrivalThreshold;
}
[Component]
public struct RangeComponent
{
    public float value;
}

[Component]
public struct TargetingRangeComponent
{
    public Entity targetEntity;
    public Rect2 targetAabb;
    public bool hasTarget;
}
[Component]
public struct TeamComponent
{
    public int team;
}
[Component]
public struct BuildingComponent
{
    public int id;        
}
[Component]
public struct BuildingUpgradeComponent
{
    public int id;
}
[Component]
public struct AttackCooldownComponent
{
    public float timeLeft;     // tiempo que falta para poder volver a atacar
    public float maxCooldown;  // tiempo total entre ataques
}
[Component]
public struct AttackRangeComponent
{
    public float attackRange;
}
[Component]
public struct AttackDamageComponent
{
    public int damage;
    public int physical;    
    public int fire;
    public int cold;
}
[Component]
public struct LevelComponent
{
    public int level;
}
[Component]
public struct HealthComponent
{ 
    public int current;
}

public enum AttackEffectType
{
    None,
    Stun,
    Slow,
    Poison
}
[Component]
public struct AttackEffectComponent
{
    public AttackEffectType effectType;
    public float duration;   // duración del efecto
    public float intensity;  // opcional: % slow, daño por tick, etc.
}
[Component]
public struct TakeHitComponent
{
    public float stunTime; // cuánto dura el "hitstun"
}
[Component]
public struct DeadComponent { }


public enum ProjectileTypeShoot
{
    Direct,   // Sigue al target (Tower Defense style)
    Physics,   // Vuela recto y usa colisión
    Homing // dirigido a la posicion no usa collision
}
[Component]
public struct ProjectileComponent
{
    public Entity source;
    public Entity target;     // solo para Direct
    public Vector2 direction; // solo para Physics
    public Vector2 initialTargetPos;   // posición inicial del target
    public float speed;
    public ProjectileTypeShoot type;
}
[Component]
public struct DamageOnHitComponent
{
    public int damage;
}
// Componente marker para proyectiles inactivos
[Component]
public struct ActiveProjectileComponent 
{
    public bool isActive;
    public bool isInitialized; // <- false al principio, true después de la primera vez
}
[Component]
public struct PlayerInputComponent
{
    public Vector2 moveDirection;
    public bool attackPressed;
    public bool attackReleased;
}

public enum MovementMode
{
    PointToPoint = 0,
    FreeWander = 1,      // nuevo modo: se mueve libremente dentro del radio
    Orbit = 2 // reservado para más adelante
}
[Component]
public struct RadiusMovementComponent
{
    public Vector2 center;           // Centro del radio
    public float desiredRadius;      // Radio máximo
    public Vector2 currentTarget;    // Objetivo actual (solo para RandomWander)
    public MovementMode mode;  // Modo de movimiento
    public bool hasTarget;           // Solo para RandomWander

    // ⏱️ cooldown para nueva búsqueda de destino
    public float movementCooldown;
    public float cooldownTimer;
}
[Component]
public struct MeleeTargetCandidateComponent
{
    // 🔹 Referencia directa al enemigo elegido
    public Entity target;

    // 🔹 Última posición conocida del enemigo al momento de asignarse
    public Godot.Vector2 lastKnownPosition;

    // 🔹 (Opcional) tiempo de validez, útil para evitar targets colgados
    public float timeToLive;
}
/// <summary>
/// Define el radio de búsqueda de enemigos para unidades melee.
/// No es la distancia de ataque, sino hasta dónde pueden detectar rivales.
/// </summary>
[Component]
public struct EnemySearchRadiusComponent
{
    public float radius;
    public float cooldownSearch; // espera actual entre búsquedas
    public int failedAttempts;          // cuántos intentos seguidos fallaron
    public float longCooldownTimer;     // temporizador para backoff largo
}
[Component]
public struct MeleeAttackComponent
{    
    public float attackCooldown;   // opcional, cooldown propio
    public MeleeAttackType attackType;
    public float attackRange;
    public int damage;

    public float cooldownTimer;
}

/// <summary>
/// Indica que la entidad debe ser destruida después de procesar sus recursos.
/// </summary>
[Component]
public struct PendingDestroyComponent { }

public enum MeleeAttackType
{
    Physics,   // Usa colliders (el actual)
    Direct     // Ataque directo al target sin colisiones
}


internal class GenericComponents
{
}
