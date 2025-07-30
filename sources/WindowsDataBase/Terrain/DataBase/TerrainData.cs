using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;
namespace GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;

public enum TerrainCategoryType
{
    Generico = 0,
    Bosque = 1,
}

public enum TerrainType
{
    Individual = 0, // no Cuenta para el generador
    PisoBase = 1, // tierra, cesped, arena
    Elevacion = 2,    
    Agua = 3,
    CaminoPiso = 4,
    CaminoAgua = 5,     
    AguaBorde = 6,
    ElevacionBase = 7
}
public class TerrainData :IdData
{
    public bool isRule {  get; set; } 
    public bool isAnimated { get; set; }
    public TerrainType terrainType { get; set; }
    public string category { get; set; }
    public SpriteData spriteData { get; set; }
    public List<AnimationStateData> animationData { get; set; } = null; // Datos de la animación, si aplica
    public RuleData[] rules { get; set; }

    public TerrainData()
    {
    }
   
    [BsonCtor]
    public TerrainData(SpriteData spriteData)
    {        
        textureVisual = MaterialManager.Instance.GetAtlasTextureInternal(spriteData);                
    }
}
