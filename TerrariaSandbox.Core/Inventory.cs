namespace TerrariaSandbox.Core;

/// <summary>
/// 58-slot inventory layout: hotbar, backpack, coins and ammo.
/// </summary>
public sealed class Inventory
{
    public const int SlotCount = 58;
    public const int HotbarStart = 0;
    public const int HotbarEnd = 9;
    public const int BackpackStart = 10;
    public const int BackpackEnd = 49;
    public const int CoinStart = 50;
    public const int CoinEnd = 53;
    public const int AmmoStart = 54;
    public const int AmmoEnd = 57;

    public event Action? Changed;

    public Inventory()
    {
        Slots = new ItemStack[SlotCount];
    }

    public ItemStack[] Slots { get; }

    public int SelectedHotbarIndex { get; set; }

    public ItemStack this[int index]
    {
        get => Slots[index];
        set => Slots[index] = value;
    }

    public ItemStack GetHeldItem()
    {
        if (SelectedHotbarIndex < HotbarStart || SelectedHotbarIndex > HotbarEnd)
        {
            return ItemStack.Empty;
        }

        return Slots[SelectedHotbarIndex];
    }

    public bool TryAdd(ref ItemStack item)
    {
        if (item.IsEmpty)
        {
            return false;
        }

        ushort type = item.Type;
        int stackLimit = ItemDefs.Get(type).MaxStack;
        short remaining = item.Stack;

        if (type >= 50 && type <= 53)
        {
            int index = type - 50 + CoinStart;
            ItemStack slot = Slots[index];
            int space = stackLimit - slot.Stack;
            int move = Math.Min(remaining, space);
            if (move > 0)
            {
                slot.Type = type;
                slot.Stack += (short)move;
                Slots[index] = slot;
                remaining -= (short)move;
            }

            if (remaining > 0)
            {
                item.Stack = remaining;
                ConvertCoins();
                Changed?.Invoke();
                return true;
            }

            item = ItemStack.Empty;
            ConvertCoins();
            Changed?.Invoke();
            return true;
        }

        for (int i = 0; i < Slots.Length; ++i)
        {
            if (Slots[i].Type == type && !Slots[i].IsEmpty)
            {
                int space = stackLimit - Slots[i].Stack;
                if (space <= 0)
                {
                    continue;
                }

                int move = Math.Min(remaining, space);
                Slots[i].Stack += (short)move;
                remaining -= (short)move;
                if (remaining <= 0)
                {
                    item = ItemStack.Empty;
                    Changed?.Invoke();
                    return true;
                }
            }
        }

        for (int i = 0; i < Slots.Length; ++i)
        {
            if (Slots[i].IsEmpty)
            {
                int move = Math.Min(remaining, stackLimit);
                Slots[i] = new ItemStack(type, (short)move, item.Prefix);
                remaining -= (short)move;
                if (remaining <= 0)
                {
                    item = ItemStack.Empty;
                    Changed?.Invoke();
                    return true;
                }
            }
        }

        item.Stack = remaining;
        Changed?.Invoke();
        return true;
    }

    public bool TryRemove(int slotIndex, int count, out ItemStack removed)
    {
        removed = ItemStack.Empty;
        if (slotIndex < 0 || slotIndex >= Slots.Length)
        {
            return false;
        }

        ItemStack slot = Slots[slotIndex];
        if (slot.IsEmpty || count <= 0)
        {
            return false;
        }

        int transfer = Math.Min(count, slot.Stack);
        slot.Stack -= (short)transfer;
        removed = new ItemStack(slot.Type, transfer, slot.Prefix);

        if (slot.Stack <= 0)
        {
            Slots[slotIndex] = ItemStack.Empty;
        }
        else
        {
            Slots[slotIndex] = slot;
        }

        Changed?.Invoke();
        return true;
    }

