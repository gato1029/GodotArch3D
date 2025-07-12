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
    public SpriteData miniatureData { set; get; }
    public AccesoryClassType accesoryClassType { set; get; }
    public AccesoryType accesoryType { set; get; }
    public AccesoryBodyPartType accesoryBodyPartType { set; get; }
    public int idBodyAnimationBaseData { set; get; }    
    public SpriteAnimationData animationTilesData { set; get; }
    
    public BonusData[] bonusDataArray { get; set; }
    public ElementsData[] damageDataArray { get; set; }
    public ElementsData[] defenseDataArray { get; set; }
    public StatsData[] statsDataArray { get; set; }    
    public RequirementsData requirementsData { get; set; }

    [BsonIgnore]
    public AccesoryAnimationBodyData accesoryAnimationBodyData { set; get; }

    public AccessoryData() 
    {
    
    }

    [BsonCtor]
    public AccessoryData(SpriteData miniatureData, int idBodyAnimationBaseData) : base()
    {
        if (miniatureData!=null && miniatureData.idMaterial>0 )
        {
            
            var atlas = MaterialManager.Instance.GetAtlasTexture(miniatureData.idMaterial, miniatureData.x, miniatureData.y, miniatureData.widht, miniatureData.height);            
            textureVisual = atlas;
        }
        accesoryAnimationBodyData = DataBaseManager.Instance.FindById<AccesoryAnimationBodyData>(idBodyAnimationBaseData);
    }
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

