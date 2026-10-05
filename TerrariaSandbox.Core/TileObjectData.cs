namespace TerrariaSandbox.Core;

/// <summary>
/// Metadata describing a multi-cell tile object, such as a door or platform.
/// The origin is the tile offset from the active cell used to anchor placement.
/// </summary>
public sealed class TileObjectData
{
    private static readonly TileObjectData[] Registry = new TileObjectData[256];

    static TileObjectData()
    {
        Register((ushort)TileType.Door, new TileObjectData(
            width: 2,
            height: 3,
            originX: 0,
            originY: 0,
            anchorX: new[] { 0, 1 },
            anchorY: new[] { 0, 1, 2 }));
    }

    public TileObjectData(int width = 1, int height = 1, int originX = 0, int originY = 0, int[]? anchorX = null, int[]? anchorY = null)
    {
        Width = width;
        Height = height;
        OriginX = originX;
        OriginY = originY;
        AnchorX = anchorX ?? new[] { 0 };
        AnchorY = anchorY ?? new[] { 0 };
    }

    public int Width { get; }
    public int Height { get; }
    public int OriginX { get; }
    public int OriginY { get; }
    public int[] AnchorX { get; }
    public int[] AnchorY { get; }

    public static void Register(ushort tileType, TileObjectData data)
    {
        if (tileType >= Registry.Length)
        {
            return;
        }

        Registry[tileType] = data;
    }

    public static TileObjectData GetForTile(ushort tileType)
    {
        if ((uint)tileType >= (uint)Registry.Length || Registry[tileType] is null)
        {
            return new TileObjectData();
        }

        return Registry[tileType];
    }

    public bool ContainsCell(int worldX, int worldY, int originTileX, int originTileY)
    {
        int minX = originTileX + OriginX;
        int minY = originTileY + OriginY;

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                if (worldX == minX + x && worldY == minY + y)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
