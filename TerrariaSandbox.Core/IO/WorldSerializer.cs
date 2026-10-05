using System.Buffers.Binary;
using System.Text;

namespace TerrariaSandbox.Core.IO;

/// <summary>
/// Saves and loads world files with a compact header, versioned migrations and integrity footer.
/// </summary>
public sealed class WorldSerializer
{
    public const int CurrentVersion = 2;
    public const string Magic = "MYWORLD";

    private readonly SaveMigrator _migrator = new();

    public WorldSerializer()
    {
    }

    public void Save(World world, Stream destination)
    {
        if (world is null)
        {
            throw new ArgumentNullException(nameof(world));
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        using var buffer = new MemoryStream();
        using (var writer = new BinaryWriter(buffer, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Encoding.ASCII.GetBytes(Magic));
            writer.Write(CurrentVersion);

            long offsetsPosition = buffer.Position;
            for (int i = 0; i < 6; ++i)
            {
                writer.Write(0);
            }

            int[] sectionOffsets = new int[6];
            sectionOffsets[0] = (int)buffer.Position;
            WriteMetadata(writer, world);

            sectionOffsets[1] = (int)buffer.Position;
            TileRle.Encode(world, writer);

            sectionOffsets[2] = (int)buffer.Position;
            WriteEmptySection(writer, 0);
            sectionOffsets[3] = (int)buffer.Position;
            WriteEmptySection(writer, 0);
            sectionOffsets[4] = (int)buffer.Position;
            WriteEmptySection(writer, 0);
            sectionOffsets[5] = (int)buffer.Position;

            buffer.Position = offsetsPosition;
            for (int i = 0; i < sectionOffsets.Length; ++i)
            {
                writer.Write(sectionOffsets[i]);
            }

            buffer.Position = buffer.Length;
            ulong hash = IntegrityHash.Compute(buffer.GetBuffer(), 0, (int)buffer.Length);
            writer.Write(hash);
        }

        byte[] data = buffer.ToArray();
        destination.Write(data, 0, data.Length);
    }

    public void SaveToFile(string path, World world)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string tempPath = path + ".tmp";
        string backupPath = path + ".bak";

        try
        {
            using (var file = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536))
            {
                Save(world, file);
                file.Flush(true);
            }

            if (File.Exists(path))
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                File.Move(path, backupPath, overwrite: true);
            }

            File.Move(tempPath, path, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }

    public World Load(Stream source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        byte[] data = ReadAllBytes(source);
        if (data.Length < 7 + 4 + (6 * 4) + 8)
        {
            throw new InvalidDataException("World file is too short to contain a valid header.");
        }

        int index = 0;
        byte[] magicBytes = new byte[7];
        Array.Copy(data, index, magicBytes, 0, 7);
        index += 7;

        if (!Encoding.ASCII.GetString(magicBytes).Equals(Magic, StringComparison.Ordinal))
        {
            throw new InvalidDataException("World file does not start with the MYWORLD magic header.");
        }

        int version = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(index));
        index += sizeof(int);

        int[] sectionOffsets = new int[6];
        for (int i = 0; i < sectionOffsets.Length; ++i)
        {
            sectionOffsets[i] = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(index));
            index += sizeof(int);
        }

        version = _migrator.Migrate(version);
        var world = ReadMetadata(data, sectionOffsets, version);
        if (sectionOffsets[1] >= 0 && sectionOffsets[1] < data.Length)
        {
            using var tileStream = new MemoryStream(data, sectionOffsets[1], data.Length - sectionOffsets[1]);
            TileRle.Decode(tileStream, world);
        }

        int footerOffset = data.Length - sizeof(ulong);
        ulong expected = BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(footerOffset));
        ulong actual = IntegrityHash.Compute(data.AsSpan(0, footerOffset));
        if (actual != expected)
        {
            throw new InvalidDataException("World file integrity check failed.");
        }

        return world;
    }

    private static void WriteMetadata(BinaryWriter writer, World world)
    {
        writer.Write(world.Name ?? string.Empty);
        writer.Write(world.Seed);
        writer.Write(world.Width);
        writer.Write(world.Height);
        writer.Write(world.Spawn.X);
        writer.Write(world.Spawn.Y);
        writer.Write(world.WorldSurface);
        writer.Write(world.RockLayer);
        writer.Write(world.TimeOfDay);
        writer.Write((uint)world.BossFlags);
    }

    private static World ReadMetadata(byte[] data, int[] offsets, int version)
    {
        using var metadataStream = new MemoryStream(data, offsets[0], data.Length - offsets[0]);
        using var reader = new BinaryReader(metadataStream, Encoding.UTF8, leaveOpen: true);

        var name = reader.ReadString();
        uint seed = reader.ReadUInt32();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        var world = new World(width, height)
        {
            Name = name,
            Seed = seed,
            Spawn = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
            WorldSurface = reader.ReadInt32(),
            RockLayer = reader.ReadInt32(),
            TimeOfDay = reader.ReadInt32(),
            BossFlags = (BossFlags)reader.ReadUInt32()
        };

        return world;
    }

    private static void WriteEmptySection(BinaryWriter writer, int value)
    {
        writer.Write(value);
    }

    private static byte[] ReadAllBytes(Stream source)
    {
        if (source.CanSeek)
        {
            long originalPosition = source.Position;
            long length = source.Length - source.Position;
            byte[] buffer = new byte[length];
            source.ReadExactly(buffer, 0, buffer.Length);
            source.Position = originalPosition;
            return buffer;
        }

        using var ms = new MemoryStream();
        source.CopyTo(ms);
        return ms.ToArray();
    }
}
