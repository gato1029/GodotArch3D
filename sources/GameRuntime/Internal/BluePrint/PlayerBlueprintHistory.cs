using GodotEcsArch.sources.WindowsDataBase.Blueprint.DataBase;

namespace GodotEcsArch.sources.GameRuntime.Internal.BluePrint;

using LiteDB;
using System;
using System.Collections.Generic;



/// <summary>
/// Reglas por defecto de Maestría (N desde 3) y Síntesis (M de 15 a 3)
/// para la escala de 16 rarezas (0..15).
/// </summary>
public static class BlueprintRarityRules
{
    private static readonly int[] DefaultMasteryByRarity =
    {
        3,  // 0: Vestigio
        3,  // 1: Bosquejo
        4,  // 2: Comun
        4,  // 3: Estable
        5,  // 4: Resonante
        5,  // 5: Alterado
        6,  // 6: Templado
        6,  // 7: Raro
        7,  // 8: Anomalo
        8,  // 9: Arcano
        9,  // 10: Dimensional
        10, // 11: Soberano
        11, // 12: Epico
        13, // 13: Ancestral
        15, // 14: Primordial
        18  // 15: Trascendente
    };

    // Síntesis M para ascender desde esta rareza (0 en Trascendente por ser el tope)
    private static readonly int[] DefaultSynthesisCountByRarity =
    {
        15, // 0: Vestigio
        14, // 1: Bosquejo
        13, // 2: Comun
        12, // 3: Estable
        11, // 4: Resonante
        10, // 5: Alterado
        9,  // 6: Templado
        8,  // 7: Raro
        7,  // 8: Anomalo
        6,  // 9: Arcano
        6,  // 10: Dimensional
        5,  // 11: Soberano
        5,  // 12: Epico
        4,  // 13: Ancestral
        3,  // 14: Primordial
        0   // 15: Trascendente
    };

    public static int GetDefaultMastery(ItemsRarity rarity)
    {
        int index = (int)rarity;
        if ((uint)index >= (uint)DefaultMasteryByRarity.Length) return 3;
        return DefaultMasteryByRarity[index];
    }

    public static int GetDefaultSynthesisCount(ItemsRarity rarity)
    {
        int index = (int)rarity;
        if ((uint)index >= (uint)DefaultSynthesisCountByRarity.Length) return 0;
        return DefaultSynthesisCountByRarity[index];
    }
}

/// <summary>
/// Registro persistente del progreso de un plano específico para el jugador.
/// </summary>
public class BlueprintProgressEntry
{
    public long BlueprintId { get; set; }
    public int PhysicalOwned { get; set; } // Copias físicas disponibles en inventario/almacén
    public int StudiedCount { get; set; }  // Copias ya consumidas en estudio (0..MasteryRequired)
    public bool IsMastered { get; set; }   // True cuando StudiedCount >= BlueprintData.MasteryRequired
    public bool IsDiscovered { get; set; } // True si alguna vez lo obtuvo (para el Libro de Planos)
}

/// <summary>
/// Historial persistente de planos del jugador (sobrevive al reinicio de edificios entre oleadas).
/// </summary>
public class PlayerBlueprintHistory
{
    // Lista serializable para guardado (LiteDB / MessagePack / JSON)
    public List<BlueprintProgressEntry> Entries { get; set; } = new();

    // Índice rápido en memoria para consultas O(1) sin recorrer listas en runtime
    [BsonIgnore]
    private readonly Dictionary<long, BlueprintProgressEntry> _lookup = new();

    /// <summary>
    /// Reconstruye el diccionario interno tras cargar la partida desde disco/BD.
    /// </summary>
    public void RebuildLookup()
    {
        _lookup.Clear();
        for (int i = 0; i < Entries.Count; i++)
        {
            _lookup[Entries[i].BlueprintId] = Entries[i];
        }
    }

    public BlueprintProgressEntry GetOrCreateEntry(long blueprintId)
    {
        if (!_lookup.TryGetValue(blueprintId, out var entry))
        {
            entry = new BlueprintProgressEntry { BlueprintId = blueprintId };
            Entries.Add(entry);
            _lookup[blueprintId] = entry;
        }
        return entry;
    }

    public bool TryGetEntry(long blueprintId, out BlueprintProgressEntry entry)
    {
        return _lookup.TryGetValue(blueprintId, out entry);
    }

    /// <summary>
    /// Agrega copias físicas de un plano obtenidas de cofres, monstruos, portales o síntesis.
    /// </summary>
    public void AddPhysicalBlueprint(long blueprintId, int amount = 1)
    {
        if (blueprintId <= 0 || amount <= 0) return;

        var entry = GetOrCreateEntry(blueprintId);
        entry.PhysicalOwned += amount;
        entry.IsDiscovered = true;
    }

    /// <summary>
    /// Verifica si el jugador puede construir el edificio (por maestría permanente o gastando 1 plano físico).
    /// </summary>
    public bool CanBuild(long blueprintId)
    {
        if (blueprintId == 0) return true; // Edificio libre sin plano
        if (!_lookup.TryGetValue(blueprintId, out var entry)) return false;

        return entry.IsMastered || entry.PhysicalOwned > 0;
    }

    /// <summary>
    /// Consume 1 plano físico si aún no tiene maestría permanente. Devuelve true si procede la construcción.
    /// </summary>
    public bool ConsumeForBuild(long blueprintId)
    {
        if (blueprintId == 0) return true;
        if (!_lookup.TryGetValue(blueprintId, out var entry)) return false;

        if (entry.IsMastered) return true; // Maestría permanente: solo gasta materiales
        if (entry.PhysicalOwned <= 0) return false;

        entry.PhysicalOwned--;
        return true;
    }

