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

namespace GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
public class ResourceData:IdData
{    
    public string description { get; set; }
    public int amount { get; set; }
    public bool isAnimated { get; set; }
    public BuildingPosition buildingPosition { get; set; } // posicion de construccion
    public SpriteData spriteData { get; set; }
    public SpriteData miniatura { get; set; } // Miniatura del edificio, puede ser una textura o atlas
    public List<AnimationStateData> animationData { get; set; } = null; // Datos de la animación, si aplica
    public ResourceData()
    { 
    }

    [BsonCtor]
    public ResourceData(SpriteData miniatura)
    {
        if (miniatura != null)
        {
            textureVisual = MaterialManager.Instance.GetAtlasTexture(miniatura.idMaterial, miniatura.x, miniatura.y, miniatura.widht, miniatura.height);
        }        
    }
}
