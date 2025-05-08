
using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Weapons;
public class AnimationBaseData : IdData
{
  
    public int idGroup { set; get; } // aquien le pertenece el arma
    public string pathTexture { set; get; }

    public int widhtTexture { get; set; }
    public int heightTexture { get; set; }
    public int divisionPixelX { get; set; }
    public int divisionPixelY { get; set; }

    public GeometricShape2D[] colliders { get; set; }
    public AnimationStateData[] animationDataArray { get; set; }

    [BsonIgnore]
    public Texture2D texture { set; get; }
    [BsonIgnore]
    public AtlasTexture textureMiniature { set; get; }

    public AnimationBaseData()
    { 
    }
    [BsonCtor]
    public AnimationBaseData(string pathTexture, int divisionPixelX, int divisionPixelY)
    {
        texture = (Texture2D)TextureHelper.LoadTextureLocal(FileHelper.GetPathGameDB(pathTexture));
        textureVisual = new AtlasTexture();
        textureVisual.Atlas = texture;
        textureVisual.Region = new Rect2(0,0, divisionPixelX, divisionPixelY);
    }
    
}




