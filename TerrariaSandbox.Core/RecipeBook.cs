namespace TerrariaSandbox.Core;

/// <summary>
/// Static collection of crafting recipes and their ingredient lookup index.
/// </summary>
public static class RecipeBook
{
    public const ushort WorkbenchItemType = RecipeGroups.WorkbenchResult;
    public const ushort FurnaceItemType = RecipeGroups.FurnaceResult;
    public const ushort LiquidItemType = RecipeGroups.LiquidResult;
    public const ushort IronBarItemType = RecipeGroups.IronBarResult;

    public static readonly Recipe[] Recipes =
    [
        new(new ItemStack(WorkbenchItemType, 1), new[] { (RecipeGroup.WoodGroup, (short)2), (RecipeGroup.StoneGroup, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(204, 1), new[] { (RecipeGroup.WoodGroup, (short)3), (RecipeGroup.StoneGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(205, 1), new[] { (RecipeGroup.WoodGroup, (short)2), (RecipeGroups.Copper, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(206, 1), new[] { (RecipeGroup.WoodGroup, (short)2), (RecipeGroups.Copper, (short)3) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(207, 1), new[] { (RecipeGroup.WoodGroup, (short)1), (RecipeGroups.Copper, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(208, 1), new[] { (RecipeGroup.WoodGroup, (short)1), (RecipeGroups.Silver, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(209, 4), new[] { (RecipeGroups.Sand, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(210, 1), new[] { (RecipeGroups.Sand, (short)2) }, new[] { (ushort)TileType.Workbench }, needLava: true),
        new(new ItemStack(211, 1), new[] { (RecipeGroup.WoodGroup, (short)5), (RecipeGroup.IronGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(212, 1), new[] { (RecipeGroup.IronGroup, (short)3), (RecipeGroup.StoneGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(213, 4), new[] { (RecipeGroup.StoneGroup, (short)4) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(214, 1), new[] { (RecipeGroup.WoodGroup, (short)1), (RecipeGroups.Sand, (short)1) }, new[] { (ushort)TileType.Workbench }, needWater: true),
        new(new ItemStack(215, 4), new[] { (RecipeGroup.WoodGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(216, 1), new[] { (RecipeGroup.IronGroup, (short)3), (RecipeGroup.WoodGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(217, 1), new[] { (RecipeGroup.IronGroup, (short)3), (RecipeGroup.WoodGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(218, 1), new[] { (RecipeGroup.IronGroup, (short)2), (RecipeGroup.WoodGroup, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(219, 1), new[] { (RecipeGroup.IronGroup, (short)1), (RecipeGroup.WoodGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(220, 1), new[] { (RecipeGroup.WoodGroup, (short)3), (RecipeGroup.IronGroup, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(221, 1), new[] { (RecipeGroup.StoneGroup, (short)4), (RecipeGroup.WoodGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(222, 1), new[] { (RecipeGroup.IronGroup, (short)2), (RecipeGroups.Sand, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(223, 1), new[] { (RecipeGroups.Silver, (short)2), (RecipeGroup.WoodGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(224, 1), new[] { (RecipeGroups.Gold, (short)2), (RecipeGroup.WoodGroup, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(225, 1), new[] { (RecipeGroup.IronGroup, (short)2), (RecipeGroup.StoneGroup, (short)2) }, new[] { (ushort)TileType.Furnace }),
        new(new ItemStack(226, 1), new[] { (RecipeGroup.StoneGroup, (short)3), (RecipeGroups.Sand, (short)1) }, new[] { (ushort)TileType.Furnace }),
        new(new ItemStack(227, 1), new[] { (RecipeGroup.WoodGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(228, 1), new[] { (RecipeGroup.IronGroup, (short)2) }, new[] { (ushort)TileType.Furnace }),
        new(new ItemStack(229, 1), new[] { (RecipeGroup.StoneGroup, (short)5) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(230, 1), new[] { (RecipeGroup.WoodGroup, (short)1), (RecipeGroup.StoneGroup, (short)1), (RecipeGroups.Sand, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(231, 1), new[] { (RecipeGroup.WoodGroup, (short)2), (RecipeGroup.IronGroup, (short)2) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(232, 1), new[] { (RecipeGroup.WoodGroup, (short)2), (RecipeGroups.Copper, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(233, 1), new[] { (RecipeGroup.WoodGroup, (short)1), (RecipeGroup.StoneGroup, (short)1), (RecipeGroup.IronGroup, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(234, 1), new[] { (RecipeGroup.WoodGroup, (short)2), (RecipeGroups.Sand, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(235, 1), new[] { (RecipeGroup.WoodGroup, (short)1), (RecipeGroup.StoneGroup, (short)2), (RecipeGroups.Sand, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(236, 1), new[] { (RecipeGroup.WoodGroup, (short)2), (RecipeGroup.IronGroup, (short)1), (RecipeGroups.Copper, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(237, 1), new[] { (RecipeGroup.WoodGroup, (short)1), (RecipeGroup.StoneGroup, (short)1) }, new[] { (ushort)TileType.Workbench }),
        new(new ItemStack(LiquidItemType, 1), new[] { (RecipeGroup.WoodGroup, (short)1) }, Array.Empty<ushort>(), needWater: true)
    ];

    private static readonly Dictionary<ushort, List<int>> IngredientLookup = BuildIngredientLookup();

    public static IReadOnlyList<int> GetRecipeIndicesForIngredient(ushort itemType)
    {
        if (IngredientLookup.TryGetValue(itemType, out var matches))
        {
            return matches;
        }

        return Array.Empty<int>();
    }

    public static int FindIndexByResult(ushort resultType)
    {
        for (int i = 0; i < Recipes.Length; ++i)
        {
            if (Recipes[i].Result.Type == resultType)
            {
                return i;
            }
        }

        return -1;
    }

    private static Dictionary<ushort, List<int>> BuildIngredientLookup()
    {
        var lookup = new Dictionary<ushort, List<int>>();

        for (int recipeIndex = 0; recipeIndex < Recipes.Length; ++recipeIndex)
        {
            Recipe recipe = Recipes[recipeIndex];
            for (int ingredientIndex = 0; ingredientIndex < recipe.Ingredients.Length; ++ingredientIndex)
            {
                ushort ingredientType = recipe.Ingredients[ingredientIndex].type;
                if (RecipeGroup.IsEncoded(ingredientType))
                {
                    RecipeGroup? group = RecipeGroup.GetById(RecipeGroup.Decode(ingredientType));
                    if (group is not null)
                    {
                        for (int i = 0; i < group.ItemTypes.Length; ++i)
                        {
                            AddMatch(lookup, group.ItemTypes[i], recipeIndex);
                        }
                    }
                }
                else
                {
                    AddMatch(lookup, ingredientType, recipeIndex);
                }
            }
        }

        return lookup;
    }

    private static void AddMatch(Dictionary<ushort, List<int>> lookup, ushort ingredientType, int recipeIndex)
    {
        if (!lookup.TryGetValue(ingredientType, out var matches))
        {
            matches = new List<int>();
            lookup[ingredientType] = matches;
        }

        matches.Add(recipeIndex);
    }
}
