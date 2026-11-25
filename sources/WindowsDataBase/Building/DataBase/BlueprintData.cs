using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using LiteDB;
using System;

namespace GodotEcsArch.sources.WindowsDataBase.Blueprint.DataBase;

/// <summary>
/// Grado o rareza del plano (escala la dificultad de obtención y la cantidad N de estudio).
/// </summary>
/// <summary>
/// Escala de 16 grados de rareza de planos (0 a 15), desde fragmentos iniciales
/// hasta reliquias trascendentes de portales profundos.
/// </summary>
public enum ItemsRarity
{
    Vestigio = 0,
    Bosquejo = 1,
    Comun = 2,
    Estable = 3,
    Resonante = 4,
    Alterado = 5,
    Templado = 6,
    Raro = 7,
    Anomalo = 8,
    Arcano = 9,
    Dimensional = 10,
    Soberano = 11,
    Epico = 12,
    Ancestral = 13,
    Primordial = 14,
    Trascendente = 15
}
/// <summary>
/// Template de Plano en LiteDB. Cada plano apunta al edificio (BuildingData) que permite construir o mejorar.
/// </summary>
public class BlueprintData : IdDataLong
{
    // --- VISUALES E IDENTIDAD ---
    public long IdTileSpriteIcon { get; set; } // Ícono del plano en UI, inventario y libro de planos
    public string Description { get; set; } // Descripción del plano
    public ItemsRarity Rarity { get; set; } = ItemsRarity.Vestigio; // Grado: Básico a Épico

    // --- ENLACE CON EL EDIFICIO ---
    public long TargetBuildingId { get; set; } // ID del BuildingData que este plano construye o mejora

    // --- REGLA DE MAESTRÍA PERMANENTE ---
    public int MasteryRequired { get; set; } = 1; // Cantidad N de planos físicos a estudiar para maestría permanente

    // --- METADATOS PARA SISTEMAS EXTERNOS DE LOOT (COFRES / MONSTRUOS / PORTALES) ---
    public int DropWeight { get; set; } = 100; // Peso relativo dentro de su rareza para tablas de botín
    public bool CanDropFromMonsters { get; set; } = true;
    public bool CanDropFromChests { get; set; } = true;

    public BlueprintData()
    {
        id = EpochIdGenerator.NewId();
    }

    [BsonCtor]
    public BlueprintData(long idTileSpriteIcon) : this()
    {
        IdTileSpriteIcon = idTileSpriteIcon;
        RefreshTextureVisual();
    }

    public override void RefreshTextureVisual()
    {
        if (IdTileSpriteIcon > 0)
        {
            textureVisual = MasterDataManager.GetData<TileSpriteData>(IdTileSpriteIcon).textureVisual;
        }
    }
}