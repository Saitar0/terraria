namespace TerrariaSandbox.Core.Gen;

public enum BiomeType
{
    Forest,
    Desert,
    Snow,
    Jungle,
    Ocean,
    Corrupted
}

/// <summary>
/// Applies deterministic horizontal biome bands across the world surface.
/// </summary>
public sealed class BiomePass : IGenPass
{
    public string Name => "Biome";
    public float Weight => 1.2f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        if (ctx is null || ctx.World is null)
        {
            p.Report(1f);
            return;
        }

        var world = ctx.World;
        var noise = new Noise();
        int width = world.Width;
        int height = world.Height;

        for (int x = 0; x < width; ++x)
        {
            float band = noise.Fbm1D(x * 0.017f + 18.1f, 4, 0.55f, 2.1f, 991u + (uint)x);
            float edge = MathF.Min(x / (float)Math.Max(1, width), 1f - (x / (float)Math.Max(1, width)));
            int surfaceY = FindSurfaceY(world, x);

            BiomeType biome = BiomeFromBand(band, edge, x, width, ctx.Rng);
            ApplyBiomeAt(world, x, surfaceY, biome, height, noise, ctx.Rng);

            if (x < width * 0.12f || x > width * 0.88f)
            {
                FillOceanBands(world, x, surfaceY, height);
            }

            p.Report((x + 1f) / width);
        }
    }

    private static BiomeType BiomeFromBand(float band, float edge, int x, int worldWidth, Pcg32 rng)
    {
        if (edge < 0.12f)
        {
            return BiomeType.Ocean;
        }

        if (band < 0.18f)
        {
            return BiomeType.Desert;
        }

        if (band < 0.42f)
        {
            return BiomeType.Forest;
        }

        if (band < 0.60f)
        {
            return BiomeType.Jungle;
        }

        if (band < 0.78f)
        {
            return BiomeType.Snow;
        }

        return ((x + (int)rng.NextRange(0, 256)) % 7 == 0) ? BiomeType.Corrupted : BiomeType.Snow;
    }

    private static void ApplyBiomeAt(World world, int x, int surfaceY, BiomeType biome, int height, Noise noise, Pcg32 rng)
    {
        int startY = Math.Max(0, surfaceY - 12);
        int endY = Math.Min(height - 1, surfaceY + 18);

        for (int y = startY; y <= endY; ++y)
        {
            ushort tile = world.Get(x, y);
            if (tile == 0)
            {
                continue;
            }

            if (y == surfaceY)
            {
                world.Set(x, y, biome switch
                {
                    BiomeType.Desert => (ushort)TileType.Sand,
                    BiomeType.Snow => (ushort)TileType.Snow,
                    BiomeType.Jungle => (ushort)TileType.JungleGrass,
                    BiomeType.Corrupted => (ushort)TileType.CorruptGrass,
                    BiomeType.Ocean => (ushort)TileType.Sand,
                    _ => (ushort)TileType.Grass
                });
            }
            else if (y > surfaceY && y <= surfaceY + 4)
            {
                world.Set(x, y, biome switch
                {
                    BiomeType.Desert => (ushort)TileType.Sand,
                    BiomeType.Snow => (ushort)TileType.Snow,
                    BiomeType.Jungle => (ushort)TileType.JungleDirt,
                    BiomeType.Corrupted => (ushort)TileType.CorruptStone,
                    BiomeType.Ocean => (ushort)TileType.Sand,
                    _ => (ushort)TileType.Dirt
                });
            }
            else if (tile == (ushort)TileType.Stone || tile == (ushort)TileType.Dirt || tile == (ushort)TileType.Grass || tile == (ushort)TileType.Sand)
            {
                world.Set(x, y, biome switch
                {
                    BiomeType.Desert => (ushort)TileType.Sandstone,
                    BiomeType.Snow => (ushort)TileType.Ice,
                    BiomeType.Jungle => (ushort)TileType.JungleDirt,
                    BiomeType.Corrupted => (ushort)TileType.CorruptStone,
                    BiomeType.Ocean => (ushort)TileType.Sand,
                    _ => (ushort)TileType.Stone
                });
            }
        }

        if (biome == BiomeType.Corrupted && rng.NextRange(0, 8) == 0)
        {
            int spreadY = Math.Clamp(surfaceY + rng.NextRange(-3, 4), 0, height - 1);
            for (int yy = spreadY - 1; yy <= spreadY + 1; ++yy)
            {
                if ((uint)yy >= (uint)height)
                {
                    continue;
                }

                if (noise.Fbm2D(x * 0.16f, yy * 0.16f, 3, 0.5f, 2f, 11u) > 0.55f)
                {
                    world.Set(x, yy, (ushort)TileType.CorruptStone);
                }
            }
        }
    }

    private static void FillOceanBands(World world, int x, int surfaceY, int height)
    {
        int oceanFloor = Math.Max(0, Math.Min(height - 1, surfaceY + 3));
        for (int y = oceanFloor; y < Math.Min(height, oceanFloor + 6); ++y)
        {
            if (world.Get(x, y) == 0)
            {
                world.Set(x, y, (ushort)TileType.Water);
            }
        }
    }

    private static int FindSurfaceY(World world, int x)
    {
        int height = world.Height;
        for (int y = 0; y < height; ++y)
        {
            if (world.Get(x, y) != 0)
            {
                return y;
            }
        }

        return Math.Clamp(height / 2, 0, height - 1);
    }
}
