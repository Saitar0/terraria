namespace TerrariaSandbox.Core;

/// <summary>
/// Represents a logical chunk of tiles used by the world generator and renderer.
/// </summary>
public sealed class WorldChunk
{
    public WorldChunk(int chunkX, int chunkY)
    {
        ChunkX = chunkX;
        ChunkY = chunkY;
        Tiles = new TileType[TileCount];
    }

    public int ChunkX { get; }

    public int ChunkY { get; }

    public int Width => WorldConstants.ChunkSideTiles;

    public int Height => WorldConstants.ChunkSideTiles;

    public int TileCount => Width * Height;

    public TileType[] Tiles { get; }

    public int GetLocalIndex(int localX, int localY)
    {
        return localY * Width + localX;
    }

    public TileType GetTile(int localX, int localY)
    {
        return Tiles[GetLocalIndex(localX, localY)];
    }

    public void SetTile(int localX, int localY, TileType tile)
    {
        Tiles[GetLocalIndex(localX, localY)] = tile;
    }
}
