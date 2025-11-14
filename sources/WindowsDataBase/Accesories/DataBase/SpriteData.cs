using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using LiteDB;
using System.Linq;

namespace GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

public class SpriteData {
    
    public int idMaterial { get; set; }
    public bool haveCollider { get; set; }    
    public float x { get; set; }
    public float y { get; set; }
    public float xFormat { get; set; }
    public float yFormat { get; set; }
    public float widht { get; set; }
    public float height { get; set; }
    public float widhtFormat { get; set; }
    public float heightFormat { get; set; }
    public GeometricShape2D collisionBody { get; set; } // esto ya no se usa, lo dejo por compatibilidad
    public GeometricShape2D[] listCollisionBody{get; set;}
    public float scale { get; set; }
    public float offsetX { get; set; }
    public float offsetY { get; set; }
    public bool mirrorX { get; set; }
    public bool mirrorY { get; set; }
    public string colorString { get; set; } // Color en formato hexadecimal, por ejemplo: "#FF0000" para rojo
    [BsonIgnore]
    public Vector2 offsetInternal { get; set; }
    [BsonIgnore]
    public Color color { get; set; }
    public float yDepthRender { get; set; } // Valor para ajustar la profundidad de renderizado en el eje Y
    
    [BsonIgnore]
    public Color uv { get; set; }
    public SpriteData()
    {

    }

    [BsonCtor]
    public SpriteData(string colorString, float offsetX, float offsetY)
    {
        offsetInternal = new Godot.Vector2(MeshCreator.PixelsToUnits(offsetX), MeshCreator.PixelsToUnits(offsetY));
        if (colorString != null)
        {
            var components = colorString.Trim('(', ')')
                       .Split(',')
                       .Select(s => float.Parse(s.Trim()))
                       .ToArray();
            color = new Color(components[0], components[1], components[2], components[3]);
        }
    }

    public Color GetUv()
    {
        return TextureHelper.GetUvFormatFromSprite(this);
    }
}