    /// <summary>
    /// Estudia 1 plano físico para avanzar hacia la maestría permanente (N).
    /// </summary>
    public bool StudyBlueprint(BlueprintData blueprintData)
    {
        if (blueprintData == null) return false;

        var entry = GetOrCreateEntry(blueprintData.id);
        if (entry.IsMastered || entry.PhysicalOwned <= 0) return false;

        entry.PhysicalOwned--;
        entry.StudiedCount++;

        if (entry.StudiedCount >= blueprintData.MasteryRequired)
        {
            entry.StudiedCount = blueprintData.MasteryRequired;
            entry.IsMastered = true;
        }

        return true;
    }

    /// <summary>
    /// Valida si el jugador posee los planos físicos suficientes para ejecutar una receta de síntesis.
    /// </summary>
    public bool CanSynthesize(BlueprintSynthesisRecipe recipe, Func<long, BlueprintData> blueprintResolver)
    {
        if (recipe == null || recipe.Inputs.Count == 0 || recipe.Outputs.Count == 0) return false;

        for (int i = 0; i < recipe.Inputs.Count; i++)
        {
            var input = recipe.Inputs[i];
            if (input.BlueprintId > 0)
            {
                if (!_lookup.TryGetValue(input.BlueprintId, out var entry) || entry.PhysicalOwned < input.Count)
                    return false;
            }
            else if (input.RequiredRarity.HasValue)
            {
                int availableByRarity = 0;
                for (int j = 0; j < Entries.Count; j++)
                {
                    if (Entries[j].PhysicalOwned <= 0) continue;
                    var bp = blueprintResolver(Entries[j].BlueprintId);
                    if (bp != null && bp.Rarity == input.RequiredRarity.Value)
                    {
                        availableByRarity += Entries[j].PhysicalOwned;
                        if (availableByRarity >= input.Count) break;
                    }
                }
                if (availableByRarity < input.Count) return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Ejecuta la síntesis: consume los Inputs y entrega el plano resultante de Outputs.
    /// Si input.BlueprintId == 0 (por rareza), usa los IDs seleccionados por el jugador en <paramref name="selectedIdsByRarity"/>.
    /// </summary>
    public bool ExecuteSynthesis(
        BlueprintSynthesisRecipe recipe,
        Func<long, BlueprintData> blueprintResolver,
        Random rng,
        out BlueprintSynthesisOutput grantedOutput,
        IReadOnlyList<long> selectedIdsByRarity = null)
    {
        grantedOutput = null;
        if (!CanSynthesize(recipe, blueprintResolver)) return false;

        // 1. Consumir insumos
        int selectionCursor = 0;
        for (int i = 0; i < recipe.Inputs.Count; i++)
        {
            var input = recipe.Inputs[i];
            if (input.BlueprintId > 0)
            {
                _lookup[input.BlueprintId].PhysicalOwned -= input.Count;
            }
            else if (input.RequiredRarity.HasValue)
            {
                int remaining = input.Count;

                // Si la UI envió qué copias exactas eligió el jugador, se consumen primero
                if (selectedIdsByRarity != null)
                {
                    while (remaining > 0 && selectionCursor < selectedIdsByRarity.Count)
                    {
                        long chosenId = selectedIdsByRarity[selectionCursor++];
                        if (_lookup.TryGetValue(chosenId, out var chosenEntry) && chosenEntry.PhysicalOwned > 0)
                        {
                            var bp = blueprintResolver(chosenId);
                            if (bp != null && bp.Rarity == input.RequiredRarity.Value)
                            {
                                chosenEntry.PhysicalOwned--;
                                remaining--;
                            }
                        }
                    }
                }

                // Si faltan por descontar (o fue automático), toma de las entradas disponibles de esa rareza
                for (int j = 0; j < Entries.Count && remaining > 0; j++)
                {
                    if (Entries[j].PhysicalOwned <= 0) continue;
                    var bp = blueprintResolver(Entries[j].BlueprintId);
                    if (bp != null && bp.Rarity == input.RequiredRarity.Value)
                    {
                        int take = Math.Min(Entries[j].PhysicalOwned, remaining);
                        Entries[j].PhysicalOwned -= take;
                        remaining -= take;
                    }
                }
            }
        }

        // 2. Resolver salida (determinista si Outputs.Count == 1, ponderada por Weight si hay varias)
        if (recipe.Outputs.Count == 1)
        {
            grantedOutput = recipe.Outputs[0];
        }
        else
        {
            int totalWeight = 0;
            for (int i = 0; i < recipe.Outputs.Count; i++)
                totalWeight += Math.Max(0, recipe.Outputs[i].Weight);

            int roll = (rng ?? Random.Shared).Next(totalWeight);
            int acc = 0;
            for (int i = 0; i < recipe.Outputs.Count; i++)
            {
                acc += Math.Max(0, recipe.Outputs[i].Weight);
                if (roll < acc)
                {
                    grantedOutput = recipe.Outputs[i];
                    break;
                }
            }
            grantedOutput ??= recipe.Outputs[0];
        }

        // 3. Entregar el plano resultante al historial del jugador
        AddPhysicalBlueprint(grantedOutput.ResultBlueprintId, grantedOutput.Amount);
        return true;
    }
}