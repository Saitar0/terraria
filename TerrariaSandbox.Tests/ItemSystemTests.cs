using TerrariaSandbox.Core;

namespace TerrariaSandbox.Tests;

public class ItemSystemTests
{
    [Fact]
    public void Inventory_ShouldStackExistingItemsBeforeFillingEmptySlots()
    {
        var inventory = new Inventory();
        var add1 = new ItemStack(1, 10);
        var add2 = new ItemStack(1, 5);

        Assert.True(inventory.TryAdd(ref add1));
        Assert.True(inventory.TryAdd(ref add2));

        Assert.Equal(1, inventory.Slots.Count(s => s.Type == 1 && s.Stack > 0));
        Assert.Equal(15, inventory.Slots[0].Stack);
        Assert.Equal(0, inventory.Slots[1].Stack);
    }

    [Fact]
    public void Inventory_ShouldConvertCoinsAndQuickStack()
    {
        var inventory = new Inventory();
        var copper = new ItemStack(50, 600);

        Assert.True(inventory.TryAdd(ref copper));

        Assert.Equal(0, inventory.Slots[50].Stack);
        Assert.Equal(6, inventory.Slots[51].Stack);

        inventory.QuickStack();

        Assert.Equal(6, inventory.Slots[51].Stack);
    }
}
