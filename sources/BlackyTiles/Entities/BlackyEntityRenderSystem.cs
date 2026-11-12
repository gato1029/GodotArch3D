using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.GpuDriven;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using System.Collections.Concurrent;

namespace GodotEcsArch.sources.BlackyTiles.Entities;

public class BlackyEntityRenderSystem
{
    private readonly BlackySpatialEntityMap spatialMap;
    private readonly ChunkManagerBase chunkManager;

    // Colas thread-safe para recibir los eventos desde cualquier hilo
    private readonly ConcurrentQueue<Vector2I> _loadQueue = new();
    private readonly ConcurrentQueue<Vector2I> _unloadQueue = new();

    private const int MaxChunkOpsPerFrame = 1; // Presupuesto por frame para evitar tirones
    private int layer = (int)BlackyRenderLayer.Personajes_Arboles_Edificios;

    public BlackyEntityRenderSystem(
        BlackySpatialEntityMap spatialMap,
        ChunkManagerBase chunkManager)
    {
        this.spatialMap = spatialMap;
        this.chunkManager = chunkManager;

        // Ahora solo encolamos cuando el gestor de chunks avisa
        chunkManager.OnChunkLoad += OnChunkLoadRequested;
        chunkManager.OnChunkUnload += OnChunkUnloadRequested;
    }

    // --- MÉTODOS DE ENCOLADO (Seguros para hilos secundarios) ---

    private void OnChunkLoadRequested(Vector2I chunkCoord)
    {
        _loadQueue.Enqueue(chunkCoord);
    }

    private void OnChunkUnloadRequested(Vector2I chunkCoord)
    {
        _unloadQueue.Enqueue(chunkCoord);
    }

    // --- MÉTODO DE PROCESAMIENTO (Llamado por Flecs en el hilo principal) ---

    public void ProcessPendingChunks()
    {
        int loaded = 0;
        while (loaded < MaxChunkOpsPerFrame && _loadQueue.TryDequeue(out var chunkCoord))
        {
            InternalExecuteLoadChunk(chunkCoord);
            loaded++;
        }

        int unloaded = 0;
        while (unloaded < MaxChunkOpsPerFrame && _unloadQueue.TryDequeue(out var chunkCoord))
        {
            InternalExecuteUnloadChunk(chunkCoord);
            unloaded++;
        }
    }

    // --- LÓGICA REAL (Ejecución 100% segura en el hilo principal) ---

    private void InternalExecuteUnloadChunk(Vector2I chunkCoord)
    {
        var bucket = spatialMap.GetBucket(chunkCoord);
        if (bucket == null) return;

        for (int i = 0; i < bucket.Exist.Length; i++)
        {
            bool itemExist = bucket.Exist[i];
            if (itemExist)
            {
                Entity ent = bucket.Entities[i];
                if (!ent.IsAlive() || ent.Has<DeadTag>())
                {
                    continue;
                }
                ref var gpu = ref ent.Ensure<RenderGPUComponent>();
                if (gpu.instance!=-1)
                {
                    AtlasTexturesModsManager.Instance.DrivenRenderManager.FreeInstance(gpu.gpuindex, gpu.instance); // liberamos instancia
                    gpu.rid = default;
                    gpu.gpuindex = 0;
                    gpu.instance = -1;
                    gpu.layerTextureMaterial = -1;

                    //if (ent.Has<SpriteSimpleAnimationTag>())
                    //{
                    //    ent.Remove<SpriteSimpleAnimationTag>();
                    //}
                }                
            }
        }
    }

    private void InternalExecuteLoadChunk(Vector2I chunkCoord)
    {
        var bucket = spatialMap.GetBucket(chunkCoord);
        if (bucket == null) return;

        for (int i = 0; i < bucket.Exist.Length; i++)
        {
            bool itemExist = bucket.Exist[i];
            bool isBuilding = bucket.IsBuilding[i];
            if (itemExist)
            {
                Entity ent = bucket.Entities[i];
                if (!ent.IsAlive() || ent.Has<DeadTag>())
                {
                    continue; // no renderizo por esta muerto
                }
                int spriteId = 0;
                if (isBuilding)
                {
                    var bd = ent.Get<BuildingDefinitionComponent>();
                    spriteId = bd.idSpriteTemplateNormal;
                  
                }
                else
                {
                    var rd = ent.Get<ResourceDefinitionComponent>();
                    spriteId = rd.idSpriteTemplate;
                }
                                
                var pos = ent.Get<PositionComponent>();
                

                AtlasModsManager.TryGetTileSprite(spriteId, out var sprite);
                switch (sprite.tileSpriteType)
                {
                    case TileSpriteType.Static:
                        CreateSprite(ent, sprite.spriteData, pos.height, pos.tilePosition);
                        break;
                    case TileSpriteType.Animated:
                        CreateAnimation(ent, sprite.animationData, spriteId, pos.height, pos.tilePosition);
                        break;
                }
            }
        }
    }

