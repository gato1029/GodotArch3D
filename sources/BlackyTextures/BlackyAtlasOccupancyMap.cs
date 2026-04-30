using Godot;

namespace GodotEcsArch.sources.BlackyTextures
{
    public class BlackyAtlasOccupancyMap
    {
        private bool[,] occupiedMap;

        public int Columns { get; private set; }
        public int Rows { get; private set; }

        public void Build(Texture2D texture, Vector2I cellSize)
        {
            if (texture == null)
                return;

            Image img = texture.GetImage();

            Columns = Mathf.CeilToInt(texture.GetWidth() / (float)cellSize.X);
            Rows = Mathf.CeilToInt(texture.GetHeight() / (float)cellSize.Y);

            occupiedMap = new bool[Columns, Rows];

            for (int cy = 0; cy < Rows; cy++)
            {
                for (int cx = 0; cx < Columns; cx++)
                {
                    occupiedMap[cx, cy] = !IsTileEmpty(img, cx, cy, cellSize);
                }
            }
        }

        private bool IsTileEmpty(Image img, int cellX, int cellY, Vector2I cellSize)
        {
            int px = cellX * cellSize.X;
            int py = cellY * cellSize.Y;

            int endX = Mathf.Min(px + cellSize.X, img.GetWidth());
            int endY = Mathf.Min(py + cellSize.Y, img.GetHeight());

            for (int y = py; y < endY; y++)
            {
                for (int x = px; x < endX; x++)
                {
                    if (img.GetPixel(x, y).A > 0.01f)
                        return false;
                }
            }

            return true;
        }

        public bool IsEmpty(int cellX, int cellY)
        {
            if (occupiedMap == null)
                return true;

            if (cellX < 0 || cellY < 0 || cellX >= Columns || cellY >= Rows)
                return true;

            return !occupiedMap[cellX, cellY];
        }
        public void Clear()
        {
            occupiedMap = null;
            Columns = 0;
            Rows = 0;
        }
    }
}