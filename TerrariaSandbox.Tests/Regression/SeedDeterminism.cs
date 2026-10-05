using TerrariaSandbox.DesktopGL.Game.Screens;

namespace TerrariaSandbox.Tests.Regression;

public class SeedDeterminism
{
    [Fact]
    public void SameSeedProducesEquivalentWorldSignature()
    {
        var a = new WorldSelect { Seed = 1337, WorldName = "Alpha", WorldSize = WorldSize.Medium };
        var b = new WorldSelect { Seed = 1337, WorldName = "Alpha", WorldSize = WorldSize.Medium };

        Assert.Equal(a.CreateWorldSignature(), b.CreateWorldSignature());
    }
}
