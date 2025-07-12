using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum WindowType
{
    SELECTED, CREATOR
}
public enum CharacterType
{
    MAIN,
    NPC,
    ENEMIGO
}
public enum CharacterBehaviorType   
{
    NINGUNO,
    PERSONAJE_PRINCIPAL,
    GENERICO
}
public enum UnitMoveType
{
   ALERTA,
   MOVIMIENTO_RADIO_FIJO,
   BUSQUEDA_RADIO_FIJO,
   MOVIMIENTO_RADIO_VARIABLE,
   BUSQUEDA_RADIO_VARIBLE,
}
public enum UnitType
{
    TERRESTRE,
    AEREO,
    ACUATICO
}
public enum UnitDirectionType
{
    DOS,
    CUATRO,
    OCHO
}
public enum AccesoryAvatarType
{
    WEAPON,
    HEAD,  
    FINGER,
    LEGS,
    FOOTS,
    BODY,
}

public enum DirectionArrowArray 
{
    REMOVE,UP, DOWN
}
public enum DirectionAnimationType
{
    NINGUNO,
    DOS,
    CUATRO,
    OCHO
}
public enum PositionAnimationType
{
    IZQUIERDA,
    DERECHA,
    ARRIBA,
    ABAJO,
    IZQUIERDA_ARRIBA,
    DERECHA_ARRIBA,
    IZQUIERDA_ABAJO,
    DERECHA_ABAJO,
    CENTRO
}




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
    VELOCITY_MOVE,
    RANGO_ATAQUE_EDIFICIOS,
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
