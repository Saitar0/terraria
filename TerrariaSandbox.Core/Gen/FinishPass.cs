namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Final cleanup pass: smoothing, edge cleanup, and liquid settling.
/// </summary>
public sealed class FinishPass : IGenPass
{
    public string Name => "Finish";
    public float Weight => 0.8f;

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

        for (int x = 1; x < width - 1; ++x)
        {
            for (int y = 1; y < height - 1; ++y)
            {
                ushort tile = world.Get(x, y);
                if (tile == (ushort)TileType.Dirt && IsAdjacentToGrass(world, x, y))
                {
                    world.Set(x, y, (ushort)TileType.Grass);
                }

                if (tile == (ushort)TileType.Water && (world.Get(x, y + 1) == 0 || world.Get(x, y - 1) == 0))
                {
                    world.Set(x, y, (ushort)TileType.Water);
                }
            }

            p.Report((x + 1f) / width);
        }

        for (int x = 0; x < width; ++x)
        {
            for (int y = height - 2; y >= 0; --y)
            {
                if (world.Get(x, y) == 0 && world.Get(x, y + 1) == (ushort)TileType.Water)
                {
                    world.Set(x, y, (ushort)TileType.Water);
                }
            }
        }

        p.Report(1f);
    }

    private static bool IsAdjacentToGrass(World world, int x, int y)
    {
        return world.Get(x - 1, y) == (ushort)TileType.Grass ||
               world.Get(x + 1, y) == (ushort)TileType.Grass ||
               world.Get(x, y - 1) == (ushort)TileType.Grass ||
               world.Get(x, y + 1) == (ushort)TileType.Grass;
    }
}
