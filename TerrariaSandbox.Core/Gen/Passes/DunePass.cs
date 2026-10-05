namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Adds dunes and sand bands near the shore.
/// </summary>
public sealed class DunePass : IGenPass
{
    public string Name => "Dunes";
    public float Weight => 0.6f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        var noise = new Noise();
        var world = ctx.World;
        int width = world.Width;
        int height = world.Height;

        Parallel.For(0, width, x =>
        {
            int surface = ctx.worldSurface;
            float duneStrength = noise.Fbm1D(x * 0.016f, 3, 0.55f, 2f, 11u);
            int sandBand = (int)Math.Round(surface + duneStrength * 8f);

            for (int y = 0; y < height; ++y)
            {
                ushort tile = world.Get(x, y);
                if (tile == (ushort)TileType.Grass && y >= sandBand - 1 && y <= sandBand + 2)
                {
                    world.Set(x, y, (ushort)TileType.Sand);
                }

                if (y >= height - 40 && (x % 9 == 0))
                {
                    world.Set(x, y, (ushort)TileType.Stone);
                }
            }

            p.Report((x + 1f) / width);
        });
    }
}
