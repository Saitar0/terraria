namespace TerrariaSandbox.Core.IO;

/// <summary>
/// Compact, column-oriented RLE encoding for world tiles.
/// </summary>
public static class TileRle
{
    private const byte HasHeader2 = 1 << 0;
    private const byte Active = 1 << 1;
    private const byte HasWall = 1 << 2;
    private const byte LiquidShift = 3;
    private const byte LiquidMask = 0b11 << LiquidShift;
    private const byte TypeUShort = 1 << 5;
    private const byte RleMask = 0b11 << 6;

    public static void Encode(World world, BinaryWriter writer)
    {
        if (world is null)
        {
            throw new ArgumentNullException(nameof(world));
        }

        if (writer is null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        for (int x = 0; x < world.Width; ++x)
        {
            int y = 0;
            while (y < world.Height)
            {
                int index = world.ToIndex(x, y);
                ushort tileType = world.Get(x, y);
                ushort wallType = world.GetWall(x, y);
                byte liquid = world.Liquid[index];
                byte liquidType = world.LiquidType[index];
                ushort flags = world.Flags[index];
                int runLength = 1;

                while (y + runLength < world.Height)
                {
                    int nextIndex = world.ToIndex(x, y + runLength);
                    if (world.Get(x, y + runLength) != tileType)
                    {
                        break;
                    }

                    if (world.GetWall(x, y + runLength) != wallType)
                    {
                        break;
                    }

                    if (world.Liquid[nextIndex] != liquid || world.LiquidType[nextIndex] != liquidType || world.Flags[nextIndex] != flags)
                    {
                        break;
                    }

                    runLength++;
                }

                WriteCell(writer, tileType, wallType, liquid, liquidType, flags, runLength);
                y += runLength;
            }
        }
    }

    public static void Decode(Stream stream, World world)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (world is null)
        {
            throw new ArgumentNullException(nameof(world));
        }

        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        for (int x = 0; x < world.Width; ++x)
        {
            for (int y = 0; y < world.Height;)
            {
                byte header1 = reader.ReadByte();
                int runLength = 1;
                int rleMode = (header1 & RleMask) >> 6;
                if (rleMode == 1)
                {
                    runLength = reader.ReadByte();
                }
                else if (rleMode == 2)
                {
                    runLength = reader.ReadUInt16();
                }
                else if (rleMode == 3)
                {
                    runLength = reader.ReadInt32();
                }

                ushort tileType = (ushort)(header1 & TypeUShort) != 0 ? reader.ReadUInt16() : reader.ReadByte();
                ushort wallType = (header1 & HasWall) != 0 ? reader.ReadUInt16() : (ushort)0;
                byte liquidType = (byte)((header1 & LiquidMask) >> LiquidShift);
                byte liquidValue = liquidType == 0 ? (byte)0 : reader.ReadByte();
                ushort flags = 0;
                if ((header1 & HasHeader2) != 0)
                {
                    byte header2 = reader.ReadByte();
                    byte header3 = reader.ReadByte();
                    flags = (ushort)(header2 | (header3 << 8));
                }

                for (int offset = 0; offset < runLength && y + offset < world.Height; ++offset)
                {
                    int index = world.ToIndex(x, y + offset);
                    world.Types[index] = tileType;
                    world.Walls[index] = wallType;
                    world.Liquid[index] = liquidValue;
                    world.LiquidType[index] = liquidType;
                    world.Flags[index] = flags;
                }

                y += runLength;
            }
        }
    }

    private static void WriteCell(BinaryWriter writer, ushort tileType, ushort wallType, byte liquid, byte liquidType, ushort flags, int runLength)
    {
        byte header1 = 0;
        if (tileType != 0)
        {
            header1 |= Active;
        }

        if (wallType != 0)
        {
            header1 |= HasWall;
        }

        if (flags != 0)
        {
            header1 |= HasHeader2;
        }

        if (liquidType != 0)
        {
            header1 |= (byte)((liquidType & 0x03) << LiquidShift);
        }

        if (tileType > byte.MaxValue)
        {
            header1 |= TypeUShort;
        }

        if (runLength <= 1)
        {
            header1 |= (byte)(0 << 6);
        }
        else if (runLength <= byte.MaxValue)
        {
            header1 |= (byte)(1 << 6);
        }
        else if (runLength <= ushort.MaxValue)
        {
            header1 |= (byte)(2 << 6);
        }
        else
        {
            header1 |= (byte)(3 << 6);
        }

        writer.Write(header1);

        if (runLength > 1 && runLength <= byte.MaxValue)
        {
            writer.Write((byte)runLength);
        }
        else if (runLength > byte.MaxValue && runLength <= ushort.MaxValue)
        {
            writer.Write((ushort)runLength);
        }
        else if (runLength > ushort.MaxValue)
        {
            writer.Write(runLength);
        }

        if ((header1 & TypeUShort) != 0)
        {
            writer.Write(tileType);
        }
        else
        {
            writer.Write((byte)tileType);
        }

        if ((header1 & HasWall) != 0)
        {
            writer.Write(wallType);
        }

        if (liquidType != 0)
        {
            writer.Write(liquid);
        }

        if ((header1 & HasHeader2) != 0)
        {
            writer.Write((byte)(flags & 0xFF));
            writer.Write((byte)((flags >> 8) & 0xFF));
        }
    }
}
