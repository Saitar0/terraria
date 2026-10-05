namespace TerrariaSandbox.Core;

/// <summary>
/// Bitmask-based frame selection for solid tiles.
/// </summary>
public static class AutoTile
{
    public static readonly byte[] Lut =
    [
        0, 1, 2, 3,
        0, 1, 2, 3,
        0, 1, 2, 3,
        0, 1, 2, 3
    ];

    public static int GetMask(World world, int x, int y)
    {
        int mask = 0;

        if (IsSolid(world, x, y - 1))
        {
            mask |= 1;
        }

        if (IsSolid(world, x + 1, y))
        {
            mask |= 2;
        }

        if (IsSolid(world, x, y + 1))
        {
            mask |= 4;
        }

        if (IsSolid(world, x - 1, y))
        {
            mask |= 8;
        }

        return mask;
    }

    public static void Frame(World world, int x, int y)
    {
        if (!world.InBounds(x, y))
        {
            return;
        }

        int index = world.ToIndex(x, y);
        ushort tileType = world.Types[index];
        TileDef tileDef = TileDefs.GetTile(tileType);

        if (!tileDef.Solid)
        {
            world.FrameX[index] = 0;
            world.FrameY[index] = 0;
            return;
        }

        int mask = GetMask(world, x, y);
        int variant = Lut[mask & 15] & 3;
        world.FrameX[index] = (short)(variant & 1);
        world.FrameY[index] = (short)((variant >> 1) & 1);
    }

    public static void ProcessQueue(World world, int maxPerTick)
    {
        int processed = 0;

        while (processed < maxPerTick && world.FrameQueue.TryDequeue(out int x, out int y, world.Width))
        {
            Frame(world, x, y);
            processed++;
        }
    }

    private static bool IsSolid(World world, int x, int y)
    {
        if (!world.InBounds(x, y))
        {
            return false;
        }

        return TileDefs.GetTile(world.Types[world.ToIndex(x, y)]).Solid;
    }
}
