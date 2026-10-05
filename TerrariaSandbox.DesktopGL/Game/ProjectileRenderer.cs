using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL.Game;

/// <summary>
/// Minimal visualization for active projectiles as colored quads.
/// </summary>
public sealed class ProjectileRenderer
{
    private readonly Texture2D _pixel;

    public ProjectileRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, ProjectileSystem system, Vector2 cameraPosition)
    {
        for (int i = 0; i < system.Pool.Capacity; ++i)
        {
            if (!system.Pool.IsActive(i))
            {
                continue;
            }

            ref Projectile projectile = ref system.Pool[i];
            if (!projectile.Active)
            {
                continue;
            }

            Rectangle rect = new(
                (int)MathF.Round(projectile.Position.X - cameraPosition.X),
                (int)MathF.Round(projectile.Position.Y - cameraPosition.Y),
                Math.Max(4, projectile.Width),
                Math.Max(4, projectile.Height));

            Color color = projectile.Hostile ? new Color(210, 80, 80) : new Color(110, 180, 255);
            spriteBatch.Draw(_pixel, rect, color);
        }
    }
}
