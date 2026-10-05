using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Cursor state expressed in tile-space.
/// </summary>
public struct Cursor
{
    public int TileX;
    public int TileY;
    public int ReachTiles;
    public bool Visible;
    public bool UseLineOfSight;
    public float ScreenX;
    public float ScreenY;

    public static Cursor FromScreen(Camera camera, Vector2 screenPosition, int reachTiles = 8, bool useLineOfSight = false)
    {
        Point tile = camera.ScreenToTile(screenPosition, WorldConstants.TileSizePixels);
        return new Cursor
        {
            TileX = tile.X,
            TileY = tile.Y,
            ReachTiles = reachTiles,
            Visible = true,
            UseLineOfSight = useLineOfSight,
            ScreenX = screenPosition.X,
            ScreenY = screenPosition.Y
        };
    }

    public bool CanReach(Player player)
    {
        if (!Visible)
        {
            return false;
        }

        if (ReachTiles <= 0)
        {
            return true;
        }

        Vector2 playerTile = new(
            player.Position.X / WorldConstants.TileSizePixels,
            player.Position.Y / WorldConstants.TileSizePixels);

        float dx = TileX - playerTile.X;
        float dy = TileY - playerTile.Y;
        return MathF.Sqrt(dx * dx + dy * dy) <= ReachTiles;
    }
}
