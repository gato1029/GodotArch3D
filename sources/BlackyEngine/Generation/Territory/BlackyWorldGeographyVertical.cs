using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
// (mismos using de BlackyWorldConfig / BlackyWorldSeed / BlackyWorldHeight que en BlackyWorldGeography)

/// <summary>
/// Geografía "de arriba hacia abajo" para juegos top down: el terreno es más alto en un extremo vertical
/// del mapa y más bajo en el opuesto. Usa Altura0..3, AlturaMaxima, Frecuencia, FormaPeso y Sinuosidad
/// de ConfiguracionGeograficaData (la propiedad Forma se ignora).
///   FormaPeso  = cuánto manda el gradiente vertical frente al ruido (0 = solo ruido, 1 = franjas casi rectas)
///   Sinuosidad = cuánto se deforman/ondulan las franjas (0 = rectas)
/// </summary>
public class BlackyWorldGeographyVertical
{
    private readonly BlackyWorldConfig worldConfig;
    private readonly ConfiguracionGeograficaData cfg;
    private readonly BlackyWorldSeed worldSeed;
    private readonly BlackyWorldHeight heightMapWorld;

    private const long MuestrasObjetivo = 4_000_000;

    /// <summary>true = la altura máxima está en Y positiva (MaxY); false = en Y negativa (MinY).</summary>
    public bool HighAtPositiveY { get; set; } = true;

    public float TerrainScale { get; set; } = 1f;
    public bool AutoTerrainScale { get; set; } = true;
    public int ReferenceMapSize { get; set; } = 512;

    public int MinX => -(worldConfig.MapSize.X / 2);
    public int MinY => -(worldConfig.MapSize.Y / 2);
    public int MaxX => MinX + worldConfig.MapSize.X - 1;
    public int MaxY => MinY + worldConfig.MapSize.Y - 1;

    public int GeographySeed => worldSeed.GetRng("geography").Next();

    public BlackyWorldGeographyVertical(BlackyWorldConfig worldConfig, BlackyWorldSeed worldSeed,
        BlackyWorldHeight heightMapWorld, ConfiguracionGeograficaData cfg)
    {
        this.worldConfig = worldConfig;
        this.worldSeed = worldSeed;
        this.heightMapWorld = heightMapWorld;
        this.cfg = cfg;
    }

    public void Generate()
    {
        var size = worldConfig.MapSize;
        int w = size.X, h = size.Y;
        if (w <= 0 || h <= 0) throw new InvalidOperationException($"MapSize inválido: {w}x{h}");
        if (TerrainScale <= 0f) throw new InvalidOperationException("TerrainScale debe ser mayor que 0");
        if (ReferenceMapSize <= 0) throw new InvalidOperationException("ReferenceMapSize debe ser mayor que 0");

        float auto = AutoTerrainScale ? Math.Max(w, h) / (float)ReferenceMapSize : 1f;
        float freq = cfg.Frecuencia / (TerrainScale * auto);

        var field = new VerticalField(cfg, GeographySeed, w, h, MinX, MinY, freq, HighAtPositiveY);
        float[] cuts = ComputeCuts(field, w, h, GetWeights(cfg));
        int maxLevel = cfg.AlturaMaxima;

        byte[] levels = new byte[(long)w * h];
        Parallel.For(0, h, y =>
        {
            long row = (long)y * w;
            for (int x = 0; x < w; x++)
            {
                float v = field.Evaluate(x, y);
                int level = v < cuts[0] ? 0 : v < cuts[1] ? 1 : v < cuts[2] ? 2 : 3;
                levels[row + x] = (byte)Math.Min(level, maxLevel);
            }
        });

        int minX = MinX, minY = MinY;
        for (int y = 0; y < h; y++)
        {
            long row = (long)y * w;
            for (int x = 0; x < w; x++)
                heightMapWorld.SetTopHeight(minX + x, minY + y, levels[row + x]);
        }
    }

