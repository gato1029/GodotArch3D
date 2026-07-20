using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Procedural;


/// <summary>
/// Semilla maestra del mundo. Deriva PRNGs independientes por subsistema
/// para que agregar/quitar sistemas de generación no desordene la
/// reproducibilidad de los demás.
/// </summary>
public class BlackyWorldSeed
{
    public int MasterSeed { get; }

    public BlackyWorldSeed(int masterSeed)
    {
        MasterSeed = masterSeed;
    }

    /// <summary>
    /// Devuelve un PRNG determinístico para un propósito específico
    /// (ej: "graph", "rasterize", "props", "creatures").
    /// Mismo seed + mismo purpose = siempre el mismo PRNG.
    /// </summary>
    public Random GetRng(string purpose)
    {
        int derived = HashCombine(MasterSeed, purpose.GetHashCode());
        return new Random(derived);
    }

    /// <summary>
    /// Variante para subsistemas que necesitan un PRNG distinto por
    /// instancia (ej: uno por chunk), evitando colisiones entre chunks.
    /// </summary>
    public Random GetRng(string purpose, int instanceId)
    {
        int derived = HashCombine(HashCombine(MasterSeed, purpose.GetHashCode()), instanceId);
        return new Random(derived);
    }

    private static int HashCombine(int a, int b)
    {
        unchecked { return a * 397 ^ b; }
    }
}
