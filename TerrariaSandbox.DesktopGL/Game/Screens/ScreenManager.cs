namespace TerrariaSandbox.DesktopGL.Game.Screens;

/// <summary>
/// Manages the current game screen and simple transitions between menu, world and gameplay states.
/// </summary>
public sealed class ScreenManager
{
    private readonly Dictionary<ScreenType, ScreenBase> _screens;

    public ScreenManager()
    {
        _screens = new Dictionary<ScreenType, ScreenBase>
        {
            [ScreenType.MainMenu] = new MainMenu(),
            [ScreenType.CharacterSelect] = new CharacterSelect(),
            [ScreenType.WorldSelect] = new WorldSelect(),
            [ScreenType.Options] = new OptionsScreen(),
            [ScreenType.Pause] = new PauseScreen(),
            [ScreenType.Gameplay] = new MainMenu()
        };

        ActiveScreen = ScreenType.MainMenu;
    }

    public ScreenType ActiveScreen { get; private set; }

    public ScreenBase Current => _screens[ActiveScreen];

    public void NavigateTo(ScreenType screen)
    {
        if (_screens.ContainsKey(screen))
        {
            ActiveScreen = screen;
        }
    }

    public void Update(double deltaSeconds)
    {
        _screens[ActiveScreen].Update(deltaSeconds);
    }

    public void Draw()
    {
        _screens[ActiveScreen].Draw();
    }
}

public enum ScreenType
{
    MainMenu,
    CharacterSelect,
    WorldSelect,
    Options,
    Pause,
    Gameplay
}
