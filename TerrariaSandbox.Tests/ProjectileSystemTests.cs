using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.Tests;

public class ProjectileSystemTests
{
    [Fact]
    public void ProjectileAi_ShouldReducePenetrationAndRemoveWhenExhausted()
    {
        var projectile = new Projectile
        {
            Type = ProjectileTypes.Arrow,
            Position = new Vector2(96f, 96f),
            Velocity = Vector2.Zero,
            Damage = 10,
            Penetrate = 2,
            TimeLeft = 300,
            AiStyle = ProjectileAiStyles.Straight,
            Owner = 0,
            Hostile = false,
            TileCollide = true,
            Active = true
        };

        ProjectileAi.HandleImpact(ref projectile, true);
        Assert.Equal(1, projectile.Penetrate);
        Assert.True(projectile.Active);

        ProjectileAi.HandleImpact(ref projectile, true);
        Assert.False(projectile.Active);
    }

    [Fact]
    public void ProjectileSystem_ShouldConsumeAmmoFromAmmoSlots()
    {
        var inventory = new Inventory();
        inventory.Slots[54] = new ItemStack(54, 3);
        inventory.Slots[55] = new ItemStack(55, 2);

        Assert.True(ProjectileSystem.TryConsumeAmmo(inventory, 54, 1));
        Assert.Equal(2, inventory.Slots[54].Stack);

        Assert.True(ProjectileSystem.TryConsumeAmmo(inventory, 55, 2));
        Assert.Equal(0, inventory.Slots[55].Stack);

        Assert.False(ProjectileSystem.TryConsumeAmmo(inventory, 54, 3));
    }
}
