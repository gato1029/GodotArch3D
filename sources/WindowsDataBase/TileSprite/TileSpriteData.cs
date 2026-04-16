using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs.Components;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
public class IdDataLong
{
    [BsonId]
    public long id { get; set; }
    public ushort idSave { get; set; }
    public string name { get; set; }
    public long idGrouping { get; set; }        
    public ushort idGroupingSave { get; set; }

    [BsonIgnore]
    public AtlasTexture textureVisual { get; set; }
    

    public void ReGerenateId()
    {
        id = EpochIdGenerator.NewId();
    }
    public virtual void RefreshTextureVisual()
    {
        // implementación por defecto
    }

}
public struct TileInfoKuro
{
    public int idMaterial;
    public int x;
    public int y;
    public int width;
    public int height;
    public Texture2D texture;
}

public enum TileSpriteType
{
    Static = 0,
    Animated = 1,
    AnimatedDirectionMultiple = 2,
    AnimatedMultiple = 3,
}

public class SpriteAnimationMultiple
{
    public Dictionary<string, SpriteAnimationData> animations { get; set; }
}
public class SpriteMultipleAnimationDirection
{
    public DirectionAnimationType directionAnimationType { get; set; }
    public Dictionary<string, SpriteAnimationDirection> animations { get; set; } = new Dictionary<string, SpriteAnimationDirection>();
    public Dictionary<AnimationType, SpriteAnimationDirection> animationsTypes { get; set; } = new Dictionary<AnimationType, SpriteAnimationDirection>();


}
public class SpriteAnimationDirection
{
    public DirectionAnimationType directionAnimationType { get; set; }
    public string name { get; set; }
    public AnimationType animationType { get; set; }
    public Dictionary<AnimationDirection, SpriteAnimationData> animations {get; set;}

    public SpriteAnimationDirection()
    {

    }
    public SpriteAnimationDirection(DirectionAnimationType directionAnimationType,string name)
    {
        animations = new Dictionary<AnimationDirection, SpriteAnimationData>();
        this.directionAnimationType = directionAnimationType;
        this.name = name;   
        switch (directionAnimationType)
        {
            case DirectionAnimationType.NINGUNO:
                break;
            case DirectionAnimationType.DOS:
                animations.Add(AnimationDirection.LEFT, new SpriteAnimationData());
                animations.Add(AnimationDirection.RIGHT, new SpriteAnimationData());
                break;
            case DirectionAnimationType.CUATRO:
                animations.Add(AnimationDirection.LEFT, new SpriteAnimationData());
                animations.Add(AnimationDirection.RIGHT, new SpriteAnimationData());
                animations.Add(AnimationDirection.UP, new SpriteAnimationData());
                animations.Add(AnimationDirection.DOWN, new SpriteAnimationData());
                break;
            case DirectionAnimationType.OCHO:
                animations.Add(AnimationDirection.LEFT, new SpriteAnimationData());
                animations.Add(AnimationDirection.RIGHT, new SpriteAnimationData());
                animations.Add(AnimationDirection.UP, new SpriteAnimationData());
                animations.Add(AnimationDirection.DOWN, new SpriteAnimationData());
                animations.Add(AnimationDirection.LEFTDOWN, new SpriteAnimationData());
                animations.Add(AnimationDirection.RIGHTDOWN, new SpriteAnimationData());
                animations.Add(AnimationDirection.LEFTUP, new SpriteAnimationData());
                animations.Add(AnimationDirection.RIGHTUP, new SpriteAnimationData());
                break;
            default:
                break;
        }
    }

}
public class TileSpriteData:IdDataLong
{
    //public ushort idSave { get; set; }
    public TileSpriteType tileSpriteType { get; set; }
    public SpriteData spriteData { get; set; }
    public SpriteAnimationData animationData { set; get; }

    public SpriteAnimationMultiple spriteAnimationMultiple { get; set; }

    public SpriteMultipleAnimationDirection spriteMultipleAnimationDirection { get; set; }
    public List<KuroTile> tilesOcupancy { get; set; } = new List<KuroTile>();

    public TileSpriteData()
    {
        id = EpochIdGenerator.NewId();
    }

    public string CreateUniqueId()
    {
        string raw = "";
        switch (tileSpriteType)
        {
            case TileSpriteType.Static:
                raw = $"{tileSpriteType}-" +
                    $"{spriteData.idMaterial}-" +
                    $"{spriteData.x:F3}-" +
                    $"{spriteData.y:F3}-" +
                    $"{spriteData.widht:F3}-" +
                    $"{spriteData.height:F3}-" +
                    $"{spriteData.mirrorX}-" +
                    $"{spriteData.mirrorY}";
                break;
            case TileSpriteType.Animated:
                raw = $"{tileSpriteType}-" +
                    $"{animationData.idMaterial}-" +
                    $"{animationData.framesArray[0].x:F3}-" +
                    $"{animationData.framesArray[0].y:F3}-" +
                    $"{animationData.framesArray[0].widht:F3}-" +
                    $"{animationData.framesArray[0].height:F3}-" +
                    $"{animationData.mirrorX}-" +
                    $"{animationData.mirrorY}-";

                break;
            case TileSpriteType.AnimatedDirectionMultiple:
                string temp = "";
                foreach (var item in spriteMultipleAnimationDirection.animations)
                {
                    if (item.Value.animations[AnimationDirection.LEFT].framesArray[0]!=null)
                    {
                        temp = temp + item.Value.animations[AnimationDirection.LEFT].framesArray[0].x.ToString();
                    }
                    
                }
                raw = $"{tileSpriteType}-" +
                    $"{spriteMultipleAnimationDirection.animations}"+temp;
                break;
            case TileSpriteType.AnimatedMultiple:
                break;
            default:
                break;
        }


        name = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return name;
    }

    [BsonCtor]
    public TileSpriteData(SpriteData spriteData, TileSpriteType tileSpriteType, SpriteAnimationData animationData, SpriteMultipleAnimationDirection spriteMultipleAnimationDirection) : base()
    {
        switch (tileSpriteType)
        {
            case TileSpriteType.Static:
                textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(spriteData);
                break;
            case TileSpriteType.Animated:
                textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(
                    animationData.idMaterial,
                    animationData.framesArray[0].x,
                    animationData.framesArray[0].y,
                    animationData.framesArray[0].widht,
                    animationData.framesArray[0].height);
                break;
            case TileSpriteType.AnimatedDirectionMultiple:
                SpriteAnimationData data =null;
                foreach (var item in spriteMultipleAnimationDirection.animations)
                {
                    data = item.Value.animations[AnimationDirection.LEFT];
                }
                if (data!=null)
                {
                    textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(
                      data.idMaterial,
                      data.framesArray[0].x,
                      data.framesArray[0].y,
                      data.framesArray[0].widht,
                      data.framesArray[0].height);
                }                           
                break;
            case TileSpriteType.AnimatedMultiple:
                break;
            default:
                break;
        }

    }
}

public class GroupingData : IdDataLong
{
    public int idMaterial { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int width { get; set; }
    public int height { get; set; }

    public GroupingData()
    {
        id = EpochIdGenerator.NewId();
    }

    [BsonCtor]
    public GroupingData(int idMaterial, int x, int y, int width, int height)
    {
        textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, x, y, width, height);
    }
}

