using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Lightweight visual helper that draws a tile outline under the cursor.
/// </summary>
public sealed class CursorRenderer
{
    public void Draw(SpriteBatch spriteBatch, Camera camera, Cursor cursor, Texture2D pixel, Color color)
    {
        if (!cursor.Visible)
        {
            return;
        }

        Vector2 position = new(
            cursor.TileX * WorldConstants.TileSizePixels,
            cursor.TileY * WorldConstants.TileSizePixels);

        Vector2 screenPosition = camera.WorldToScreen(position);
        Rectangle rect = new(
            (int)MathF.Round(screenPosition.X),
            (int)MathF.Round(screenPosition.Y),
            WorldConstants.TileSizePixels,
            WorldConstants.TileSizePixels);

        DrawOutline(spriteBatch, rect, pixel, color, 2);
    }

    public static void DrawOutline(SpriteBatch spriteBatch, Rectangle rect, Texture2D pixel, Color color, int thickness)
    {
        if (thickness <= 0 || pixel is null)
        {
            return;
        }

        Rectangle top = new(rect.X, rect.Y, rect.Width, thickness);
        Rectangle bottom = new(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness);
        Rectangle left = new(rect.X, rect.Y, thickness, rect.Height);
        Rectangle right = new(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height);

        spriteBatch.Draw(pixel, top, color);
        spriteBatch.Draw(pixel, bottom, color);
        spriteBatch.Draw(pixel, left, color);
        spriteBatch.Draw(pixel, right, color);
    }
}
