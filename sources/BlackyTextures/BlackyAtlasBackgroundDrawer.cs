using Godot;

namespace GodotEcsArch.sources.BlackyTextures
{
    public static class BlackyAtlasBackgroundDrawer
    {
        private static readonly Color colorA = new Color("#262B33");
        private static readonly Color colorB = new Color("#2D333C");

        private const int extraCellsAround = 30;

        public static void Draw(
            Node2D canvas,
            Vector2 atlasDrawStart,
            int atlasColumns,
            int atlasRows,
            Vector2I cellSize)
        {
            int startX = -extraCellsAround;
            int startY = -extraCellsAround;

            int endX = atlasColumns + extraCellsAround;
            int endY = atlasRows + extraCellsAround;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    Color c = ((x + y) % 2 == 0) ? colorA : colorB;

                    Rect2 rect = new Rect2(
                        atlasDrawStart + new Vector2(x * cellSize.X, y * cellSize.Y),
                        cellSize
                    );

                    canvas.DrawRect(rect, c);
                }
            }
        }
    }
}