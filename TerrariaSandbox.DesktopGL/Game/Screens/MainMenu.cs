using Microsoft.Xna.Framework;

namespace TerrariaSandbox.DesktopGL.Game.Screens;

/// <summary>
/// Main menu screen with keyboard and mouse-driven selection.
/// </summary>
public sealed class MainMenu : ScreenBase
{
    private readonly List<(string Label, Rectangle Bounds)> _items = new();
    private int _selectedIndex;

    public MainMenu()
    {
        Title = "TerrariaSandbox";
        AddItem("New Game", new Rectangle(0, 0, 180, 36));
        AddItem("Continue", new Rectangle(0, 40, 180, 36));
        AddItem("Options", new Rectangle(0, 80, 180, 36));
        AddItem("Exit", new Rectangle(0, 120, 180, 36));
    }

    public int SelectedIndex => _selectedIndex;

    public IReadOnlyList<string> Options => _items.Select(static item => item.Label).ToList();

    public void MoveSelection(int delta)
    {
        if (_items.Count == 0)
        {
            return;
        }

        _selectedIndex = (_selectedIndex + delta + _items.Count) % _items.Count;
    }

    public void SelectByMouse(int x, int y)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            var bounds = _items[i].Bounds;
            if (x >= bounds.X && x <= bounds.Right && y >= bounds.Y && y <= bounds.Bottom)
            {
                _selectedIndex = i;
                return;
            }
        }
    }

    public string ConfirmSelection()
    {
        return _items.Count == 0 ? string.Empty : _items[_selectedIndex].Label;
    }

    public void AddItem(string label, Rectangle bounds)
    {
        _items.Add((label, bounds));
    }

    public override void Draw()
    {
        base.Draw();
    }
}
