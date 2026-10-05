using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Static delegate table used to dispatch AI behavior by style without per-entity virtual calls.
/// </summary>
public static class NpcAiHandlers
{
    public delegate void AiHandler(ref Npc npc, World? world, Vector2 playerPosition, float deltaSeconds, int biome, int depth, int lightLevel, Random random);

    private static readonly AiHandler[] Dispatch =
    [
        RunSlime,
        RunZombie,
        RunFlyingEye,
        RunBoss
    ];

    public static void Update(ref Npc npc, World? world, Vector2 playerPosition, float deltaSeconds, int biome, int depth, int lightLevel, Random random)
    {
        if (npc.Type <= 0 || npc.Type >= Dispatch.Length + 1)
        {
            return;
        }

        Dispatch[npc.AiStyle % Dispatch.Length](ref npc, world, playerPosition, deltaSeconds, biome, depth, lightLevel, random);
    }

    private static void RunSlime(ref Npc npc, World? world, Vector2 playerPosition, float deltaSeconds, int biome, int depth, int lightLevel, Random random)
    {
        var context = new AiContext
        {
            World = world,
            PlayerPosition = playerPosition,
            DeltaSeconds = deltaSeconds,
            Biome = biome,
            Depth = depth,
            LightLevel = lightLevel,
            Random = random
        };

        TerrariaSandbox.Core.Ai.SlimeAi.Update(ref npc, in context);
    }

    private static void RunZombie(ref Npc npc, World? world, Vector2 playerPosition, float deltaSeconds, int biome, int depth, int lightLevel, Random random)
    {
        var context = new AiContext
        {
            World = world,
            PlayerPosition = playerPosition,
            DeltaSeconds = deltaSeconds,
            Biome = biome,
            Depth = depth,
            LightLevel = lightLevel,
            Random = random
        };

        TerrariaSandbox.Core.Ai.ZombieAi.Update(ref npc, in context);
    }

    private static void RunFlyingEye(ref Npc npc, World? world, Vector2 playerPosition, float deltaSeconds, int biome, int depth, int lightLevel, Random random)
    {
        var context = new AiContext
        {
            World = world,
            PlayerPosition = playerPosition,
            DeltaSeconds = deltaSeconds,
            Biome = biome,
            Depth = depth,
            LightLevel = lightLevel,
            Random = random
        };

        TerrariaSandbox.Core.Ai.FlyerAi.Update(ref npc, in context);
    }

    private static void RunBoss(ref Npc npc, World? world, Vector2 playerPosition, float deltaSeconds, int biome, int depth, int lightLevel, Random random)
    {
        var context = new AiContext
        {
            World = world,
            PlayerPosition = playerPosition,
            DeltaSeconds = deltaSeconds,
            Biome = biome,
            Depth = depth,
            LightLevel = lightLevel,
            Random = random
        };

        TerrariaSandbox.Core.Ai.BossAi.Update(ref npc, in context);
    }
}

/// <summary>
/// Simple spatial hash used to pair nearby entities for cheap proximity queries.
/// </summary>
public sealed class NpcSpatialHash
{
    private const int CellSize = 64;
    private readonly Dictionary<long, List<int>> _cells = new();

    public void Clear()
    {
        _cells.Clear();
    }

    public void Insert(int npcId, Vector2 position)
    {
        long cellX = (long)MathF.Floor(position.X / CellSize);
        long cellY = (long)MathF.Floor(position.Y / CellSize);
        long key = (cellX << 32) ^ (cellY & 0xffffffffL);

        if (!_cells.TryGetValue(key, out List<int>? bucket))
        {
            bucket = new List<int>();
            _cells[key] = bucket;
        }

        bucket.Add(npcId);
    }

    public IEnumerable<int> Query(Vector2 position, float radius)
    {
        int minCellX = (int)MathF.Floor((position.X - radius) / CellSize);
        int maxCellX = (int)MathF.Floor((position.X + radius) / CellSize);
        int minCellY = (int)MathF.Floor((position.Y - radius) / CellSize);
        int maxCellY = (int)MathF.Floor((position.Y + radius) / CellSize);

        for (int cellY = minCellY; cellY <= maxCellY; ++cellY)
        {
            for (int cellX = minCellX; cellX <= maxCellX; ++cellX)
            {
                long key = ((long)cellX << 32) ^ (cellY & 0xffffffffL);
                if (_cells.TryGetValue(key, out List<int>? bucket))
                {
                    foreach (int id in bucket)
                    {
                        yield return id;
                    }
                }
            }
        }
    }
}

/// <summary>
/// Orchestrates NPC spawning, AI updates, and despawn rules.
/// </summary>
public sealed class NpcSystem
{
    public const int MaxNpcCount = 200;
    public const int DespawnDistanceTiles = 96;
    public const int DespawnDistancePixels = DespawnDistanceTiles * 16;

    private readonly Pool<Npc> _pool;
    private readonly NpcSpawner _spawner;
    private readonly NpcSpatialHash _spatialHash = new();
    private readonly Random _random;
    private int _tick;

