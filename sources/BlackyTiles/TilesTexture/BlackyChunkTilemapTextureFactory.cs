namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public static class BlackyChunkTilemapTextureFactory
{
    public static IBlackyChunkTilemapTexture Create(
        TilePaletteCount palette,
        int layerIndex,
        int size,
        int chunkWorldX,
        int chunkWorldY)
    {
        if (palette.Count <= byte.MaxValue)
        {
            return new BlackyChunkTilemapTextureByte(
                layerIndex, size, chunkWorldX, chunkWorldY);
        }
        else
        {
            return new BlackyChunkTilemapTextureUShort(
                layerIndex, size, chunkWorldX, chunkWorldY);
        }
    }
    public static IBlackyChunkTilemapTexture Create(    
    int layerIndex,
    int size,
    int chunkWorldX,
    int chunkWorldY)
    {        
            return new BlackyChunkTilemapTextureUShort(
            layerIndex, size, chunkWorldX, chunkWorldY);        
    }
}