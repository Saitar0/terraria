namespace TerrariaSandbox.Core;

/// <summary>
/// Open-addressed sparse damage map used to accumulate mining progress without allocations.
/// </summary>
public sealed class TileDamage
{
    private int[] _x;
    private int[] _y;
    private float[] _damage;
    private float[] _required;
    private byte[] _active;
    private int _count;

    public TileDamage(int capacity = 256)
    {
        int size = NextPowerOfTwo(Math.Max(16, capacity));
        _x = new int[size];
        _y = new int[size];
        _damage = new float[size];
        _required = new float[size];
        _active = new byte[size];
    }

    public bool Advance(int x, int y, float power, float required, out float remaining)
    {
        EnsureCapacity();
        int index = FindSlot(x, y, true);
        if (_active[index] == 0)
        {
            _x[index] = x;
            _y[index] = y;
            _damage[index] = 0f;
            _required[index] = required;
            _active[index] = 1;
            _count++;
        }

        _damage[index] += power;
        remaining = MathF.Max(0f, _required[index] - _damage[index]);

        if (_damage[index] >= _required[index])
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    public bool Contains(int x, int y)
    {
        int index = FindSlot(x, y, false);
        return index >= 0 && _active[index] != 0;
    }

    public void Clear(int x, int y)
    {
        int index = FindSlot(x, y, false);
        if (index >= 0)
        {
            RemoveAt(index);
        }
    }

    private void EnsureCapacity()
    {
        if (_count < _active.Length * 0.7f)
        {
            return;
        }

        int oldCapacity = _active.Length;
        int[] oldX = _x;
        int[] oldY = _y;
        float[] oldDamage = _damage;
        float[] oldRequired = _required;
        byte[] oldActive = _active;

        int newSize = NextPowerOfTwo(oldCapacity * 2);
        _x = new int[newSize];
        _y = new int[newSize];
        _damage = new float[newSize];
        _required = new float[newSize];
        _active = new byte[newSize];
        _count = 0;

        for (int i = 0; i < oldActive.Length; ++i)
        {
            if (oldActive[i] == 0)
            {
                continue;
            }

            int index = FindSlot(oldX[i], oldY[i], true);
            _x[index] = oldX[i];
            _y[index] = oldY[i];
            _damage[index] = oldDamage[i];
            _required[index] = oldRequired[i];
            _active[index] = 1;
            _count++;
        }
    }

    private void RemoveAt(int index)
    {
        _active[index] = 0;
        _x[index] = 0;
        _y[index] = 0;
        _damage[index] = 0f;
        _required[index] = 0f;
        _count--;

        int next = (index + 1) & (_active.Length - 1);
        while (_active[next] != 0)
        {
            int current = next;
            int currentHash = Hash(_x[current], _y[current]);
            int desired = Hash(_x[index], _y[index]);
            if ((currentHash & (_active.Length - 1)) <= (desired & (_active.Length - 1)))
            {
                break;
            }

            _x[index] = _x[current];
            _y[index] = _y[current];
            _damage[index] = _damage[current];
            _required[index] = _required[current];
            _active[index] = _active[current];
            _active[current] = 0;
            index = current;
            next = (index + 1) & (_active.Length - 1);
        }
    }

    private int FindSlot(int x, int y, bool allowInsert)
    {
        int capacity = _x.Length;
        int slot = Hash(x, y) & (capacity - 1);

        for (int i = 0; i < capacity; ++i)
        {
            int index = (slot + i) & (capacity - 1);
            if (_active[index] == 0)
            {
                return allowInsert ? index : -1;
            }

            if (_x[index] == x && _y[index] == y)
            {
                return index;
            }
        }

        return -1;
    }

    private static int Hash(int x, int y)
    {
        unchecked
        {
            int hash = (x * 397) ^ (y * 8573);
            hash ^= hash >> 16;
            return hash;
        }
    }

    private static int NextPowerOfTwo(int value)
    {
        int result = 1;
        while (result < value)
        {
            result <<= 1;
        }

        return result;
    }
}

/// <summary>
/// Tracks current tile mining progress for the active interaction state.
/// </summary>
public sealed class MiningState
{
    private readonly TileDamage _damage = new();

    public bool Advance(int x, int y, float power, float hardness, out float remaining)
    {
        float required = MathF.Max(1f, hardness * 16f);
        return _damage.Advance(x, y, power, required, out remaining);
    }

    public void Clear(int x, int y)
    {
        _damage.Clear(x, y);
    }
}
