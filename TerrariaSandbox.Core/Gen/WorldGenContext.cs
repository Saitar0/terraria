namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Shared state for the deterministic world generation pipeline.
/// </summary>
public sealed class WorldGenContext
{
    public WorldGenContext(World world, Pcg32 rng)
    {
        World = world;
        Rng = rng;
        worldSurface = Math.Max(8, world.Height / 2);
        rockLayer = worldSurface + 28;
        UnderworldStart = Math.Max(8, world.Height - 200);
    }

    public WorldGenContext(World world, Pcg32 rng, int surface, int rock)
    {
        World = world;
        Rng = rng;
        worldSurface = Math.Clamp(surface, 8, world.Height - 4);
        rockLayer = Math.Clamp(rock, worldSurface + 8, world.Height - 8);
        UnderworldStart = Math.Max(8, world.Height - 200);
    }

    public World World { get; }
    public Pcg32 Rng { get; }
    public int worldSurface { get; set; }
    public int rockLayer { get; set; }
    public int UnderworldStart { get; set; }
    public int WorldSurface => worldSurface;
    public int RockLayer => rockLayer;
}
