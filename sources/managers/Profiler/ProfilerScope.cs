


#define ENABLE_PROFILER

using System;
namespace GodotEcsArch.sources.managers.Profiler;
public class ProfileScope : IDisposable
{
#if ENABLE_PROFILER
    private string label;

    public ProfileScope(string label)
    {
        this.label = label;
        PerformanceTimer.Instance.Start(label);
    }

    public void Dispose()
    {
        PerformanceTimer.Instance.Stop(label);
    }
#else
    public ProfileScope(string label) { }
    public void Dispose() { }
#endif
}
