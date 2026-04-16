using Godot;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
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
    TierraNivel0 = 5, // bajo Agua
    Agua = 6,
    TierraNivel1 = 7,
    CespedNivel1 = 8,
    TierraNivel2 = 9,
    CespedNivel2 = 10,
    AdornosAgua = 11,    
    AdornosTierra = 12,
    AdornosCesped = 13,
    AguaCamino = 14,
    TierraCamino = 15,

     
}

public enum TerrinLayerType
{
    Base = 0,
    Superficie = 1,
    DecoracionBase = 2,
    DecoracionSuperficie = 3
}
public partial class TerrainLayer : GodotObject
{    
    public TerrinLayerType layerType { get; set; }
    public int layer { get; set; }
    public long idAutoTile { get; set; }

    

    public TerrainLayer(TerrinLayerType layerType, int layer, long idAutoTile)
    {
        this.layerType = layerType;
        this.layer = layer;
        this.idAutoTile = idAutoTile;
    }

    public TerrainLayer()
    {
    }
}
public partial class TerrainTileEntry:GodotObject
{
    public int height { get; set; }
    public int heightReal { get; set; }
    public List<TerrainLayer> layersRelative { get; set; } = new List<TerrainLayer>();
    public TerrainTileEntry()
    {

    }    
}
public partial class ResourceEntry: GodotObject
{
    public ushort idResourceSave { get; set;  }
    public long ResourceSourceId { get; set; }
    public float Probability { get; set; }
    public ResourceSourceType ResourceType { get; set;  }
    public ResourceEntry(long resourceSourceId, float probability, ushort idResourceSave, ResourceSourceType resourceType)
    {
        ResourceSourceId = resourceSourceId;
        Probability = probability;
        this.idResourceSave = idResourceSave;
        ResourceType = resourceType;
    }
}

public class TerrainDataTransition:IdDataLong
{
    public ushort idTerrainBeginId { get; set; }
    public ushort idTerrainEndId { get; set; }
    public ushort idTerrainResoluteId { get; set; }
    public int thickness { get; set; } // 🔥 clave
}
public class TerrainData :IdDataLong
{
    public Dictionary<int, TerrainTileEntry> terrains { get; set; } = new Dictionary<int, TerrainTileEntry>();
  
    public Dictionary<int, List<ResourceEntry>> idsElevacionResources { get; set; } = new Dictionary<int, List<ResourceEntry>>();
    public float minTemperature { get; set; }
    public float maxTemperature { get; set; }

    public float minHumidity { get; set; }
    public float maxHumidity { get; set; }    
    public bool isTransition { get; set; } // determinar si es un bioma de transcion, aqui no importa la humedad y temperatura    
    public int heightBegin { get; set; } 
 
    public bool isWater { get; set; }

    public int paddingBorder { get; set; }

    public TerrainData()
    {
        id = EpochIdGenerator.NewId();
    }
   
    [BsonCtor]
    public TerrainData(Dictionary<int, TerrainTileEntry> terrains)
    {
        foreach (var item in terrains)
        {
            var temp =item.Value;            
            var id = temp.layersRelative.First().idAutoTile;
            var auto = MasterDataManager.GetData<AutoTileSpriteData>(id);
            textureVisual = auto.textureVisual;                        
        }

    }
}


/*
=========================================
🌍 SISTEMA DE BIOMAS (TEMPERATURA / HUMEDAD)
=========================================

Cada bioma se define por un rango de:

- temperatura (0 → 1)
- humedad    (0 → 1)

El sistema NO usa rangos directamente,
usa el CENTRO del bioma:

    centerTemp     = (minTemperature + maxTemperature) * 0.5
    centerHumidity = (minHumidity + maxHumidity) * 0.5

Luego elige el bioma MÁS CERCANO usando distancia:

    dist = (temp - centerTemp)^2 + (humidity - centerHumidity)^2

-----------------------------------------
🎯 ¿Qué significa esto?

- Cada bioma es un "punto" en un mapa 2D
- temperatura = eje X
- humedad    = eje Y

-----------------------------------------
📊 REFERENCIA DEL ESPACIO

Temperatura:
0.0 → frío (nieve)
0.5 → templado
1.0 → caliente (desierto)

Humedad:
0.0 → seco
0.5 → medio
1.0 → húmedo (selva/bosque)

-----------------------------------------
🌊 AGUA (especial)

El agua NO depende de temperatura/humedad,
se define por el ruido continental (IsLand).

Configuración recomendada:
minTemperature = 0
maxTemperature = 1
minHumidity    = 0
maxHumidity    = 1

-----------------------------------------
🌲 EJEMPLO: BOSQUE

Bosque = temperatura media + humedad alta

minTemperature = 0.4
maxTemperature = 0.7

minHumidity = 0.5
maxHumidity = 1.0

Centro aproximado:
temp ≈ 0.55
humidity ≈ 0.75

-----------------------------------------
🏜️ EJEMPLO: DESIERTO

Desierto = caliente + seco

minTemperature = 0.7
maxTemperature = 1.0

minHumidity = 0.0
maxHumidity = 0.3

-----------------------------------------
🌿 EJEMPLO: PRADERA

Pradera = templado + humedad media

minTemperature = 0.4
maxTemperature = 0.7

minHumidity = 0.3
maxHumidity = 0.6

-----------------------------------------
⚠️ REGLAS IMPORTANTES

1. NO solapar demasiado biomas
   → causa cambios bruscos o incorrectos

2. Mantener separación entre centros
   → cada bioma debe tener su "espacio"

3. Pensar en centros, NO en rangos
   → el sistema usa distancia, no límites

4. Agua NO participa en este sistema

-----------------------------------------
🧠 CONSEJO PRO

Diseña los biomas como puntos en este plano:

        HUMEDAD ↑
        1.0 |    🌲 bosque
            |
        0.5 |    🌿 pradera
            |
        0.0 |    🏜️ desierto
            +----------------→ temperatura
             0.0   0.5   1.0

-----------------------------------------
*/