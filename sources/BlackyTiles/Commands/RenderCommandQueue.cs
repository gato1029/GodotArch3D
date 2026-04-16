using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;
using System.Collections.Concurrent;

public static class RenderCommandQueue
{
    private static readonly ConcurrentQueue<IRenderCommand> _queue = new();

    // presupuesto por frame (ajustable)
    public static int MaxCommandsPerFrame = 100;

    public static void Enqueue(IRenderCommand command)
    {
        _queue.Enqueue(command);
    }

    public static void ExecuteFrame()
    {
        int limit = MaxCommandsPerFrame;

        if (_queue.Count > 2000)
            limit *= 3;

        int executed = 0;

        while (executed < limit && _queue.TryDequeue(out var command))
        {
            command.Execute();
            executed++;
        }
    }

    public static int Count => _queue.Count;
}