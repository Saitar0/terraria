using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Placeholder player renderer used for debugging the movement and animation.
/// </summary>
public static class PlayerRenderer
{
    public static void Draw(SpriteBatch spriteBatch, Texture2D pixel, in Player player, Vector2 cameraOffset)
    {
        float x = player.Position.X - cameraOffset.X;
        float y = player.Position.Y - cameraOffset.Y;
        float flip = player.Direction >= 0 ? 1f : -1f;
        float legSwing = player.OnGround ? (float)Math.Sin((player.Tick % 20) / 20f * Math.PI * 2f) * 3f : 0f;

        DrawRect(spriteBatch, pixel, new Rectangle((int)Math.Round(x + 5f), (int)Math.Round(y + 2f), 10, 10), new Color(240, 210, 180));
        DrawRect(spriteBatch, pixel, new Rectangle((int)Math.Round(x + 7f), (int)Math.Round(y + 12f), 6, 14), new Color(120, 180, 255));

        float leftLegX = x + 6f + (player.Direction < 0 ? -legSwing : legSwing);
        float rightLegX = x + 14f - (player.Direction < 0 ? -legSwing : legSwing);
        DrawRect(spriteBatch, pixel, new Rectangle((int)Math.Round(leftLegX), (int)Math.Round(y + 26f), 4, 16), new Color(50, 50, 60));
        DrawRect(spriteBatch, pixel, new Rectangle((int)Math.Round(rightLegX), (int)Math.Round(y + 26f), 4, 16), new Color(60, 60, 80));

        if (player.Direction < 0)
        {
            float mirroredX = x + player.Width - 5f;
            DrawRect(spriteBatch, pixel, new Rectangle((int)Math.Round(mirroredX), (int)Math.Round(y + 2f), 10, 10), new Color(240, 210, 180));
            DrawRect(spriteBatch, pixel, new Rectangle((int)Math.Round(mirroredX - 1f), (int)Math.Round(y + 12f), 6, 14), new Color(120, 180, 255));
        }

        if (Math.Abs(legSwing) > 0.5f)
        {
            DrawRect(spriteBatch, pixel, new Rectangle((int)Math.Round(x + 6f), (int)Math.Round(y + 32f), 8, 6), new Color(90, 90, 100));
        }
    }

    private static void DrawRect(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(pixel, rectangle, color);
    }
}
