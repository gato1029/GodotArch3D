using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Character.DataBase
{
    public class AnimationStateData
    {
        public int id {  get; set; }

        public int idMaterial { get; set; }
        public DirectionAnimationType directionAnimationType { get; set; }
        public bool eightDirection { get; set; }
        public float frameDuration { get; set; }
        public bool loop {  get; set; }
        public bool mirrorHorizontal { get; set; }
        public bool mirrorVertical { get; set; }
        public AnimationData[] animationData { get; set; }

        public AnimationStateData( bool EightDirection = false)
        {
            eightDirection = EightDirection;
            if (eightDirection)
            {
                animationData = new AnimationData[8];
            }
            else
            {
                animationData = new AnimationData[4];
            }
            for (int i = 0; i < animationData.Length; i++)
            {
                animationData[i] = new AnimationData();
            }
        }
    }
    public class AnimationData
    {
        public int id { get; set; }
        public int[] idFrames { get; set; } // Cantidad de frames en la animación        
        public FrameData[] frameDataArray { get; set; }
        public bool hasCollider { get; set; }
        public GeometricShape2D collider { get; set; }
        public bool hasColliderMultiple { get; set; }
        public GeometricShape2D[] colliderMultiple { get; set; }

    }

    public class FrameData
    {
        public float x { get; set; }
        public float y { get; set; }
        public float widht { get; set; }
        public float height { get; set; }

    }
    public class CharacterColliderAtackData
    {
        public int id { get; set; }
        public GeometricShape2D[] colliders { get; set; }
     
        public CharacterColliderAtackData()
        {
            colliders = new GeometricShape2D[4];
        }
     
    }


    public class CharacterBaseData : IdData
    {
        public int idMaterial { get; set; }
        public int idExtraMaterial { get; set; }
        public int idGroup {  get; set; }
        public float zOrderingOrigin { get; set; }
        public bool hasCompositeAnimation { get; set; }

        public GeometricShape2D collisionMove { get; set; }
        public GeometricShape2D collisionBody { get; set; }

        public  CharacterColliderAtackData atackDataColliders { get; set; } // solo Si esta dentro
        public AnimationStateData[] animationDataArray { get; set; }
        public AnimationStateData[] animationExtraDataArray { get; set; }

        [BsonCtor]
        public CharacterBaseData(AnimationStateData[] animationDataArray, int idMaterial) : base()
        {
            if (animationDataArray != null&& animationDataArray.Length>0)
            {
                if (animationDataArray[0] != null && animationDataArray[0].animationData[0] != null && animationDataArray[0].animationData[0].idFrames!=null)
                {
                    int idInternalPosition = animationDataArray[0].animationData[0].idFrames[0];
                    textureVisual = MaterialManager.Instance.GetAtlasTexture(idMaterial, idInternalPosition);
                }
            }
          
            
        }
        public CharacterBaseData() 
        {
         
        }

        public AnimationStateData GetAnimationData(int id)
        {
            return  animationDataArray[id];
        }
    }

    public class CharacterCustomData 
    {
        public int idWeaponTextureAnimation;
        public int idShieldAnimation;
        //public int idWeaponAnimation;
        //public int idWeaponAnimation;
        //public int idWeaponAnimation;
    }
    public class CharacterModelBaseData :IdData
    {
        public int idCharacterBaseData { get; set; }
        public string colorBase { set; get; }
        public float scale {  set; get; }
        public BonusData[] bonusDataArray { get; set; }
        public ElementsData[] damageDataArray { get; set; }
        public ElementsData[] defenseDataArray { get; set; }
        public StatsData[] statsDataArray { get; set; }
    }
}
