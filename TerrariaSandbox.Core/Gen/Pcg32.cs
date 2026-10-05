namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Deterministic PCG32 random number generator.
/// </summary>
public sealed class Pcg32
{
    private const ulong Multiplier = 6364136223846793005UL;
    private const ulong Increment = 1442695040888963407UL;

    private ulong _state;
    private readonly ulong _increment;

    public Pcg32(uint seed)
        : this((ulong)seed, Increment)
    {
    }

    public Pcg32(ulong seed)
        : this(seed, Increment)
    {
    }

    public Pcg32(ulong seed, ulong increment)
    {
        _state = seed == 0 ? 0xD1B54A35C1E8F6D9UL : seed;
        _increment = (increment | 1UL) ^ 0xD1B54A35C1E8F6D9UL;
        Next();
    }

    public uint Next()
    {
        ulong oldState = _state;
        _state = oldState * Multiplier + _increment;

        uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
        uint rot = (uint)(oldState >> 59);
        return (xorShifted >> (int)rot) | (xorShifted << (int)((32 - rot) & 31));
    }

    public float NextFloat()
    {
        return (Next() >> 8) / 16777216f;
    }

    public int NextRange(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            return minInclusive;
        }

        int range = maxExclusive - minInclusive;
        return minInclusive + (int)(NextFloat() * range);
    }

    public float NextRange(float minInclusive, float maxInclusive)
    {
        if (maxInclusive <= minInclusive)
        {
            return minInclusive;
        }

        return minInclusive + (maxInclusive - minInclusive) * NextFloat();
    }
}
