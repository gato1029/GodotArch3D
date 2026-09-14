using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotFlecs.sources.Flecs.Components;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.WindowsDataBase.Building.DataBase;


public enum BuildingType
{
    Ninguno = 0,
    ProductorMaterial = 1,
    ProductorUnidades = 2,
    ProductorRecurso = 3,
    Torres = 4,           
    Procesador = 5,
    GeneradorMejoras = 6,
    Adorno = 7,
    Murallas = 8,
}

public class CostInput
{
    public long Id { get; set; } // ID del recurso o material requerido
    public ResourceType CostInputType { get; set; } // Tipo: recurso o material
    public int Amount { get; set; }  // Cantidad necesaria (ej. 3 palos, 4 de oro)
}

public class ProductionOutput
{
    public long Id { get; set; } // ID de la unidad o material producido
    public int AmountProduced { get; set; } // Cantidad que se obtiene al finalizar
    public float ProductionTime { get; set; } // Tiempo de producción en segundos
    public List<CostInput> ProductionCost { get; set; } = new(); // Insumos necesarios para producirlo (ej. 3 palos, 4 oro)
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

// esto ya no se usa
public class BuildingPosition
{ 
    public int centerX { get; set; }
    public int centerY { get; set; }
    public int sizeGridX { get; set; }
    public int sizeGridY { get; set; }
    public List<BuildingTilePosition> buildingTilePositions { get; set; } // posicion de construccion
}
public class BuildingData : IdDataLong
{
    public long IdTileSpriteNormal { get; set; } // Edificio Normal
    public long IdTileSpriteDestruccion { get; set; } // Edificio Destrucción
    public long IdTileSpriteConstruccion { get; set; } // Edificio Construcción (Corregido typo: Contruccion -> Construccion)
    public long StorageCapacityId { get; set; } = 0; // Si almacena recursos, ID o límite de inventario
    public long IdBuildingUpgrade { get; set; } = 0; // ID del edificio al que se puede mejorar, 0 si no hay mejora
    public int IdProjectile { get; set; } // ID del Proyectil (Corregido typo: Proyectile -> Projectile)
    public int Level { get; set; } = 1; // Nivel del edificio, por defecto 1
    public int MaxHealth { get; set; } = 100; // Salud máxima del edificio, por defecto 100    
    public float AttackRange { get; set; } = 0f; // Rango de ataque, por defecto 0 (sin ataque)
    public float AttackCooldown { get; set; } = 0f; // Tiempo de recarga del ataque, por defecto 0 (sin ataque)    
    public string Description { get; set; }
    public BuildingType BuildingType { get; set; } = BuildingType.Ninguno;
    public float TimeToBuild { get; set; } // Tiempo hasta terminar la construcción     

    public List<CostInput> ConstructionCost { get; set; } = new(); // Costo para construir el edificio
    public List<CostInput> OperationCost { get; set; } = new(); // Costo de operación o mantenimiento periódico
    public List<ProductionOutput> Outputs { get; set; } = new(); // Salidas unificadas (materiales o unidades con sus propios insumos)

    public List<ElementsData> AttackPowers { get; set; } = new(); // Poder de ataque del edificio, si aplica
    public List<ElementsData> DefensePowers { get; set; } = new(); // Poder de defensa del edificio, si aplica
    public List<BonusData> BonusPowers { get; set; } = new(); // Poderes Bonus


    [BsonIgnore]
    public List<FastCollider> bodyColliders { get; set; }

    public BuildingData()
    {
        id = EpochIdGenerator.NewId();
    }

    [BsonCtor]
    public BuildingData(long idTileSpriteNormal) : this()
    {
        IdTileSpriteNormal = idTileSpriteNormal;
        RefreshTextureVisual();
        
        AtlasModsManager.GetSpriteUniqueId(idTileSpriteNormal, out var data);
        bodyColliders = data.fastCollidersBody;        
    }
    public void SetBodyCollider(GeometricShape2D shape2D)
    {
        FastCollider fastCollider = new FastCollider();
        switch (shape2D)
        {
            case Circle circle:
                fastCollider.Shape = ShapeType.Circle;
                fastCollider.Width = circle.Radius;
                fastCollider.Height = circle.Radius;
                fastCollider.Offset = new Vector2(circle.OriginCurrent.X, circle.OriginCurrent.Y);
                break;
            case managers.Collision.Rectangle rectangle:
                fastCollider.Shape = ShapeType.Rect;
                fastCollider.Width = rectangle.Width;
                fastCollider.Height = rectangle.Height;
                fastCollider.Offset = new Vector2(rectangle.OriginCurrent.X, rectangle.OriginCurrent.Y);
                break;
            case Slope slope:
                fastCollider.Shape = ShapeType.Slope;
                fastCollider.Slope = slope.slopeType;
                fastCollider.Width = slope.Width;
                fastCollider.Height = slope.Height;
                fastCollider.Offset = new Vector2(slope.OriginCurrent.X, slope.OriginCurrent.Y);
                break;
            default:
                break;
        }
        if (bodyColliders == null)
        {
            bodyColliders = new List<FastCollider>();
        }
        bodyColliders.Add(fastCollider);
    }
    public override void RefreshTextureVisual()
    {
        textureVisual = MasterDataManager.GetData<TileSpriteData>(IdTileSpriteNormal).textureVisual;
    }
}