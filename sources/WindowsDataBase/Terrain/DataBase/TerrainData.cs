using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
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
    MazmorraBase = 20,
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
    ElevacionBase = 7,
    Ornamentos = 8,
    Limite=9,
    ElevacionBorde = 10,
    Muro = 11,    
    Mosaico = 12,
    PisoDetalle = 13,
    CaminoPiso2 = 14,
    CaminoPiso3 = 15,
    CaminoPiso4 = 16,
    CaminoPiso5 = 17,
}


public enum BiomeType
{
    SoloAgua = 0,
    SoloTerreno = 1,
    AguaYTerreno = 2,    
}
public class Biome: IdDataLong
{
    public BiomeType biomeType { get; set; }
    public long idTerrain {  get; set; }
    public long idWater { get; set; }
    public List<long> idSourcesResources { get; set; }
}
public enum TerrainTileType
{
    Agua = 0,
    TierraNivel1 = 1,
    CespedNivel1 = 2,
    TierraNivel2 = 3,
    CespedNivel2 = 4,
    AdornosAgua = 5,    
    AdornosTierra = 6,
    AdornosCesped = 7,
    AguaCamino = 8,
    TierraCamino = 9,
    TierraElevacionNivel2 = 10,
}
public partial class TerrainTileEntry:GodotObject
{
    public TerrainTileType Type { get; set; }
    public long TileId { get; set; }

    public TerrainTileEntry(TerrainTileType type, long tileId)
    {
        Type = type;
        TileId = tileId;
    }
}
public partial class ResourceEntry: GodotObject
{
    public long ResourceSourceId { get; set; }
    public float Probability { get; set; }
    public ResourceEntry(long resourceSourceId, float probability)
    {
        ResourceSourceId = resourceSourceId;
        Probability = probability;
    }
}
public class TerrainData :IdDataLong
{
    public List<TerrainTileEntry> idsAutoTileSprite { get; set; } = new();
    public Dictionary<TerrainTileType, List<ResourceEntry>> idsResources { get; set; } = new();
    public TerrainData()
    {
        id = EpochIdGenerator.NewId();
    }
   
    [BsonCtor]
    public TerrainData(List<TerrainTileEntry> idsAutoTileSprite)
    {
        foreach (var item in idsAutoTileSprite)
        {
            if (item.Type == TerrainTileType.Agua)
            {
                var auto = MasterDataManager.GetData<AutoTileSpriteData>(item.TileId);
                textureVisual = auto.textureVisual;
            }
        }
        
    }
}
