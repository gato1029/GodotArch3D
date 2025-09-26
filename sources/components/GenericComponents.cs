using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;

namespace GodotEcsArch.sources.components;

public enum AnimationDirection
{
    LEFT,
    RIGHT,
    UP,
    DOWN,
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
}
[Component]
public struct RenderGPUComponent
{
    public Rid rid;
    public int instance;
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
public struct RangeComponent
{
    public float value;
}

[Component]
public struct TargetingComponent
{
    public Entity targetEntity;
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
}
internal class GenericComponents
{
}
