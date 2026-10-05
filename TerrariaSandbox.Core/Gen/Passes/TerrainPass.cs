namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Generates the main terrain silhouette and central spawning plateau.
/// </summary>
public sealed class TerrainPass : IGenPass
{
    public string Name => "Terrain";
    public float Weight => 1.0f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        var noise = new Noise();
        var world = ctx.World;
        int width = world.Width;
        int height = world.Height;
        float[] heights = new float[width];

        float center = width * 0.5f;
        for (int x = 0; x < width; ++x)
        {
            float normalizedX = x / (float)Math.Max(1, width - 1);
            float centerDistance = MathF.Abs(x - center) / MathF.Max(1f, center);
            float plateau = 1f - MathF.Pow(centerDistance, 1.7f);
            float fbm = noise.Fbm1D(x * 0.0065f + 15.7f, 4, 0.52f, 2.1f, 17u);
            float ridge = noise.Ridge1D(x * 0.0102f + 9.1f, 5, 0.61f, 2.2f, 33u);
            float oceanRamp = MathF.Max(0f, 1f - normalizedX) * 0.8f + MathF.Max(0f, normalizedX) * 0.8f;
            float surface = height * 0.42f;
            float baseHeight = surface + (fbm - 0.5f) * 52f + (ridge - 0.5f) * 38f + plateau * 22f - oceanRamp * 34f;
            heights[x] = Math.Clamp(baseHeight, 18f, height - 80f);
        }

        for (int pass = 0; pass < 3; ++pass)
        {
            for (int x = 1; x < width - 1; ++x)
            {
                heights[x] = (heights[x - 1] + heights[x] + heights[x + 1]) / 3f;
            }
        }

        int surfaceY = (int)Math.Round(heights[width / 2]);
        ctx.worldSurface = Math.Clamp(surfaceY, 24, height - 40);
        ctx.rockLayer = Math.Clamp(ctx.worldSurface + 28, 40, height - 12);

        Parallel.For(0, width, x =>
        {
            int top = (int)MathF.Round(heights[x]);
            for (int y = 0; y < height; ++y)
            {
                if (y > top)
                {
                    if (y > ctx.rockLayer)
                    {
                        world.Set(x, y, (ushort)TileType.Dirt);
                    }
                    else
                    {
                        world.Set(x, y, (ushort)TileType.Stone);
                    }
                }
                else if (y == top)
                {
                    ushort surfaceTile = ((top + x) % 7 == 0) ? (ushort)TileType.Sand : (ushort)TileType.Grass;
                    world.Set(x, y, surfaceTile);
                }
                else
                {
                    world.Set(x, y, 0);
                }
            }

            p.Report((x + 1f) / width);
        });
    }
}
