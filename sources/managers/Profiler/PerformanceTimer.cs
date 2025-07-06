

#define ENABLE_PROFILER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot; // Solo para usar GD.Print (puedes quitarlo si no lo usas)
namespace GodotEcsArch.sources.managers.Profiler;
public class PerformanceTimer
{
    // Singleton
    private static PerformanceTimer _instance = new PerformanceTimer();
    public static PerformanceTimer Instance => _instance;

#if ENABLE_PROFILER
    public bool Enabled { get; set; } = true;

    private Dictionary<string, Stopwatch> timers = new();
    private Dictionary<string, double> lastDurations = new();

    private PerformanceTimer() { } // privado para evitar instanciación externa

    public void Start(string label)
    {
        if (!Enabled) return;

        if (!timers.ContainsKey(label))
            timers[label] = new Stopwatch();

        timers[label].Restart();
    }

    public void Stop(string label)
    {
        if (!Enabled) return;

        if (timers.ContainsKey(label))
        {
            timers[label].Stop();
            long ticks = timers[label].ElapsedTicks;
            double ns = (ticks * (1000.0 / Stopwatch.Frequency));
            lastDurations[label] = ns;
        }
    }

    public void PrintAll(int frameInterval, int currentFrame)
    {
        if (!Enabled || currentFrame % frameInterval != 0)
            return;

        GD.Print("=== Rendimiento por sistema ===");
        foreach (var entry in lastDurations)
        {
            GD.Print($"{entry.Key}: {entry.Value:F4} ms");
        }
    }
#else
    public bool Enabled { get; set; } = false;
    private PerformanceTimer() { }

    public void Start(string label) { }
    public void Stop(string label) { }
    public void PrintAll(int frameInterval, int currentFrame) { }
#endif
}
