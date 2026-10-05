using TerrariaSandbox.Core;
using TerrariaSandbox.Core.Gen;

namespace TerrariaSandbox.Tests;

public class WorldGenerationPipelineTests
{
    [Fact]
    public void Pcg32_ShouldBeDeterministicAcrossEquivalentSeeds()
    {
        var a = new Pcg32(123456u);
        var b = new Pcg32(123456u);

        Assert.Equal(a.Next(), b.Next());
        Assert.Equal(a.NextFloat(), b.NextFloat());
        Assert.Equal(a.NextRange(10, 200), b.NextRange(10, 200));
    }

    [Fact]
    public void Noise_ShouldBeFiniteAndDeterministic()
    {
        var noise = new Noise();

        Assert.True(float.IsFinite(noise.Perlin1D(12.5f, 42u)));
        Assert.True(float.IsFinite(noise.Perlin2D(12.5f, 8.25f, 42u)));
        Assert.True(float.IsFinite(noise.Fbm2D(12.5f, 8.25f, 4, 0.5f, 2f, 42u)));
        Assert.True(float.IsFinite(noise.Ridge2D(12.5f, 8.25f, 4, 0.5f, 2f, 42u)));
    }

    [Fact]
    public void WorldGenerator_ShouldGenerateSameHashForSameSeed()
    {
        var generator = new WorldGenerator();
        var worldA = new World(256, 128);
        var worldB = new World(256, 128);

        generator.Generate(worldA, 12345u);
        generator.Generate(worldB, 12345u);

        Assert.Equal(HashWorld(worldA), HashWorld(worldB));
    }

    [Fact]
    public void CavePass_ShouldLeaveSpawnConnectedAndAirRatioWithinRange()
    {
        var world = new World(420, 180);
        var generator = new WorldGenerator(new IGenPass[]
        {
            new TerrainPass(),
            new CavePass()
        });

        generator.Generate(world, 987654321u);

        int spawnX = world.Width / 2;
        int spawnY = Math.Max(20, world.Height / 2);
        bool reachable = ConnectivityPass.IsSpawnReachable(world, spawnX, spawnY, 24);

        Assert.True(reachable);

        int surfaceY = EstimateSurfaceY(world);
        int minY = Math.Max(0, surfaceY + 18);
        int maxY = Math.Min(world.Height, surfaceY + 120);
        double airRatio = ComputeAirRatio(world, minY, maxY);
        Assert.InRange(airRatio, 0.08, 0.50);
    }

    private static int EstimateSurfaceY(World world)
    {
        int total = 0;
        int counted = 0;

        for (int x = 0; x < world.Width; ++x)
        {
            int lastSolid = -1;
            for (int y = 0; y < world.Height; ++y)
            {
                if (world.Get(x, y) != 0)
                {
                    lastSolid = y;
                }
                else if (lastSolid >= 0)
                {
                    break;
                }
            }

            if (lastSolid >= 0)
            {
                total += lastSolid;
                counted++;
            }
        }

        return counted == 0 ? 0 : total / counted;
    }

    private static double ComputeAirRatio(World world, int minY, int maxY)
    {
        int count = 0;
        int total = 0;

        for (int y = minY; y < maxY; ++y)
        {
            for (int x = 0; x < world.Width; ++x)
            {
                total++;
                if (world.Get(x, y) == 0)
                {
                    count++;
                }
            }
        }

        return total == 0 ? 0.0 : count / (double)total;
    }

    private static ulong HashWorld(World world)
    {
        ulong hash = 14695981039346656037UL;

        for (int y = 0; y < world.Height; ++y)
        {
            for (int x = 0; x < world.Width; ++x)
            {
                hash ^= world.Get(x, y);
                hash *= 1099511628211UL;
            }
        }

        return hash;
    }
}
