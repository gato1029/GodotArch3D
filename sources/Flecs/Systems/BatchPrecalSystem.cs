using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems;
internal class BatchPrecalSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsPreUpdate;
    protected override bool MultiThreaded => false;
    protected override bool HasQuery => false;

    private int batchIndex = 0;
    private float batchTimer = 0f;
    private const float batchInterval = 0.02f;
    private const int targetUpdatesPerFrame = 600;
    private int numBatches = 1;

    private const float batchIntervalMove = 0.001f;
    private float batchTimerMove = 0f;
    private const int targetUpdatesPerFrameMove = 1000;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        
    }
    protected override void OnIter(Iter it)
    {
        int totalColliders = CollisionManager.Instance.characterEntitiesFlecs.Count;
        numBatches = Math.Max(1, (int)Math.Ceiling(totalColliders / (float)targetUpdatesPerFrame));
        GlobalData.numBatchColliders = numBatches;

        GlobalData.numBatchMoveColliders = Math.Max(1, (int)Math.Ceiling(totalColliders / (float)targetUpdatesPerFrameMove));
        batchTimer += it.DeltaTime();
        batchTimerMove += it.DeltaTime();
        if (batchTimer >= batchInterval)
        {
            batchTimer -= batchInterval;
            batchIndex = (batchIndex + 1) % numBatches;
            GlobalData.batchIndexColliders = batchIndex;
        }

        if (batchTimerMove >= batchIntervalMove)
        {
            batchTimerMove =0;
            GlobalData.batchIndexMoveColliders = (GlobalData.batchIndexMoveColliders + 1) % GlobalData.numBatchMoveColliders;            
        }
    }
}
