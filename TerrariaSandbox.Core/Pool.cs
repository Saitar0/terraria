namespace TerrariaSandbox.Core;

/// <summary>
/// Fixed-capacity pool backed by a dense array and a free-index stack.
/// </summary>
public sealed class Pool<T> where T : struct
{
    private readonly T[] _items;
    private readonly int[] _freeStack;
    private readonly bool[] _active;
    private int _freeCount;
    private int _nextIndex;

    public Pool(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _items = new T[capacity];
        _freeStack = new int[capacity];
        _active = new bool[capacity];
        _nextIndex = 0;
        _freeCount = 0;

        for (int i = 0; i < capacity; ++i)
        {
            _freeStack[i] = i;
        }

        _nextIndex = 0;
    }

    public int Capacity => _items.Length;
    public int ActiveCount { get; private set; }

    public ref T this[int index]
    {
        get => ref _items[index];
    }

    public bool IsActive(int index)
    {
        return (uint)index < (uint)_items.Length && _active[index];
    }

    public int Spawn(T value)
    {
        int index;
        if (_freeCount > 0)
        {
            index = _freeStack[--_freeCount];
        }
        else if (_nextIndex < _items.Length)
        {
            index = _nextIndex++;
        }
        else
        {
            return -1;
        }

        _items[index] = value;
        _active[index] = true;
        ActiveCount++;
        return index;
    }

    public bool Despawn(int index)
    {
        if (!IsActive(index))
        {
            return false;
        }

        _active[index] = false;
        _items[index] = default;
        _freeStack[_freeCount++] = index;
        ActiveCount--;
        return true;
    }

    public void Clear()
    {
        for (int i = 0; i < _items.Length; ++i)
        {
            _items[i] = default;
            _active[i] = false;
            _freeStack[i] = i;
        }

        _freeCount = _items.Length;
        _nextIndex = 0;
        ActiveCount = 0;
    }
}
