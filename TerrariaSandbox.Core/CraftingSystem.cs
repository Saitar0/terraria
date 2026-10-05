namespace TerrariaSandbox.Core;

/// <summary>
/// Computes which recipes are available in the current player vicinity and executes craft requests.
/// </summary>
public sealed class CraftingSystem
{
    private readonly int[] _available = new int[256];
    private readonly bool[] _candidateRecipes = new bool[256];
    private readonly ushort[] _adjacentTiles = new ushort[256];
    private Inventory? _currentInventory;
    private int _availableCount;
    private int _lastTick;
    private bool _dirty = true;

    public bool Dirty
    {
        get => _dirty;
        set => _dirty = value;
    }

    public ReadOnlySpan<int> Available => _available.AsSpan(0, _availableCount);

    public void MarkDirty()
    {
        _dirty = true;
    }

    public void Refresh(Player p, World w, Inventory inv)
    {
        if (!_dirty && p.Tick - _lastTick < 10)
        {
            return;
        }

        _currentInventory = inv;
        _lastTick = p.Tick;
        _dirty = false;
        _availableCount = 0;
        Array.Clear(_candidateRecipes, 0, _candidateRecipes.Length);
        Array.Clear(_adjacentTiles, 0, _adjacentTiles.Length);

        int centerX = (int)MathF.Floor(p.Position.X / 16f);
        int centerY = (int)MathF.Floor((p.Position.Y + p.Height * 0.5f) / 16f);

        bool adjWater = false;
        bool adjLava = false;
        bool adjHoney = false;
        bool adjSnow = false;

        for (int y = centerY - 3; y <= centerY + 3; ++y)
        {
            for (int x = centerX - 4; x <= centerX + 4; ++x)
            {
                ushort tile = w.Get(x, y);
                if ((uint)tile < (uint)_adjacentTiles.Length)
                {
                    _adjacentTiles[tile]++;
                }

                if (tile == (ushort)TileType.Water)
                {
                    adjWater = true;
                }
                else if (tile == (ushort)TileType.Lava)
                {
                    adjLava = true;
                }
                else if (tile == (ushort)TileType.Honey)
                {
                    adjHoney = true;
                }
                else if (tile == (ushort)TileType.Snow)
                {
                    adjSnow = true;
                }
            }
        }

        for (int slot = 0; slot < inv.Slots.Length; ++slot)
        {
            ItemStack item = inv.Slots[slot];
            if (item.IsEmpty)
            {
                continue;
            }

            IReadOnlyList<int> matches = RecipeBook.GetRecipeIndicesForIngredient(item.Type);
            for (int i = 0; i < matches.Count; ++i)
            {
                int recipeIndex = matches[i];
                if ((uint)recipeIndex < (uint)_candidateRecipes.Length)
                {
                    _candidateRecipes[recipeIndex] = true;
                }
            }
        }

        for (int recipeIndex = 0; recipeIndex < RecipeBook.Recipes.Length; ++recipeIndex)
        {
            if (!_candidateRecipes[recipeIndex])
            {
                continue;
            }

            Recipe recipe = RecipeBook.Recipes[recipeIndex];
            if (!RecipeMatchesStations(recipe, _adjacentTiles, adjWater, adjLava, adjHoney, adjSnow))
            {
                continue;
            }

            if (!HasInventoryFor(recipe, inv))
            {
                continue;
            }

            _available[_availableCount++] = recipeIndex;
        }
    }

    public bool Craft(int recipeIndex)
    {
        if (_currentInventory is null)
        {
            return false;
        }

        if ((uint)recipeIndex >= (uint)RecipeBook.Recipes.Length)
        {
            return false;
        }

        if (!ContainsIndex(Available, recipeIndex))
        {
            return false;
        }

        Recipe recipe = RecipeBook.Recipes[recipeIndex];
        if (!CanAddResult(_currentInventory, recipe.Result))
        {
            return false;
        }

        if (!TryConsumeIngredients(_currentInventory, recipe))
        {
            return false;
        }

        ItemStack result = recipe.Result;
        _currentInventory.TryAdd(ref result);
        _dirty = true;
        return true;
    }

    private static bool ContainsIndex(ReadOnlySpan<int> available, int recipeIndex)
    {
        for (int i = 0; i < available.Length; ++i)
        {
            if (available[i] == recipeIndex)
            {
                return true;
            }
        }

        return false;
    }

    private static bool RecipeMatchesStations(Recipe recipe, ushort[] adjacentTiles, bool adjWater, bool adjLava, bool adjHoney, bool adjSnow)
    {
        foreach (ushort requiredTile in recipe.RequiredTiles)
        {
            if (!MatchesRequiredTile(requiredTile, adjacentTiles))
            {
                return false;
            }
        }

        if (recipe.NeedWater && !adjWater)
        {
            return false;
        }

        if (recipe.NeedLava && !adjLava)
        {
            return false;
        }

        if (recipe.NeedHoney && !adjHoney)
        {
            return false;
        }

        if (recipe.NeedSnow && !adjSnow)
        {
            return false;
        }

        return true;
    }

