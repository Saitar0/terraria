using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Small camera used for world-space to screen-space conversion and viewport culling.
/// </summary>
public sealed class Camera
{
    public Vector2 ScreenPosition;
    public float Zoom = 1f;
    public int ViewportWidth = 1280;
    public int ViewportHeight = 720;
    public int WorldPixelWidth = WorldConstants.WorldWidthTiles * WorldConstants.TileSizePixels;
    public int WorldPixelHeight = WorldConstants.WorldHeightTiles * WorldConstants.TileSizePixels;

    public Vector2 RenderPosition => new(
        MathF.Round(ScreenPosition.X),
        MathF.Round(ScreenPosition.Y));

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return (worldPosition - ScreenPosition) * Zoom;
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return screenPosition / Zoom + ScreenPosition;
    }

    public Point ScreenToTile(Vector2 screenPosition, int tileSizePixels)
    {
        Vector2 worldPosition = ScreenToWorld(screenPosition);
        return new Point(
            (int)MathF.Floor(worldPosition.X / tileSizePixels),
            (int)MathF.Floor(worldPosition.Y / tileSizePixels));
    }

    public Rectangle VisibleTileRect(int viewportWidth, int viewportHeight, int tileSizePixels, int worldWidthTiles, int worldHeightTiles)
    {
        float viewWorldWidth = viewportWidth / Math.Max(Zoom, 0.0001f);
        float viewWorldHeight = viewportHeight / Math.Max(Zoom, 0.0001f);

        int minX = (int)MathF.Floor(ScreenPosition.X / tileSizePixels) - 1;
        int minY = (int)MathF.Floor(ScreenPosition.Y / tileSizePixels) - 1;
        int maxX = (int)MathF.Floor((ScreenPosition.X + viewWorldWidth) / tileSizePixels) + 1;
        int maxY = (int)MathF.Floor((ScreenPosition.Y + viewWorldHeight) / tileSizePixels) + 1;

        minX = Math.Clamp(minX, 0, worldWidthTiles - 1);
        minY = Math.Clamp(minY, 0, worldHeightTiles - 1);
        maxX = Math.Clamp(maxX, 0, worldWidthTiles - 1);
        maxY = Math.Clamp(maxY, 0, worldHeightTiles - 1);

        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
