using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL.Game.UI;

/// <summary>
/// Inventory grid renderer with drag-and-drop and slot hit testing by integer division.
/// </summary>
public sealed class InventoryUI
{
    private readonly Rectangle[] _slotBounds = new Rectangle[58];
    private readonly Rectangle[] _hotbarBounds = new Rectangle[10];
    private int _dragSource;
    private bool _dragging;

    public Inventory Inventory { get; }
    public int Scale { get; set; } = 2;
    public bool Visible { get; set; } = true;
    public int SelectedSlot { get; set; }

    public InventoryUI(Inventory? inventory = null)
    {
        Inventory = inventory ?? new Inventory();
    }

    public void RefreshLayout(int screenWidth, int screenHeight)
    {
        int slotSize = 32 * Scale;
        int startX = (screenWidth - (slotSize * 10)) / 2;
        int hotbarY = screenHeight - slotSize - 12;

        for (int i = 0; i < 10; ++i)
        {
            _hotbarBounds[i] = new Rectangle(startX + (i * slotSize), hotbarY, slotSize, slotSize);
        }

        int gridX = (screenWidth - (slotSize * 10)) / 2;
        int gridY = hotbarY - (slotSize * 5) - 8;
        for (int i = 0; i < 58; ++i)
        {
            int x = gridX + ((i % 10) * slotSize);
            int y = gridY + ((i / 10) * slotSize);
            _slotBounds[i] = new Rectangle(x, y, slotSize, slotSize);
        }
    }

    public bool TryHitSlot(int mouseX, int mouseY, out int slotIndex)
    {
        slotIndex = -1;
        if (!Visible)
        {
            return false;
        }

        int slotSize = 32 * Scale;
        int columns = 10;
        int startX = (mouseX / slotSize) * slotSize;
        int startY = (mouseY / slotSize) * slotSize;
        int indexX = mouseX / slotSize;
        int indexY = mouseY / slotSize;
        if (indexX < 0 || indexY < 0)
        {
            return false;
        }

        int column = indexX % columns;
        int row = indexY % 6;
        slotIndex = (row * columns) + column;
        return slotIndex >= 0 && slotIndex < _slotBounds.Length && _slotBounds[slotIndex].Contains(mouseX, mouseY);
    }

    public void HandleShiftClick(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Inventory.Slots.Length)
        {
            return;
        }

        ItemStack slot = Inventory.Slots[slotIndex];
        if (slot.IsEmpty)
        {
            return;
        }

        for (int i = 0; i < Inventory.Slots.Length; ++i)
        {
            if (i == slotIndex)
            {
                continue;
            }

            if (Inventory.Slots[i].Type == slot.Type && Inventory.Slots[i].Stack < ItemDefs.Get(slot.Type).MaxStack)
            {
                int move = Math.Min(slot.Stack, ItemDefs.Get(slot.Type).MaxStack - Inventory.Slots[i].Stack);
                Inventory.Slots[i].Stack += (short)move;
                slot.Stack -= (short)move;
                if (slot.Stack <= 0)
                {
                    Inventory.Slots[slotIndex] = ItemStack.Empty;
                    return;
                }

                Inventory.Slots[slotIndex] = slot;
                return;
            }
        }
    }

    public bool TryBeginDrag(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Inventory.Slots.Length || Inventory.Slots[slotIndex].IsEmpty)
        {
            return false;
        }

        _dragSource = slotIndex;
        _dragging = true;
        return true;
    }

    public void DropDragTo(int targetIndex)
    {
        if (!_dragging || targetIndex < 0 || targetIndex >= Inventory.Slots.Length)
        {
            return;
        }

        ItemStack source = Inventory.Slots[_dragSource];
        ItemStack target = Inventory.Slots[targetIndex];
        if (source.IsEmpty)
        {
            _dragging = false;
            return;
        }

        if (target.IsEmpty)
        {
            Inventory.Slots[targetIndex] = source;
            Inventory.Slots[_dragSource] = ItemStack.Empty;
        }
        else if (target.Type == source.Type)
        {
            int cap = ItemDefs.Get(source.Type).MaxStack;
            int move = Math.Min(source.Stack, cap - target.Stack);
            target.Stack += (short)move;
            source.Stack -= (short)move;
            Inventory.Slots[targetIndex] = target;
            Inventory.Slots[_dragSource] = source;
        }
        else
        {
            Inventory.Slots[_dragSource] = target;
            Inventory.Slots[targetIndex] = source;
        }

        _dragging = false;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D? slotTexture, SpriteFont? font)
    {
        if (!Visible || slotTexture is null)
        {
            return;
        }

        for (int i = 0; i < Inventory.Slots.Length; ++i)
        {
            Rectangle bounds = _slotBounds[i];
            spriteBatch.Draw(slotTexture, bounds, new Color(30, 30, 30, 200));

            ItemStack slot = Inventory.Slots[i];
            if (slot.IsEmpty)
            {
                continue;
            }

            if (font is not null && slot.Stack > 1)
            {
                spriteBatch.DrawString(font, slot.Stack.ToString(), new Vector2(bounds.Right - 18, bounds.Bottom - 18), Color.White);
            }
        }
    }
}
