using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Building.DataBase;

public enum CostInputType
{
    Recurso,   // Ej: oro, comida, energía
    Material    // Ej: lingote, tablón, ladrillo
}
public enum BuildingType
{
    Ninguno = 0,
    ProductorMaterial = 1,
    ProductorUnidades = 2,
    TorreDefensa = 3, // Edificio de defensa, como una torre
    Procesador = 4,
}
public class CostInput
{
    public int id; // ID del coste, puede ser el ID de un recurso o material
    public CostInputType costInputType; // Tipo: recurso o material
    public int amount;  
}
public class MaterialOutput
{
    public int id; // ID del material producido
    public int amount;
    public float ProductionTime; // Tiempo de producción en segundos
}
public class UnitOutput
{
    public int id; // ID de la unidad Producida
    public int amount;
    public float ProductionTime; // Tiempo de producción en segundos
}

public class BuildingTilePosition
{
    public int X { get; set; }
    public int Y { get; set; }

    public BuildingTilePosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class BuildingPosition
{ 
    public int centerX { get; set; }
    public int centerY { get; set; }
    public int sizeGridX { get; set; }
    public int sizeGridY { get; set; }
    public List<BuildingTilePosition> buildingTilePositions { get; set; } // posicion de construccion
}
public class BuildingData:IdData
{
    public BuildingPosition buildingPosition { get; set; } // posicion de construccion
    public SpriteData miniatura { get; set; } // Miniatura del edificio, puede ser una textura o atlas
    public SpriteData spriteData { get; set; } // Datos del sprite del edificio, puede ser una textura o atlas
    public List<AnimationStateData> animationData { get; set; } = null; // Datos de la animación del edificio, si aplica
    public int level { get; set; } = 1; // Nivel del edificio, por defecto 1
    public int maxHealth { get; set; } = 100; // Salud máxima del edificio, por defecto 100    
    public float attackRange { get; set; } = 0f; // Rango de ataque, por defecto 0 (sin ataque)
    public float attackCooldown { get; set; } = 0f; // Tiempo de recarga del ataque, por defecto 0 (sin ataque)
    public string description { get; set; }
    public BuildingType buildingType { get; set; } = BuildingType.Ninguno;    
    public float timeToBuild { get; set; } // Tiempo hasta terminar la construccion     
    public bool IsBuilt { get; set; } = false; // Indica si el edificio ya ha sido construido
    public List<CostInput> constructionCost { get; set; } = null; // Costo de construccion
    //public List<CostInput> OperationCost; // Costo de operacion, si es necesario
    public List<MaterialOutput> materialOutputs { get; set; } = null; // Materiales producidos por el edificio
    public List<UnitOutput> unitOutputs { get; set; } = null; // Unidades producidas por el edificio
    public int idBuildingUpgrade { get; set; } = 0; // ID del edificio al que se puede mejorar, 0 si no hay mejora disponible
    public List<ElementsData> attackPowers { get; set; } = null; // Poder de ataque del edificio, si aplica
    public List<ElementsData> defensePowers { get; set; } = null; // Poder de defensa del edificio, si aplica

    public BuildingData()
    {
    }
    [BsonCtor]
    public BuildingData(SpriteData miniatura)
    {
        if (miniatura!= null &&miniatura.idMaterial>0)
        {
            textureVisual = MaterialManager.Instance.GetAtlasTexture(miniatura.idMaterial, miniatura.x, miniatura.y, miniatura.widht, miniatura.height);
        }
        
    }
}
