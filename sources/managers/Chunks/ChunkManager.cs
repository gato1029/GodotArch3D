using Godot;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Chunks;
public class ChunkManager
{
    public ChunkManagerBase tiles16X16 { get; private set; }
    public ChunkManagerBase tiles32X32 { get; private set; }

    public static ChunkManager Instance { get; private set; }
    public static void Initialize()
    {
        if (Instance == null)
        {
            Instance = new ChunkManager();
        }
    }
    private ChunkManager()
    {
        tiles16X16 = new ChunkManagerBase(CommonAtributes.VIEW_DISTANCE_CHUNK_16, new Vector2I(8,8), new Vector2I(16,16));
        tiles32X32 = new ChunkManagerBase(CommonAtributes.VIEW_DISTANCE_CHUNK_32, new Vector2I(8, 8), new Vector2I(32,32));
        //chunkDimencion = PositionsManager.Instance.chunkDimencion;
        //ViewDistance = viewDistance;
        //workerThread = new Thread(ProcessChunkQueue);
        //workerThread.Start();
    }

    public void ForcedUpdate()
    {
        tiles16X16.ForcedUpdateChunks(new Vector2(0, 0));
        tiles32X32.ForcedUpdateChunks(new Vector2(0, 0));
    }

    internal void UpdatePlayerPosition(Vector2 positionCamera)
    {
        tiles16X16.UpdatePlayerPosition(positionCamera);
        //tiles32X32.UpdatePlayerPosition(positionCamera);
    }
}
