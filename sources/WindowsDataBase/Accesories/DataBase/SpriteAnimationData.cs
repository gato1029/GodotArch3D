using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

public class SpriteAnimationData {    
    public bool haveCollider { get; set; }
    public GeometricShape2D collisionBody { get; set; }
    public List<GeometricShape2D> collisionBodyArray { get; set; }    
    public int idMaterial { get; set; }        
    public float scale { get; set; }
    public float offsetX { get; set; }
    public float offsetY { get; set; }

    public bool mirrorX { get; set; }
    public bool mirrorY { get; set; }
     
    public float yDepthRender { get; set; }
    
    public string colorString { get; set; } // Color en formato hexadecimal, por ejemplo: "#FF0000" para rojo
    [BsonIgnore]
    public float yDepthRenderFormat { get; set; }

    [BsonIgnore]
    public Vector2 offsetInternal { get; set; }
    [BsonIgnore]
    public Color color { get; set; }

    public FrameData[] framesArray { get; set; }
    public bool loop { set; get; }    
    public float frameDuration {  set; get; }
    public string idModMaterial { get; set; }

    [BsonIgnore]
    public Color[] uvFramesArray { get; set; }

    [BsonIgnore]
    public Dictionary<string, GeometricShape2D> collisionBodyDictionary { get; set; } = new Dictionary<string, GeometricShape2D>(); // esto es para darle un tag a la animacion
    public SpriteAnimationData()
    {

    }
    [BsonCtor]
    public SpriteAnimationData(string colorString, float offsetX, float offsetY, int idMaterial, bool mirrorX, bool mirrorY, FrameData[] framesArray, float yDepthRender, List<GeometricShape2D> collisionBodyArray)
    {
        yDepthRenderFormat = MeshCreator.PixelsToUnits(yDepthRender);
        offsetInternal = new Godot.Vector2(MeshCreator.PixelsToUnits(offsetX), MeshCreator.PixelsToUnits(offsetY));
        if (colorString != null)
        {
            var components = colorString.Trim('(', ')')
                       .Split(',')
                       .Select(s => float.Parse(s.Trim()))
                       .ToArray();
            color = new Color(components[0], components[1], components[2], components[3]);
        }
        uvFramesArray = TextureHelper.GetUvAllFormatFromFrames(idMaterial, mirrorX, mirrorY, framesArray).ToArray();
        this.yDepthRenderFormat = yDepthRenderFormat;

        if (collisionBodyArray!=null)
        {
            foreach (var item in collisionBodyArray)
            {
                if (item.name != null)
                {
                    collisionBodyDictionary.Add(item.name, item);
                }
            }
        }
        
    }

}

