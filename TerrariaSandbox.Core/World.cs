using System.Runtime.CompilerServices;

namespace TerrariaSandbox.Core;

/// <summary>
/// Bitfield definitions for tile flags.
/// </summary>
public static class TileFlags
{
    public const ushort Active = 1 << 0;
    public const ushort Actuated = 1 << 1;
    public const ushort HalfBrick = 1 << 2;
    public const ushort SlopeMask = 7 << 3;
    public const ushort WireRed = 1 << 6;
    public const ushort WireBlue = 1 << 7;
    public const ushort WireGreen = 1 << 8;
    public const ushort WireYellow = 1 << 9;
    public const ushort Actuator = 1 << 10;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetSlope(ushort flags)
    {
        return (ushort)((flags & SlopeMask) >> 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort SetSlope(ushort flags, int slope)
    {
        int normalized = slope & 7;
        return (ushort)((flags & ~SlopeMask) | ((ushort)normalized << 3));
    }
}

/// <summary>
/// Listener notified whenever a tile changes in the world.
/// </summary>
public interface IWorldListener
{
    void OnTileChanged(int x, int y);
}

/// <summary>
/// Static metadata for a tile type.
/// </summary>
public sealed class TileDef
{
    public TileDef(
        string name,
        bool solid,
        bool lightBlock,
        byte lightR,
        byte lightG,
        byte lightB,
        bool frameImportant,
        int color,
        float hardness,
        bool breakable = true,
        bool supports = false,
        bool fallsWithoutSupport = false)
    {
        Name = name;
        Solid = solid;
        LightBlock = lightBlock;
        LightR = lightR;
        LightG = lightG;
        LightB = lightB;
        FrameImportant = frameImportant;
        Color = color;
        Hardness = hardness;
        Breakable = breakable;
        Supports = supports;
        FallsWithoutSupport = fallsWithoutSupport;
    }

    public string Name { get; }
    public bool Solid { get; }
    public bool LightBlock { get; }
    public byte LightR { get; }
    public byte LightG { get; }
    public byte LightB { get; }
    public bool FrameImportant { get; }
    public int Color { get; }
    public float Hardness { get; }
    public bool Breakable { get; }
    public bool Supports { get; }
    public bool FallsWithoutSupport { get; }
}

/// <summary>
/// Static metadata for a wall type.
/// </summary>
public sealed class WallDef
{
    public WallDef(string name, bool solid, bool opaque)
    {
        Name = name;
        Solid = solid;
        Opaque = opaque;
    }

    public string Name { get; }
    public bool Solid { get; }
    public bool Opaque { get; }
}

/// <summary>
/// Static tile wall definitions used by the world storage model.
/// </summary>
public static class TileDefs
{
    public static readonly TileDef[] Tiles =
    [
        new("Air", false, false, 0, 0, 0, false, 0, 0f),
        new("Dirt", true, false, 0, 0, 0, false, 0x5B3A1F, 1f),
        new("Grass", true, false, 0, 0, 0, false, 0x2F8F40, 1f),
        new("Stone", true, true, 0, 0, 0, false, 0x5C5C5C, 2f),
        new("Sand", true, false, 0, 0, 0, false, 0xD9C57B, 0.7f),
        new("Water", false, false, 60, 110, 180, false, 0x3C6EB4, 0f),
        new("Wood", true, false, 0, 0, 0, false, 0x7F5B2E, 1.4f),
        new("Leaves", false, false, 40, 120, 60, false, 0x3A9D4A, 0.25f),
        new("Snow", true, false, 0, 0, 0, false, 0xDDEAEA, 1.2f),
        new("Platform", false, false, 0, 0, 0, false, 0xB98A5D, 0.3f),
        new("Brick", true, true, 0, 0, 0, false, 0x7D7A6C, 2.6f),
        new("CopperOre", true, true, 120, 140, 170, false, 0x6C7FA9, 2.5f),
        new("SilverOre", true, true, 140, 140, 190, false, 0xA6B7D8, 2.8f),
        new("GoldOre", true, true, 180, 170, 90, false, 0xD9BE42, 3.1f),
        new("Gem", true, true, 160, 90, 220, false, 0xFF7DE7, 3.2f),
        new("JungleGrass", true, false, 0, 0, 0, false, 0x2E8B57, 1f),
        new("JungleDirt", true, false, 0, 0, 0, false, 0x5A7A3B, 1f),
        new("CorruptGrass", true, false, 0, 0, 0, false, 0x5B2B7D, 1.1f),
        new("CorruptStone", true, true, 0, 0, 0, false, 0x3D2F4A, 2.2f),
        new("Sandstone", true, true, 0, 0, 0, false, 0xD0B77A, 1.8f),
        new("Ice", true, false, 120, 180, 220, false, 0xCDE9FF, 1.2f),
        new("DirtWall", false, false, 0, 0, 0, false, 0, 0f),
        new("StoneWall", false, false, 0, 0, 0, false, 0, 0f),
        new("FrozenWall", false, false, 0, 0, 0, false, 0, 0f),
        new("Door", true, false, 0, 0, 0, false, 0x8B5E3C, 1.8f),
        new("Torch", false, false, 255, 190, 80, false, 0xFFBE4D, 0.1f, true, false, true),
        new("Plant", false, false, 40, 110, 55, false, 0x3C9C4A, 0.1f, true, false, true),
        new("Workbench", true, false, 150, 110, 70, false, 0xB77A4B, 1.6f),
        new("Furnace", true, true, 150, 120, 90, false, 0x9A8B7A, 2.8f),
        new("Lava", false, false, 220, 70, 20, false, 0xD94E18, 0f),
        new("Honey", false, false, 255, 190, 70, false, 0xFFD34D, 0f),
        new("Table", true, false, 140, 90, 60, false, 0x8B5A2B, 1.1f),
        new("Anvil", true, true, 130, 130, 140, false, 0x9495A0, 3.4f),
        new("Campfire", false, false, 255, 120, 35, false, 0xE77C22, 0.2f)
    ];

    public static readonly WallDef[] Walls =
    [
        new("Air", false, false),
        new("Dirt", false, false),
        new("Grass", false, false),
        new("Stone", true, true),
        new("Sand", false, false),
        new("Water", false, false),
        new("Wood", false, false),
        new("Leaves", false, false),
        new("Snow", false, false),
        new("Platform", false, false),
        new("Brick", true, true),
        new("CopperOre", true, true),
        new("SilverOre", true, true),
        new("GoldOre", true, true),
        new("Gem", true, true),
        new("JungleGrass", false, false),
        new("JungleDirt", false, false),
        new("CorruptGrass", false, false),
        new("CorruptStone", true, true),
        new("Sandstone", true, true),
        new("Ice", false, false),
        new("DirtWall", false, false),
        new("StoneWall", true, true),
        new("FrozenWall", false, false),
        new("Door", false, false),
        new("Torch", false, false),
        new("Plant", false, false),
        new("Workbench", false, false),
        new("Furnace", false, false),
        new("Lava", false, false),
        new("Honey", false, false),
        new("Table", false, false),
        new("Anvil", false, false),
        new("Campfire", false, false)
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TileDef GetTile(ushort type)
    {
        return type < Tiles.Length ? Tiles[type] : Tiles[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WallDef GetWall(ushort type)
    {
        return type < Walls.Length ? Walls[type] : Walls[0];
    }
}

/// <summary>
/// Represents a logical chunk with dirty tracking flags.
/// </summary>
public sealed class Chunk
{
    public bool DirtyRender { get; set; }
    public bool DirtyLight { get; set; }
    public bool DirtyNetwork { get; set; }
    public bool DirtyLiquid { get; set; }
}

/// <summary>
/// Maintains the chunk grid and dirty propagation for a world.
/// </summary>
public sealed class ChunkGrid
{
    private readonly int _worldWidth;
    private readonly int _worldHeight;

    public ChunkGrid(int worldWidth, int worldHeight)
    {
        _worldWidth = worldWidth;
        _worldHeight = worldHeight;
        ChunkCountX = (worldWidth + WorldConstants.ChunkSideTiles - 1) / WorldConstants.ChunkSideTiles;
        ChunkCountY = (worldHeight + WorldConstants.ChunkSideTiles - 1) / WorldConstants.ChunkSideTiles;
        Chunks = GC.AllocateUninitializedArray<Chunk>(ChunkCountX * ChunkCountY);

        for (int i = 0; i < Chunks.Length; ++i)
        {
            Chunks[i] = new Chunk();
        }
    }

    public int ChunkCountX { get; }
    public int ChunkCountY { get; }
    public Chunk[] Chunks { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Chunk GetChunk(int x, int y)
    {
        int chunkX = x / WorldConstants.ChunkSideTiles;
        int chunkY = y / WorldConstants.ChunkSideTiles;
        return Chunks[GetChunkIndex(chunkX, chunkY)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetChunkIndex(int chunkX, int chunkY)
    {
        if ((uint)chunkX >= (uint)ChunkCountX || (uint)chunkY >= (uint)ChunkCountY)
        {
            return 0;
        }

        return chunkY * ChunkCountX + chunkX;
    }

    public void MarkDirty(int x, int y)
    {
        int chunkX = x / WorldConstants.ChunkSideTiles;
        int chunkY = y / WorldConstants.ChunkSideTiles;

        MarkChunkDirty(chunkX, chunkY);

        if ((x & (WorldConstants.ChunkSideTiles - 1)) == 0 && chunkX > 0)
        {
            MarkChunkDirty(chunkX - 1, chunkY);
        }

        if ((x & (WorldConstants.ChunkSideTiles - 1)) == WorldConstants.ChunkSideTiles - 1 && chunkX + 1 < ChunkCountX)
        {
            MarkChunkDirty(chunkX + 1, chunkY);
        }

        if ((y & (WorldConstants.ChunkSideTiles - 1)) == 0 && chunkY > 0)
        {
            MarkChunkDirty(chunkX, chunkY - 1);
        }

        if ((y & (WorldConstants.ChunkSideTiles - 1)) == WorldConstants.ChunkSideTiles - 1 && chunkY + 1 < ChunkCountY)
        {
            MarkChunkDirty(chunkX, chunkY + 1);
        }

        if ((x & (WorldConstants.ChunkSideTiles - 1)) == 0 && (y & (WorldConstants.ChunkSideTiles - 1)) == 0 && chunkX > 0 && chunkY > 0)
        {
            MarkChunkDirty(chunkX - 1, chunkY - 1);
        }

        if ((x & (WorldConstants.ChunkSideTiles - 1)) == WorldConstants.ChunkSideTiles - 1 && (y & (WorldConstants.ChunkSideTiles - 1)) == 0 && chunkX + 1 < ChunkCountX && chunkY > 0)
        {
            MarkChunkDirty(chunkX + 1, chunkY - 1);
        }

        if ((x & (WorldConstants.ChunkSideTiles - 1)) == 0 && (y & (WorldConstants.ChunkSideTiles - 1)) == WorldConstants.ChunkSideTiles - 1 && chunkX > 0 && chunkY + 1 < ChunkCountY)
        {
            MarkChunkDirty(chunkX - 1, chunkY + 1);
        }

        if ((x & (WorldConstants.ChunkSideTiles - 1)) == WorldConstants.ChunkSideTiles - 1 && (y & (WorldConstants.ChunkSideTiles - 1)) == WorldConstants.ChunkSideTiles - 1 && chunkX + 1 < ChunkCountX && chunkY + 1 < ChunkCountY)
        {
            MarkChunkDirty(chunkX + 1, chunkY + 1);
        }
    }

    private void MarkChunkDirty(int chunkX, int chunkY)
    {
        if ((uint)chunkX >= (uint)ChunkCountX || (uint)chunkY >= (uint)ChunkCountY)
        {
            return;
        }

        Chunk chunk = Chunks[chunkY * ChunkCountX + chunkX];
        chunk.DirtyRender = true;
        chunk.DirtyLight = true;
        chunk.DirtyNetwork = true;
        chunk.DirtyLiquid = true;
    }
}

/// <summary>
/// Ring-buffer for tile-frame refresh. Uses a bitset to avoid duplicate enqueues.
/// </summary>
public sealed class TileFrameQueue
{
    private readonly int[] _buffer;
    private readonly ulong[] _enqueued;
    private readonly int _capacity;
    private int _head;
    private int _tail;

    public TileFrameQueue(int capacity)
    {
        _capacity = capacity > 0 ? capacity : 256;
        _buffer = new int[_capacity];
        _enqueued = new ulong[(_capacity + 63) / 64];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty()
    {
        return _head == _tail;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ToSlot(int x, int y, int worldWidth)
    {
        int tileIndex = y * worldWidth + x;
        return tileIndex % _capacity;
    }

    public void Enqueue(int x, int y, int worldWidth)
    {
        int slot = ToSlot(x, y, worldWidth);
        int bit = slot & 63;
        int word = slot >> 6;

        if ((ulong)(_enqueued[word] & (1UL << bit)) != 0UL)
        {
            return;
        }

        _enqueued[word] |= 1UL << bit;
        _buffer[_tail] = slot;
        _tail = (_tail + 1) % _capacity;

        if (_tail == _head)
        {
            _head = (_head + 1) % _capacity;
        }
    }

    public bool TryDequeue(out int x, out int y, int worldWidth)
    {
        if (IsEmpty())
        {
            x = 0;
            y = 0;
            return false;
        }

        int slot = _buffer[_head];
        _head = (_head + 1) % _capacity;
        int bit = slot & 63;
        int word = slot >> 6;
        _enqueued[word] &= ~(1UL << bit);

        x = slot % worldWidth;
        y = slot / worldWidth;
        return true;
    }
}

/// <summary>
/// Dense world storage using a struct-of-arrays layout with logical chunks.
/// </summary>
public sealed class World
{
    private IWorldListener[] _listeners;
    private int _listenerCount;

    public World(int width, int height)
    {
        Width = width;
        Height = height;
        SupportChecker = new SupportChecker();

        long totalTiles = (long)width * height;
        if (totalTiles <= 0 || totalTiles > int.MaxValue)
        {
            Types = Array.Empty<ushort>();
            Walls = Array.Empty<ushort>();
            Liquid = Array.Empty<byte>();
            LiquidType = Array.Empty<byte>();
            Flags = Array.Empty<ushort>();
            FrameX = Array.Empty<short>();
            FrameY = Array.Empty<short>();
            Paint = Array.Empty<byte>();
            Chunks = new ChunkGrid(0, 0);
            FrameQueue = new TileFrameQueue(256);
            LiquidQueue = new LiquidQueue(8192);
            _listeners = Array.Empty<IWorldListener>();
            _listenerCount = 0;
            return;
        }

        int tileCount = checked((int)totalTiles);
        Types = GC.AllocateUninitializedArray<ushort>(tileCount);
        Walls = GC.AllocateUninitializedArray<ushort>(tileCount);
        Liquid = GC.AllocateUninitializedArray<byte>(tileCount);
        LiquidType = GC.AllocateUninitializedArray<byte>(tileCount);
        Flags = GC.AllocateUninitializedArray<ushort>(tileCount);
        FrameX = GC.AllocateUninitializedArray<short>(tileCount);
        FrameY = GC.AllocateUninitializedArray<short>(tileCount);
        Paint = GC.AllocateUninitializedArray<byte>(tileCount);

        Chunks = new ChunkGrid(width, height);
        FrameQueue = new TileFrameQueue(Math.Max(256, Math.Min(4096, width * height / 16)));
        LiquidQueue = new LiquidQueue(Math.Max(8192, Math.Min(65536, width * height / 8)));
        _listeners = Array.Empty<IWorldListener>();
        _listenerCount = 0;
    }

    public void GenerateTerrain(int seed)
    {
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                int noise = Noise2D(x, y, seed);
                ushort tile = 0;

                if (y < 32)
                {
                    tile = (ushort)TileType.Snow;
                }
                else if (y < 72)
                {
                    tile = (ushort)TileType.Grass;
                }
                else if (y < 120)
                {
                    tile = (ushort)TileType.Dirt;
                }
                else if (y < 180)
                {
                    tile = (ushort)TileType.Stone;
                }
                else if ((noise % 7) == 0)
                {
                    tile = (ushort)TileType.Sand;
                }
                else if ((noise % 5) == 0)
                {
                    tile = (ushort)TileType.Stone;
                }

                Set(x, y, tile);
            }
        }
    }

    public int Width { get; }
    public int Height { get; }
    public string Name { get; set; } = "New World";
    public uint Seed { get; set; }
    public Vector2Int Spawn { get; set; }
    public int WorldSurface { get; set; }
    public int RockLayer { get; set; }
    public int TimeOfDay { get; set; }
    public BossFlags BossFlags { get; set; }
    public ushort[] Types { get; }
    public ushort[] Walls { get; }
    public byte[] Liquid { get; }
    public byte[] LiquidType { get; }
    public ushort[] Flags { get; }
    public short[] FrameX { get; }
    public short[] FrameY { get; }
    public byte[] Paint { get; }
    public ChunkGrid Chunks { get; }
    public TileFrameQueue FrameQueue { get; }
    public LiquidQueue LiquidQueue { get; }
    public SupportChecker SupportChecker { get; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBounds(int x, int y)
    {
        return (uint)x < (uint)Width && (uint)y < (uint)Height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ClampX(int x)
    {
        if (Width <= 0)
        {
            return 0;
        }

        if (x < 0)
        {
            return 0;
        }

        if (x >= Width)
        {
            return Width - 1;
        }

        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ClampY(int y)
    {
        if (Height <= 0)
        {
            return 0;
        }

        if (y < 0)
        {
            return 0;
        }

        if (y >= Height)
        {
            return Height - 1;
        }

        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ToIndex(int x, int y)
    {
        return y * Width + x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort Get(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return 0;
        }

        return Types[ToIndex(x, y)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort GetWall(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return 0;
        }

        return Walls[ToIndex(x, y)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref ushort GetUnsafe(int x, int y)
    {
        int clampedX = ClampX(x);
        int clampedY = ClampY(y);
        return ref Types[ToIndex(clampedX, clampedY)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y, ushort type)
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        int clampedX = ClampX(x);
        int clampedY = ClampY(y);
        int index = ToIndex(clampedX, clampedY);

        Types[index] = type;
        MarkTileDirty(clampedX, clampedY);
        SupportChecker.NotifyTileChanged(this, clampedX, clampedY);

        TileDef tileDef = TileDefs.GetTile(type);
        if (tileDef.FallsWithoutSupport && SupportChecker.NeedsSupport(this, clampedX, clampedY))
        {
            Types[index] = 0;
            MarkTileDirty(clampedX, clampedY);
            SupportChecker.NotifyTileChanged(this, clampedX, clampedY);
        }

        NotifyListeners(clampedX, clampedY);
        EnqueueTileFrameRing(clampedX, clampedY);
        WakeLiquid(clampedX, clampedY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWall(int x, int y, ushort wallType)
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        int clampedX = ClampX(x);
        int clampedY = ClampY(y);
        int index = ToIndex(clampedX, clampedY);
        Walls[index] = wallType;
        MarkTileDirty(clampedX, clampedY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLiquid(int x, int y, byte amount, byte type)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        int index = ToIndex(x, y);
        Liquid[index] = amount;
        LiquidType[index] = type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTile(int x, int y, ushort type)
    {
        Set(x, y, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WakeLiquid(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        LiquidQueue.Enqueue(x, y);

        for (int oy = -1; oy <= 1; ++oy)
        {
            int ny = y + oy;
            if ((uint)ny >= (uint)Height)
            {
                continue;
            }

            for (int ox = -1; ox <= 1; ++ox)
            {
                int nx = x + ox;
                if ((uint)nx >= (uint)Width)
                {
                    continue;
                }

                LiquidQueue.Enqueue(nx, ny);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessSupport(int maxPerTick)
    {
        SupportChecker.Update(this, maxPerTick);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddListener(IWorldListener listener)
    {
        if (listener is null)
        {
            return;
        }

        if (_listenerCount == _listeners.Length)
        {
            IWorldListener[] next = new IWorldListener[Math.Max(4, _listeners.Length * 2)];
            Array.Copy(_listeners, next, _listenerCount);
            _listeners = next;
        }

        _listeners[_listenerCount++] = listener;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveListener(IWorldListener listener)
    {
        for (int i = 0; i < _listenerCount; ++i)
        {
            if (_listeners[i] == listener)
            {
                _listeners[i] = _listeners[_listenerCount - 1];
                _listeners[_listenerCount - 1] = null!;
                _listenerCount--;
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSolid(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return false;
        }

        return TileDefs.GetTile(Types[ToIndex(x, y)]).Solid;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MarkTileDirty(int x, int y)
    {
        Chunks.MarkDirty(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotifyListeners(int x, int y)
    {
        for (int i = 0; i < _listenerCount; ++i)
        {
            _listeners[i].OnTileChanged(x, y);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnqueueTileFrameRing(int x, int y)
    {
        FrameQueue.Enqueue(x, y, Width);

        for (int offsetY = -1; offsetY <= 1; ++offsetY)
        {
            int neighborY = y + offsetY;
            if ((uint)neighborY >= (uint)Height)
            {
                continue;
            }

            for (int offsetX = -1; offsetX <= 1; ++offsetX)
            {
                int neighborX = x + offsetX;
                if ((uint)neighborX >= (uint)Width)
                {
                    continue;
                }

                FrameQueue.Enqueue(neighborX, neighborY, Width);
            }
        }
    }

    private static int Noise2D(int x, int y, int seed)
    {
        int value = x * 374761393 + y * 668265263 + seed * 1103515245;
        value = (value ^ (value >> 13)) * 127773;
        value = value ^ (value >> 7);
        return value & 0x7FFFFFFF;
    }
}
