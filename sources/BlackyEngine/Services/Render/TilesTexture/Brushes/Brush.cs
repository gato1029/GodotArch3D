namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;

public struct Brush
{
    public (int x, int y)[] Cells;

    public Brush(params (int x, int y)[] cells)
    {
        Cells = cells;
    }
}
