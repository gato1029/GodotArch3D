using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Animations;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotFlecs.sources.Flecs.Components;
using LiteDB;
using SadRogue.Primitives;
using System.Collections.Generic;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;

namespace GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
public enum UnitRoleType
{
    NINGUNO,               // Enemigos base o personajes sin rol de trabajo
    CONSTRUCTOR,
    GUERRERO,
    ARQUERO,
    RECOLECTOR_TRABAJADOR
}
public struct BuilderTemplateConfig
{
    public float BuildSpeedMultiplier { get; set; }
    public float BuildRange { get; set; }
    public long[] AllowedStructureIds { get; set; }
}

public struct WarriorTemplateConfig
{
    public float MeleeRange { get; set; }
    public float AttackCooldown { get; set; }
}

public struct ArcherTemplateConfig
{
    public int ProjectileId { get; set; }
    public float AttackRange { get; set; }
    public float MinAttackRange { get; set; }
}

public struct GathererTemplateConfig
{
    public int CarryCapacity { get; set; }
    public float GatherSpeedMultiplier { get; set; }
    public float GatherRange { get; set; }
    public long[] AllowedResourceIds { get; set; }
}
    public class UnitEggData : IdDataLong
    {
        public string eggName { get; set; }
        public string description { get; set; }
        public long idTileSpriteData { get; set; }

        // Referencia al CharacterModelBaseData que saldrá al eclosionar
        public long targetCharacterId { get; set; }

        // Reglas de creación / incubación
        public float hatchTimeSeconds { get; set; }
        public int populationCost { get; set; }
        public CostInput[] costInputs { get; set; }

        public UnitEggData()
        {
            id = EpochIdGenerator.NewId();
        }
    }
    
    public struct UnitMoveData
    {
        public float radiusMove { get; set; }
        public float radiusSearch { get; set; }
    }
    public class CharacterModelBaseData : IdDataLong
    {        
        public long idTileSpriteData { get; set; }
        public BehaviorType characterBehaviorType { get; set; }
        public UnitRoleType unitRoleType { get; set; }
        public CharacterType characterType { get; set; }
        public UnitDirectionType unitDirectionType { get; set; }
        public UnitType unitType { get; set; }
        public UnitMoveType unitMoveType { get; set; }
        public UnitMoveData unitMoveData { get; set; }
        public UnitAttackType unitAttackType { get; set; }
        public int idProjectile { get; set; }
        public string colorBase { set; get; }
        public string description { set; get; }
        public float scale { set; get; } // si
        public BonusData[] bonusDataArray { get; set; }
        public ElementsData[] damageDataArray { get; set; }
        public ElementsData[] defenseDataArray { get; set; }
        public StatsData[] statsDataArray { get; set; }
        public bool isPersist { get; set; }

        [BsonIgnore]
        public List<FastCollider> bodyColliders { get; set; }
        [BsonIgnore]
        public FastCollider colliderUmbralAtaque { get; set; }
        public CharacterModelBaseData()
        {
            id = EpochIdGenerator.NewId();
        }
        public void SetBodyCollider(GeometricShape2D shape2D)
        {
            FastCollider fastCollider = new FastCollider();            
            switch (shape2D)
            {
                case Circle circle:
                    fastCollider.Shape = ShapeType.Circle;
                    fastCollider.Width = circle.Radius;
                    fastCollider.Height = circle.Radius;
                    fastCollider.Offset = new Vector2(circle.OriginCurrent.X, circle.OriginCurrent.Y);
                    break;
                case managers.Collision.Rectangle rectangle:
                    fastCollider.Shape = ShapeType.Rect;
                    fastCollider.Width = rectangle.Width;
                    fastCollider.Height = rectangle.Height;
                    fastCollider.Offset = new Vector2(rectangle.OriginCurrent.X, rectangle.OriginCurrent.Y);
                    break;
                case Slope slope:
                    fastCollider.Shape = ShapeType.Slope;
                    fastCollider.Slope = slope.slopeType;
                    fastCollider.Width = slope.Width;
                    fastCollider.Height = slope.Height;
                    fastCollider.Offset = new Vector2(slope.OriginCurrent.X, slope.OriginCurrent.Y);
                    break;
                default:
                    break;
            }
            if (bodyColliders == null)
            {
                bodyColliders = new List<FastCollider>();
            }
            bodyColliders.Add(fastCollider);
        }
        [BsonCtor]
        public CharacterModelBaseData(long idTileSpriteData,float scale) : base()
        {
            if (idTileSpriteData!=0)
            {
                AtlasModsManager.GetSpriteUniqueId(idTileSpriteData, out var data);
                textureVisual = data.textureVisual;
                var MoveData = data.spriteMultipleAnimationDirection.animationsTypes[AnimationType.PARADO].animations[GodotEcsArch.sources.components.AnimationDirection.LEFT];
                foreach (var item in MoveData.collisionBodyArray)
                {
                    if (item.collisionUseType == CollisionUseType.CUERPO)
                    {
                        SetBodyCollider(item.Multiplicity(scale));
                    }
                }
                colliderUmbralAtaque = data.fastColliderUmbralAtaque;
            }            
        }
    }