    private static bool MatchesRequiredTile(ushort requiredTile, ushort[] adjacentTiles)
    {
        if (requiredTile == 0)
        {
            return true;
        }

        if (requiredTile < adjacentTiles.Length && adjacentTiles[requiredTile] > 0)
        {
            return true;
        }

        return IsAlternateStation(requiredTile, adjacentTiles);
    }

    private static bool IsAlternateStation(ushort requiredTile, ushort[] adjacentTiles)
    {
        if (requiredTile == (ushort)TileType.Workbench)
        {
            return adjacentTiles[(ushort)TileType.Workbench] > 0 ||
                   adjacentTiles[(ushort)TileType.Wood] > 0 ||
                   adjacentTiles[(ushort)TileType.Platform] > 0 ||
                   adjacentTiles[(ushort)TileType.Table] > 0;
        }

        if (requiredTile == (ushort)TileType.Furnace)
        {
            return adjacentTiles[(ushort)TileType.Furnace] > 0 || adjacentTiles[(ushort)TileType.Brick] > 0;
        }

        return false;
    }

    private static bool HasInventoryFor(Recipe recipe, Inventory inv)
    {
        for (int i = 0; i < recipe.Ingredients.Length; ++i)
        {
            (ushort type, short count) = recipe.Ingredients[i];
            if (!HasInventoryCount(inv, type, count))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasInventoryCount(Inventory inv, ushort ingredientType, short requiredCount)
    {
        if (requiredCount <= 0)
        {
            return true;
        }

        if (RecipeGroup.IsEncoded(ingredientType))
        {
            ushort groupId = RecipeGroup.Decode(ingredientType);
            RecipeGroup? group = RecipeGroup.GetById(groupId);
            if (group is null)
            {
                return false;
            }

            int remaining = requiredCount;
            for (int i = 0; i < group.ItemTypes.Length; ++i)
            {
                remaining -= CountItem(inv, group.ItemTypes[i]);
                if (remaining <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        return CountItem(inv, ingredientType) >= requiredCount;
    }

    private static bool TryConsumeIngredients(Inventory inv, Recipe recipe)
    {
        for (int i = 0; i < recipe.Ingredients.Length; ++i)
        {
            (ushort type, short count) = recipe.Ingredients[i];
            if (RecipeGroup.IsEncoded(type))
            {
                ushort groupId = RecipeGroup.Decode(type);
                RecipeGroup? group = RecipeGroup.GetById(groupId);
                if (group is null)
                {
                    return false;
                }

                int remaining = count;
                for (int slot = 0; slot < inv.Slots.Length && remaining > 0; ++slot)
                {
                    ItemStack slotItem = inv.Slots[slot];
                    if (slotItem.IsEmpty || !group.Contains(slotItem.Type))
                    {
                        continue;
                    }

                    int take = Math.Min(remaining, slotItem.Stack);
                    slotItem.Stack -= (short)take;
                    remaining -= take;
                    inv.Slots[slot] = slotItem.Stack > 0 ? slotItem : ItemStack.Empty;
                }

                if (remaining > 0)
                {
                    return false;
                }
            }
            else
            {
                int remaining = count;
                for (int slot = 0; slot < inv.Slots.Length && remaining > 0; ++slot)
                {
                    ItemStack slotItem = inv.Slots[slot];
                    if (slotItem.Type != type || slotItem.IsEmpty)
                    {
                        continue;
                    }

                    int take = Math.Min(remaining, slotItem.Stack);
                    slotItem.Stack -= (short)take;
                    remaining -= take;
                    inv.Slots[slot] = slotItem.Stack > 0 ? slotItem : ItemStack.Empty;
                }

                if (remaining > 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static int CountItem(Inventory inv, ushort type)
    {
        int total = 0;
        for (int i = 0; i < inv.Slots.Length; ++i)
        {
            if (inv.Slots[i].Type == type)
            {
                total += inv.Slots[i].Stack;
            }
        }

        return total;
    }

    private static bool CanAddResult(Inventory inv, ItemStack result)
    {
        if (result.IsEmpty)
        {
            return false;
        }

        int count = result.Stack;
        if (count <= 0)
        {
            return false;
        }

        for (int i = 0; i < inv.Slots.Length; ++i)
        {
            ItemStack slot = inv.Slots[i];
            if (slot.Type == result.Type && !slot.IsEmpty)
            {
                int space = ItemDefs.Get(result.Type).MaxStack - slot.Stack;
                count -= Math.Min(space, count);
            }
            else if (slot.IsEmpty)
            {
                int stackLimit = ItemDefs.Get(result.Type).MaxStack;
                count -= Math.Min(count, stackLimit);
            }

            if (count <= 0)
            {
                return true;
            }
        }

        return false;
    }
}
