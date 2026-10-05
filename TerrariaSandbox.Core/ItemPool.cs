using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Drop item entity placed in the world and simulated as a small pickup.
/// </summary>
public struct ItemDrop
{
    public ushort Type;
    public short Stack;
    public byte Prefix;
    public Vector2 Position;
    public Vector2 Velocity;
    public int LifeTicks;
    public int Id;
    public bool Active;

    public bool IsEmpty => Type == 0 || Stack <= 0;

    public static ItemDrop Empty => default;
}

/// <summary>
/// Simple spatial hash used to merge nearby ground pickups.
/// </summary>
public sealed class SpatialHash
{
    private readonly Dictionary<long, List<int>> _cells;
    private readonly int _cellSize;

    public SpatialHash(int cellSize = 64)
    {
        _cellSize = cellSize;
        _cells = new Dictionary<long, List<int>>();
    }

    public void Clear()
    {
        _cells.Clear();
    }

    public void Insert(int itemId, Vector2 position)
    {
        long cellX = (long)MathF.Floor(position.X / _cellSize);
        long cellY = (long)MathF.Floor(position.Y / _cellSize);
        long key = (cellX << 32) ^ (cellY & 0xffffffffL);

        if (!_cells.TryGetValue(key, out List<int>? bucket))
        {
            bucket = new List<int>();
            _cells[key] = bucket;
        }

        bucket.Add(itemId);
    }

    public IEnumerable<KeyValuePair<long, List<int>>> Cells => _cells;
}

/// <summary>
/// Pool of grounded items with magnetic pickup, merge and despawn behavior.
/// </summary>
public sealed class ItemPool
{
    public const int MaxItems = 400;
    public const int DespawnTicks = 5 * 60 * 60;

    private readonly ItemDrop[] _items;
    private readonly SpatialHash _spatialHash;

    public ItemPool(int cellSize = 64)
    {
        _items = new ItemDrop[MaxItems];
        _spatialHash = new SpatialHash(cellSize);
    }

    public int ActiveCount { get; private set; }

    public ref ItemDrop this[int index]
    {
        get => ref _items[index];
    }

    public bool TryAdd(Vector2 position, ushort type, short stack = 1, byte prefix = 0)
    {
        if (type == 0 || stack <= 0)
        {
            return false;
        }

        for (int i = 0; i < _items.Length; ++i)
        {
            if (_items[i].IsEmpty)
            {
                _items[i] = new ItemDrop
                {
                    Type = type,
                    Stack = stack,
                    Prefix = prefix,
                    Position = position,
                    Velocity = Vector2.Zero,
                    LifeTicks = 0,
                    Id = i,
                    Active = true
                };
                ActiveCount++;
                return true;
            }
        }

        return false;
    }

    public void Update(World? world, Vector2 playerPosition, float deltaSeconds)
    {
        _spatialHash.Clear();

        for (int i = 0; i < _items.Length; ++i)
        {
            if (_items[i].IsEmpty)
            {
                continue;
            }

            var item = _items[i];
            item.LifeTicks += 1;
            if (item.LifeTicks >= DespawnTicks)
            {
                item = ItemDrop.Empty;
                _items[i] = item;
                ActiveCount = Math.Max(0, ActiveCount - 1);
                continue;
            }

            if (world is not null)
            {
                Vector2 pos = item.Position;
                Vector2 vel = item.Velocity;
                Collision.Move(world, ref pos, ref vel, 8, 8, new CollisionOptions { TileCollide = true }, out _);
                item.Position = pos;
                item.Velocity = vel;
            }

            float dx = playerPosition.X - item.Position.X;
            float dy = playerPosition.Y - item.Position.Y;
            float distanceSq = dx * dx + dy * dy;
            if (distanceSq > 0f && distanceSq < 120f * 120f)
            {
                float distance = MathF.Sqrt(distanceSq);
                float pull = Math.Clamp((120f - distance) / 120f, 0.02f, 0.45f);
                if (distance > 0f)
                {
                    item.Velocity += new Vector2((dx / distance) * pull * 4f, (dy / distance) * pull * 4f);
                }
            }

            item.Position += item.Velocity * deltaSeconds * 60f;
            item.Velocity *= 0.94f;

            _items[i] = item;
            _spatialHash.Insert(i, item.Position);
        }

        MergeNearbyItems();
    }

    private void MergeNearbyItems()
    {
        for (int i = 0; i < _items.Length; ++i)
        {
            if (_items[i].IsEmpty)
            {
                continue;
            }

            for (int j = i + 1; j < _items.Length; ++j)
            {
                if (_items[j].IsEmpty)
                {
                    continue;
                }

                if (_items[i].Type != _items[j].Type)
                {
                    continue;
                }

                float distanceSq = (_items[i].Position - _items[j].Position).LengthSquared();
                if (distanceSq > 128f * 128f)
                {
                    continue;
                }

                var left = _items[i];
                var right = _items[j];
                int maxStack = ItemDefs.Get(left.Type).MaxStack;
                int combined = left.Stack + right.Stack;
                if (combined <= maxStack)
                {
                    left.Stack = (short)combined;
                    right = ItemDrop.Empty;
                    _items[i] = left;
                    _items[j] = right;
                    continue;
                }

                int moved = Math.Min(maxStack - left.Stack, right.Stack);
                if (moved > 0)
                {
                    left.Stack += (short)moved;
                    right.Stack -= (short)moved;
                    _items[i] = left;
                    _items[j] = right;
                }
            }
        }
    }
}
