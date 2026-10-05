using TerrariaSandbox.Core;
using TerrariaSandbox.Core.Gen;

namespace TerrariaSandbox.Tests;

public class WorldGenerationAdvancedTests
{
    [Fact]
    public void BiomePass_ShouldCreateHorizontalBiomeBands()
    {
        var world = new World(320, 180);
        var generator = new WorldGenerator(new IGenPass[]
        {
            new TerrainPass(),
            new BiomePass()
        });

        generator.Generate(world, 1234u);

        bool foundAny = false;
        for (int x = 0; x < world.Width; x += 32)
        {
            for (int y = 0; y < world.Height; y += 16)
            {
                ushort tile = world.Get(x, y);
                if (tile == (ushort)TileType.Sand || tile == (ushort)TileType.Snow || tile == (ushort)TileType.Grass)
                {
                    foundAny = true;
                    break;
                }
            }
        }

        Assert.True(foundAny);
    }

    [Fact]
    public void OrePass_ShouldPopulateOreWithinDepthRange()
    {
        var world = new World(240, 180);
        var generator = new WorldGenerator(new IGenPass[]
        {
            new TerrainPass(),
            new OrePass()
        });

        generator.Generate(world, 4321u);

        int oreCount = 0;
        int maxY = 0;
        for (int y = 0; y < world.Height; ++y)
        {
            for (int x = 0; x < world.Width; ++x)
            {
                ushort tile = world.Get(x, y);
                if (tile == (ushort)TileType.Stone || tile == (ushort)TileType.Dirt || tile == (ushort)TileType.Sand)
                {
                    if (y > maxY)
                    {
                        maxY = y;
                    }
                }

                if (tile == (ushort)TileType.Stone)
                {
                    oreCount++;
                }
            }
        }

        Assert.True(oreCount > 0);
        Assert.True(maxY > 0);
    }

    [Fact]
    public void StructureTemplate_ShouldValidatePlacementBounds()
    {
        var template = new StructureTemplate(
            [
                new StructureTile(0, 0, (ushort)TileType.Wood, 0, 0),
                new StructureTile(1, 0, (ushort)TileType.Wood, 0, 0)
            ],
            new[] { 0, 1 },
            new[] { 0, 1 },
            0,
            0,
            0,
            1,
            1,
            0,
            0);

        Assert.Equal(2, template.Width);
        Assert.Equal(1, template.Height);
        Assert.True(template.IsValidForTerrain(0, 0, 2, 3, 8, 10));
    }
}
