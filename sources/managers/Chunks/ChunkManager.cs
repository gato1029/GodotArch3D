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

    public int maxTileView { get; private set; }
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
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        var dataViewDistance = GetChunksVisible(screenSize);
        maxTileView = GetVisibleTileCount(dataViewDistance);
        tiles16X16 = new ChunkManagerBase(dataViewDistance, new Vector2I(8,8), new Vector2I(16,16));
        //tiles32X32 = new ChunkManagerBase(CommonAtributes.VIEW_DISTANCE_CHUNK_32, new Vector2I(8, 8), new Vector2I(32,32));
        //chunkDimencion = PositionsManager.Instance.chunkDimencion;
        //ViewDistance = viewDistance;
        //workerThread = new Thread(ProcessChunkQueue);
        //workerThread.Start();


        GD.Print(dataViewDistance);
    }
    public int GetVisibleTileCount(Vector2I visibleChunks, int tilesPerChunk = 8)
    {
        int totalTilesX = visibleChunks.X * tilesPerChunk;
        int totalTilesY = visibleChunks.Y * tilesPerChunk;
        return totalTilesX * totalTilesY;
    }
    public  Vector2I GetChunksVisible(Vector2I screenSizePixels, int bufferChunks = 1)
    {
        int maxZoom = 20;
        float aspectRatio = (float)screenSizePixels.X / screenSizePixels.Y;

        float visibleHeight = maxZoom * 2;
        float visibleWidth = visibleHeight * aspectRatio;

        float chunkSizeUnits = 8f; // 8 tiles de 1 unidad cada uno

        int chunksX = (int)MathF.Ceiling(visibleWidth / chunkSizeUnits) + bufferChunks * 2;
        int chunksY = (int)MathF.Ceiling(visibleHeight / chunkSizeUnits) + bufferChunks * 2;

        return new Vector2I(chunksX, chunksY);
    }
    public void ForcedUpdate()
    {
        tiles16X16.ForcedUpdateChunks(new Vector2(0, 0));
        //tiles32X32.ForcedUpdateChunks(new Vector2(0, 0));
    }

    internal void UpdatePlayerPosition(Vector2 positionCamera)
    {
        tiles16X16.UpdatePlayerPosition(positionCamera);
        //tiles32X32.UpdatePlayerPosition(positionCamera);
    }
}
