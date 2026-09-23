using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Data;
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
    }

    private static void CreateCircle(Entity ent)
    {
        int sprite = AtlasModsManager.GetSpriteUniqueId(idCircle,out TileSpriteData tileSpriteData);        
        var instance = AtlasTexturesModsManager.Instance.CreateInstanceRender(tileSpriteData.spriteData.idModMaterial);
        var uv = tileSpriteData.spriteData.uv;
        var position =  ent.Get<PositionComponent>();
        var moveCollider = ent.Get<MoveColliderComponent>();
        
        float depthOffset = 0;
        float z = CommonAtributes.CalculateSelection(depthOffset, position.height, layer, position.position);

        Transform3D transform = new Transform3D(Basis.Identity, Godot.Vector3.Zero);
        transform.Origin = new Godot.Vector3(position.position.X +moveCollider.Offset.X , position.position.Y +moveCollider.Offset.Y , z);

        float scaleX = (moveCollider.Radius * 3) / MeshCreator.PixelsToUnits(16f);
        float scaleY = (moveCollider.Radius * 3) / MeshCreator.PixelsToUnits(16f);

        transform = transform.ScaledLocal(new Godot.Vector3(scaleX,scaleY, 1));

        RenderingServer.MultimeshInstanceSetTransform(instance.rid, instance.instance, transform); // luego comentar esto
        RenderingServer.MultimeshInstanceSetCustomData(instance.rid, instance.instance, uv); 
        RenderingServer.MultimeshInstanceSetColor(instance.rid, instance.instance, new Color(1, 1, 1, instance.layerTexture));

        ent.Set(new RenderSelectionGPUComponent(instance.rid, instance.instance, instance.layerTexture,layer,moveCollider.Offset));

        
    }
}
