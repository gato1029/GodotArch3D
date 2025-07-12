using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using LiteDB;

namespace GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

public class SpriteAnimationData {
    public bool haveCollider { get; set; }
    public GeometricShape2D collisionBody { get; set; }
    public int idMaterial { get; set; }    
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

    public FrameData[] framesArray { get; set; }
    public bool loop { set; get; }
    //public bool mirrorHorizontal { set; get; }
    public float frameDuration {  set; get; }
                 
}

