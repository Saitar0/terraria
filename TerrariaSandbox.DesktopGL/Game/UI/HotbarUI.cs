using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL.Game.UI;

/// <summary>
/// Displays the player's hotbar with fixed slots and selection by scroll / keys.
/// </summary>
public sealed class HotbarUI
{
    public const int SlotCount = 10;

    private Rectangle[] _slots = new Rectangle[SlotCount];

    public float Scale { get; set; } = 2f;
    public int SelectedSlot { get; set; }

    public void RefreshLayout(int screenWidth, int screenHeight)
    {
        int slotSize = (int)MathF.Round(32f * Scale);
        int startX = (screenWidth - (slotSize * SlotCount)) / 2;
        int y = screenHeight - slotSize - 12;

        for (int i = 0; i < SlotCount; ++i)
        {
            _slots[i] = new Rectangle(startX + (i * slotSize), y, slotSize, slotSize);
        }
    }

    public bool TrySelectByKey(int key)
    {
        int index = key - 1;
        if (key == 0)
        {
            index = 9;
        }

        if (index < 0 || index >= SlotCount)
        {
            return false;
        }

        SelectedSlot = index;
        return true;
    }

    public void Draw(SpriteBatch spriteBatch, Inventory inventory, Texture2D? background, SpriteFont? font)
    {
        if (background is null)
        {
            return;
        }

        for (int i = 0; i < SlotCount; ++i)
        {
            Rectangle slotRect = _slots[i];
            spriteBatch.Draw(background, slotRect, i == SelectedSlot ? new Color(120, 180, 255, 180) : new Color(50, 50, 50, 200));

            ItemStack slot = inventory.Slots[i];
            if (slot.Type == 0 || slot.Stack <= 0)
            {
                continue;
            }

            var itemDef = ItemDefs.Get(slot.Type);
            if (font is not null && itemDef.MaxStack > 1)
            {
                spriteBatch.DrawString(font, slot.Stack.ToString(), new Vector2(slotRect.Right - 18, slotRect.Bottom - 16), Color.White);
            }
        }
    }
}