    public NpcSystem(Pool<Npc>? pool = null, Random? random = null)
    {
        _pool = pool ?? new Pool<Npc>(MaxNpcCount);
        _random = random ?? new Random();
        _spawner = new NpcSpawner(_pool, _random);
    }

    public Pool<Npc> Pool => _pool;
    public int ActiveCount => _pool.ActiveCount;

    public int Spawn(in Npc npc, Vector2 playerPosition)
    {
        Npc instance = npc;
        instance.Active = true;
        instance.Position = npc.Position;
        instance.Target = 0;
        instance.Direction = npc.Direction == 0 ? 1 : npc.Direction;
        instance.Type = npc.Type;

        int index = _pool.Spawn(instance);
        if (index < 0)
        {
            return -1;
        }

        ref Npc spawned = ref _pool[index];
        spawned.Position = instance.Position;
        spawned.Active = true;
        return index;
    }

    public void Update(World? world, Vector2 playerPosition, float deltaSeconds, int biome, int depth, int lightLevel)
    {
        _tick++;

        if (world is not null && _tick % 12 == 0)
        {
            _spawner.TrySpawn(world, playerPosition, biome, depth, lightLevel, out _);
        }

        _spatialHash.Clear();

        for (int i = 0; i < _pool.Capacity; ++i)
        {
            if (!_pool.IsActive(i))
            {
                continue;
            }

            ref Npc npc = ref _pool[i];
            if (npc.Type <= 0)
            {
                continue;
            }

            if (npc.Life <= 0)
            {
                _pool.Despawn(i);
                continue;
            }

            bool updateFrequency = (_tick & 3) == 0 || (playerPosition - npc.Position).LengthSquared() < 220f * 220f;
            if (!updateFrequency)
            {
                _spatialHash.Insert(i, npc.Position);
                continue;
            }

            if (npc.AiStyle == 0 && npc.Type == 1)
            {
                npc.AiStyle = 0;
            }

            NpcAiHandlers.Update(ref npc, world, playerPosition, deltaSeconds, biome, depth, lightLevel, _random);

            float distSq = (playerPosition - npc.Position).LengthSquared();
            if (distSq > DespawnDistancePixels * DespawnDistancePixels)
            {
                _pool.Despawn(i);
                continue;
            }

            _spatialHash.Insert(i, npc.Position);
        }

        for (int i = 0; i < _pool.Capacity; ++i)
        {
            if (!_pool.IsActive(i))
            {
                continue;
            }

            ref Npc npc = ref _pool[i];
            foreach (int otherId in _spatialHash.Query(npc.Position, 32f))
            {
                if (otherId == i || !_pool.IsActive(otherId))
                {
                    continue;
                }

                ref Npc other = ref _pool[otherId];
                float sq = (other.Position - npc.Position).LengthSquared();
                if (sq < 32f * 32f)
                {
                    Vector2 push = npc.Position - other.Position;
                    if (push.LengthSquared() > 0f)
                    {
                        push.Normalize();
                        npc.Position += push * 0.2f;
                        other.Position -= push * 0.2f;
                    }
                }
            }
        }
    }
}

/// <summary>
/// Handles spawn attempts and selection weights for NPCs.
/// </summary>
public sealed class NpcSpawner
{
    private readonly Pool<Npc> _pool;
    private readonly Random _random;

    public NpcSpawner(Pool<Npc> pool, Random random)
    {
        _pool = pool;
        _random = random;
    }

    public bool TrySpawn(World? world, Vector2 playerPosition, int biome, int depth, int lightLevel, out int npcIndex)
    {
        npcIndex = -1;
        if (world is null)
        {
            return false;
        }

        if (_pool.ActiveCount >= NpcSystem.MaxNpcCount)
        {
            return false;
        }

        int type = NpcDefs.SelectWeighted(_random, biome, depth, lightLevel);
        NpcDef def = NpcDefs.Get(type);

        float angle = (float)(_random.NextDouble() * Math.PI * 2.0);
        float radiusTiles = 62f + (float)_random.NextDouble() * (125f - 62f);
        float spawnX = playerPosition.X + MathF.Cos(angle) * radiusTiles * 16f;
        float spawnY = playerPosition.Y + MathF.Sin(angle) * radiusTiles * 16f;

        if (world is not null)
        {
            int tileX = (int)MathF.Floor(spawnX / 16f);
            int tileY = (int)MathF.Floor(spawnY / 16f);
            if (!world.InBounds(tileX, tileY))
            {
                return false;
            }

            if (world.Get(tileX, tileY) != 0)
            {
                return false;
            }
        }

        Npc npc = new()
        {
            Type = type,
            Position = new Vector2(spawnX, spawnY),
            Velocity = Vector2.Zero,
            Width = def.Width,
            Height = def.Height,
            Life = def.MaxLife,
            LifeMax = def.MaxLife,
            Damage = def.Damage,
            Defense = def.Defense,
            AiStyle = def.AiStyle,
            Direction = _random.Next(0, 2) == 0 ? -1 : 1,
            Frame = 0,
            Active = true
        };

        npcIndex = _pool.Spawn(npc);
        return npcIndex >= 0;
    }
}
