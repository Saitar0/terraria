namespace TerrariaSandbox.Core.IO;

/// <summary>
/// Fast non-cryptographic hash used to validate world-file integrity.
/// </summary>
public static class IntegrityHash
{
    public static ulong Compute(ReadOnlySpan<byte> bytes)
    {
        ulong hash = 14695981039346656037UL;

        foreach (byte value in bytes)
        {
            hash ^= value;
            hash *= 1099511628211UL;
        }

        return hash;
    }

    public static ulong Compute(byte[] bytes, int offset, int count)
    {
        if (bytes is null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        if (offset < 0 || count < 0 || offset + count > bytes.Length)
        {
            throw new ArgumentOutOfRangeException();
        }

        return Compute(new ReadOnlySpan<byte>(bytes, offset, count));
    }
}
