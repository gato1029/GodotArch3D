using GodotEcsArch.sources.managers.Collision;
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
        public bool eightDirection { get; set; }
        public float frameDuration { get; set; }
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
        public int idCharacterBase {  get; set; }
        public float healthBase { get; set; }
        public float damageBase { get; set; }
        public string colorBase { set; get; }

        public GeometricShape2D collisionMove { get; set; }
        public GeometricShape2D collisionBody { get; set; }

        public  CharacterColliderAtackData[] atackDataCollidersArray { get; set; }
        public AnimationStateData[] animationDataArray { get; set; }

        [BsonCtor]
        public CharacterBaseData(AnimationStateData[] animationDataArray, int idMaterial) : base()
        {
            if (animationDataArray != null && animationDataArray[0]!=null && animationDataArray[0].animationData[0] !=null)
            {
                int idInternalPosition = animationDataArray[0].animationData[0].idFrames[0];
                textureVisual = MaterialManager.Instance.GetAtlasTexture(idMaterial, idInternalPosition);
            }
            
        }
        public CharacterBaseData() 
        {
         
        }
        public  CharacterColliderAtackData GetColliderAtack(int id)
        {
            return  atackDataCollidersArray[id];
        }

        public AnimationStateData GetAnimationData(int id)
        {
            return  animationDataArray[id];
        }
    }
}
