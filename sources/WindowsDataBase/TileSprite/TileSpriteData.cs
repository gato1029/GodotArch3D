using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
public class IdDataLong
{
    [BsonId]
    public long id { get; set; }
    public string name { get; set; }
    public long idGrouping { get; set; }
    [BsonIgnore]
    public AtlasTexture textureVisual { get; set; }

    public void ReGerenateId()
    {
        id = EpochIdGenerator.NewId();
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
    Animated = 1
}
public class TileSpriteData:IdDataLong
{    
 //   public bool isAnimated { get; set; }
    public TileSpriteType tileSpriteType { get; set; }
    public SpriteData spriteData { get; set; }
    public SpriteAnimationData animationData { set; get; }
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
            default:
                break;
        }
  

        name = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return name;
    }

    [BsonCtor]
    public TileSpriteData(SpriteData spriteData, TileSpriteType tileSpriteType, SpriteAnimationData animationData) : base()
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

