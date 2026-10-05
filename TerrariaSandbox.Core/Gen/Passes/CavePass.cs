namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Carves cave networks by combining worm tunnels, cellular caverns, and noise-based chambers.
/// </summary>
public sealed class CavePass : IGenPass
{
    public string Name => "Caves";
    public float Weight => 1.8f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        if (ctx is null || ctx.World is null)
        {
            p.Report(1f);
            return;
        }

        var world = ctx.World;
        var noise = new Noise();
        var runner = new TileRunner(world, ctx.Rng, noise);
        int width = world.Width;
        int height = world.Height;
        int minLayer = Math.Max(24, ctx.worldSurface + 18);
        int maxLayer = Math.Min(height - 40, Math.Max(minLayer + 30, ctx.rockLayer + 70));

        int wormCount = Math.Max(10, width / 32);
        for (int i = 0; i < wormCount; ++i)
        {
            int x = ctx.Rng.NextRange(0, width);
            int y = ctx.Rng.NextRange(minLayer, maxLayer);
            runner.Run(x, y, 2 + ctx.Rng.NextRange(0, 2), 52 + ctx.Rng.NextRange(0, 40), 0, true);
        }

        int smallHoleCount = Math.Max(24, width / 18);
        for (int i = 0; i < smallHoleCount; ++i)
        {
            int x = ctx.Rng.NextRange(0, width);
            int y = ctx.Rng.NextRange(minLayer, maxLayer);
            int radius = 1 + ctx.Rng.NextRange(0, 2);

            for (int yy = y - radius; yy <= y + radius; ++yy)
            {
                if ((uint)yy >= (uint)height)
                {
                    continue;
                }

                for (int xx = x - radius; xx <= x + radius; ++xx)
                {
                    if ((uint)xx >= (uint)width)
                    {
                        continue;
                    }

                    int dx = xx - x;
                    int dy = yy - y;
                    if ((dx * dx) + (dy * dy) <= radius * radius + 1)
                    {
                        world.Set(xx, yy, 0);
                    }
                }
            }
        }

        int deepStart = Math.Max(20, ctx.worldSurface + 14);
        int deepEnd = Math.Min(height - 20, deepStart + Math.Max(24, (height - deepStart) / 3));

        for (int x = 0; x < width; ++x)
        {
            float ridge = noise.Fbm1D(x * 0.02f, 4, 0.58f, 2.2f, 77u);
            int band = (int)MathF.Round((ctx.worldSurface + 24) + (ridge - 0.5f) * 30f);
            for (int y = deepStart; y < deepEnd && y < band + 12; ++y)
            {
                if (world.Get(x, y) != 0 && noise.Perlin2D(x * 0.08f, y * 0.08f, 59u + (uint)band) > 0.72f)
                {
                    world.Set(x, y, 0);
                }
            }

            p.Report((x + 1f) / width);
        }

        var cellular = new CellularCaves(width, height);
        cellular.Apply(world, deepStart, deepEnd, 4, 5);

        var noiseCaves = new NoiseCaves();
        noiseCaves.Apply(world, deepStart, deepEnd, (uint)ctx.Rng.NextRange(1, int.MaxValue), ctx.worldSurface);

        p.Report(1f);
    }
}
