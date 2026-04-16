using GodotEcsArch.sources.managers.Animations;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;

namespace GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase
{
    public struct UnitMoveData
    {
        public float radiusMove { get; set; }
        public float radiusSearch { get; set; }
    }
    public class CharacterModelBaseData : IdData
    {
        public int idAnimationCharacterBaseData { get; set; }
        public long idTileSpriteData { get; set; }
        public CharacterBehaviorType characterBehaviorType { get; set; }
        public CharacterType characterType { get; set; }
        public UnitDirectionType unitDirectionType { get; set; }
        public UnitType unitType { get; set; }
        public UnitMoveType unitMoveType { get; set; }
        public UnitMoveData unitMoveData { get; set; }

        public string colorBase { set; get; }
        public string description { set; get; }
        public float scale { set; get; }
        public BonusData[] bonusDataArray { get; set; }
        public ElementsData[] damageDataArray { get; set; }
        public ElementsData[] defenseDataArray { get; set; }
        public StatsData[] statsDataArray { get; set; }
        public GeometricShape2D collisionMove { get; set; }
        public GeometricShape2D collisionBody { get; set; }
        [BsonIgnore]
        public AnimationCharacterBaseData animationCharacterBaseData { get; set; }
        public CharacterModelBaseData()
        {
        }

        [BsonCtor]
        public CharacterModelBaseData( int idAnimationCharacterBaseData, long idTileSpriteData) : base()
        {
            if (idTileSpriteData!=0)
            {
                var data = MasterDataManager.GetData<TileSpriteData>(idTileSpriteData);
                textureVisual = data.textureVisual;
            }
            

            //animationCharacterBaseData = AnimationCharacterManager.Instance.GetCharacterBaseData(idAnimationCharacterBaseData); 

            //AnimationStateData[] animationDataArray = animationCharacterBaseData.animationDataArray;
            //if (animationDataArray != null && animationDataArray.Length > 0)
            //{
            //    if (animationDataArray != null)
            //    {
            //        AnimationStateData dataAnim = animationDataArray[0];
            //        if (dataAnim.animationData[0].frameDataArray != null)
            //        {
            //            FrameData iFrame = dataAnim.animationData[0].frameDataArray[0];
            //            textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(dataAnim.idMaterial, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
            //        }

            //    }
            //}


        }
    }
}