    private void CreateSprite(Entity entity, WindowsDataBase.Accesories.DataBase.SpriteData spriteData, int heightRender, Vector2I positionTile)
    {
        var renderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRenderGpu(spriteData.idModMaterial);
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(positionTile.X, positionTile.Y);
        Vector2 offset = spriteData.offsetInternal;

        float depthOffset = spriteData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, heightRender, layer, positionCenter);

        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
        BufferSlot.Transform, renderInstance.instanceId, worldPosition.X, worldPosition.Y, worldPosition.Z, spriteData.scale);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
            BufferSlot.StaticData, renderInstance.instanceId, spriteData.uv);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
            BufferSlot.Flags, renderInstance.instanceId, spriteData.mirrorLocalX, spriteData.mirrorLocalY, 1, renderInstance.layer);
        entity.Set(new RenderGPUComponent(renderInstance.gpuDataIndex, renderInstance.multimeshRid, renderInstance.instanceId, 0, renderInstance.layer,
                layer, depthOffset, spriteData.scale, offset));
    }

    private void CreateAnimation(Entity entity, WindowsDataBase.Accesories.DataBase.SpriteAnimationData animationData, int idSprite, int heightRender, Vector2I positionTile)
    {
        var renderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRenderGpu(animationData.idModMaterial);
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(positionTile);
        Vector2 offset = animationData.offsetInternal;

        float depthOffset = animationData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, heightRender, layer, positionCenter);
        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        float indiceAnimation = AtlasTexturesModsManager.Instance.DrivenRenderManager.SimpleAnimations.GetLutIndex(idSprite);
        float currentTime = Time.GetTicksMsec() / 1000.0f; // Sincronizador de inicio de animación
       

        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
        BufferSlot.Transform, renderInstance.instanceId, worldPosition.X, worldPosition.Y, worldPosition.Z, animationData.scale);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
        BufferSlot.AnimState, renderInstance.instanceId, indiceAnimation, currentTime, 0, 0);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
        BufferSlot.Flags, renderInstance.instanceId, 0, 0, 1, renderInstance.layer);
        entity.Set(new RenderGPUComponent(renderInstance.gpuDataIndex, renderInstance.multimeshRid, renderInstance.instanceId, 0, renderInstance.layer,
             layer, depthOffset, animationData.scale, offset));
    }


    // ===============================
    // 🔥 MÉTODO CLEAR OPTIMIZADO (Flecs.NET)
    // ===============================

    public void Clear()
    {
        // 1. Vaciar las colas pendientes para evitar procesar eventos viejos
        while (_loadQueue.TryDequeue(out _)) { }
        while (_unloadQueue.TryDequeue(out _)) { }

        // 2. Obtener únicamente los chunks que están activos actualmente
        var activeChunks = chunkManager.GetActiveChunks();
        if (activeChunks == null) return;

        // 3. Recorrer exclusivamente los buckets de los chunks activos
        foreach (var chunkCoord in activeChunks)
        {
            var bucket = spatialMap.GetBucket(chunkCoord);
            if (bucket?.Exist == null) continue;

            for (int i = 0; i < bucket.Exist.Length; i++)
            {
                if (!bucket.Exist[i]) continue;

                Entity ent = bucket.Entities[i];
                if (!ent.IsAlive()) continue;

                // Si tiene el componente de renderizado GPU activo, liberamos la instancia
                if (ent.Has<RenderGPUComponent>())
                {
                    ref var gpu = ref ent.GetMut<RenderGPUComponent>();
                    if (gpu.instance != -1)
                    {
                        AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
                        gpu.rid = default;
                        gpu.instance = -1;
                        gpu.layerTextureMaterial = -1;
                    }

                    // Remover componentes o etiquetas de animación asociados
                    if (ent.Has<SpriteSimpleAnimationTag>())
                    {
                        ent.Remove<SpriteSimpleAnimationTag>();
                    }
                    if (ent.Has<RenderTransformComponent>())
                    {
                        ent.Remove<RenderTransformComponent>();
                    }
                    if (ent.Has<AnimationSimpleComponent>())
                    {
                        ent.Remove<AnimationSimpleComponent>();
                    }
                    if (ent.Has<RenderFrameDataComponent>())
                    {
                        ent.Remove<RenderFrameDataComponent>();
                    }
                }
            }
        }
    }
}