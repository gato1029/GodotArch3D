using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ColliderType 
{
    RECTANGLE,
    CIRCLE
}

public enum AccesoryBodyPartType
{
    NONE,
    HEAD,
    LEFT_HAND,
    RIGHT_HAND,
    BOTH_HAND,
    FINGER,
    LEGS,
    FOOTS,
    BODY,
}

public enum AccesoryClassType
{
    NONE,
    PROJECTILE,
    WEAPON,
    SHIELD,
    CLOTHES,
    JEWELS
}
public enum WeaponType
{
    SWORD,
    BOW,
    STAFF
}
public enum BonusType
{
    DURABILITY,
    VELOCITY_ATTACK,
    SPACE_BAG,
}
public enum ElementType
{
    BASE,
    AIR,
    WATER,
    DARK,
    LIGHT,
    FIRE,
    EARTH
}
public enum StatsType
{    
  AGILITY,
  STRENGTH,
  INTELIGENCI,

}
public enum ProjectileType
{
    ARROW,
    SPELL,
}
public enum AccesoryType
{
    NONE,
    ARROW,
    SWORD,
    LONGSWORD,
    SHIELD,
}
public enum AnimationAction
    {
        NONE=-1,
        IDLE_WEAPON,
        WALK,
        DEATH,                
        RUN,
        STUN,        
        HIT,        
        CASTER,
        ATACK,
        ATACK2,
        ATACK3,
        ATACK4
}
internal class PublicEnums
{
}