    private static float[] ComputeCuts(VerticalField field, int w, int h, float[] weights)
    {
        long total = (long)w * h;
        int s = Math.Max(1, (int)Math.Ceiling(Math.Sqrt((double)total / MuestrasObjetivo)));
        int sw = (w + s - 1) / s, sh = (h + s - 1) / s;

        float[] sample = new float[(long)sw * sh];
        Parallel.For(0, sh, j =>
        {
            for (int i = 0; i < sw; i++)
                sample[j * sw + i] = field.Evaluate(i * s, j * s);
        });
        Array.Sort(sample);

        float[] cum = { weights[0], weights[0] + weights[1], weights[0] + weights[1] + weights[2] };
        float[] cuts = new float[3];
        for (int k = 0; k < 3; k++)
            cuts[k] = cum[k] >= 0.9999f
                ? float.PositiveInfinity
                : sample[Math.Min(sample.Length - 1, (int)(cum[k] * sample.Length))];
        return cuts;
    }

    private static float[] GetWeights(ConfiguracionGeograficaData c)
    {
        float[] p = { c.Altura0, c.Altura1, c.Altura2, c.Altura3 };
        for (int i = c.AlturaMaxima + 1; i < 4; i++) p[i] = 0f;
        float sum = p[0] + p[1] + p[2] + p[3];
        if (sum <= 0f) { p[0] = 1f; return p; }
        for (int i = 0; i < 4; i++) p[i] /= sum;
        return p;
    }

    /// <summary>valor = (1 - peso) * ruido + peso * gradiente vertical ondulado. Thread-safe una vez construido.</summary>
    private sealed class VerticalField
    {
        private readonly BlackyWorldGeography.PerlinNoise noise;
        private readonly BlackyWorldGeography.PerlinNoise warpNoise;
        private readonly float freq, shapeWeight, irregularity;
        private readonly int minX, minY;
        private readonly float invW, invH;
        private readonly bool highAtPositiveY;

        public VerticalField(ConfiguracionGeograficaData cfg, int seed, int w, int h,
            int minX, int minY, float frequency, bool highAtPositiveY)
        {
            noise = new BlackyWorldGeography.PerlinNoise(seed);
            warpNoise = new BlackyWorldGeography.PerlinNoise(seed ^ 0x6A09E667);
            shapeWeight = Math.Clamp(cfg.FormaPeso, 0f, 1f);
            irregularity = cfg.Sinuosidad;
            freq = frequency;
            this.minX = minX;
            this.minY = minY;
            this.highAtPositiveY = highAtPositiveY;
            invW = 1f / w;
            invH = 1f / h;
        }

        /// <summary>x, y son ÍNDICES de celda (0..w-1, 0..h-1); y crece con la Y de mundo.</summary>
        public float Evaluate(int x, int y)
        {
            float n = noise.Fbm((x + minX) * freq, (y + minY) * freq);
            if (shapeWeight <= 0f) return n;

            float u = (x + 0.5f) * invW;
            float v = (y + 0.5f) * invH;               // 0 en MinY ... 1 en MaxY

            float s = highAtPositiveY ? v : 1f - v;    // 1 = lo más alto
            // Ondulación de las franjas (lenta, independiente del detalle del ruido)
            float warp = Math.Clamp((warpNoise.Fbm(u * 3f + 5.1f, v * 3f + 9.7f) - 0.5f) * 4f, -1f, 1f);
            s = Math.Clamp(s + irregularity * 0.5f * warp, 0f, 1f);

            return (1f - shapeWeight) * n + shapeWeight * s;
        }


    }

    // ============================================================
    //  DEBUG: imagen PNG de la geografía
    // ============================================================

    /// <summary>Lado máximo (en píxeles) de la imagen de debug.</summary>
    private const int MaxImageSide = 8192;

    /// <summary>
    /// Solo afecta al DEBUG (.png), no a los datos generados: true = Y+ hacia arriba (la fila de arriba es
    /// MaxY); false = Y+ hacia abajo (la fila de arriba es MinY).
    /// </summary>
    public bool DebugYAxisUp { get; set; } = true;

