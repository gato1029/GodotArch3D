using Godot;
using GodotEcsArch.sources.BlackyTiles.Tiles;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Multimesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;
public class RemoveInstanceCommand : IRenderCommand
{
    private readonly BlackyTileRenderInstance renderData;

    public RemoveInstanceCommand(BlackyTileRenderInstance renderData)
    {
        this.renderData = renderData;
    }

    public void Execute()
    {
        if (renderData.IsDestroyed == false)
            return;
        if (renderData == null)
            return;
        AtlasTexturesModsManager.Instance.FreeInstance(
            renderData.rid,
            renderData.Instance            
        );
        //MultimeshManager.Instance.FreeInstance(
        //    renderData.rid,
        //    renderData.Instance,
        //    renderData.MaterialId
        //);
    }
}