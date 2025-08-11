using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
public enum ResourceSourceType
{
    Arboles = 0,
    MinaOro = 1,
    Piedras = 2,
}
public class ResourceSourceData:IdData
{
    public string description { get; set; }
    public ResourceSourceType resourceSourceType { get; set; }
    public int amount { get; set; }             // Cuánto puede dar en total (ej. 3 de madera)        
    public int idResourceProduce { get; set; }
    public bool isAnimated { get; set; }    
    public BuildingPosition buildingPosition { get; set; } // posicion de construccion
    public SpriteData spriteData { get; set; }    
    public List<AnimationStateData> animationData { get; set; } = null; // Datos de la animación, si aplica
    public ResourceSourceData()
    {

    }

    [BsonCtor]
    public ResourceSourceData(SpriteData spriteData)
    {
        if (spriteData != null)
        {
            textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(spriteData.idMaterial, spriteData.x, spriteData.y, spriteData.widht, spriteData.height);
        }
    }
}
