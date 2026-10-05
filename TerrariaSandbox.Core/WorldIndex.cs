namespace TerrariaSandbox.Core;

/// <summary>
/// Converts tile coordinates to row-major world indexes.
/// </summary>
public static class WorldIndex
{
    /// <summary>
    /// Calculates the index for a tile in the flat world array.
    /// </summary>
    public static int FromTile(int x, int y)
    {
        return y * WorldConstants.WorldWidthTiles + x;
    }

    /// <summary>
    /// Converts a world index back to the X component.
    /// </summary>
    public static int ToTileX(int index)
    {
        return index % WorldConstants.WorldWidthTiles;
    }

    /// <summary>
    /// Converts a world index back to the Y component.
    /// </summary>
    public static int ToTileY(int index)
    {
        return index / WorldConstants.WorldWidthTiles;
    }
}
