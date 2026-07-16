using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.managers.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Commands;

public class DestroyTileInstanceTextureCommand : IRenderCommand
{

    public readonly int height;
    public readonly int layer;
    public readonly int x;
    public readonly int y;
    private readonly TileRenderTextureInstance textureInstance;
    private readonly BlackyChunkRenderTiles chunkRender;
    private readonly bool dual;
    private readonly bool isMarkFreeChunk;

    public DestroyTileInstanceTextureCommand(int height, int layer, int x, int y,bool dual, TileRenderTextureInstance textureInstance, BlackyChunkRenderTiles chunkRender)
    {
        this.dual = dual;
        this.height = height;
        this.layer = layer;
        this.x = x;
        this.y = y;
        this.textureInstance = textureInstance;
        this.chunkRender = chunkRender;
        isMarkFreeChunk = false;
    }
    public DestroyTileInstanceTextureCommand(TileRenderTextureInstance textureInstance)
    {
        this.textureInstance = textureInstance;
        isMarkFreeChunk = true;
    }

    public void Execute()
    {
        if (!textureInstance.IsDestroyed)
        { //tiene que estar marcado para destruccion
            return;
        }
        AtlasTexturesModsManager.Instance.FreeInstance(
            textureInstance.Rid,
            textureInstance.InstanceId
        );
        if (textureInstance.HasEntity)
        {            
            textureInstance.Entity.Destruct();
        }
        // 3. Eliminación del diccionario de forma defensiva
        // Agregamos la comprobación chunkRender != null y !IsDestroyed
        if (!isMarkFreeChunk && chunkRender != null && !chunkRender.IsDestroyed)
        {
            chunkRender.Remove((height, layer, x, y));
        }

    }
}
