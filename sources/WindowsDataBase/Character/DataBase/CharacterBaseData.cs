using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Character.DataBase
{
    public class CharacterAnimationStateData
    {
        public int id {  get; set; }
        public bool eightDirection { get; set; }
        public float frameDuration { get; set; }
        public CharacterAnimationData[] animationData { get; set; }

        public CharacterAnimationStateData( bool EightDirection = false)
        {
            eightDirection = EightDirection;
            if (eightDirection)
            {
                animationData = new CharacterAnimationData[8];
            }
            else
            {
                animationData = new CharacterAnimationData[4];
            }
            for (int i = 0; i < animationData.Length; i++)
            {
                animationData[i] = new CharacterAnimationData();
            }
        }
    }
    public class CharacterAnimationData
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
        public GeometricShape2D collisionMove { get; set; }
        public GeometricShape2D collisionBody { get; set; }

        public  CharacterColliderAtackData[] atackDataCollidersArray { get; set; }
        public CharacterAnimationData[] animationDataArray { get; set; }

        public  CharacterColliderAtackData GetColliderAtack(int id)
        {
            return  atackDataCollidersArray[id];
        }

        public  CharacterAnimationData GetAnimationData(int id)
        {
            return  animationDataArray[id];
        }
    }
}
