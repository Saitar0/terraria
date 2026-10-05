namespace TerrariaSandbox.Core;

/// <summary>
/// Defines the fixed world simulation constants used across the project.
/// </summary>
public static class WorldConstants
{
    public const int TileSizePixels = 16;
    public const int TicksPerSecond = 60;
    public const int WorldWidthTiles = 8400;
    public const int WorldHeightTiles = 2400;
    public const int ChunkSideTiles = 64;
    public const int ChunkTileCount = ChunkSideTiles * ChunkSideTiles;
    public const int TotalTileCount = WorldWidthTiles * WorldHeightTiles;
    public const int ChunkCountX = (WorldWidthTiles + ChunkSideTiles - 1) / ChunkSideTiles;
    public const int ChunkCountY = (WorldHeightTiles + ChunkSideTiles - 1) / ChunkSideTiles;
}
