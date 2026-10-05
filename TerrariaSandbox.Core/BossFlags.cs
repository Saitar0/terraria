namespace TerrariaSandbox.Core;

/// <summary>
/// World boss progression flags persisted in the save metadata.
/// </summary>
[Flags]
public enum BossFlags : uint
{
    None = 0,
    EyeOfCthulhu = 1u << 0,
    Skeletron = 1u << 1,
    KingSlime = 1u << 2,
    TheDestroyer = 1u << 3,
    TheTwins = 1u << 4,
    WallOfFlesh = 1u << 5,
    QueenBee = 1u << 6,
    Plantera = 1u << 7,
    Golem = 1u << 8,
    DukeFishron = 1u << 9,
    MoonLord = 1u << 10
}
