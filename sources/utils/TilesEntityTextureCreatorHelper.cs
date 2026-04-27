using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.TilesTexture;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;

public static class TilesEntityTextureCreatorHelper
{
    public static Entity CreateSingle(FlecsManager flecsManager, int idMaterial, int index , Vector2I tilePosition, int renderLayer = 0)
    {
        if (flecsManager == null)
        {
            return default;
        }
        var mat = MasterDataManager.GetData<MaterialData>(idMaterial);
        var instanceRender = AtlasTexturesModsManager.Instance.CreateInstanceRender(mat.idNameMod);        
        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);        
        transform = transform.ScaledLocal(new Godot.Vector3(1, 1, 1));                
        var world  = flecsManager.WorldFlecs.GetCtx<BlackyWorld>();
        var pallete = world.tilesPalette;
        var entity = flecsManager.WorldFlecs.Entity();
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instanceRender.rid, instanceRender.instance,mat.id, instanceRender.layerTexture, renderLayer, 0, 1, Vector2.Zero));
        ushort idInternal = pallete.GetOrCreateTile(mat.idNameMod, (ushort)index);        
        entity.Set(new RenderFrameDataComponent { uvMap = pallete.GetTileUV(idInternal)});
        entity.Set(new PositionComponent { tilePosition = tilePosition, height = 3 });
        entity.Add<DirtyTileRenderTag>();
        entity.Add<TileTextureTag>();
        return entity;
    }
}
