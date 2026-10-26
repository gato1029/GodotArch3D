using GodotEcsArch.sources.utils;
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
    public long idTileSprite;          // Gráfico del portal en el mapa
    public PortalType type;            // Enum: MonsterSpawn, Miniworld, BuildingSchema, Hybrid
    public int difficultyLevel;        // Nivel de dificultad que afectará a la ruleta de pesos

    public PortalConfigData()
    {
        id = EpochIdGenerator.NewId();
    }
}
