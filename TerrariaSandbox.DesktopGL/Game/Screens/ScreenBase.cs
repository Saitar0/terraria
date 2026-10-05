namespace TerrariaSandbox.DesktopGL.Game.Screens;

/// <summary>
/// Base contract for a game screen in the main state machine.
/// </summary>
public abstract class ScreenBase
{
    public string Title { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;

    public virtual void Update(double deltaSeconds)
    {
    }

    public virtual void Draw()
    {
    }
}
