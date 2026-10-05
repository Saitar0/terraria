namespace TerrariaSandbox.Core;

/// <summary>
/// Lightweight support-state checker for tiles that must remain attached to a supporting tile.
/// </summary>
public sealed class SupportChecker
{
    private readonly int[] _x;
    private readonly int[] _y;
    private readonly ulong[] _queued;
    private readonly int _capacity;
    private int _head;
    private int _tail;

    public SupportChecker(int capacity = 2048)
    {
        _capacity = capacity > 0 ? capacity : 2048;
        _x = new int[_capacity];
        _y = new int[_capacity];
        _queued = new ulong[(_capacity + 63) / 64];
    }

    public void Enqueue(int x, int y)
    {
        int slot = ((y * 8192) + x) % _capacity;
        if (slot < 0)
        {
            slot += _capacity;
        }

        int bit = slot & 63;
        int word = slot >> 6;
        ulong mask = 1UL << bit;

        if ((_queued[word] & mask) != 0UL)
        {
            return;
        }

        _queued[word] |= mask;
        _x[_tail] = x;
        _y[_tail] = y;
        _tail = (_tail + 1) % _capacity;

        if (_tail == _head)
        {
            _head = (_head + 1) % _capacity;
        }
    }

    public bool TryDequeue(out int x, out int y)
    {
        if (_head == _tail)
        {
            x = 0;
            y = 0;
            return false;
        }

        x = _x[_head];
        y = _y[_head];
        _head = (_head + 1) % _capacity;

        int slot = ((y * 8192) + x) % _capacity;
        if (slot < 0)
        {
            slot += _capacity;
        }

        int bit = slot & 63;
        int word = slot >> 6;
        _queued[word] &= ~(1UL << bit);
        return true;
    }

    public void NotifyTileChanged(World world, int x, int y)
    {
        if (world is null)
        {
            return;
        }

        int[] offsets = [-1, 0, 1];
        for (int oy = 0; oy < offsets.Length; ++oy)
        {
            for (int ox = 0; ox < offsets.Length; ++ox)
            {
                int nx = x + offsets[ox];
                int ny = y + offsets[oy];
                if (world.InBounds(nx, ny))
                {
                    Enqueue(nx, ny);
                }
            }
        }
    }

    public void Update(World world, int maxPerTick)
    {
        int processed = 0;
        while (processed < maxPerTick && TryDequeue(out int x, out int y))
        {
            if (!world.InBounds(x, y))
            {
                continue;
            }

            ushort tileType = world.Get(x, y);
            if (tileType == 0)
            {
                processed++;
                continue;
            }

            TileDef def = TileDefs.GetTile(tileType);
            if (!def.FallsWithoutSupport)
            {
                processed++;
                continue;
            }

            if (NeedsSupport(world, x, y))
            {
                world.Set(x, y, 0);
            }

            processed++;
        }
    }

    public static bool NeedsSupport(World world, int x, int y)
    {
        int belowX = x;
        int belowY = y + 1;
        if (!world.InBounds(belowX, belowY))
        {
            return true;
        }

        ushort belowType = world.Get(belowX, belowY);
        return belowType == 0 || !TileDefs.GetTile(belowType).Solid;
    }
}
