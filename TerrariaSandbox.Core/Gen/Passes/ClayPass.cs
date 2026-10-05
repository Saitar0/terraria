namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Applies clay-like patches in the upper soil layers.
/// </summary>
public sealed class ClayPass : IGenPass
{
    public string Name => "Clay";
    public float Weight => 0.4f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        var noise = new Noise();
        var world = ctx.World;
        int width = world.Width;
        int height = world.Height;

        Parallel.For(0, width, x =>
        {
            for (int y = 0; y < height; ++y)
            {
                if (world.Get(x, y) != (ushort)TileType.Dirt)
                {
                    continue;
                }

                float clayChance = noise.Perlin2D(x * 0.03f, y * 0.05f, 91u);
                if (clayChance > 0.65f && y > ctx.worldSurface - 12 && y < ctx.worldSurface + 16)
                {
                    world.Set(x, y, (ushort)TileType.Sand);
                }
            }

            p.Report((x + 1f) / width);
        });
    }
}
