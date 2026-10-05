using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Renders a low-resolution light map as a single multiply pass over the world scene.
/// </summary>
public sealed class LightRenderer
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly int _cellSize;
    private Color[] _reusableBuffer;
    private Texture2D _lightTexture;
    private SmoothLightMesh? _smoothLightMesh;

    public LightRenderer(GraphicsDevice graphicsDevice, int cellSize = 16)
    {
        _graphicsDevice = graphicsDevice;
        _cellSize = Math.Max(1, cellSize);
        _reusableBuffer = Array.Empty<Color>();
        _lightTexture = new Texture2D(graphicsDevice, 1, 1);
        _lightTexture.SetData(new[] { Color.White });
    }

    public bool UseSmoothMesh { get; set; }
    public double LastFrameMilliseconds { get; private set; }

    public void Apply(SpriteBatch spriteBatch, LightingEngine lightingEngine, Camera camera)
    {
        if (lightingEngine is null || camera is null || lightingEngine.World is null)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        if (UseSmoothMesh)
        {
            _smoothLightMesh ??= new SmoothLightMesh();
            _smoothLightMesh.Draw(spriteBatch, lightingEngine, camera, _cellSize, _lightTexture, _reusableBuffer);
            LastFrameMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            return;
        }

        World world = lightingEngine.World;
        int viewportWidth = Math.Max(1, camera.ViewportWidth);
        int viewportHeight = Math.Max(1, camera.ViewportHeight);
        int renderWidth = Math.Max(1, (int)MathF.Ceiling(viewportWidth / (float)_cellSize));
        int renderHeight = Math.Max(1, (int)MathF.Ceiling(viewportHeight / (float)_cellSize));

        EnsureLightTexture(renderWidth, renderHeight);

        int left = Math.Clamp((int)MathF.Floor(camera.ScreenPosition.X / WorldConstants.TileSizePixels) - 1, 0, Math.Max(0, world.Width - 1));
        int top = Math.Clamp((int)MathF.Floor(camera.ScreenPosition.Y / WorldConstants.TileSizePixels) - 1, 0, Math.Max(0, world.Height - 1));
        int right = Math.Clamp((int)MathF.Ceiling((camera.ScreenPosition.X + viewportWidth) / WorldConstants.TileSizePixels) + 1, 0, Math.Max(0, world.Width - 1));
        int bottom = Math.Clamp((int)MathF.Ceiling((camera.ScreenPosition.Y + viewportHeight) / WorldConstants.TileSizePixels) + 1, 0, Math.Max(0, world.Height - 1));
        RectI visible = new RectI(left, top, Math.Max(1, right - left + 1), Math.Max(1, bottom - top + 1));

        lightingEngine.Update(world, visible, lightingEngine.Sun);

        for (int y = 0; y < renderHeight; ++y)
        {
            for (int x = 0; x < renderWidth; ++x)
            {
                int sampleX = left + (int)MathF.Floor(x * ((float)(visible.Width) / renderWidth));
                int sampleY = top + (int)MathF.Floor(y * ((float)(visible.Height) / renderHeight));
                sampleX = Math.Clamp(sampleX, 0, Math.Max(0, world.Width - 1));
                sampleY = Math.Clamp(sampleY, 0, Math.Max(0, world.Height - 1));
                _reusableBuffer[y * renderWidth + x] = lightingEngine.GetSample(sampleX, sampleY);
            }
        }

        _lightTexture.SetData(_reusableBuffer);

        spriteBatch.Begin(blendState: Game1.LightBlend, samplerState: SamplerState.LinearClamp, sortMode: SpriteSortMode.Deferred);
        spriteBatch.Draw(_lightTexture, new Rectangle(0, 0, viewportWidth, viewportHeight), Color.White);
        spriteBatch.End();

        LastFrameMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
    }

    private void EnsureLightTexture(int width, int height)
    {
        if (_lightTexture.Width != width || _lightTexture.Height != height)
        {
            _lightTexture.Dispose();
            _lightTexture = new Texture2D(_graphicsDevice, width, height);

            var whitePixels = new Color[width * height];
            Array.Fill(whitePixels, Color.White);
            _lightTexture.SetData(whitePixels);
        }

        if (_reusableBuffer.Length != width * height)
        {
            Array.Resize(ref _reusableBuffer, width * height);
        }
    }
}
