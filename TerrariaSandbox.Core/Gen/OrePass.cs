namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Generates ore veins as deterministic depth-based deposits within the stone layer.
/// </summary>
public sealed class OreDef
{
    public OreDef(string name, ushort tileType, int yMin, int yMax, float factor, int minStrength, int maxStrength, int minSteps, int maxSteps)
    {
        Name = name;
        TileType = tileType;
        YMin = yMin;
        YMax = yMax;
        Factor = factor;
        MinStrength = minStrength;
        MaxStrength = maxStrength;
        MinSteps = minSteps;
        MaxSteps = maxSteps;
    }

    public string Name { get; }
    public ushort TileType { get; }
    public int YMin { get; }
    public int YMax { get; }
    public float Factor { get; }
    public int MinStrength { get; }
    public int MaxStrength { get; }
    public int MinSteps { get; }
    public int MaxSteps { get; }
}

public sealed class OrePass : IGenPass
{
    public OrePass()
        : this(DefaultOres())
    {
    }

    public OrePass(OreDef[] ores)
    {
        Ores = ores ?? Array.Empty<OreDef>();
    }

    public string Name => "Ore";
    public float Weight => 1.8f;
    public OreDef[] Ores { get; }

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        if (ctx is null || ctx.World is null)
        {
            p.Report(1f);
            return;
        }

        var world = ctx.World;
        int width = world.Width;
        int height = world.Height;
        int oreIndex = 0;

        foreach (OreDef ore in Ores)
        {
            int quantity = Math.Max(8, (int)MathF.Round(width * height * ore.Factor));
            for (int i = 0; i < quantity; ++i)
            {
                int x = ctx.Rng.NextRange(0, width);
                int y = ctx.Rng.NextRange(Math.Max(0, ore.YMin), Math.Min(height, ore.YMax));
                int strength = ctx.Rng.NextRange(ore.MinStrength, ore.MaxStrength + 1);
                int steps = ctx.Rng.NextRange(ore.MinSteps, ore.MaxSteps + 1);

                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }

                if (IsOreCandidate(world, x, y))
                {
                    var runner = new TileRunner(world, ctx.Rng, new Noise());
                    runner.Run(x, y, strength, steps, ore.TileType, true);
                }
            }

            oreIndex++;
            p.Report((oreIndex + 0f) / Math.Max(1, Ores.Length));
        }

        p.Report(1f);
    }

    private static bool IsOreCandidate(World world, int x, int y)
    {
        ushort tile = world.Get(x, y);
        return tile == (ushort)TileType.Stone || tile == (ushort)TileType.Dirt || tile == (ushort)TileType.Grass || tile == (ushort)TileType.Sand || tile == (ushort)TileType.Snow;
    }

    private static OreDef[] DefaultOres()
    {
        return new[]
        {
            new OreDef("Copper", (ushort)TileType.CopperOre, 18, 90, 0.0009f, 3, 7, 3, 7),
            new OreDef("Silver", (ushort)TileType.SilverOre, 32, 140, 0.00055f, 3, 6, 3, 7),
            new OreDef("Gold", (ushort)TileType.GoldOre, 55, 170, 0.00026f, 3, 6, 3, 6),
            new OreDef("Gem", (ushort)TileType.Gem, 40, 120, 0.00018f, 2, 4, 3, 6)
        };
    }
}
