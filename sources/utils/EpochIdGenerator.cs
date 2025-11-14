using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
public static class EpochIdGenerator
{
    private static long _lastTimestamp = 0;
    private static int _counter = 0;
    private static readonly object _lock = new();

    public static long NewId()
    {
        lock (_lock)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (now == _lastTimestamp)
            {
                _counter++;
            }
            else
            {
                _counter = 0;
                _lastTimestamp = now;
            }

            // Combina epoch (milisegundos) + contador (hasta 999)
            return now * 1000 + _counter;
        }
    }
}
