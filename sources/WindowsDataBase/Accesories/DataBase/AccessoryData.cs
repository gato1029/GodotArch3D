using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using LiteDB;

namespace GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

public class AccessoryData : IdData
{
    public string description {  get; set; }
    public string colorBase { set; get; }
    public bool hasBodyAnimation { set; get; }
    public bool hasAnimationTile { set; get; }
    public bool hasRequirements { set; get; }
    public MiniatureData miniatureData { set; get; }
    public AccesoryClassType accesoryClassType { set; get; }
    public AccesoryType accesoryType { set; get; }
    public AccesoryBodyPartType accesoryBodyPartType { set; get; }
    public int idBodyAnimationBaseData { set; get; }    
    public AnimationTilesData animationTilesData { set; get; }
    
    public BonusData[] bonusDataArray { get; set; }
    public ElementsData[] damageDataArray { get; set; }
    public ElementsData[] defenseDataArray { get; set; }
    public StatsData[] statsDataArray { get; set; }    
    public RequirementsData requirementsData { get; set; }

    public AccessoryData() 
    {
    
    }

    [BsonCtor]
    public AccessoryData(MiniatureData miniatureData) : base()
    {
        if (miniatureData.idMaterial>0 && miniatureData.idTile>0)
        {
            var tile =DataBaseManager.Instance.FindById<TileDynamicData>(miniatureData.idTile);
            textureVisual = tile.textureVisual;
        }
        
    }
}

public struct MiniatureData {
    public int idMaterial { get; set; }
    public int idTile { get; set; } 
}
public class AnimationTilesData {
    public bool loop { set; get; }
    public bool mirrorHorizontal { set; get; }
    public float frameDuration {  set; get; }
    public int idMaterialTiles { get; set; }
    public int[] idFrames { get; set; }    
    public bool hasCollider { get; set; }
    public GeometricShape2D collider { get; set; }
}
public class RequirementsData
{
    public StatsData[] statsDataArray { get; set; }
    public int level { get; set; }
}
public class ProjectileData
{
    public bool hasAnimation { get; set; }
    public int idMaterialTiles {  set; get; }
    public int[] idTiles { get; set; }
    public ProjectileType projectileType { set; get; }
    public string colorBase { set; get; }
    public ElementsData damageData { get; set; }
}

public struct StatsData
{
    public StatsType type { get; set; }
    public int value { get; set; }
}
public struct ElementsData
{
    public ElementType type { get; set; }
    public float value { get; set; }
}
public struct BonusData
{
    public BonusType type { get; set; }
    public float value { get; set; }
}

public class WeaponCommonData : IdData
{
    public int idInstanceMiniature { set; get; }
    public WeaponType weaponType { set; get; }
    public int idAnimationBaseData { set; get; }
    [BsonIgnore]
    public AnimationBaseData weaponBaseData { set; get; }
    public string colorBase { set; get; }
    public float speedAtack { get; set; }
    public float durability { get; set; }
    public StatsData[] damageData { get; set; }
}