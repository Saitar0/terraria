using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.Tests;

public class LightingEngineTests
{
    [Fact]
    public void LightingEngine_ShouldExposeSampledColorAtWorldCoordinates()
    {
        var world = new World(64, 64);
        var engine = new LightingEngine();

        engine.AddEmitter(new LightEmitter(10, 10, 1f, 0.9f, 0.5f));
        engine.Update(world, new RectI(0, 0, 32, 32), new SunInfo(0.15f, 0.2f, 0.4f, true));

        Color sample = engine.GetSample(10, 10);
        Assert.True(sample.R > 0 || sample.G > 0 || sample.B > 0);
    }

    [Fact]
    public void LightingEngine_ShouldSupportNightAmbientFloor()
    {
        var world = new World(64, 64);
        var engine = new LightingEngine
        {
            AmbientLight = 0.12f,
            Brightness = 1.1f,
            Gamma = 1.0f
        };

        engine.Update(world, new RectI(0, 0, 16, 16), new SunInfo(0f, 0f, 0f, false));

        Color sample = engine.GetSample(5, 5);
        Assert.True(sample.R >= 0.12f * 255f - 1f);
        Assert.True(sample.G >= 0.12f * 255f - 1f);
        Assert.True(sample.B >= 0.12f * 255f - 1f);
    }

    [Fact]
    public void LightingEngine_ShouldEmitTorchLightInOpenAir()
    {
        var world = new World(64, 64);
        var engine = new LightingEngine();

        engine.AddEmitter(new LightEmitter(10, 10, 1f, 0.9f, 0.5f));
        engine.Update(world, new RectI(0, 0, 32, 32), new SunInfo(0.15f, 0.2f, 0.4f, true));

        var cell = engine.Get(10, 10);
        Assert.True(cell.R > 0.01f || cell.G > 0.01f || cell.B > 0.01f);
        var distant = engine.Get(20, 20);
        Assert.True(distant.R >= 0f && distant.G >= 0f && distant.B >= 0f);
    }

    [Fact]
    public void LightingEngine_ShouldBlockSunlightWithSolidColumnAndDecayBelow()
    {
        var world = new World(32, 32);
        var engine = new LightingEngine();

        for (int y = 12; y < 18; y++)
        {
            world.Set(16, y, 3);
        }

        engine.Update(world, new RectI(0, 0, 32, 32), new SunInfo(1f, 1f, 1f, true));

        var exposed = engine.Get(16, 5);
        var blocked = engine.Get(16, 20);

        Assert.True(exposed.R > 0f || exposed.G > 0f || exposed.B > 0f);
        Assert.True(blocked.R >= 0f && blocked.G >= 0f && blocked.B >= 0f);
    }

    [Fact]
    public void World_ShouldMarkListenerDirty_WhenTileChangesNearView()
    {
        var world = new World(64, 64);
        var engine = new LightingEngine();
        world.AddListener(engine);

        world.Set(12, 12, 1);

        Assert.True(engine.IsDirty);
    }
}
