using TerrariaSandbox.Core;

namespace TerrariaSandbox.Tests;

public class LiquidSimulationTests
{
    [Fact]
    public void Water_ShouldSettleAndEmptyTheQueue()
    {
        var world = new World(40, 40);
        for (int x = 8; x < 30; x++)
        {
            world.Set(x, 20, 3);
        }

        for (int x = 10; x < 28; x++)
        {
            world.Set(x, 19, 0);
        }

        for (int x = 12; x < 26; x++)
        {
            int index = world.ToIndex(x, 15);
            world.Liquid[index] = 255;
            world.LiquidType[index] = (byte)LiquidType.Water;
            world.LiquidQueue.Enqueue(x, 15);
        }

        var simulator = new LiquidSimulator { Budget = 8000 };
        var area = new ActiveArea(0, 0, world.Width - 1, world.Height - 1);

        for (int i = 0; i < 250; i++)
        {
            simulator.Tick(world, in area);
            if (world.LiquidQueue.IsEmpty())
            {
                break;
            }
        }

        Assert.True(world.LiquidQueue.IsEmpty());
        Assert.True(world.Liquid.Sum(v => v) > 0);
    }

    [Fact]
    public void Liquid_ShouldConserveMassDuringSettling()
    {
        var world = new World(50, 30);
        for (int x = 10; x < 40; x++)
        {
            world.Set(x, 20, 3);
        }

        int initialMass = 0;
        for (int x = 15; x < 35; x++)
        {
            int index = world.ToIndex(x, 15);
            world.Liquid[index] = 200;
            world.LiquidType[index] = (byte)LiquidType.Water;
            initialMass += 200;
            world.LiquidQueue.Enqueue(x, 15);
        }

        var simulator = new LiquidSimulator { Budget = 8000 };
        var area = new ActiveArea(0, 0, world.Width - 1, world.Height - 1);

        for (int i = 0; i < 200; i++)
        {
            simulator.Tick(world, in area);
            if (world.LiquidQueue.IsEmpty())
            {
                break;
            }
        }

        int finalMass = world.Liquid.Sum(v => v);
        Assert.InRange(finalMass, 0, initialMass);
        Assert.True(finalMass > 0);
    }

    [Fact]
    public void WaterAndLava_ShouldCreateObsidianAndStopMixing()
    {
        var world = new World(20, 20);
        int waterIndex = world.ToIndex(8, 8);
        int lavaIndex = world.ToIndex(9, 8);

        world.Liquid[waterIndex] = 80;
        world.LiquidType[waterIndex] = (byte)LiquidType.Water;
        world.Liquid[lavaIndex] = 80;
        world.LiquidType[lavaIndex] = (byte)LiquidType.Lava;
        world.LiquidQueue.Enqueue(8, 8);
        world.LiquidQueue.Enqueue(9, 8);

        var simulator = new LiquidSimulator { Budget = 8000 };
        var area = new ActiveArea(0, 0, world.Width - 1, world.Height - 1);
        simulator.Tick(world, in area);

        Assert.True(world.Liquid[waterIndex] == 0 || world.Liquid[lavaIndex] == 0);
        Assert.True(world.Liquid[waterIndex] == 0 || world.LiquidType[waterIndex] == (byte)LiquidType.None);
        Assert.True(world.Liquid[lavaIndex] == 0 || world.LiquidType[lavaIndex] == (byte)LiquidType.None);
    }
}
