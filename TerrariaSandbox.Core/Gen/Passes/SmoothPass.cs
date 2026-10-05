namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Applies a small smoothing pass to avoid sharp terrain artifacts.
/// </summary>
public sealed class SmoothPass : IGenPass
{
    public string Name => "Smooth";
    public float Weight => 0.5f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        var world = ctx.World;
        int width = world.Width;
        int height = world.Height;

        for (int x = 1; x < width - 1; ++x)
        {
            for (int y = 1; y < height - 1; ++y)
            {
                if (world.Get(x, y) == (ushort)TileType.Grass)
                {
                    int neighbors = 0;
                    if (world.Get(x - 1, y) == (ushort)TileType.Grass) neighbors++;
                    if (world.Get(x + 1, y) == (ushort)TileType.Grass) neighbors++;
                    if (world.Get(x, y - 1) == (ushort)TileType.Grass) neighbors++;
                    if (world.Get(x, y + 1) == (ushort)TileType.Grass) neighbors++;
                    if (neighbors <= 1)
                    {
                        world.Set(x, y, (ushort)TileType.Dirt);
                    }
                }
            }

            p.Report((x + 1f) / width);
        }
    }
}
