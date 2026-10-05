using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Shared combat behavior for NPCs and the player.
/// </summary>
public struct AiContext
{
    public World? World;
    public Vector2 PlayerPosition;
    public float DeltaSeconds;
    public int Biome;
    public int Depth;
    public int LightLevel;
    public Random? Random;
}

public static class CombatSystem
{
    public const int DefaultIFrameTicks = 8;

    public static bool Hit(ref Npc npc, int damage, Vector2 knockback)
    {
        if (npc.Life <= 0 || npc.Ai0 == AiState.Dead)
        {
            return false;
        }

        if (npc.Ai3 > 0)
        {
            return false;
        }

        int mitigation = Math.Max(0, npc.Defense);
        int applied = Math.Max(1, damage - mitigation);
        npc.Life -= applied;
        npc.Velocity += knockback;
        npc.Ai0 = AiState.Hurt;
        npc.Ai1 = 6;
        npc.Ai2 = 0;
        npc.Ai3 = DefaultIFrameTicks;

        if (npc.Life <= 0)
        {
            npc.Life = 0;
            npc.Ai0 = AiState.Dead;
            npc.Ai1 = 0;
            npc.Ai3 = 0;
        }

        return true;
    }
}

public readonly struct LootEntry
{
    public LootEntry(int itemType, int weight)
    {
        ItemType = itemType;
        Weight = weight;
    }

    public int ItemType { get; }
    public int Weight { get; }
}

/// <summary>
/// Weighted loot table that rolls without heap allocations.
/// </summary>
public sealed class LootTable
{
    private readonly LootEntry[] _entries;

    public LootTable(LootEntry[] entries)
    {
        _entries = entries ?? Array.Empty<LootEntry>();
    }

    public int Roll(Random random)
    {
        if (_entries.Length == 0)
        {
            return 0;
        }

        int totalWeight = 0;
        for (int i = 0; i < _entries.Length; ++i)
        {
            totalWeight += Math.Max(0, _entries[i].Weight);
        }

        if (totalWeight <= 0)
        {
            return _entries[0].ItemType;
        }

        int roll = random.Next(totalWeight);
        int running = 0;
        for (int i = 0; i < _entries.Length; ++i)
        {
            running += Math.Max(0, _entries[i].Weight);
            if (roll < running)
            {
                return _entries[i].ItemType;
            }
        }

        return _entries[_entries.Length - 1].ItemType;
    }
}
