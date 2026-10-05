using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Draws the tile world in layer order with a visible-grid cull and atlas-backed frame lookup.
/// </summary>
public sealed class TileRenderer
{
    private readonly int _tileSize;
    private readonly Texture2D _skyTexture;

    public int TilesDrawn { get; private set; }
    public int DrawCalls { get; private set; }

    public TileRenderer(GraphicsDevice graphicsDevice, int tileSize = WorldConstants.TileSizePixels)
    {
        _tileSize = tileSize;
        _skyTexture = new Texture2D(graphicsDevice, 1, 1);
        _skyTexture.SetData(new[] { Color.CornflowerBlue });
    }

    public void Draw(SpriteBatch spriteBatch, World world, Camera camera, Texture2D atlas, Rectangle[] atlasTable, int viewportWidth, int viewportHeight)
    {
        TilesDrawn = 0;
        DrawCalls = 0;

        Rectangle visible = camera.VisibleTileRect(viewportWidth, viewportHeight, _tileSize, world.Width, world.Height);

        DrawTileLayer(spriteBatch, world, camera, atlas, atlasTable, visible, false);
        DrawTileLayer(spriteBatch, world, camera, atlas, atlasTable, visible, true);
    }

    private void DrawTileLayer(SpriteBatch spriteBatch, World world, Camera camera, Texture2D atlas, Rectangle[] atlasTable, Rectangle visible, bool solidLayer)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred);

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
                ushort tileType = world.Types[index];
                if (tileType == 0)
                {
                    continue;
                }

                TileDef tileDef = TileDefs.GetTile(tileType);
                if (solidLayer != tileDef.Solid)
                {
                    continue;
                }

                short frameX = world.FrameX[index];
                short frameY = world.FrameY[index];
                int atlasIndex = (tileType * 4) + (frameY * 2 + frameX);
                Rectangle source = atlasTable[Math.Clamp(atlasIndex, 0, atlasTable.Length - 1)];
                Vector2 worldPosition = new Vector2(x * _tileSize, y * _tileSize);
                Vector2 screenPosition = camera.WorldToScreen(worldPosition);

                Rectangle destination = new Rectangle(
                    (int)MathF.Round(screenPosition.X),
                    (int)MathF.Round(screenPosition.Y),
                    _tileSize,
                    _tileSize);

                spriteBatch.Draw(atlas, destination, source, Color.White);
                DrawCalls++;
                TilesDrawn++;
            }
        }

        spriteBatch.End();
    }
}
