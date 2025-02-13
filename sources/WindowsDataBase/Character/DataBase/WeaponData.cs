using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Character.DataBase;

public class StatsDamageData
{   
    public float fireDamage { get; set; }
    public float waterDamage { get; set; }
    public float airDamage { get; set; }
    public float earthDamage { get; set; }
    public float darkDamage { get; set; }
    public float lightDamage { get; set; }
}

public class StatsResistData
{
    public float baseResist { get; set; }
    public float fireResist { get; set; }
    public float waterResist { get; set; }
    public float airResist { get; set; }
    public float earthResist { get; set; }
    public float darkResist { get; set; }
    public float lightResist { get; set; }
}
public class WeaponData : IdData
{
    public int idMaterial { set; get; }
    public int idCharacterBase { set; get; }

    public string colorBase { set; get; }
    public float baseDamage { get; set; }
    public float speedAtack { get; set; }
    public float durability { get; set; }
    public StatsDamageData damageData { get; set; }
    public AnimationStateData[] animationStateDataArray { get; set; }
}

public class ShieldData : IdData
{
    public int idMaterial { set; get; }
    public int idCharacterBase { set; get; }
    public string colorBase { set; get; }
    public float baseResist { get; set; }
    StatsResistData statsResistData { get; set; }
}

public class BulletData: IdData
{
    public int idMaterial { set; get; }
    public int idWeapon { set; get; }
    public string colorBase { set; get; }
    public float baseDamage { get; set; }
    public float speedMove { get; set; }
    public float distanceMax { get; set; }
    public int impactMax { get; set; }
    public AnimationData[] animationDataArray { get; set; }
}
