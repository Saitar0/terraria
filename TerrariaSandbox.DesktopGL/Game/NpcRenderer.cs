using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL.Game;

/// <summary>
/// Placeholder renderer used to visualize active NPCs and their health bars.
/// </summary>
public sealed class NpcRenderer
{
    private readonly Texture2D _pixel;
    private readonly Color _bodyColor;

    public NpcRenderer(GraphicsDevice graphicsDevice, Color? bodyColor = null)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _bodyColor = bodyColor ?? new Color(190, 90, 90);
    }

    public void Draw(SpriteBatch spriteBatch, Pool<Npc> pool, Vector2 cameraPosition)
    {
        for (int i = 0; i < pool.Capacity; ++i)
        {
            if (!pool.IsActive(i))
            {
                continue;
            }

            ref Npc npc = ref pool[i];
            Vector2 screenPos = npc.Position - cameraPosition;
            Rectangle body = new(
                (int)MathF.Round(screenPos.X),
                (int)MathF.Round(screenPos.Y),
                (int)MathF.Ceiling(npc.Width),
                (int)MathF.Ceiling(npc.Height));

            spriteBatch.Draw(_pixel, body, _bodyColor);

            float lifeRatio = Math.Clamp((float)npc.Life / Math.Max(1, npc.LifeMax), 0f, 1f);
            Rectangle bar = new(
                body.X,
                body.Y - 8,
                body.Width,
                5);
            spriteBatch.Draw(_pixel, bar, new Color(25, 25, 25, 220));

            Rectangle fill = new(bar.X, bar.Y, (int)MathF.Round(bar.Width * lifeRatio), bar.Height);
            spriteBatch.Draw(_pixel, fill, new Color(115, 220, 110));
        }
    }
}
