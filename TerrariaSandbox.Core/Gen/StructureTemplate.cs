namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// A compact, deterministic structure definition with tile and wall data.
/// </summary>
public readonly struct StructureTile
{
    public StructureTile(int x, int y, ushort tileId, ushort wallId, ushort flags)
    {
        X = x;
        Y = y;
        TileId = tileId;
        WallId = wallId;
        Flags = flags;
    }

    public int X { get; }
    public int Y { get; }
    public ushort TileId { get; }
    public ushort WallId { get; }
    public ushort Flags { get; }
}

public sealed class StructureTemplate
{
    public StructureTemplate(StructureTile[] tiles, int[] tileIds, int[] wallIds, int anchorX, int anchorY, int minX, int minY, int maxX, int maxY, int width = 0, int height = 0)
    {
        Tiles = tiles ?? Array.Empty<StructureTile>();
        TileIds = tileIds ?? Array.Empty<int>();
        WallIds = wallIds ?? Array.Empty<int>();
        AnchorX = anchorX;
        AnchorY = anchorY;
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
        Width = width > 0 ? width : ComputeWidth();
        Height = height > 0 ? height : ComputeHeight();
    }

    public StructureTile[] Tiles { get; }
    public int[] TileIds { get; }
    public int[] WallIds { get; }
    public int AnchorX { get; }
    public int AnchorY { get; }
    public int MinX { get; }
    public int MinY { get; }
    public int MaxX { get; }
    public int MaxY { get; }
    public int Width { get; }
    public int Height { get; }

    public bool IsValidForTerrain(int originX, int originY, int minX, int minY, int maxX, int maxY)
    {
        return Width > 0 && Height > 0 && (originY + minY) >= 0 && (originY + maxY) >= 0;
    }

    public bool CanPlaceAt(int worldX, int worldY, int terrainMinY, int terrainMaxY, int minClearance, int maxDepth)
    {
        int top = worldY + MinY;
        int bottom = worldY + MaxY;
        return top >= minClearance && bottom <= maxDepth && top >= terrainMinY && bottom <= terrainMaxY;
    }

    private int ComputeWidth()
    {
        int width = 0;
        foreach (var tile in Tiles)
        {
            width = Math.Max(width, tile.X + 1);
        }

        return width;
    }

    private int ComputeHeight()
    {
        int height = 0;
        foreach (var tile in Tiles)
        {
            height = Math.Max(height, tile.Y + 1);
        }

        return height;
    }
}

public static class StructurePlacer
{
    public static bool TryPlace(World world, StructureTemplate template, int originX, int originY, bool requireEmpty = true, bool allowWater = false)
    {
        if (world is null || template is null)
        {
            return false;
        }

        foreach (var tile in template.Tiles)
        {
            int worldX = originX + tile.X - template.AnchorX;
            int worldY = originY + tile.Y - template.AnchorY;
            if (!world.InBounds(worldX, worldY))
            {
                return false;
            }

            if (requireEmpty && world.Get(worldX, worldY) != 0 && tile.TileId != 0)
            {
                return false;
            }

            if (!allowWater && world.Get(worldX, worldY) == (ushort)TileType.Water)
            {
                return false;
            }

            if (tile.TileId != 0)
            {
                world.Set(worldX, worldY, tile.TileId);
            }

            if (tile.WallId != 0)
            {
                world.SetWall(worldX, worldY, tile.WallId);
            }
        }

        return true;
    }
}
