using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
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
    public static Entity CreateSingle(FlecsManager flecsManager, long idSprite, Vector2I tilePosition, int renderLayer = 1)
    {
        if (flecsManager == null)
        {
            return default;
        }

        int idSpriteInternal= AtlasModsManager.GetSpriteUniqueId(idSprite, out TileSpriteData templateSprite);

        //var mat = MasterDataManager.GetData<MaterialData>(idMaterial);
        var instanceRender = AtlasTexturesModsManager.Instance.CreateInstanceRender(templateSprite.spriteData.idModMaterial);
        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform = transform.ScaledLocal(new Godot.Vector3(templateSprite.spriteData.scale, templateSprite.spriteData.scale, 1));
        var world = flecsManager.WorldFlecs.GetCtx<BlackyWorld>();

        //BlackyPersistentTilePalette pallete = world.State.TilePalette;
        //ushort idInternal = pallete.GetOrCreateTile(idMod, (ushort)index);
        //pallete.TryGetTileDataMod(idInternal, out TileSpriteData tileSpriteData);



        // aqui necesito la paleta por region y de acuerdo a eso creamos tile simple o animado
        var entity = flecsManager.WorldFlecs.Entity();
        entity.Set(new RenderTransformComponent(transform));
        entity.Set(new RenderGPUComponent(instanceRender.rid, instanceRender.instance, 0, instanceRender.layerTexture, renderLayer, templateSprite.spriteData.yDepthRender, templateSprite.spriteData.scale, Vector2.Zero));

        entity.Set(new RenderFrameDataComponent { uvMap = templateSprite.spriteData.uv });
        entity.Set(new PositionComponent { tilePosition = tilePosition, height = 10 });
        entity.Add<DirtyTileSpriteTextureRenderTag>();
        entity.Set(new TileSpriteTextureComponent { idSpriteInternal = idSpriteInternal });
        return entity;
    }

    //public static Entity CreateSingle(FlecsManager flecsManager, int idMaterial, string idMod, int index , Vector2I tilePosition, int renderLayer = 1)
    //{
    //    if (flecsManager == null)
    //    {
    //        return default;
    //    }

    //    //var mat = MasterDataManager.GetData<MaterialData>(idMaterial);
    //    var instanceRender = AtlasTexturesModsManager.Instance.CreateInstanceRender(idMod);        
    //    Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);        
    //    transform = transform.ScaledLocal(new Godot.Vector3(1, 1, 1));                
    //    var world  = flecsManager.WorldFlecs.GetCtx<BlackyWorld>();
    //    BlackyPersistentTilePalette pallete = world.State.TilePalette;
    //    ushort idInternal = pallete.GetOrCreateTile(idMod, (ushort)index);
    //    pallete.TryGetTileDataMod(idInternal, out TileSpriteData tileSpriteData);



    //    // aqui necesito la paleta por region y de acuerdo a eso creamos tile simple o animado
    //    var entity = flecsManager.WorldFlecs.Entity();
    //    entity.Set(new RenderTransformComponent(transform));
    //    entity.Set(new RenderGPUComponent(instanceRender.rid, instanceRender.instance,0, instanceRender.layerTexture, renderLayer, 0, 1, Vector2.Zero));
        
    //    entity.Set(new RenderFrameDataComponent { uvMap = tileSpriteData.spriteData.uv});
    //    entity.Set(new PositionComponent { tilePosition = tilePosition, height = 10 });
    //    entity.Add<DirtyTileSpriteTextureRenderTag>();
    //    entity.Set(new TileSpriteTextureComponent { indexTile = idInternal, isAnimated = false });
    //    return entity;
    //}
}
