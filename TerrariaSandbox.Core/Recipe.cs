namespace TerrariaSandbox.Core;

/// <summary>
/// A crafting recipe with ingredients, required nearby tiles and liquid flags.
/// </summary>
public readonly struct Recipe
{
    public Recipe(
        ItemStack result,
        (ushort type, short count)[] ingredients,
        ushort[] requiredTiles,
        bool needWater = false,
        bool needLava = false,
        bool needHoney = false,
        bool needSnow = false)
    {
        Result = result;
        Ingredients = ingredients ?? Array.Empty<(ushort type, short count)>();
        RequiredTiles = requiredTiles ?? Array.Empty<ushort>();
        NeedWater = needWater;
        NeedLava = needLava;
        NeedHoney = needHoney;
        NeedSnow = needSnow;
    }

    public ItemStack Result { get; }
    public (ushort type, short count)[] Ingredients { get; }
    public ushort[] RequiredTiles { get; }
    public bool NeedWater { get; }
    public bool NeedLava { get; }
    public bool NeedHoney { get; }
    public bool NeedSnow { get; }
}
