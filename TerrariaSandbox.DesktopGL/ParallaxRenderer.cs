using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Draws layered repeated background textures with gentle parallax motion.
/// </summary>
public sealed class ParallaxRenderer
{
    private const int LayerTextureWidth = 512;
    private const int LayerTextureHeight = 256;

    private readonly Texture2D[] _layers = new Texture2D[4];
    private readonly float[] _motionFactors = [0.10f, 0.22f, 0.38f, 0.60f];
    private readonly int[] _baseOffsets = [0, 0, 18, 42];

    public ParallaxRenderer(GraphicsDevice graphicsDevice)
    {
        for (int i = 0; i < _layers.Length; ++i)
        {
            _layers[i] = BuildLayerTexture(graphicsDevice, i);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera, DayClock dayClock, int viewportWidth, int viewportHeight)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, sortMode: SpriteSortMode.Deferred);

        int relativeSurfaceY = (int)MathF.Floor(camera.ScreenPosition.Y / WorldConstants.TileSizePixels) - (WorldConstants.WorldHeightTiles / 6);
        ParallaxSet set = SelectSet(relativeSurfaceY);

        for (int layerIndex = 0; layerIndex < _layers.Length; ++layerIndex)
        {
            Texture2D layer = _layers[layerIndex];
            float factor = _motionFactors[layerIndex] * GetSetFactor(set, layerIndex);
            float offset = camera.ScreenPosition.X * factor;
            int startX = ((int)MathF.Floor(-offset) % layer.Width) - layer.Width;
            int baseY = (int)(viewportHeight * (0.16f + layerIndex * 0.18f)) + _baseOffsets[layerIndex];
            int tileCount = (viewportWidth / layer.Width) + 2;

            for (int x = startX; x <= viewportWidth + layer.Width; x += layer.Width)
            {
                spriteBatch.Draw(
                    layer,
                    new Rectangle(x, baseY, layer.Width, layer.Height),
                    Color.White);
            }
        }

        spriteBatch.End();
    }

    private static ParallaxSet SelectSet(int relativeSurfaceY)
    {
        if (relativeSurfaceY < -30)
        {
            return ParallaxSet.Deep;
        }

        if (relativeSurfaceY < 0)
        {
            return ParallaxSet.Hills;
        }

        if (relativeSurfaceY < 40)
        {
            return ParallaxSet.Plains;
        }

        return ParallaxSet.Desert;
    }

    private static float GetSetFactor(ParallaxSet set, int layerIndex)
    {
        return set switch
        {
            ParallaxSet.Deep => 0.75f + layerIndex * 0.10f,
            ParallaxSet.Hills => 0.9f + layerIndex * 0.12f,
            ParallaxSet.Plains => 1.0f + layerIndex * 0.08f,
            ParallaxSet.Desert => 1.15f + layerIndex * 0.10f,
            _ => 1.0f
        };
    }

    private Texture2D BuildLayerTexture(GraphicsDevice graphicsDevice, int layerIndex)
    {
        Color[] data = new Color[LayerTextureWidth * LayerTextureHeight];
        Color tint = layerIndex switch
        {
            0 => new Color(72, 110, 160),
            1 => new Color(114, 145, 178),
            2 => new Color(146, 168, 130),
            _ => new Color(180, 182, 190),
        };

        for (int y = 0; y < LayerTextureHeight; ++y)
        {
            for (int x = 0; x < LayerTextureWidth; ++x)
            {
                float nx = x / (float)LayerTextureWidth;
                float ny = y / (float)LayerTextureHeight;
                float mountain = 0.5f + 0.35f * MathF.Sin((x + layerIndex * 27) / 52f)
                    + 0.20f * MathF.Sin((x + layerIndex * 47) / 128f)
                    + 0.15f * MathF.Cos((y - layerIndex * 11) / 31f);

                float cloud = 0f;
                if (layerIndex >= 2)
                {
                    float c = MathF.Sin((x * 0.17f) + layerIndex * 12.8f) + MathF.Cos((y * 0.15f) - layerIndex * 8.3f);
                    cloud = MathF.Max(0f, c * 0.5f + 0.5f);
                }

                int idx = y * LayerTextureWidth + x;
                if (layerIndex == 0)
                {
                    data[idx] = new Color(
                        (byte)Math.Clamp(18 + (int)(mountain * 30), 18, 70),
                        (byte)Math.Clamp(36 + (int)(mountain * 36), 36, 96),
                        (byte)Math.Clamp(52 + (int)(mountain * 42), 52, 128),
                        (byte)255);
                }
                else if (layerIndex == 1)
                {
                    data[idx] = new Color(
                        (byte)Math.Clamp(80 + (int)(mountain * 40), 80, 170),
                        (byte)Math.Clamp(96 + (int)(mountain * 35), 96, 170),
                        (byte)Math.Clamp(110 + (int)(mountain * 36), 110, 200),
                        (byte)255);
                }
                else if (layerIndex == 2)
                {
                    float shade = MathF.Max(0f, 1f - (float)Math.Sqrt((nx - 0.5f) * (nx - 0.5f) + (ny - 0.4f) * (ny - 0.4f)) * 2f);
                    data[idx] = new Color(
                        (byte)Math.Clamp((tint.R * 0.7f) + shade * 60f, 0f, 255f),
                        (byte)Math.Clamp((tint.G * 0.7f) + shade * 60f, 0f, 255f),
                        (byte)Math.Clamp((tint.B * 0.7f) + shade * 60f, 0f, 255f),
                        (byte)255);
                }
                else
                {
                    data[idx] = new Color(
                        (byte)Math.Clamp(220f + cloud * 25f, 0f, 255f),
                        (byte)Math.Clamp(232f + cloud * 18f, 0f, 255f),
                        (byte)Math.Clamp(246f + cloud * 6f, 0f, 255f),
                        (byte)255);
                }
            }
        }

        Texture2D texture = new Texture2D(graphicsDevice, LayerTextureWidth, LayerTextureHeight);
        texture.SetData(data);
        return texture;
    }

    private enum ParallaxSet
    {
        Plains,
        Hills,
        Desert,
        Deep
    }
}
