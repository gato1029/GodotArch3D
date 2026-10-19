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
public sealed class BlackyWorldSeed
{
    public int MasterSeed { get; }

    public BlackyWorldSeed(int masterSeed)
    {
        MasterSeed = masterSeed;
    }

    /// <summary>
    /// Devuelve un PRNG determinístico para un propósito específico.
    /// Mismo seed + mismo propósito = mismo PRNG.
    /// El resultado es estable entre ejecuciones.
    /// </summary>
    public Random GetRng(string purpose)
    {
        int purposeHash = StableHash(purpose);
        int derived = HashCombine(MasterSeed, purposeHash);

        return new Random(derived);
    }

    /// <summary>
    /// Devuelve un PRNG determinístico para una instancia específica.
    /// Útil para generar contenido independiente por chunk.
    /// </summary>
    public Random GetRng(string purpose, int instanceId)
    {
        int purposeHash = StableHash(purpose);

        int derived = HashCombine(
            HashCombine(MasterSeed, purposeHash),
            instanceId
        );

        return new Random(derived);
    }

    /// <summary>
    /// Hash determinístico FNV-1a de una cadena.
    /// </summary>
    private static int StableHash(string text)
    {
        unchecked
        {
            uint hash = 2166136261;

            foreach (char c in text)
            {
                hash ^= c;
                hash *= 16777619;
            }

            return (int)hash;
        }
    }

    private static int HashCombine(int a, int b)
    {
        unchecked
        {
            return a * 397 ^ b;
        }
    }
}