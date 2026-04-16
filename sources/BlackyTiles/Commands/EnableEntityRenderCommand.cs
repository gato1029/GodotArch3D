using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyTiles.Commands;

public class EnableEntityRenderCommand : IRenderCommand
{
    private readonly Entity entity;
    private readonly Vector3 worldPosition;
    private readonly SpriteAnimationData data;

    public EnableEntityRenderCommand(
        Entity entity,
        Vector3 worldPosition,
        SpriteAnimationData data)
    {
        this.entity = entity;
        this.worldPosition = worldPosition;
        this.data = data;
    }

    public void Execute()
    {
        //var dataInstance = MultimeshManager.Instance.CreateInstance(data.idMaterial);
        var dataInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(data.idModMaterial);
            
        
        Transform3D transform = new(Basis.Identity, worldPosition);

        RenderingServer.MultimeshInstanceSetTransform(
            dataInstance.rid,
            dataInstance.instance,
            transform
        );

        RenderingServer.MultimeshInstanceSetCustomData(
            dataInstance.rid,
            dataInstance.instance,
            data.uvFramesArray[0]
        );
        RenderingServer.MultimeshInstanceSetColor(
         dataInstance.rid,
         dataInstance.instance,
         new Godot.Color(0, 0, 0, dataInstance.layerTexture)
     );
        ref var render = ref entity.Ensure<RenderInstanceComponent>();

        render.rid = dataInstance.rid;
        render.instance = dataInstance.instance;
        render.materialId = data.idMaterial;
        render.isActive = true;

        ref var rendergpu = ref entity.Ensure<RenderGPUComponent>();

        rendergpu.rid = dataInstance.rid;
        rendergpu.instance = dataInstance.instance;
        rendergpu.idMaterial = data.idMaterial;
        rendergpu.layerTextureMaterial= dataInstance.layerTexture;     

        entity.Remove<RenderDisabledTag>();
    }
}
