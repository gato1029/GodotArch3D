using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.utils;
internal class SpriteHelper
{
    public static Entity CreateEntity(SpriteData spriteRender, float scale, Vector2 worldPosition, int layer, Vector2 originOffset, int zOrdering = 0)
    {
        Entity entity = EcsManager.Instance.World.Create();
        

        Transform3D transform3D = new Transform3D(Basis.Identity, new Vector3(worldPosition.X, worldPosition.Y, worldPosition.Y));
        transform3D = transform3D.ScaledLocal(new Vector3(scale, scale, 1));

        var instanceComplex = MultimeshManager.Instance.CreateInstance(spriteRender.idMaterial);

        SpriteRenderGPUComponent spriteRenderGPU = new SpriteRenderGPUComponent();
        spriteRenderGPU.idMaterial = spriteRender.idMaterial;
        spriteRenderGPU.rid = instanceComplex.rid;
        spriteRenderGPU.instance = instanceComplex.instance;
        spriteRenderGPU.arrayPositiontexture = instanceComplex.materialBatchPosition;
        spriteRenderGPU.uvMap = new Color(spriteRender.xFormat, spriteRender.yFormat, spriteRender.widhtFormat, spriteRender.heightFormat);
        spriteRenderGPU.transform = transform3D;
        spriteRenderGPU.layerRender = layer;
        spriteRenderGPU.zOrdering = zOrdering;
        spriteRenderGPU.originOffset = originOffset;
        spriteRenderGPU.scale = scale;

        PositionComponent position = new PositionComponent();
        position.position = worldPosition;

        TilePositionComponent tilePositionComponent = new TilePositionComponent();
        tilePositionComponent.x = 0;
        tilePositionComponent.y = 0;
        entity.Add(spriteRenderGPU);
        entity.Add(position);
        entity.Add(tilePositionComponent);

        RenderingServer.MultimeshInstanceSetTransform(instanceComplex.rid, instanceComplex.instance, transform3D);
        RenderingServer.MultimeshInstanceSetCustomData(instanceComplex.rid, instanceComplex.instance, spriteRenderGPU.uvMap);
        RenderingServer.MultimeshInstanceSetColor(instanceComplex.rid, instanceComplex.instance, new Color(0, 0, 0, instanceComplex.materialBatchPosition));
        return entity;
    }
}
