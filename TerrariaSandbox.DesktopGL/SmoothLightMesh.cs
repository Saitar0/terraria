using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Optional smooth-light mesh hook. The default path uses the low-resolution texture pass,
/// while this type preserves the extension point for a future vertex-color light mesh.
/// </summary>
public sealed class SmoothLightMesh
{
    public void Draw(SpriteBatch spriteBatch, LightingEngine lightingEngine, Camera camera, int cellSize, Texture2D lightTexture, Color[] reusableBuffer)
    {
        spriteBatch.Begin(blendState: Game1.LightBlend, samplerState: SamplerState.LinearClamp, sortMode: SpriteSortMode.Deferred);
        spriteBatch.Draw(lightTexture, new Rectangle(0, 0, camera.ViewportWidth, camera.ViewportHeight), Color.White);
        spriteBatch.End();
    }
}
