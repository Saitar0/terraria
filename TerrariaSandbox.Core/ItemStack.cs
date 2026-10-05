namespace TerrariaSandbox.Core;

/// <summary>
/// Stack of a placeable or useable item in the inventory or hand.
/// </summary>
public struct ItemStack
{
    public ushort Type;
    public short Stack;
    public byte Prefix;

    public ItemStack(ushort type, short stack = 1, byte prefix = 0)
    {
        Type = type;
        Stack = stack;
        Prefix = prefix;
    }

    public ItemStack(ushort type, int count, byte prefix = 0)
        : this(type, (short)Math.Clamp(count, 0, short.MaxValue), prefix)
    {
    }

    public bool IsEmpty => Type == 0 || Stack <= 0;

    public int Count
    {
        get => Stack;
        set => Stack = (short)Math.Clamp(value, 0, short.MaxValue);
    }

    public static ItemStack Empty => default;

    public static bool operator ==(ItemStack left, ItemStack right)
        => left.Type == right.Type && left.Stack == right.Stack && left.Prefix == right.Prefix;

    public static bool operator !=(ItemStack left, ItemStack right)
        => !(left == right);

    public override bool Equals(object? obj)
        => obj is ItemStack other && this == other;

    public override int GetHashCode()
        => HashCode.Combine(Type, Stack, Prefix);

    public override string ToString()
        => $"{Type}:{Stack}:{Prefix}";
}
