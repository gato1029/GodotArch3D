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

namespace GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase
{
    public struct UnitMoveData
    {
        public float radiusMove { get; set; }
        public float radiusSearch { get; set; }
    }
    public class CharacterModelBaseData : IdDataLong
    {        
        public long idTileSpriteData { get; set; }
        public BehaviorType characterBehaviorType { get; set; }
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
}
