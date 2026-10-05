namespace TerrariaSandbox.Core;

/// <summary>
/// Stores the world state in a single row-major array and provides deterministic generation.
/// </summary>
public sealed class WorldMap
{
    private readonly TileType[] _tiles;
    private readonly int _seed;

    public WorldMap(int seed)
    {
        _seed = seed;
        _tiles = new TileType[WorldConstants.TotalTileCount];
        GenerateTerrain();
    }

    public int Width => WorldConstants.WorldWidthTiles;

    public int Height => WorldConstants.WorldHeightTiles;

    public int TileCount => WorldConstants.TotalTileCount;

    public TileType GetTile(int x, int y)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
        {
            return TileType.Air;
        }

        return _tiles[WorldIndex.FromTile(x, y)];
    }

    public void SetTile(int x, int y, TileType tile)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
        {
            return;
        }

        _tiles[WorldIndex.FromTile(x, y)] = tile;
    }

    public void GenerateTerrain()
    {
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                int noise = Noise2D(x, y, _seed);
                TileType tile = TileType.Air;

                if (y < 32)
                {
                    tile = TileType.Snow;
                }
                else if (y < 70)
                {
                    tile = TileType.Grass;
                }
                else if (y < 120)
                {
                    tile = TileType.Dirt;
                }
                else if (y < 170)
                {
                    tile = TileType.Stone;
                }
                else if (noise % 7 == 0)
                {
                    tile = TileType.Sand;
                }
                else if (noise % 5 == 0)
                {
                    tile = TileType.Stone;
                }
                else
                {
                    tile = TileType.Air;
                }

                _tiles[WorldIndex.FromTile(x, y)] = tile;
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
