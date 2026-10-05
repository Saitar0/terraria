using TerrariaSandbox.DesktopGL.Game.Screens;

namespace TerrariaSandbox.Tests.Regression;

public class SaveRoundTrip
{
    [Fact]
    public void SavedStateCanBeRestoredWithoutLosingPlayerIdentity()
    {
        var original = new WorldSelect
        {
            WorldName = "TestWorld",
            Seed = 42,
            WorldSize = WorldSize.Large,
            CharacterName = "Aria"
        };

        var payload = original.SerializeState();
        var clone = WorldSelect.DeserializeState(payload);

        Assert.Equal(original.WorldName, clone.WorldName);
        Assert.Equal(original.CharacterName, clone.CharacterName);
        Assert.Equal(original.Seed, clone.Seed);
    }
}
