using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Bitflags used by NPCs to control movement behavior.
/// </summary>
public static class NpcFlags
{
    public const ushort NoGravity = 1 << 0;
    public const ushort NoTileCollide = 1 << 1;
}

/// <summary>
/// Static metadata for a spawned NPC archetype.
/// </summary>
public sealed class NpcDef
{
    public NpcDef(
        string name,
        int width,
        int height,
        int maxLife,
        int damage,
        int defense,
        int aiStyle,
        float spawnWeight,
        float speed,
        int value = 0)
    {
        Name = name;
        Width = width;
        Height = height;
        MaxLife = maxLife;
        Damage = damage;
        Defense = defense;
        AiStyle = aiStyle;
        SpawnWeight = spawnWeight;
        Speed = speed;
        Value = value;
    }

    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int MaxLife { get; }
    public int Damage { get; }
    public int Defense { get; }
    public int AiStyle { get; }
    public float SpawnWeight { get; }
    public float Speed { get; }
    public int Value { get; }
}

/// <summary>
/// Runtime instance for world NPCs. Uses fixed fields instead of per-instance arrays.
/// </summary>
public struct Npc
{
    public int Type;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Width;
    public float Height;
    public int Life;
    public int LifeMax;
    public int Damage;
    public int Defense;
    public int AiStyle;
    public int Target;
    public int Direction;
    public int Frame;
    public ushort Flags;
    public int Ai0;
    public int Ai1;
    public int Ai2;
    public int Ai3;
    public int LocalAi0;
    public int LocalAi1;
    public int LocalAi2;
    public int LocalAi3;
    public bool Active;

    public bool NoGravity => (Flags & NpcFlags.NoGravity) != 0;
    public bool NoTileCollide => (Flags & NpcFlags.NoTileCollide) != 0;
}

/// <summary>
/// Static NPC definition table and weighted spawn selectors.
/// </summary>
public static class AiState
{
    public const int Idle = 0;
    public const int Wander = 1;
    public const int Chase = 2;
    public const int Attack = 3;
    public const int Hurt = 4;
    public const int Dead = 5;
}

public static class NpcDefs
{
    public static readonly NpcDef[] Table =
    [
        new("Empty", 0, 0, 0, 0, 0, 0, 0f, 0f),
        new("Slime", 18, 18, 16, 6, 0, 0, 55f, 1.3f, 1),
        new("Zombie", 18, 36, 28, 10, 1, 1, 35f, 1.0f, 2),
        new("FlyingEye", 22, 22, 22, 12, 2, 2, 25f, 2.2f, 3),
        new("Boss", 42, 46, 120, 18, 6, 3, 0f, 2.8f, 10)
    ];

    public static NpcDef Get(int type)
    {
        return (uint)type < (uint)Table.Length ? Table[type] : Table[0];
    }

    public static int SelectWeighted(Random random, int biome, int depth, int lightLevel)
    {
        int[] candidates = [1, 2, 3];
        float[] weights = [55f, 35f, 25f];

        if (biome == 1)
        {
            weights[0] = 65f;
            weights[1] = 40f;
            weights[2] = 20f;
        }
        else if (biome == 2)
        {
            weights[0] = 40f;
            weights[1] = 35f;
            weights[2] = 45f;
        }

        if (depth > 120)
        {
            weights[1] += 12f;
            weights[2] += 8f;
        }

        if (lightLevel < 35)
        {
            weights[2] += 18f;
        }

        float total = weights[0] + weights[1] + weights[2];
        float roll = (float)random.NextDouble() * total;

        if (roll < weights[0])
        {
            return candidates[0];
        }

        if (roll < weights[0] + weights[1])
        {
            return candidates[1];
        }

        return candidates[2];
    }
}
