using Godot;
using GodotEcsArch.sources.BlackyTiles.Tiles;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;
public class CreateTileInstanceCommand : IRenderCommand
{
    private readonly Vector3 worldPosition;

    private readonly int height;
    private readonly int layer;
    private readonly int localX;
    private readonly int localY;

    private readonly SpriteData data;

    private readonly Action<BlackyTileRenderInstance> onCreated;

    public CreateTileInstanceCommand(
        Vector3 worldPosition,
        int height,
        int layer,
        int localX,
        int localY,
        SpriteData data,
        Action<BlackyTileRenderInstance> onCreated)
    {
        this.worldPosition = worldPosition;

        this.height = height;
        this.layer = layer;
        this.localX = localX;
        this.localY = localY;

        this.data = data;

        this.onCreated = onCreated;
    }

    public void Execute()
    {
        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(data.scale, data.scale, 1));

        //var dataInstance = MultimeshManager.Instance.CreateInstance(data.idMaterial);

        var dataInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(data.idModMaterial);
        RenderingServer.MultimeshInstanceSetTransform(
            dataInstance.rid,
            dataInstance.instance,
            transform
        );

        RenderingServer.MultimeshInstanceSetCustomData(
            dataInstance.rid,
            dataInstance.instance,
            data.GetUv()
        );

        RenderingServer.MultimeshInstanceSetColor(
            dataInstance.rid,
            dataInstance.instance,
            new Color(0, 0, 0, dataInstance.layerTexture)
        );

        var renderData = new BlackyTileRenderInstance(
            dataInstance.rid,
            dataInstance.instance,
            data.idMaterial,height,layer,localX, localY
        );

        // devolver el resultado
        onCreated?.Invoke(renderData);
    }
}