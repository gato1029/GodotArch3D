using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.WindowsDataBase.Blueprint.DataBase;

/// <summary>
/// Define un insumo requerido para sintetizar planos (un plano concreto o cualquier plano de una rareza).
/// </summary>
public class BlueprintSynthesisInput
{
    public long BlueprintId { get; set; } // 0 = acepta cualquier plano que cumpla RequiredRarity; > 0 = plano específico
    public ItemsRarity? RequiredRarity { get; set; } // Rareza exigida cuando BlueprintId == 0
    public int Count { get; set; } = 1; // Cantidad M de planos físicos a consumir
}

/// <summary>
/// Define un resultado posible al ejecutar la síntesis.
/// Si la lista Outputs tiene 1 solo elemento, el resultado es 100% determinista.
/// </summary>
public class BlueprintSynthesisOutput
{
    public long ResultBlueprintId { get; set; } // ID del BlueprintData que bota la síntesis
    public int Amount { get; set; } = 1; // Cantidad de copias físicas entregadas
    public int Weight { get; set; } = 100; // Peso relativo si hay múltiples salidas posibles
}

/// <summary>
/// Receta del Taller de Síntesis: define explícitamente qué planos entran (Inputs) y qué plano(s) bota (Outputs).
/// </summary>
public class BlueprintSynthesisRecipe : IdDataLong
{
    public string Name { get; set; } // Nombre visible en la UI del Taller de Síntesis
    public string Description { get; set; } // Descripción opcional de la receta
    public List<BlueprintSynthesisInput> Inputs { get; set; } = new(); // Planos requeridos (M)
    public List<BlueprintSynthesisOutput> Outputs { get; set; } = new(); // Planos que bota al completar

    public BlueprintSynthesisRecipe()
    {
        id = EpochIdGenerator.NewId();
    }
}