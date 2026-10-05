using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL.Game.UI;

/// <summary>
/// Minimal boss health bar rendered in the HUD.
/// </summary>
public sealed class BossBar
{
    private const int DefaultWidth = 420;
    private const int DefaultHeight = 18;

    public bool Visible { get; set; }
    public string Name { get; set; } = "Boss";
    public int Current { get; set; }
    public int Max { get; set; } = 100;

    public void UpdateFromNpc(in Npc npc)
    {
        Name = npc.Type > 0 ? "Boss" : "Unknown";
        Current = Math.Max(0, npc.Life);
        Max = Math.Max(1, npc.LifeMax);
        Visible = npc.Type > 0 && npc.Life > 0;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont? font, int screenWidth, int screenHeight)
    {
        if (!Visible || pixel is null)
        {
            return;
        }

        int x = (screenWidth - DefaultWidth) / 2;
        int y = 20;
        Rectangle background = new(x, y, DefaultWidth, DefaultHeight);
        Rectangle fill = new(x + 2, y + 2, DefaultWidth - 4, DefaultHeight - 4);
        float amount = Max > 0 ? Current / (float)Max : 0f;
        Rectangle health = new(fill.X, fill.Y, (int)((fill.Width) * amount), fill.Height);

        spriteBatch.Draw(pixel, background, new Color(25, 25, 25, 200));
        spriteBatch.Draw(pixel, health, new Color(190, 45, 40, 220));

        if (font is not null)
        {
            float nameX = x + 8f;
            float nameY = y - 18f;
            spriteBatch.DrawString(font, Name, new Vector2(nameX, nameY), Color.White);
        }
    }
}
