using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.WindowsDataBase.Character.DataBase
{
    public class AnimationStateData
    {
        public string name {  get; set; }
        public int idMaterial { get; set; }
        public DirectionAnimationType directionAnimationType { get; set; }
        public AnimationData[] animationData { get; set; }
        public AnimationStateData()
        {
        }
    }
    public class AnimationData
    {
        public int id { get; set; }
        public float frameDuration { get; set; }
        public bool loop { get; set; }
        public bool mirrorHorizontal { get; set; }
        public bool mirrorVertical { get; set; }
        public int[] idFrames { get; set; } // Cantidad de frames en la animación        
        public FrameData[] frameDataArray { get; set; }
        public bool hasCollider { get; set; }
        public GeometricShape2D collider { get; set; }
        public bool hasColliderMultiple { get; set; }
        public GeometricShape2D[] colliderMultiple { get; set; }

    }

    public class FrameData
    {
        public float xFormat { get; set; }
        public float yFormat { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float widht { get; set; }
        public float height { get; set; }
        public float widhtFormat { get; set; }
        public float heightFormat { get; set; }

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


    public class AnimationCharacterBaseData : IdData
    {
        public float zOrderingOrigin { get; set; }
        public bool hasCompositeAnimation { get; set; }

        public GeometricShape2D collisionMove { get; set; }
        public GeometricShape2D collisionBody { get; set; }

        public  CharacterColliderAtackData atackDataColliders { get; set; } // solo Si esta dentro
        public AnimationStateData[] animationDataArray { get; set; }
        public AnimationStateData[] animationExtraDataArray { get; set; }

        [BsonCtor]
        public AnimationCharacterBaseData(AnimationStateData[] animationDataArray) : base()
        {
            if (animationDataArray != null && animationDataArray.Length > 0)
            {
                if (animationDataArray!=null)
                {
                    AnimationStateData dataAnim = animationDataArray[0];
                    if (dataAnim.animationData[0].frameDataArray!=null)
                    {
                        FrameData iFrame = dataAnim.animationData[0].frameDataArray[0];
                        textureVisual = MaterialManager.Instance.GetAtlasTexture(dataAnim.idMaterial, iFrame.x, iFrame.y, iFrame.widht, iFrame.height);
                    }
                 
                }
            }


        }
        public AnimationCharacterBaseData() 
        {
         
        }

        public AnimationStateData GetAnimationData(int id)
        {
            return  animationDataArray[id];
        }
    }


}
