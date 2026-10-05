namespace TerrariaSandbox.Core;

/// <summary>
/// Represents an alternative ingredient group such as any wood or any iron.
/// </summary>
public sealed class RecipeGroup
{
    public const ushort GroupTag = 0x8000;

    public static readonly ushort WoodGroup = Encode(1);
    public static readonly ushort StoneGroup = Encode(2);
    public static readonly ushort IronGroup = Encode(3);
    public static readonly ushort OreGroup = Encode(4);

    public RecipeGroup(ushort id, string name, params ushort[] itemTypes)
    {
        Id = id;
        Name = name;
        ItemTypes = itemTypes ?? Array.Empty<ushort>();
    }

    public ushort Id { get; }
    public string Name { get; }
    public ushort[] ItemTypes { get; }

    public bool Contains(ushort itemType)
    {
        for (int i = 0; i < ItemTypes.Length; ++i)
        {
            if (ItemTypes[i] == itemType)
            {
                return true;
            }
        }

        return false;
    }

    public static ushort Encode(int groupId)
    {
        return (ushort)(GroupTag | ((ushort)groupId & 0x7FFF));
    }

    public static bool IsEncoded(ushort type)
    {
        return (type & GroupTag) != 0;
    }

    public static ushort Decode(ushort type)
    {
        return (ushort)(type & 0x7FFF);
    }

    public static readonly RecipeGroup Wood = new(1, "Wood", RecipeGroups.Wood, 4, 15, 20);
    public static readonly RecipeGroup Stone = new(2, "Stone", RecipeGroups.Stone, 2, 3, 6);
    public static readonly RecipeGroup Iron = new(3, "Iron", RecipeGroups.Iron, 16, 18, 22, 202);
    public static readonly RecipeGroup[] All = [Wood, Stone, Iron];

    public static RecipeGroup? GetById(ushort id)
    {
        for (int i = 0; i < All.Length; ++i)
        {
            if (All[i].Id == id)
            {
                return All[i];
            }
        }

        return null;
    }
}

public static class RecipeGroups
{
    public const ushort Wood = 4;
    public const ushort Stone = 2;
    public const ushort Iron = 16;
    public const ushort Sand = 5;
    public const ushort Copper = 7;
    public const ushort Silver = 8;
    public const ushort Gold = 9;
    public const ushort WorkbenchResult = 200;
    public const ushort LiquidResult = 201;
    public const ushort IronBarResult = 202;
    public const ushort FurnaceResult = 203;
}
