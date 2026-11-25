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

/// <summary>
/// Rol funcional de cada edificio dentro del bucle de expansión territorial,
/// eclosión de huevos sin barracas, planos/maestría y recursos finitos.
/// </summary>
public enum BuildingType
{
    Ninguno = 0,
    BasePrincipal = 1,      // Núcleo del asentamiento, radio inicial de territorio y condición de derrota
    BalizaTerritorio = 2,   // Extiende el radio de construcción conectado y despeja niebla de guerra
    Incubadora = 3,         // Eclosiona huevos encontrados explorando y suma cupo de criaturas activas
    Granja = 4,             // Produce comida para criaturas y crece por mejora (IdBuildingUpgrade)
    Arsenal = 5,            // Aporta cupo de soporte para torres de cualquier portal y crece por mejora
    PuntoAcopio = 6,        // Depósito logístico intermedio para recolectores y reparto de suministros
    ExtractorNodo = 7,      // Se construye sobre veta o portal finito; acelera/facilita extracción
    Muralla = 8,            // Defensa pasiva (incluye portones según IsPassableForAllies)
    Torre = 9,              // Defensa activa que consume cupo de soporte del Arsenal
    TallerSintesis = 10,    // Estudia planos físicos para maestría permanente y fusiona M planos básicos
    TallerCampo = 11,       // Repara edificios y cura criaturas en el frente
    Adorno = 12             // Elemento estético sin impacto mecánico directo
}

/// <summary>
/// Define qué recurso o material puede almacenarse en un edificio y su límite máximo.
/// Si <see cref="Id"/> es 0, aplica a cualquier elemento del <see cref="StorageType"/> indicado.
/// </summary>
public class StorageRule
{
    public long Id { get; set; } // 0 = cualquiera del tipo, o ID específico del recurso/material
    public ResourceType StorageType { get; set; } // Tipo: recurso o material
    public int MaxCapacity { get; set; } // Capacidad máxima permitida en el edificio
}

public class CostInput
{
    public long Id { get; set; } // ID del recurso o material requerido
    public ResourceType CostInputType { get; set; } // Tipo: recurso o material
    public int Amount { get; set; } // Cantidad necesaria (ej. 3 palos, 4 de oro)
}

public class ProductionOutput
{
    public long Id { get; set; } // ID de la unidad o material producido
    public int AmountProduced { get; set; } // Cantidad que se obtiene al finalizar
    public float ProductionTime { get; set; } // Tiempo de producción en segundos
    public List<CostInput> ProductionCost { get; set; } = new(); // Insumos necesarios para producirlo
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

// Esto ya no se usa porque el idTile ya resuelve los colliders y tamaño de forma automática
public class BuildingPosition
{
    public int centerX { get; set; }
    public int centerY { get; set; }
    public int sizeGridX { get; set; }
    public int sizeGridY { get; set; }
    public List<BuildingTilePosition> buildingTilePositions { get; set; } // Posición de construcción
}

/// <summary>
/// Template de edificio serializable en LiteDB que define visuales, estadísticas,
/// costos, producción, almacenamiento, territorio, planos y sustento de criaturas/torres.
/// </summary>
public class BuildingData : IdDataLong
{
    // --- VISUALES, MEJORA Y PROGRESIÓN BÁSICA ---
    public long IdTileSpriteNormal { get; set; } // Sprite del edificio en estado normal
    public long IdTileSpriteDestruccion { get; set; } // Sprite del edificio destruido o en ruinas
    public long IdTileSpriteConstruccion { get; set; } // Sprite mientras está en construcción
    public long IdBuildingUpgrade { get; set; } = 0; // ID del siguiente nivel/mejora (0 si no tiene mejora)
    public int Level { get; set; } = 1; // Nivel del edificio
    public string Description { get; set; } // Descripción para UI o tooltip
    public BuildingType BuildingType { get; set; } = BuildingType.Ninguno; // Rol funcional del edificio
    public float TimeToBuild { get; set; } // Tiempo en segundos para completar la construcción

    // --- COMBATE Y SALUD ---
    public int MaxHealth { get; set; } = 100; // Salud máxima del edificio
    public int IdProjectile { get; set; } // ID del proyectil si el edificio ataca (ej. torres)
    public float AttackRange { get; set; } = 0f; // Rango de ataque (0 si no ataca)
    public float AttackCooldown { get; set; } = 0f; // Tiempo entre ataques en segundos

    // --- COSTOS, PRODUCCIÓN Y ALMACENAMIENTO ---
    public List<CostInput> ConstructionCost { get; set; } = new(); // Costo inicial para construir
    public List<CostInput> OperationCost { get; set; } = new(); // Costo periódico de operación/mantenimiento
    public List<ProductionOutput> Outputs { get; set; } = new(); // Salidas producidas (comida, cargas, etc.)
    public List<StorageRule> StorageRules { get; set; } = new(); // Qué puede almacenar y cuánta capacidad tiene

    // --- ATRIBUTOS ELEMENTALES Y BONUS ---
    public List<ElementsData> AttackPowers { get; set; } = new(); // Poderes de ataque elementales
    public List<ElementsData> DefensePowers { get; set; } = new(); // Resistencias o defensas elementales
    public List<BonusData> BonusPowers { get; set; } = new(); // Bonificaciones pasivas otorgadas

    // --- TERRITORIO Y VISIÓN ---
    public float TerritoryRadius { get; set; } = 0f; // Radio de territorio de construcción que proyecta (Base / Baliza)
    public float VisionRadius { get; set; } = 0f; // Radio de visión para despejar niebla de guerra
    public bool RequiresTerritoryConnection { get; set; } = true; // Si exige estar dentro del territorio conectado

    // --- PLANOS Y MAESTRÍA ---
    public long BlueprintId { get; set; } = 0; // ID del plano físico asociado (0 si no requiere plano)
    public int MasteryRequired { get; set; } = 1; // Cantidad N de planos a estudiar para maestría permanente
    public int SynthesisInputCount { get; set; } = 0; // Cantidad M de planos básicos si exige síntesis

    // --- SUSTENTO DE CRIATURAS Y TORRES ---
    public int PopulationCapBonus { get; set; } = 0; // Cupo extra de criaturas activas (Base / Incubadora)
    public int TowerSupportCapacity { get; set; } = 0; // Cupo total de soporte de torres que aporta (Arsenal)
    public int TowerSupportCost { get; set; } = 0; // Cupo de soporte que consume esta torre (1 paja, 2 escopeta, 3 magia...)

    // --- NODOS FINITOS, PASO DE ALIADOS Y SOPORTE EN FRENTE ---
    public bool RequiresResourceNode { get; set; } = false; // True si debe montarse sobre una veta o portal finito
    public List<long> AllowedResourceNodeIds { get; set; } = new(); // IDs de nodos/portales válidos para este extractor
    public float ExtractionSpeedMultiplier { get; set; } = 1f; // Multiplicador de extracción sobre el nodo finito
    public bool IsPassableForAllies { get; set; } = false; // True para portones que permiten el paso de aliados
    public float SupportRadius { get; set; } = 0f; // Radio de acción para Taller de Campo o Acopio
    public float HealOrRepairPerSecond { get; set; } = 0f; // Velocidad de curación o reparación en el área

    // --- COLLIDERS EN TIEMPO DE EJECUCIÓN ---
    [BsonIgnore]
    public List<FastCollider> bodyColliders { get; set; } // Colliders derivados del sprite normal (y base para validar espacio)

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

    public override void RefreshTextureVisual()
    {
        textureVisual = MasterDataManager.GetData<TileSpriteData>(IdTileSpriteNormal).textureVisual;
    }
}