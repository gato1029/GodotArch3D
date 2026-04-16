using GodotEcsArch.sources.managers.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;

public static class CollisionHelper
{

    // Obtiene una máscara que incluye a todos los equipos MENOS al indicado
    public static uint AllTeamsExcept(uint myTeamBit)
    {
        return CollisionConfig.AllTeams & ~myTeamBit;
    }

    // Crea una máscara de ataque estándar: Enemigos + Objetos Neutrales
    public static uint StandardAttackMask(uint myTeamBit)
    {
        return (CollisionConfig.AllTeams & ~myTeamBit) | CollisionConfig.Neutral;
    }

    // Crea una máscara para proyectiles que atraviesan todo (Aliados, Enemigos y Muros)
    public static uint PiercingEverythingMask()
    {
        return CollisionConfig.AllTeams | CollisionConfig.Neutral;
    }

    // Útil para saber si dos entidades son del mismo equipo usando sus Layers
    public static bool AreSameTeam(uint layerA, uint layerB)
    {
        return (layerA & CollisionConfig.AllTeams & layerB) != 0;
    }
}


/*
 * // --- UN SOLDADO DEL EQUIPO 4 ---
var soldier4 = world.Entity()
    .Set(new SpatialID {
        Value = FastSpatialHash.GetNewIndex(),
        Layer = CollisionConfig.TypePlayer | CollisionConfig.Team4,
        // Choca con todos los otros equipos y con cosas neutrales
        Mask  = CollisionHelper.StandardAttackMask(CollisionConfig.Team4)
    });

// --- UNA BALA "CURATIVA" (Solo afecta a mi propio equipo) ---
var healerBullet = world.Entity()
    .Set(new SpatialID {
        Value = FastSpatialHash.GetNewIndex(),
        Layer = CollisionConfig.TypeBullet | CollisionConfig.Team4,
        Mask  = CollisionConfig.Team4 // Solo busca bits del Team4
    });

// --- UN PROYECTIL DE LAVA (Daña a ABSOLUTAMENTE TODO) ---
var lavaOrb = world.Entity()
    .Set(new SpatialID {
        Value = FastSpatialHash.GetNewIndex(),
        Layer = CollisionConfig.TypeBullet | CollisionConfig.Neutral,
        Mask  = CollisionHelper.PiercingEverythingMask()
    });

 */