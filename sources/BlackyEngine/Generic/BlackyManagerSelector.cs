using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.GpuDriven;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;


namespace GodotEcsArch.sources.BlackyEngine.Generic;

internal class BlackyManagerSelector
{

    static long idSquare = 1790185379417000;
    static long idCircle = 1790185466097000;
    static int layer = (int)BlackyRenderLayer.Personajes_Arboles_Edificios;
    public static void CreateSelector(Entity ent)
    {
        if (ent.Has<UnitDefinitionComponent>())
        {
            CreateCircle(ent);
        }

        if (ent.Has<ResourceDefinitionComponent>())
        {
            CreateResourceSelector(ent);
        }

        if (ent.Has<BuildingDefinitionComponent>())
        {

        }
    }

    private static void CreateResourceSelector(Entity ent)
    {
        AtlasModsManager.GetSpriteUniqueId(idCircle, out TileSpriteData tileSpriteData);
        var renderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRenderGpu(tileSpriteData.spriteData.idModMaterial);

        // Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(positionTile.X, positionTile.Y);
        var selector = ent.Get<SelectorComponent>();
        Vector2 offset =selector. offset;

        var position = ent.Get<PositionComponent>();
        var positionCenter = position.position;
        var heightRender = position.height;

        float depthOffset = 0;
        float z = CommonAtributes.CalculateSelection(depthOffset, heightRender, layer, positionCenter);

        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);
        float scaleX = (selector.size * 3) / MeshCreator.PixelsToUnits(16f);
        float scaleY = (selector.size * 3) / MeshCreator.PixelsToUnits(16f);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
            BufferSlot.Transform, renderInstance.instanceId, worldPosition.X, worldPosition.Y, worldPosition.Z, scaleX);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
            BufferSlot.StaticData, renderInstance.instanceId, tileSpriteData.spriteData.uv);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
            BufferSlot.Flags, renderInstance.instanceId, tileSpriteData.spriteData.mirrorLocalX, tileSpriteData.spriteData.mirrorLocalY, 1, renderInstance.layer);


        ent.Set(new RenderSelectionGPUComponent(renderInstance.gpuDataIndex, renderInstance.instanceId, renderInstance.layer, layer, offset));
    }

    private static void CreateCircle(Entity ent)
    {
        int sprite = AtlasModsManager.GetSpriteUniqueId(idCircle, out TileSpriteData tileSpriteData);
        var renderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRenderGpu(tileSpriteData.spriteData.idModMaterial);
        var uv = tileSpriteData.spriteData.uv;
        var position = ent.Get<PositionComponent>();
        var moveCollider = ent.Get<MoveColliderComponent>();

        float depthOffset = 0;
        float z = CommonAtributes.CalculateSelection(depthOffset, position.height, layer, position.position);
        Vector3 worldPosition = new(position.position.X + moveCollider.Offset.X, position.position.Y + moveCollider.Offset.Y, z);
        float scaleX = (moveCollider.Radius * 3) / MeshCreator.PixelsToUnits(16f);
        float scaleY = (moveCollider.Radius * 3) / MeshCreator.PixelsToUnits(16f);

        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
              BufferSlot.Transform, renderInstance.instanceId, worldPosition.X, worldPosition.Y, worldPosition.Z, scaleX);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
            BufferSlot.StaticData, renderInstance.instanceId, tileSpriteData.spriteData.uv);
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(renderInstance.gpuDataIndex).SetBufferData(
            BufferSlot.Flags, renderInstance.instanceId, tileSpriteData.spriteData.mirrorLocalX, tileSpriteData.spriteData.mirrorLocalY, 1, renderInstance.layer);

        //ent.Set(new RenderGPUComponent(renderInstance.gpuDataIndex, renderInstance.multimeshRid, renderInstance.instanceId, 0, renderInstance.layer,
        //    layer, depthOffset, tileSpriteData.spriteData.scale, offset));

        ent.Set(new RenderSelectionGPUComponent(renderInstance.gpuDataIndex, renderInstance.instanceId, renderInstance.layer, layer, moveCollider.Offset));


    }
}
