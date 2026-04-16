using GodotEcsArch.sources.utils;
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
public enum ResourceType
{
    Recurso,   // Ej: oro, comida, energía
    Material    // Ej: lingote, tablón, ladrillo
}

public enum ResourceSourceType
{
    Arbol = 0,
    MinaOro = 1,
    Piedras = 2,
}


public class ResourceSourceData:IdDataLong
{
    public string description { get; set; }
    public ResourceSourceType resourceSourceType { get; set; }
    public int amount { get; set; }  // Cuánto puede dar en total (ej. 3 de madera)        
    public int health { get; set; } // Salud de la fuente de recurso (ej. 100 de salud)
    public bool isExploitable { get; set; } = false; // Indica si la fuente de recurso se puede explotar o no
    public int idMaterialResourceProduce { get; set; } // ID del material que produce (ej. ID del material madera)
    public List<long> listIdTileSpriteData { get; set; } = new List<long>(); // Datos de los sprites asociados a la fuente de recurso

    public ResourceSourceData()
    {
        id = EpochIdGenerator.NewId();
    }

    [BsonCtor]
    public ResourceSourceData(List<long> listIdTileSpriteData)
    {
        if (listIdTileSpriteData != null && listIdTileSpriteData.Count>0)
        {
            textureVisual = MasterDataManager.GetData<TileSpriteData>(listIdTileSpriteData[0]).textureVisual;
        }
    }
}
