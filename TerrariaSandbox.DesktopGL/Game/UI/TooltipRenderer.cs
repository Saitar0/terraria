using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaSandbox.DesktopGL.Game.UI;

/// <summary>
/// Renders tooltips from a cached string builder and lookup map.
/// </summary>
public sealed class TooltipRenderer
{
    private readonly Dictionary<string, string> _cache = new();
    private readonly StringBuilder _builder = new();

    public string GetTooltip(string key, Func<string> factory)
    {
        if (_cache.TryGetValue(key, out string? cached))
        {
            return cached;
        }

        string value = factory();
        _cache[key] = value;
        return value;
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        _builder.Clear();
        _builder.Append(text);
        spriteBatch.DrawString(font, _builder, position, Color.White);
    }
}
