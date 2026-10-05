using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Builds a placeholder atlas with per-tile source rectangles and border padding to avoid bleeding.
/// </summary>
public sealed class AtlasBuilder
{
    private const int FrameCount = 4;
    private readonly int _tileSize;
    private readonly int _padding;

    public AtlasBuilder(int tileSize = WorldConstants.TileSizePixels, int padding = 2)
    {
        _tileSize = tileSize;
        _padding = padding;
    }

    public (Texture2D atlas, Rectangle[] lookup) Build(GraphicsDevice graphicsDevice)
    {
        int typeCount = Enum.GetValues<TileType>().Length;
        int cellSize = _tileSize + (_padding * 2);
        int tilesPerRow = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(typeCount * FrameCount)));
        int atlasWidth = tilesPerRow * cellSize;
        int atlasHeight = (int)Math.Ceiling((double)(typeCount * FrameCount) / tilesPerRow) * cellSize;
        Texture2D atlas = new Texture2D(graphicsDevice, atlasWidth, atlasHeight);
        Rectangle[] lookup = new Rectangle[typeCount * FrameCount];
        Color[] pixels = new Color[atlasWidth * atlasHeight];

        for (int typeIndex = 0; typeIndex < typeCount; ++typeIndex)
        {
            TileType tileType = (TileType)typeIndex;
            Color baseColor = GetTileColor(tileType);
            for (int frameIndex = 0; frameIndex < FrameCount; ++frameIndex)
            {
                int cellIndex = typeIndex * FrameCount + frameIndex;
                int cellX = (cellIndex % tilesPerRow) * cellSize;
                int cellY = (cellIndex / tilesPerRow) * cellSize;
                lookup[cellIndex] = new Rectangle(cellX + _padding, cellY + _padding, _tileSize, _tileSize);

                for (int y = 0; y < cellSize; ++y)
                {
                    for (int x = 0; x < cellSize; ++x)
                    {
                        int innerX = x - _padding;
                        int innerY = y - _padding;
                        Color color;

                        if ((uint)innerX < (uint)_tileSize && (uint)innerY < (uint)_tileSize)
                        {
                            color = ComputeInnerColor(baseColor, innerX, innerY, frameIndex);
                        }
                        else
                        {
                            color = Darken(baseColor, 0.22f);
                        }

                        pixels[(cellY + y) * atlasWidth + (cellX + x)] = color;
                    }
                }
            }
        }

        atlas.SetData(pixels);
        return (atlas, lookup);
    }

    private static Color ComputeInnerColor(Color baseColor, int x, int y, int frameIndex)
    {
        Color shade = baseColor;
        if (frameIndex == 1 && ((x + y) % 2 == 0))
        {
            shade = Lighten(baseColor, 0.10f);
        }
        else if (frameIndex == 2 && (x > 7 || y > 7))
        {
            shade = Lighten(baseColor, 0.12f);
        }
        else if (frameIndex == 3 && ((x + y) % 3 == 0))
        {
            shade = Darken(baseColor, 0.08f);
        }

        return shade;
    }

    private static Color GetTileColor(TileType tileType)
    {
        return tileType switch
        {
            TileType.Air => Color.Transparent,
            TileType.Dirt => new Color(108, 75, 44),
            TileType.Grass => new Color(80, 144, 72),
            TileType.Stone => new Color(124, 128, 137),
            TileType.Sand => new Color(201, 177, 116),
            TileType.Water => new Color(72, 126, 201),
            TileType.Wood => new Color(132, 92, 48),
            TileType.Leaves => new Color(66, 118, 64),
            TileType.Snow => new Color(214, 228, 235),
            _ => new Color(200, 200, 200)
        };
    }

    private static Color Lighten(Color color, float amount)
    {
        return new Color(
            Clamp(color.R + (int)(255f * amount)),
            Clamp(color.G + (int)(255f * amount)),
            Clamp(color.B + (int)(255f * amount)),
            color.A);
    }

    private static Color Darken(Color color, float amount)
    {
        return new Color(
            Clamp(color.R - (int)(255f * amount)),
            Clamp(color.G - (int)(255f * amount)),
            Clamp(color.B - (int)(255f * amount)),
            color.A);
    }

    private static byte Clamp(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        if (value > 255)
        {
            return 255;
        }

        return (byte)value;
    }
}
