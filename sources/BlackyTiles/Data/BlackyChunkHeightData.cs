using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTiles.Data;

public class BlackyChunkHeightData<T>
    where T : struct
{
    private readonly T[] _cells;

    private readonly int _chunkSize;

    public BlackyChunkHeightData(int chunkSize)
    {
        _chunkSize = chunkSize;

        _cells =
            new T[chunkSize * chunkSize];
    }

    public ref T GetCell(int x, int y)
    {
        return ref _cells[Index(x, y)];
    }
    public ReadOnlyMemory<T> GetCells()
    {
        return _cells;
    }
    private int Index(int x, int y)
    {
        return x + y * _chunkSize;
    }
}