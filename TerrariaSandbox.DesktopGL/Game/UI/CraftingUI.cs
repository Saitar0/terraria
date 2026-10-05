namespace Game.UI;

using TerrariaSandbox.Core;

/// <summary>
/// Lightweight UI wrapper for the crafting system.
/// </summary>
public sealed class CraftingUI
{
    public CraftingUI(CraftingSystem craftingSystem)
    {
        CraftingSystem = craftingSystem ?? throw new ArgumentNullException(nameof(craftingSystem));
    }

    public CraftingSystem CraftingSystem { get; }

    public void Draw()
    {
    }
}
