using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Renders the liquid layer with per-type color, transparency and a subtle wave.
/// </summary>
public sealed class LiquidRenderer
{
    private readonly Texture2D _pixel;

    public LiquidRenderer(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, World world, Camera camera, int viewportWidth, int viewportHeight)
    {
        Rectangle visible = camera.VisibleTileRect(viewportWidth, viewportHeight, WorldConstants.TileSizePixels, world.Width, world.Height);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Deferred);

        for (int y = visible.Top; y <= visible.Bottom; ++y)
        {
            if ((uint)y >= (uint)world.Height)
            {
                continue;
            }

            for (int x = visible.Left; x <= visible.Right; ++x)
            {
                if ((uint)x >= (uint)world.Width)
                {
                    continue;
                }

                int index = world.ToIndex(x, y);
                if (world.LiquidType[index] == 0 || world.Liquid[index] == 0)
                {
                    continue;
                }

                int liquidAmount = world.Liquid[index];
                LiquidType type = (LiquidType)world.LiquidType[index];
                float height = (liquidAmount / 255f) * WorldConstants.TileSizePixels;
                float offset = (float)Math.Sin((x * 0.55f) + (y * 0.35f) + camera.ScreenPosition.X * 0.02f) * 1.2f;
                float targetX = x * WorldConstants.TileSizePixels + offset;
                float targetY = y * WorldConstants.TileSizePixels + (WorldConstants.TileSizePixels - height);

                Color color = type switch
                {
                    LiquidType.Water => new Color(74, 156, 255, 140),
                    LiquidType.Lava => new Color(255, 96, 32, 175),
                    LiquidType.Honey => new Color(244, 190, 82, 165),
                    _ => Color.Transparent
                };

                Vector2 screenPosition = camera.WorldToScreen(new Vector2(targetX, targetY));
                Rectangle destination = new Rectangle(
                    (int)MathF.Round(screenPosition.X),
                    (int)MathF.Round(screenPosition.Y),
                    WorldConstants.TileSizePixels,
                    Math.Max(2, (int)MathF.Round(height)));

                if (destination.Height > 0)
                {
                    spriteBatch.Draw(_pixel, destination, color);
                }
            }
        }

        spriteBatch.End();
    }
}