    public bool TryConsume(ushort type, int count)
    {
        if (count <= 0)
        {
            return true;
        }

        int remaining = count;
        for (int i = 0; i < Slots.Length && remaining > 0; ++i)
        {
            ItemStack slot = Slots[i];
            if (slot.Type != type || slot.IsEmpty)
            {
                continue;
            }

            int take = Math.Min(remaining, slot.Stack);
            slot.Stack -= (short)take;
            remaining -= take;
            Slots[i] = slot.Stack > 0 ? slot : ItemStack.Empty;
        }

        if (remaining > 0)
        {
            return false;
        }

        Changed?.Invoke();
        return true;
    }

    public int CountOf(ushort type)
    {
        int total = 0;
        for (int i = 0; i < Slots.Length; ++i)
        {
            if (Slots[i].Type == type)
            {
                total += Slots[i].Stack;
            }
        }

        return total;
    }

    public bool HasSpaceFor(ushort type, int count)
    {
        if (count <= 0)
        {
            return true;
        }

        int remaining = count;
        int stackLimit = ItemDefs.Get(type).MaxStack;
        if (stackLimit <= 0)
        {
            return false;
        }

        for (int i = 0; i < Slots.Length && remaining > 0; ++i)
        {
            ItemStack slot = Slots[i];
            if (slot.Type == type && !slot.IsEmpty)
            {
                remaining -= Math.Min(remaining, stackLimit - slot.Stack);
            }
            else if (slot.IsEmpty)
            {
                remaining -= Math.Min(remaining, stackLimit);
            }
        }

        return remaining <= 0;
    }

    public void QuickStack()
    {
        for (int i = 0; i < Slots.Length; ++i)
        {
            if (Slots[i].IsEmpty)
            {
                continue;
            }

            for (int j = i + 1; j < Slots.Length; ++j)
            {
                if (Slots[j].IsEmpty)
                {
                    continue;
                }

                if (Slots[i].Type == Slots[j].Type && Slots[i].Stack < ItemDefs.Get(Slots[i].Type).MaxStack)
                {
                    int space = ItemDefs.Get(Slots[i].Type).MaxStack - Slots[i].Stack;
                    if (space <= 0)
                    {
                        continue;
                    }

                    int move = Math.Min(space, Slots[j].Stack);
                    Slots[i].Stack += (short)move;
                    Slots[j].Stack -= (short)move;
                    if (Slots[j].Stack <= 0)
                    {
                        Slots[j] = ItemStack.Empty;
                    }
                }
            }
        }

        ConvertCoins();
        Changed?.Invoke();
    }

    private void ConvertCoins()
    {
        for (int pass = 0; pass < 4; ++pass)
        {
            ConvertCoinTier(ItemDefs.CopperCoin, ItemDefs.SilverCoin, 100);
            ConvertCoinTier(ItemDefs.SilverCoin, ItemDefs.GoldCoin, 100);
            ConvertCoinTier(ItemDefs.GoldCoin, ItemDefs.PlatinumCoin, 100);
        }
    }

    private void ConvertCoinTier(ushort lowerType, ushort higherType, int conversionRate)
    {
        int lowerIndex = CoinStart + (lowerType - ItemDefs.CopperCoin);
        int higherIndex = CoinStart + (higherType - ItemDefs.CopperCoin);

        ItemStack lower = Slots[lowerIndex];
        ItemStack higher = Slots[higherIndex];

        if (lower.IsEmpty || lower.Stack < conversionRate)
        {
            return;
        }

        int converted = lower.Stack / conversionRate;
        int remainder = lower.Stack % conversionRate;
        int currentHigher = higher.Type == higherType ? higher.Stack : 0;
        int maxStack = ItemDefs.Get(higherType).MaxStack;
        int accepted = Math.Min(converted, maxStack - currentHigher);

        lower.Stack = (short)remainder;
        if (lower.Stack <= 0)
        {
            lower = ItemStack.Empty;
        }

        if (higher.Type == 0)
        {
            higher = new ItemStack(higherType, (short)accepted);
        }
        else
        {
            higher.Stack += (short)accepted;
        }

        Slots[lowerIndex] = lower;
        Slots[higherIndex] = higher;

        int overflow = converted - accepted;
        if (overflow > 0)
        {
            Slots[lowerIndex].Stack += (short)(overflow * conversionRate);
        }
    }
}
