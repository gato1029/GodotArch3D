using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyEngine.Core;

using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Simulation;

public class SimulationTick
{
    public float Accumulator;
    public float FixedDelta;
    public int TickCount; // 🔥 cuantos ticks ocurrieron este frame
    public int FrameIndex; // 🔥 índice del frame actual, para sistemas que quieran hacer algo cada N frames
    public int TotalUnits; // lo usare para luego dividir la carga de búsqueda de enemigos entre frames, para no hacer todo en un frame y que se note el lag
    public int GruposDivisionUnidadesMelle = 1;
    public int GruposDivisionUnidadesRango = 1;

    /// <summary>
    /// Ajusta automáticamente la cantidad de grupos en función de las unidades totales
    /// para balancear el rendimiento de la CPU (staggering).
    /// </summary>
    public void UpdateGroupCountMelle(int totalUnits)
    {
        if (totalUnits > 1000)
            GruposDivisionUnidadesMelle = 4;
        else if (totalUnits > 300)
            GruposDivisionUnidadesMelle = 4;
        else if (totalUnits > 50)
            GruposDivisionUnidadesMelle = 1;
        else
            GruposDivisionUnidadesMelle = 1; // Sin escalonamiento para grupos pequeños
    }
    public void UpdateGroupCountRanged(int totalUnits)
    {
        if (totalUnits > 1000)
            GruposDivisionUnidadesRango = 4;
        else if (totalUnits > 300)
            GruposDivisionUnidadesRango = 4;
        else if (totalUnits > 50)
            GruposDivisionUnidadesRango = 1;
        else
            GruposDivisionUnidadesRango = 1; // Sin escalonamiento para grupos pequeños
    }
    /// <summary>
    /// Devuelve la máscara binaria segura para usar con el operador bitwise (&).
    /// </summary>
    public int GetGroupMaskMelle() => Math.Max(1, GruposDivisionUnidadesMelle) - 1;
    public int GetGroupMaskRango() => Math.Max(1, GruposDivisionUnidadesRango) - 1;
}

public class SimulationTickSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsPreUpdate;
    protected override bool MultiThreaded => false; // 🔥 importante
    protected override bool HasQuery => false;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;
        var sim = blackyWorld.Simulation.Tick;        

        float delta = (float)it.DeltaTime();

        sim.Accumulator += delta;
        sim.TickCount = 0;

        int maxTicks = 5;

        while (sim.Accumulator >= sim.FixedDelta && sim.TickCount < maxTicks)
        {
            sim.Accumulator -= sim.FixedDelta;
            sim.TickCount++;
        }
        sim.FrameIndex++;
    }
}