using System.Runtime.CompilerServices;

namespace TerrariaSandbox.Core;

/// <summary>
/// Supported fluid types in the tile simulation.
/// </summary>
public enum LiquidType : byte
{
    None = 0,
    Water = 1,
    Lava = 2,
    Honey = 3
}

/// <summary>
/// Represents the active world area currently eligible for liquid simulation.
/// </summary>
public readonly struct ActiveArea
{
    public ActiveArea(int minX, int minY, int maxX, int maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public int MinX { get; }
    public int MinY { get; }
    public int MaxX { get; }
    public int MaxY { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int x, int y)
    {
        return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
    }
}

/// <summary>
/// Allocation-free ring buffer queue of active liquid tiles.
/// </summary>
public sealed class LiquidQueue
{
    private readonly int[] _x;
    private readonly int[] _y;
    private readonly ulong[] _queued;
    private readonly int _capacity;
    private int _head;
    private int _tail;

    public LiquidQueue(int capacity)
    {
        _capacity = capacity > 0 ? capacity : 1024;
        _x = new int[_capacity];
        _y = new int[_capacity];
        _queued = new ulong[(_capacity + 63) / 64];
    }

    public bool IsEmpty() => _head == _tail;

    public int Count { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(int x, int y)
    {
        int slot = ((y * 8192) + x) % _capacity;
        if (slot < 0)
        {
            slot += _capacity;
        }

        int bit = slot & 63;
        int word = slot >> 6;

        if ((ulong)(_queued[word] & (1UL << bit)) != 0UL)
        {
            return;
        }

        _queued[word] |= 1UL << bit;
        _x[_tail] = x;
        _y[_tail] = y;
        _tail = (_tail + 1) % _capacity;
        Count++;

        if (_tail == _head)
        {
            _head = (_head + 1) % _capacity;
            Count--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out int x, out int y)
    {
        if (IsEmpty())
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

        Count = Math.Max(0, Count - 1);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _head = 0;
        _tail = 0;
        Array.Clear(_queued, 0, _queued.Length);
        Count = 0;
    }
}

/// <summary>
/// Simulates liquid flow with a fixed tick budget and a preallocated active queue.
/// </summary>
public sealed class LiquidSimulator
{
    private World? _world;
    private long _tickCounter;
    private readonly LiquidQueue _nearQueue;
    private readonly LiquidQueue _farQueue;
    private int _playerX;
    private int _playerY;

    public LiquidSimulator()
    {
        _nearQueue = new LiquidQueue(8192);
        _farQueue = new LiquidQueue(8192);
        Budget = 8000;
    }

    public int Budget { get; set; }

    public int PlayerX
    {
        get => _playerX;
        set => _playerX = value;
    }

    public int PlayerY
    {
        get => _playerY;
        set => _playerY = value;
    }

    public void SetWorld(World world)
    {
        _world = world;
    }

    public void Wake(int x, int y)
    {
        if (_world is null)
        {
            return;
        }

        if (! _world.InBounds(x, y))
        {
            return;
        }

        int distance = Math.Abs(x - _playerX) + Math.Abs(y - _playerY);
        if (distance <= 16)
        {
            _nearQueue.Enqueue(x, y);
        }
        else
        {
            _farQueue.Enqueue(x, y);
        }

        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                int nx = x + ox;
                int ny = y + oy;
                if ((uint)nx < (uint)_world.Width && (uint)ny < (uint)_world.Height)
                {
                    int nd = Math.Abs(nx - _playerX) + Math.Abs(ny - _playerY);
                    if (nd <= 16)
                    {
                        _nearQueue.Enqueue(nx, ny);
                    }
                    else
                    {
                        _farQueue.Enqueue(nx, ny);
                    }
                }
            }
        }
    }

    public void WakeLiquid(int x, int y)
    {
        Wake(x, y);
    }

    public void Tick(World w, in ActiveArea a)
    {
        _world = w;

        while (!w.LiquidQueue.IsEmpty())
        {
            if (w.LiquidQueue.TryDequeue(out int x, out int y))
            {
                Wake(x, y);
            }
        }

        int processed = 0;
        int totalNear = 0;
        int nearBudget = Budget;
        int farBudget = Math.Max(1, Budget / 4);

        bool processFarQueue = ((_tickCounter & 3) == 0);
        _tickCounter++;

        while (processed < nearBudget && !_nearQueue.IsEmpty())
        {
            if (!_nearQueue.TryDequeue(out int x, out int y))
            {
                break;
            }

            if (!a.Contains(x, y) || !w.InBounds(x, y))
            {
                continue;
            }

            if (TickTile(w, x, y, a))
            {
                WakeLiquid(x, y);
            }

            processed++;
            totalNear++;
        }

        if (processFarQueue)
        {
            while (processed < Budget && !_farQueue.IsEmpty() && processed - totalNear < farBudget)
            {
                if (!_farQueue.TryDequeue(out int x, out int y))
                {
                    break;
                }

                if (!a.Contains(x, y) || !w.InBounds(x, y))
                {
                    continue;
                }

                if (TickTile(w, x, y, a))
                {
                    WakeLiquid(x, y);
                }

                processed++;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSolid(World w, int x, int y)
    {
        if (!w.InBounds(x, y))
        {
            return true;
        }

        return TileDefs.GetTile(w.Types[w.ToIndex(x, y)]).Solid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetLiquid(World w, int x, int y, LiquidType liquidType, byte amount)
    {
        if (!w.InBounds(x, y))
        {
            return;
        }

        int index = w.ToIndex(x, y);
        if (amount == 0 || liquidType == LiquidType.None)
        {
            w.Liquid[index] = 0;
            w.LiquidType[index] = (byte)LiquidType.None;
            return;
        }

        w.Liquid[index] = amount;
        w.LiquidType[index] = (byte)liquidType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOpenToLiquid(World world, int x, int y, LiquidType type)
    {
        if (!world.InBounds(x, y))
        {
            return false;
        }

        int index = world.ToIndex(x, y);
        if (IsSolid(world, x, y))
        {
            return false;
        }

        if (world.LiquidType[index] == 0)
        {
            return true;
        }

        LiquidType targetType = (LiquidType)world.LiquidType[index];
        if (targetType == type)
        {
            return world.Liquid[index] < 255;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReact(World world, int sourceX, int sourceY, int targetX, int targetY, LiquidType sourceType)
    {
        if (!world.InBounds(targetX, targetY))
        {
            return false;
        }

        int sourceIndex = world.ToIndex(sourceX, sourceY);
        int targetIndex = world.ToIndex(targetX, targetY);
        byte targetTypeId = world.LiquidType[targetIndex];
        if (targetTypeId == 0)
        {
            return false;
        }

        LiquidType targetType = (LiquidType)targetTypeId;
        if (sourceType == LiquidType.Water && targetType == LiquidType.Lava)
        {
            world.Liquid[sourceIndex] = 0;
            world.LiquidType[sourceIndex] = 0;
            world.Liquid[targetIndex] = 0;
            world.LiquidType[targetIndex] = 0;
            world.Types[sourceIndex] = (ushort)TileType.Stone;
            world.Types[targetIndex] = (ushort)TileType.Stone;
            return true;
        }

        if (sourceType == LiquidType.Lava && targetType == LiquidType.Water)
        {
            world.Liquid[sourceIndex] = 0;
            world.LiquidType[sourceIndex] = 0;
            world.Liquid[targetIndex] = 0;
            world.LiquidType[targetIndex] = 0;
            world.Types[sourceIndex] = (ushort)TileType.Stone;
            world.Types[targetIndex] = (ushort)TileType.Stone;
            return true;
        }

        if (sourceType == LiquidType.Honey && targetType == LiquidType.Water)
        {
            return false;
        }

        if (sourceType == LiquidType.Water && targetType == LiquidType.Honey)
        {
            return false;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TickTile(World w, int x, int y, in ActiveArea a)
    {
        if (!w.InBounds(x, y))
        {
            return false;
        }

        int index = w.ToIndex(x, y);
        if (w.LiquidType[index] == 0)
        {
            return false;
        }

        LiquidType type = (LiquidType)w.LiquidType[index];
        byte amount = w.Liquid[index];
        if (amount == 0)
        {
            w.LiquidType[index] = 0;
            return false;
        }

        if (amount < 2 && IsIsolated(w, x, y))
        {
            SetLiquid(w, x, y, LiquidType.None, 0);
            return true;
        }

        int underY = y + 1;
        if (underY < w.Height && !IsSolid(w, x, underY) && IsOpenToLiquid(w, x, underY, type))
        {
            int underIndex = w.ToIndex(x, underY);
            byte targetAmount = w.Liquid[underIndex];
            int transfer = Math.Min(amount, 255 - targetAmount);
            if (transfer > 0)
            {
                w.Liquid[index] = (byte)(amount - transfer);
                w.Liquid[underIndex] = (byte)(targetAmount + transfer);
                w.LiquidType[underIndex] = w.LiquidType[index];
                if (w.Liquid[index] == 0)
                {
                    w.LiquidType[index] = 0;
                }
                return true;
            }
        }

        int[] horizontalDirs = [ -1, 1 ];
        int dirIndex = ((x + y) & 1) == 0 ? 0 : 1;
        for (int i = 0; i < 2; i++)
        {
            int offset = horizontalDirs[(dirIndex + i) & 1];
            int nx = x + offset;
            int ny = y;
            if (!w.InBounds(nx, ny))
            {
                continue;
            }

            if (IsSolid(w, nx, ny))
            {
                continue;
            }

            int neighborIndex = w.ToIndex(nx, ny);
            if (w.LiquidType[neighborIndex] == 0)
            {
                int total = amount + w.Liquid[neighborIndex];
                if (total > 255)
                {
                    int move = Math.Min(amount, 255 - w.Liquid[neighborIndex]);
                    if (move > 0)
                    {
                        w.Liquid[index] = (byte)(amount - move);
                        w.Liquid[neighborIndex] = (byte)(w.Liquid[neighborIndex] + move);
                        w.LiquidType[neighborIndex] = (byte)type;
                        if (w.Liquid[index] == 0)
                        {
                            w.LiquidType[index] = 0;
                        }
                        return true;
                    }
                }
                else
                {
                    int move = Math.Min(amount, Math.Max(1, (amount + w.Liquid[neighborIndex]) / 2));
                    if (move > 0)
                    {
                        w.Liquid[index] = (byte)(amount - move);
                        w.Liquid[neighborIndex] = (byte)(w.Liquid[neighborIndex] + move);
                        w.LiquidType[neighborIndex] = (byte)type;
                        if (w.Liquid[index] == 0)
                        {
                            w.LiquidType[index] = 0;
                        }
                        return true;
                    }
                }
            }
            else if (w.LiquidType[neighborIndex] == (byte)type)
            {
                int delta = amount - w.Liquid[neighborIndex];
                int transfer = Math.Abs(delta) / 2;
                if (transfer > 0)
                {
                    if (amount > w.Liquid[neighborIndex])
                    {
                        w.Liquid[index] = (byte)(amount - transfer);
                        w.Liquid[neighborIndex] = (byte)(w.Liquid[neighborIndex] + transfer);
                    }
                    else
                    {
                        w.Liquid[index] = (byte)(amount + transfer);
                        w.Liquid[neighborIndex] = (byte)(w.Liquid[neighborIndex] - transfer);
                    }

                    if (w.Liquid[index] == 0)
                    {
                        w.LiquidType[index] = 0;
                    }
                    return true;
                }
            }
            else if (TryReact(w, x, y, nx, ny, type))
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIsolated(World w, int x, int y)
    {
        int index = w.ToIndex(x, y);
        if (w.Liquid[index] >= 2)
        {
            return false;
        }

        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                if (ox == 0 && oy == 0)
                {
                    continue;
                }

                int nx = x + ox;
                int ny = y + oy;
                if (!w.InBounds(nx, ny))
                {
                    continue;
                }

                int neighborIndex = w.ToIndex(nx, ny);
                if (w.LiquidType[neighborIndex] != 0 && w.Liquid[neighborIndex] >= 2)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
