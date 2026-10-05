namespace TerrariaSandbox.Core;

/// <summary>
/// Stores the visible viewport position in tile-space.
/// </summary>
public struct Camera2D
{
    public Camera2D(int worldX, int worldY)
    {
        WorldX = worldX;
        WorldY = worldY;
    }

    public int WorldX { get; set; }

    public int WorldY { get; set; }

    public void Move(int dx, int dy)
    {
        WorldX += dx;
        WorldY += dy;
    }
}
