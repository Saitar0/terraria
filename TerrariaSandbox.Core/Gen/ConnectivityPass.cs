namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Removes small isolated solid islands and guarantees that the spawn area remains connected to the cave network.
/// </summary>
public sealed class ConnectivityPass : IGenPass
{
    public string Name => "Connectivity";
    public float Weight => 1f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        if (ctx is null || ctx.World is null)
        {
            p.Report(1f);
            return;
        }

        var world = ctx.World;
        RemoveSmallSolidIslands(world, 32);

        int spawnX = world.Width / 2;
        int spawnY = Math.Clamp(ctx.worldSurface - 6, 12, world.Height - 16);
        EnsureSpawnConnectivity(world, spawnX, spawnY, 6, carve: true);

        p.Report(1f);
    }

    public static bool IsSpawnReachable(World world, int spawnX, int spawnY, int radius = 8)
    {
        return EnsureSpawnConnectivity(world, spawnX, spawnY, radius, carve: false);
    }

    public static bool EnsureSpawnConnectivity(World world, int spawnX, int spawnY, int radius, bool carve)
    {
        if (world is null)
        {
            return false;
        }

        int width = world.Width;
        int height = world.Height;
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        int seedIndex = -1;
        for (int yy = spawnY - radius; yy <= spawnY + radius; ++yy)
        {
            if ((uint)yy >= (uint)height)
            {
                continue;
            }

            for (int xx = spawnX - radius; xx <= spawnX + radius; ++xx)
            {
                if ((uint)xx >= (uint)width)
                {
                    continue;
                }

                if (world.Get(xx, yy) == 0)
                {
                    seedIndex = yy * width + xx;
                    goto FoundSeed;
                }
            }
        }

    FoundSeed:
        if (seedIndex < 0)
        {
            if (!carve)
            {
                return false;
            }

            for (int yy = spawnY - radius; yy <= spawnY + radius; ++yy)
            {
                if ((uint)yy >= (uint)height)
                {
                    continue;
                }

                for (int xx = spawnX - radius; xx <= spawnX + radius; ++xx)
                {
                    if ((uint)xx >= (uint)width)
                    {
                        continue;
                    }

                    int dx = xx - spawnX;
                    int dy = yy - spawnY;
                    if ((dx * dx) + (dy * dy) <= radius * radius)
                    {
                        world.Set(xx, yy, 0);
                    }
                }
            }

            seedIndex = spawnY * width + spawnX;
        }

        bool[] visited = new bool[width * height];
        int[] stack = new int[width * height];
        int stackCount = 0;
        stack[stackCount++] = seedIndex;
        visited[seedIndex] = true;

        while (stackCount > 0)
        {
            int currentIndex = stack[--stackCount];
            int currentX = currentIndex % width;
            int currentY = currentIndex / width;

            if (currentX > 0)
            {
                int neighborIndex = currentIndex - 1;
                TryEnqueueAir(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX - 1, currentY);
            }

            if (currentX + 1 < width)
            {
                int neighborIndex = currentIndex + 1;
                TryEnqueueAir(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX + 1, currentY);
            }

            if (currentY > 0)
            {
                int neighborIndex = currentIndex - width;
                TryEnqueueAir(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX, currentY - 1);
            }

            if (currentY + 1 < height)
            {
                int neighborIndex = currentIndex + width;
                TryEnqueueAir(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX, currentY + 1);
            }
        }

        int spawnIndex = spawnY * width + spawnX;
        return visited[(uint)spawnIndex < (uint)(width * height) ? spawnIndex : 0];
    }

    private static void TryEnqueueAir(World world, int width, int height, bool[] visited, int[] stack, ref int stackCount, int index, int x, int y)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height || visited[index])
        {
            return;
        }

        if (world.Get(x, y) != 0)
        {
            return;
        }

        visited[index] = true;
        stack[stackCount++] = index;
    }

    public static void RemoveSmallSolidIslands(World world, int minSize)
    {
        if (world is null)
        {
            return;
        }

        int width = world.Width;
        int height = world.Height;
        if (width <= 0 || height <= 0 || minSize <= 0)
        {
            return;
        }

        bool[] visited = new bool[width * height];
        int[] stack = new int[width * height];
        int[] cluster = new int[width * height];

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int index = y * width + x;
                if (world.Get(x, y) == 0 || visited[index])
                {
                    continue;
                }

                int stackCount = 0;
                int clusterCount = 0;
                stack[stackCount++] = index;
                visited[index] = true;

                while (stackCount > 0)
                {
                    int currentIndex = stack[--stackCount];
                    int currentX = currentIndex % width;
                    int currentY = currentIndex / width;
                    cluster[clusterCount++] = currentIndex;

                    if (currentX > 0)
                    {
                        int neighborIndex = currentIndex - 1;
                        TryEnqueueSolid(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX - 1, currentY);
                    }

                    if (currentX + 1 < width)
                    {
                        int neighborIndex = currentIndex + 1;
                        TryEnqueueSolid(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX + 1, currentY);
                    }

                    if (currentY > 0)
                    {
                        int neighborIndex = currentIndex - width;
                        TryEnqueueSolid(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX, currentY - 1);
                    }

                    if (currentY + 1 < height)
                    {
                        int neighborIndex = currentIndex + width;
                        TryEnqueueSolid(world, width, height, visited, stack, ref stackCount, neighborIndex, currentX, currentY + 1);
                    }
                }

                if (clusterCount < minSize)
                {
                    for (int i = 0; i < clusterCount; ++i)
                    {
                        int cellIndex = cluster[i];
                        int cellX = cellIndex % width;
                        int cellY = cellIndex / width;
                        world.Set(cellX, cellY, 0);
                    }
                }
            }
        }
    }

    private static void TryEnqueueSolid(World world, int width, int height, bool[] visited, int[] stack, ref int stackCount, int index, int x, int y)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height || visited[index])
        {
            return;
        }

        if (world.Get(x, y) == 0)
        {
            return;
        }

        visited[index] = true;
        stack[stackCount++] = index;
    }
}
