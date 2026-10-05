namespace TerrariaSandbox.Core;

/// <summary>
/// Builds deterministic tile test layouts for collision regression scenarios.
/// </summary>
public static class TestWorldGenerator
{
    public static void PopulateSampleLayout(World world)
    {
        for (int x = 0; x < world.Width; ++x)
        {
            for (int y = 0; y < world.Height; ++y)
            {
                world.Set(x, y, 0);
            }
        }

        for (int x = 0; x < world.Width; ++x)
        {
            world.Set(x, 18, 1);
        }

        for (int x = 0; x < 20; ++x)
        {
            world.Set(x, 12, 1);
        }

        for (int x = 25; x < 42; ++x)
        {
            world.Set(x, 13, 1);
        }

        for (int x = 42; x < 58; ++x)
        {
            world.Set(x, 16, 1);
        }

        for (int i = 0; i < 6; ++i)
        {
            world.Set(18 + i, 17 - i, 1);
        }

        int rampX = 52;
        int rampY = 16;
        world.Set(rampX, rampY, 1);
        world.Set(rampX + 1, rampY, 1);
        world.Set(rampX + 2, rampY, 1);
        world.Flags[world.ToIndex(rampX, rampY)] = TileFlags.SetSlope(0, 1);
        world.Flags[world.ToIndex(rampX + 1, rampY)] = TileFlags.SetSlope(0, 2);

        world.Set(64, 14, 9);
        world.Set(65, 14, 9);
        world.Set(66, 14, 9);

        for (int x = 72; x < 84; ++x)
        {
            world.Set(x, 15, 1);
        }

        world.Set(84, 14, 1);
        world.Set(85, 13, 1);
        world.Set(86, 12, 1);
        world.Set(87, 11, 1);
    }
}
