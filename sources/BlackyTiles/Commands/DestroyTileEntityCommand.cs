using GodotEcsArch.sources.BlackyTiles.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;
public class DestroyTileEntityCommand : IRenderCommand
{
    private readonly BlackyTileRenderInstance renderData;

    public DestroyTileEntityCommand(BlackyTileRenderInstance renderData)
    {
        this.renderData = renderData;
    }

    public void Execute()
    {
        if (!renderData.HasEntityReference)
            return;

        renderData.DestroyEntity();
        
    }
}