using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.VisualShaderNode;

namespace GodotEcsArch.sources.WindowsDataBase.Portals;

public enum PortalType
{
    MONSTRUOS,
    RECURSOS,
    HUEVOS,
    PLANOS
}
public class PortalConfigData:IdDataLong
{
    public long idTileSprite { get; set; }          // Gráfico del portal en el mapa
    public PortalType type { get; set; }            // Enum: MonsterSpawn, Miniworld, BuildingSchema, Hybrid
    public int difficultyLevel { get; set; }        // Nivel de dificultad que afectará a la ruleta de pesos
    
    [BsonCtor]
    public PortalConfigData(long idTileSprite)
    {
        AtlasModsManager.GetSpriteUniqueId(idTileSprite, out TileSpriteData sprite);
        textureVisual = sprite.textureVisual;
    }
    public PortalConfigData()
    {
        id = EpochIdGenerator.NewId();
    }
}
