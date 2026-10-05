namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Layers and caves inside the terrain.
/// </summary>
public sealed class LayersPass : IGenPass
{
    public string Name => "Layers";
    public float Weight => 1.5f;

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
                ushort tile = world.Get(x, y);
                if (tile == 0)
                {
                    continue;
                }

                if (y > ctx.rockLayer)
                {
                    world.Set(x, y, (ushort)TileType.Stone);
                }
                else if (y > ctx.worldSurface + 6 && (noise.Perlin2D(x * 0.07f, y * 0.07f, 13u) > 0.72f))
                {
                    world.Set(x, y, (ushort)TileType.Stone);
                }

                if (y >= height - 200 && (noise.Perlin2D(x * 0.04f, y * 0.04f, 71u) > 0.62f))
                {
                    world.Set(x, y, (ushort)TileType.Stone);
                }
            }

            p.Report((x + 1f) / width);
        });
    }
}
