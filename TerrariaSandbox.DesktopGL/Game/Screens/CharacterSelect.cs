namespace TerrariaSandbox.DesktopGL.Game.Screens;

/// <summary>
/// Character selection and creation screen.
/// </summary>
public sealed class CharacterSelect : ScreenBase
{
    private readonly List<string> _characters = new();
    private int _selectedIndex;

    public CharacterSelect()
    {
        Title = "Character";
        _characters.Add("Aria");
        _characters.Add("Bruno");
        _characters.Add("Nova");
    }

    public IReadOnlyList<string> Characters => _characters;

    public int SelectedIndex => _selectedIndex;

    public string SelectedCharacter => _characters.Count == 0 ? string.Empty : _characters[_selectedIndex];

    public void MoveSelection(int delta)
    {
        if (_characters.Count == 0)
        {
            return;
        }

        _selectedIndex = (_selectedIndex + delta + _characters.Count) % _characters.Count;
    }

    public void CreateCharacter(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        _characters.Add(name.Trim());
        _selectedIndex = _characters.Count - 1;
    }
}
