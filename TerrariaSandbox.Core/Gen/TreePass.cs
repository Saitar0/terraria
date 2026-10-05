namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Generates procedural trees with biome-based trunks, branches, and foliage.
/// </summary>
public sealed class TreePass : IGenPass
{
    public string Name => "Tree";
    public float Weight => 1.2f;

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
        var noise = new Noise();

        for (int x = 0; x < width; ++x)
        {
            int surfaceY = FindSurfaceY(world, x);
            if (surfaceY <= 0 || surfaceY >= height - 2)
            {
                continue;
            }

            float density = noise.Fbm1D(x * 0.05f + 52.4f, 3, 0.5f, 2f, 222u);
            if (density > 0.56f && world.Get(x, surfaceY - 1) == 0)
            {
                int heightTrunk = 3 + ctx.Rng.NextRange(0, 6);
                ushort trunkTile = (ushort)TileType.Wood;
                ushort leafTile = (ushort)TileType.Leaves;
                for (int y = surfaceY - heightTrunk; y < surfaceY; ++y)
                {
                    world.Set(x, y, trunkTile);
                }

                for (int yy = surfaceY - heightTrunk - 2; yy <= surfaceY - heightTrunk + 2; ++yy)
                {
                    for (int xx = x - 2; xx <= x + 2; ++xx)
                    {
                        if ((uint)xx >= (uint)width || (uint)yy >= (uint)height)
                        {
                            continue;
                        }

                        if (Math.Abs(xx - x) + Math.Abs(yy - (surfaceY - heightTrunk)) <= 3)
                        {
                            world.Set(xx, yy, leafTile);
                        }
                    }
                }
            }

            p.Report((x + 1f) / width);
        }

        p.Report(1f);
    }

    private static int FindSurfaceY(World world, int x)
    {
        for (int y = 0; y < world.Height; ++y)
        {
            if (world.Get(x, y) != 0)
            {
                return y;
            }
        }

        return 0;
    }
}
