using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.Tests;

public class NpcSystemTests
{
    [Fact]
    public void Pool_ShouldSpawnAndDespawnWithoutIndexLeaks()
    {
        var pool = new Pool<Npc>(4);

        int first = pool.Spawn(new Npc
        {
            Type = 1,
            Life = 10,
            LifeMax = 10,
            Width = 16f,
            Height = 18f,
            Position = new Vector2(32f, 32f)
        });

        int second = pool.Spawn(new Npc
        {
            Type = 2,
            Life = 12,
            LifeMax = 12,
            Width = 18f,
            Height = 18f,
            Position = new Vector2(60f, 32f)
        });

        Assert.Equal(2, pool.ActiveCount);
        Assert.True(pool.IsActive(first));
        Assert.True(pool.IsActive(second));

        pool.Despawn(first);

        Assert.False(pool.IsActive(first));
        Assert.Equal(1, pool.ActiveCount);

        int replacement = pool.Spawn(new Npc
        {
            Type = 3,
            Life = 8,
            LifeMax = 8,
            Width = 16f,
            Height = 16f,
            Position = new Vector2(80f, 32f)
        });

        Assert.Equal(first, replacement);
        Assert.Equal(2, pool.ActiveCount);
        Assert.True(pool.IsActive(replacement));
    }

    [Fact]
    public void NpcSystem_ShouldDespawnEnemiesThatAreFarFromPlayer()
    {
        var pool = new Pool<Npc>(8);
        var system = new NpcSystem(pool, new Random(42));

        var farNpc = new Npc
        {
            Type = 1,
            Position = new Vector2(2048f, 512f),
            Velocity = Vector2.Zero,
            Width = 16f,
            Height = 18f,
            Life = 10,
            LifeMax = 10,
            Damage = 2,
            Defense = 0,
            AiStyle = 0,
            Direction = 1
        };

        int npcId = system.Spawn(farNpc, new Vector2(64f, 80f));
        Assert.True(npcId >= 0);

        for (int i = 0; i < 240; i++)
        {
            system.Update(null, new Vector2(64f, 80f), 1f / 60f, 0, 0, 0);
        }

        Assert.False(pool.IsActive(npcId));
    }

    [Fact]
    public void CombatSystem_ShouldApplyDefenseAndIframes()
    {
        var npc = new Npc
        {
            Type = 2,
            Position = new Vector2(100f, 100f),
            Width = 18f,
            Height = 36f,
            Life = 30,
            LifeMax = 30,
            Defense = 4,
            AiStyle = 1,
            Direction = 1,
            Ai0 = 0,
            Ai1 = 0,
            Ai2 = 0,
            Ai3 = 0
        };

        bool first = CombatSystem.Hit(ref npc, 12, new Vector2(2f, 0f));
        Assert.True(first);
        Assert.Equal(22, npc.Life);

        bool second = CombatSystem.Hit(ref npc, 12, new Vector2(2f, 0f));
        Assert.False(second);
        Assert.Equal(22, npc.Life);
    }

    [Fact]
    public void BossAi_ShouldSwitchPhaseAtHalfHealth()
    {
        var npc = new Npc
        {
            Type = 4,
            Position = new Vector2(200f, 200f),
            Width = 44f,
            Height = 52f,
            Life = 50,
            LifeMax = 100,
            AiStyle = 3,
            Direction = 1,
            Ai0 = 0,
            Ai1 = 0,
            Ai2 = 0,
            Ai3 = 0
        };

        var context = new AiContext
        {
            World = null,
            PlayerPosition = new Vector2(300f, 200f),
            DeltaSeconds = 1f / 60f,
            Biome = 0,
            Depth = 0,
            LightLevel = 0,
            Random = new Random(5)
        };

        BossAi.Update(ref npc, in context);
        Assert.Equal(1, npc.Ai0);

        npc.Life = 49;
        BossAi.Update(ref npc, in context);
        Assert.Equal(1, npc.Ai0);

        npc.Life = 40;
        BossAi.Update(ref npc, in context);
        Assert.Equal(2, npc.Ai0);
    }

    [Fact]
    public void LootTable_ShouldHonorWeightedDrops()
    {
        var random = new Random(1234);
        var table = new LootTable(new[]
        {
            new LootEntry(1, 80),
            new LootEntry(2, 20)
        });

        int first = 0;
        int second = 0;

        for (int i = 0; i < 5000; ++i)
        {
            int item = table.Roll(random);
            if (item == 1)
            {
                first++;
            }
            else if (item == 2)
            {
                second++;
            }
        }

        Assert.InRange(first, 3850, 4150);
        Assert.InRange(second, 850, 1150);
    }
}
