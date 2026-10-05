using System.Collections.ObjectModel;

namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Executes deterministic world-generation passes in order.
/// </summary>
public sealed class WorldGenerator
{
    private readonly List<IGenPass> _passes;

    public WorldGenerator()
        : this(CreateDefaultPasses())
    {
    }

    public WorldGenerator(IEnumerable<IGenPass> passes)
    {
        _passes = new List<IGenPass>(passes);
    }

    public ReadOnlyCollection<IGenPass> Passes => _passes.AsReadOnly();

    public WorldGenContext Generate(World world, uint seed, IProgress<float>? progress = null)
    {
        var rng = new Pcg32(seed);
        world = world ?? throw new ArgumentNullException(nameof(world));
        var context = new WorldGenContext(world, rng);

        int passCount = _passes.Count;
        if (passCount == 0)
        {
            progress?.Report(1f);
            return context;
        }

        for (int i = 0; i < passCount; ++i)
        {
            IGenPass pass = _passes[i];
            float start = i / (float)passCount;
            pass.Run(context, new Progress<float>(value =>
            {
                float normalized = start + (value * (1f / passCount));
                progress?.Report(Math.Clamp(normalized, 0f, 1f));
            }));
        }

        progress?.Report(1f);
        return context;
    }

    public WorldGenContext Generate(World world, int seed, IProgress<float>? progress = null)
    {
        return Generate(world, (uint)seed, progress);
    }

    public WorldGenContext Generate(World world, string seedText, IProgress<float>? progress = null)
    {
        uint seed = StableHash(seedText);
        return Generate(world, seed, progress);
    }

    public static uint StableHash(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0x9E3779B9u;
        }

        uint hash = 2166136261u;
        foreach (char c in value)
        {
            hash ^= c;
            hash *= 16777619u;
        }

        return hash;
    }

    public static void WriteMinimapPng(World world, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("O caminho do PNG é obrigatório.", nameof(outputPath));
        }

        int width = Math.Min(512, Math.Max(64, world.Width / 16));
        int height = Math.Min(256, Math.Max(32, world.Height / 16));

        using var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var graphics = System.Drawing.Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.Color.Black);

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int worldX = x * world.Width / width;
                int worldY = y * world.Height / height;
                ushort tile = world.Get(worldX, worldY);

                System.Drawing.Color color = tile switch
                {
                    (ushort)TileType.Grass => System.Drawing.Color.ForestGreen,
                    (ushort)TileType.Dirt => System.Drawing.Color.SaddleBrown,
                    (ushort)TileType.Stone => System.Drawing.Color.Gray,
                    (ushort)TileType.Sand => System.Drawing.Color.Khaki,
                    (ushort)TileType.Water => System.Drawing.Color.DodgerBlue,
                    _ => System.Drawing.Color.Black
                };

                bitmap.SetPixel(x, y, color);
            }
        }

        bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
    }

    private static IEnumerable<IGenPass> CreateDefaultPasses()
    {
        return new IGenPass[]
        {
            new TerrainPass(),
            new BiomePass(),
            new DunePass(),
            new LayersPass(),
            new CavePass(),
            new OrePass(),
            new TreePass(),
            new DungeonPass(),
            new ConnectivityPass(),
            new ClayPass(),
            new SmoothPass(),
            new FinishPass()
        };
    }
}