    /// <summary>
    /// Guarda la geografía como imagen PNG de colores (hasta 8192x8192 píxeles). Lee heightMapWorld.
    /// Colores: 0 azul, 1 verde, 2 ocre, 3 marrón; otra altura = magenta.
    /// Arriba-izquierda = (MinX, MaxY) si DebugYAxisUp (por defecto), o (MinX, MinY) si no.
    /// <paramref name="pixelsPerTile"/>: 0 = automático (los mapas pequeños se amplían hasta ~2048 px).
    ///   Si el mapa es mayor que 8192 se reduce tomando 1 tile de cada N.
    /// Es una ruta del sistema de archivos, no "res://" ni "user://".
    /// </summary>
    public void ExportDebugPng(string path, int pixelsPerTile = 0)
    {
        var size = worldConfig.MapSize;
        int w = size.X, h = size.Y;
        int minX = MinX, minY = MinY;
        int maxSide = Math.Max(w, h);

        int step = 1, scale = 1;
        if (maxSide > MaxImageSide)
            step = (maxSide + MaxImageSide - 1) / MaxImageSide;
        else
        {
            scale = pixelsPerTile > 0 ? pixelsPerTile : Math.Max(1, 2048 / maxSide);
            scale = Math.Clamp(scale, 1, Math.Max(1, MaxImageSide / maxSide));
        }

        int tilesX = (w + step - 1) / step, tilesY = (h + step - 1) / step;
        int iw = tilesX * scale, ih = tilesY * scale;

        byte[] palette =
        {
             46,  94, 140,   // 0 azul
             94, 158,  78,   // 1 verde
            194, 176,  90,   // 2 ocre
            140, 106,  75,   // 3 marrón
            255,   0, 255    // otra altura
        };

        string dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        byte[] cache = new byte[iw];
        int cachedTileRow = -1;

        WriteIndexedPng(path, iw, ih, palette, (py, row) =>
        {
            int ty = py / scale;
            if (ty != cachedTileRow)   // las filas repetidas por la ampliación no vuelven a leer el mapa
            {
                int worldY = DebugYAxisUp ? (minY + h - 1) - ty * step : minY + ty * step;
                for (int tx = 0; tx < tilesX; tx++)
                {
                    int v = (int)heightMapWorld.GetTopHeight(minX + tx * step, worldY);
                    byte idx = (byte)(v >= 0 && v <= 3 ? v : 4);
                    int px = tx * scale;
                    for (int k = 0; k < scale; k++) cache[px + k] = idx;
                }
                cachedTileRow = ty;
            }
            Buffer.BlockCopy(cache, 0, row, 1, iw);   // row[0] es el byte de filtro PNG
        });
    }

    /// <summary>PNG de 8 bits con paleta. fillRow rellena row[1..width] con los índices de la fila y.</summary>
    private static void WriteIndexedPng(string path, int width, int height, byte[] paletteRgb, Action<int, byte[]> fillRow)
    {
        using var compressed = new MemoryStream();
        using (var z = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
        {
            byte[] row = new byte[width + 1];
            for (int y = 0; y < height; y++)
            {
                row[0] = 0;   // filtro "None"
                fillRow(y, row);
                z.Write(row, 0, row.Length);
            }
        }

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        fs.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

        byte[] ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, width);
        WriteBigEndian(ihdr, 4, height);
        ihdr[8] = 8;    // bits por píxel
        ihdr[9] = 3;    // color indexado (paleta)
        WriteChunk(fs, "IHDR", ihdr, ihdr.Length);
        WriteChunk(fs, "PLTE", paletteRgb, paletteRgb.Length);
        WriteChunk(fs, "IDAT", compressed.GetBuffer(), (int)compressed.Length);
        WriteChunk(fs, "IEND", new byte[0], 0);
    }

    private static void WriteBigEndian(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)(value >> 24);
        buf[offset + 1] = (byte)(value >> 16);
        buf[offset + 2] = (byte)(value >> 8);
        buf[offset + 3] = (byte)value;
    }

    private static void WriteChunk(Stream s, string type, byte[] data, int length)
    {
        byte[] head = new byte[8];
        WriteBigEndian(head, 0, length);
        for (int i = 0; i < 4; i++) head[4 + i] = (byte)type[i];
        s.Write(head, 0, 8);
        s.Write(data, 0, length);

        uint crc = 0xFFFFFFFFu;
        crc = Crc32Update(crc, head, 4, 4);
        crc = Crc32Update(crc, data, 0, length);
        crc ^= 0xFFFFFFFFu;
        byte[] tail = new byte[4];
        WriteBigEndian(tail, 0, unchecked((int)crc));
        s.Write(tail, 0, 4);
    }

    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var t = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            t[n] = c;
        }
        return t;
    }

    private static uint Crc32Update(uint crc, byte[] buf, int offset, int len)
    {
        for (int i = 0; i < len; i++)
            crc = CrcTable[(crc ^ buf[offset + i]) & 0xFF] ^ (crc >> 8);
        return crc;
    }
}