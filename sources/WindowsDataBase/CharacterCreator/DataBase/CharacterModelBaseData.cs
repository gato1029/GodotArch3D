using GodotEcsArch.sources.managers.Animations;
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

        [BsonIgnore]
        public AnimationCharacterBaseData animationCharacterBaseData { get; set; }
        public CharacterModelBaseData()
        {
        }

        [BsonCtor]
        public CharacterModelBaseData( int idAnimationCharacterBaseData) : base()
        {
            animationCharacterBaseData = AnimationCharacterManager.Instance.GetCharacterBaseData(idAnimationCharacterBaseData); 

            AnimationStateData[] animationDataArray = animationCharacterBaseData.animationDataArray;
            if (animationDataArray != null && animationDataArray.Length > 0)
            {
                if (animationDataArray != null)
                {
                    AnimationStateData dataAnim = animationDataArray[0];
                    if (dataAnim.animationData[0].frameDataArray != null)
                    {
                        FrameData iFrame = dataAnim.animationData[0].frameDataArray[0];
                        textureVisual = MaterialManager.Instance.GetAtlasTexture(dataAnim.idMaterial, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
                    }

                }
            }


        }
    }
}
