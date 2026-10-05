namespace TerrariaSandbox.DesktopGL.Game.Screens;

/// <summary>
/// Settings screen exposing resolution, zoom, volume and controls.
/// </summary>
public sealed class OptionsScreen : ScreenBase
{
    public int ResolutionWidth { get; set; } = 1280;
    public int ResolutionHeight { get; set; } = 720;
    public float Zoom { get; set; } = 1f;
    public bool VSync { get; set; } = true;
    public float MasterVolume { get; set; } = 1f;
    public float MusicVolume { get; set; } = 0.8f;
    public float SfxVolume { get; set; } = 0.9f;
    public LightMode LightMode { get; set; } = LightMode.Color;
    public Dictionary<string, string> KeyBindings { get; } = new()
    {
        ["MoveLeft"] = "A",
        ["MoveRight"] = "D",
        ["Jump"] = "Space",
        ["Inventory"] = "E",
        ["Pause"] = "Esc"
    };

    public void ApplyResolution(int width, int height)
    {
        ResolutionWidth = width;
        ResolutionHeight = height;
    }

    public void UpdateBinding(string action, string key)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return;
        }

        KeyBindings[action] = key;
    }
}

public enum LightMode
{
    Color,
    White
}
