using System.IO;
using TerrariaSandbox.Core;
using TerrariaSandbox.Core.IO;

namespace TerrariaSandbox.Tests;

public class WorldSerializerTests
{
    [Fact]
    public void SaveAndLoad_RoundTrip_ShouldPreserveWorldState()
    {
        var world = new World(32, 24);
        world.Name = "TestWorld";
        world.Seed = 24680u;
        world.Spawn = new Vector2Int(10, 18);
        world.WorldSurface = 12;
        world.RockLayer = 18;
        world.TimeOfDay = 4200;
        world.BossFlags = BossFlags.EyeOfCthulhu | BossFlags.Skeletron;

        for (int y = 0; y < world.Height; ++y)
        {
            for (int x = 0; x < world.Width; ++x)
            {
                ushort type = (ushort)((x + y) % 8 == 0 ? 1 + ((x * 17 + y * 13) % 7) : 0);
                world.Set(x, y, type);
                world.SetWall(x, y, (ushort)((x * 3 + y) % 4 + 21));
                if ((x + y) % 5 == 0)
                {
                    world.SetLiquid(x, y, 40, 1);
                }
            }
        }

        using var stream = new MemoryStream();
        var serializer = new WorldSerializer();
        serializer.Save(world, stream);

        stream.Position = 0;
        var loaded = serializer.Load(stream);

        Assert.Equal(world.Name, loaded.Name);
        Assert.Equal(world.Seed, loaded.Seed);
        Assert.Equal(world.Spawn, loaded.Spawn);
        Assert.Equal(world.WorldSurface, loaded.WorldSurface);
        Assert.Equal(world.RockLayer, loaded.RockLayer);
        Assert.Equal(world.TimeOfDay, loaded.TimeOfDay);
        Assert.Equal(world.BossFlags, loaded.BossFlags);
        Assert.Equal(HashWorld(world), HashWorld(loaded));
    }

    [Fact]
    public void SaveMigrator_ShouldApplyVersionedMigrations()
    {
        var migrator = new SaveMigrator();

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write("MYWORLD");
        writer.Write(1);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0L);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);

        stream.Position = 0;
        var migrated = migrator.LoadVersioned(stream);

        Assert.Equal(2, migrated.Version);
    }

    private static ulong HashWorld(World world)
    {
        ulong hash = 14695981039346656037UL;

        for (int y = 0; y < world.Height; ++y)
        {
            for (int x = 0; x < world.Width; ++x)
            {
                hash ^= (ulong)world.Get(x, y);
                hash *= 1099511628211UL;
                hash ^= (ulong)world.GetWall(x, y);
                hash *= 1099511628211UL;
            }
        }

        return hash;
    }
}
