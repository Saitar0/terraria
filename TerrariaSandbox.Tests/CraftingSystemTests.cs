using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.Tests;

public class CraftingSystemTests
{
    [Fact]
    public void Refresh_ShouldExposeCraftableRecipes_WhenIngredientsAndStationsMatch()
    {
        var inventory = new Inventory();
        var wood = new ItemStack((ushort)RecipeGroups.Wood, 2);
        var stone = new ItemStack((ushort)RecipeGroups.Stone, 1);
        Assert.True(inventory.TryAdd(ref wood));
        Assert.True(inventory.TryAdd(ref stone));

        var world = new World(128, 128);
        var player = new Player(new Vector2(32f, 64f));
        int px = (int)MathF.Floor(player.Position.X / 16f);
        int py = (int)MathF.Floor(player.Position.Y / 16f);
        world.Set(px + 1, py, (ushort)TileType.Workbench);

        var system = new CraftingSystem();
        system.Refresh(player, world, inventory);

        int workbenchRecipe = RecipeBook.FindIndexByResult(RecipeBook.WorkbenchItemType);
        Assert.Contains(workbenchRecipe, system.Available.ToArray());
    }

    [Fact]
    public void Refresh_ShouldHideRecipe_WhenRequiredStationIsMissing()
    {
        var inventory = new Inventory();
        var wood = new ItemStack((ushort)RecipeGroups.Wood, 2);
        var stone = new ItemStack((ushort)RecipeGroups.Stone, 1);
        Assert.True(inventory.TryAdd(ref wood));
        Assert.True(inventory.TryAdd(ref stone));

        var world = new World(128, 128);
        var player = new Player(new Vector2(32f, 64f));
        var system = new CraftingSystem();

        system.Refresh(player, world, inventory);

        int workbenchRecipe = RecipeBook.FindIndexByResult(RecipeBook.WorkbenchItemType);
        Assert.DoesNotContain(workbenchRecipe, system.Available.ToArray());
    }

    [Fact]
    public void Refresh_ShouldTreatRecipeGroupsAsEquivalentIngredients()
    {
        var inventory = new Inventory();
        var wood = new ItemStack((ushort)RecipeGroups.Wood, 2);
        var stone = new ItemStack((ushort)RecipeGroups.Stone, 1);
        Assert.True(inventory.TryAdd(ref wood));
        Assert.True(inventory.TryAdd(ref stone));

        var world = new World(128, 128);
        var player = new Player(new Vector2(32f, 64f));
        int px = (int)MathF.Floor(player.Position.X / 16f);
        int py = (int)MathF.Floor(player.Position.Y / 16f);
        world.Set(px + 1, py, (ushort)TileType.Workbench);

        var system = new CraftingSystem();
        system.Refresh(player, world, inventory);

        int recipe = RecipeBook.FindIndexByResult(RecipeBook.WorkbenchItemType);
        Assert.Contains(recipe, system.Available.ToArray());
    }

    [Fact]
    public void Craft_ShouldRejectWhenInventoryHasNoSpaceForResult()
    {
        var inventory = new Inventory();
        var workbenchIngredientWood = new ItemStack((ushort)RecipeGroups.Wood, 4);
        var workbenchIngredientStone = new ItemStack((ushort)RecipeGroups.Stone, 2);
        Assert.True(inventory.TryAdd(ref workbenchIngredientWood));
        Assert.True(inventory.TryAdd(ref workbenchIngredientStone));

        inventory.Slots[0] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[1] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[2] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[3] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[4] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[5] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[6] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[7] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[8] = new ItemStack(RecipeBook.WorkbenchItemType, 1);
        inventory.Slots[9] = new ItemStack(RecipeBook.WorkbenchItemType, 1);

        var world = new World(128, 128);
        var player = new Player(new Vector2(32f, 64f));
        int px = (int)MathF.Floor(player.Position.X / 16f);
        int py = (int)MathF.Floor(player.Position.Y / 16f);
        world.Set(px + 1, py, (ushort)TileType.Workbench);

        var system = new CraftingSystem();
        system.Refresh(player, world, inventory);

        Assert.False(system.Craft(RecipeBook.FindIndexByResult(RecipeBook.WorkbenchItemType)));
    }

    [Fact]
    public void Refresh_ShouldOnlyExposeLiquidRecipesNearWaterOrLava()
    {
        var inventory = new Inventory();
        var waterRecipe = RecipeBook.FindIndexByResult(RecipeBook.LiquidItemType);

        var world = new World(128, 128);
        var player = new Player(new Vector2(32f, 64f));
        int px = (int)MathF.Floor(player.Position.X / 16f);
        int py = (int)MathF.Floor(player.Position.Y / 16f);

        var wood = new ItemStack((ushort)RecipeGroups.Wood, 2);
        Assert.True(inventory.TryAdd(ref wood));

        var system = new CraftingSystem();
        system.Refresh(player, world, inventory);
        Assert.DoesNotContain(waterRecipe, system.Available.ToArray());

        world.Set(px + 1, py, (ushort)TileType.Water);
        system.MarkDirty();
        system.Refresh(player, world, inventory);
        Assert.Contains(waterRecipe, system.Available.ToArray());
    }

    [Fact]
    public void Refresh_ShouldStayBelowOneTenthOfAMillisecond()
    {
        var inventory = new Inventory();
        var wood = new ItemStack((ushort)RecipeGroups.Wood, 4);
        Assert.True(inventory.TryAdd(ref wood));

        var world = new World(128, 128);
        var player = new Player(new Vector2(32f, 64f));
        int px = (int)MathF.Floor(player.Position.X / 16f);
        int py = (int)MathF.Floor(player.Position.Y / 16f);
        world.Set(px + 1, py, (ushort)TileType.Workbench);

        var system = new CraftingSystem();
        system.Refresh(player, world, inventory);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 5000; i++)
        {
            system.Refresh(player, world, inventory);
        }
        sw.Stop();

        double elapsedMs = sw.Elapsed.TotalMilliseconds / 5000d;
        Assert.True(elapsedMs < 0.1d, $"Refresh took {elapsedMs} ms per call.");
    }
}
