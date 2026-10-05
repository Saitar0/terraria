namespace TerrariaSandbox.Core.IO;

/// <summary>
/// Encapsulates the save-version migration chain.
/// </summary>
public sealed class SaveMigrator
{
    public const int Version1 = 1;
    public const int Version2 = 2;

    public int Migrate(int version)
    {
        int current = version;
        while (current < WorldSerializer.CurrentVersion)
        {
            current = current switch
            {
                Version1 => Migrate_v1_to_v2(),
                _ => throw new InvalidOperationException($"Unsupported world version: {current}.")
            };
        }

        return current;
    }

    public VersionedWorldHeader LoadVersioned(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        byte[] magic = reader.ReadBytes(7);
        bool rawMagic = magic.SequenceEqual("MYWORLD"u8.ToArray());
        if (!rawMagic)
        {
            stream.Position = 0;
            string? stringMagic = reader.ReadString();
            if (!string.Equals(stringMagic, "MYWORLD", StringComparison.Ordinal))
            {
                throw new InvalidDataException("World file magic is invalid.");
            }
        }

        if (!rawMagic)
        {
            stream.Position = 0;
            reader.ReadString();
        }

        int version = reader.ReadInt32();
        int[] sections = new int[6];
        for (int i = 0; i < sections.Length; ++i)
        {
            sections[i] = reader.ReadInt32();
        }

        return new VersionedWorldHeader
        {
            Magic = "MYWORLD",
            Version = Migrate(version),
            SectionOffsets = sections
        };
    }

    private static int Migrate_v1_to_v2()
    {
        return Version2;
    }
}

/// <summary>
/// Versioned world header used while loading and migrating world files.
/// </summary>
public sealed class VersionedWorldHeader
{
    public required string Magic { get; init; }
    public int Version { get; set; }
    public int[] SectionOffsets { get; init; } = Array.Empty<int>();
}
