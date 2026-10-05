namespace TerrariaSandbox.DesktopGL.Game.Screens;

/// <summary>
/// Paused game state allowing resume, save and exit.
/// </summary>
public sealed class PauseScreen : ScreenBase
{
    public bool IsPaused { get; private set; } = true;
    public string SavePath { get; set; } = "autosave.sav";

    public void Toggle()
    {
        IsPaused = !IsPaused;
    }

    public void Resume()
    {
        IsPaused = false;
    }

    public string Save()
    {
        return SavePath;
    }
}
