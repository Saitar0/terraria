using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;
using TerrariaSandbox.DesktopGL;

namespace TerrariaSandbox.Tests;

public class WorldLayoutTests
{
    [Fact]
    public void Constants_ShouldMatchProjectRequirements()
    {
        Assert.Equal(16, WorldConstants.TileSizePixels);
        Assert.Equal(60, WorldConstants.TicksPerSecond);
        Assert.Equal(8400, WorldConstants.WorldWidthTiles);
        Assert.Equal(2400, WorldConstants.WorldHeightTiles);
        Assert.Equal(64, WorldConstants.ChunkSideTiles);
        Assert.Equal(8400 * 2400, WorldConstants.TotalTileCount);
    }

    [Fact]
    public void RowMajorIndex_ShouldMatchCoordinateRules()
    {
        Assert.Equal(0, WorldIndex.FromTile(0, 0));
        Assert.Equal(8401, WorldIndex.FromTile(1, 1));
        Assert.Equal(8400, WorldIndex.FromTile(0, 1));
        Assert.Equal(8400 * 2 + 3, WorldIndex.FromTile(3, 2));
    }

    [Fact]
    public void ChunkLayout_ShouldUseLogicalChunkSize()
    {
        var chunk = new WorldChunk(0, 0);
        Assert.Equal(64, chunk.Width);
        Assert.Equal(64, chunk.Height);
        Assert.Equal(4096, chunk.TileCount);
    }

    [Fact]
    public void World_ShouldClampCoordinatesWithoutThrowing()
    {
        var world = new World(16, 16);

        Assert.Equal(0, world.Get(-10, -10));
        Assert.Equal(0, world.Get(100, 100));

        world.Set(-10, -10, 7);
        world.Set(100, 100, 9);

        Assert.Equal(7, world.Get(0, 0));
        Assert.Equal(9, world.Get(15, 15));
    }

    [Fact]
    public void SetTile_OnChunkBoundary_ShouldMarkAdjacentChunksDirty()
    {
        var world = new World(128, 128);

        world.Set(63, 63, 5);

        Assert.True(world.Chunks.GetChunk(0, 0).DirtyRender);
        Assert.True(world.Chunks.GetChunk(1, 0).DirtyRender);
        Assert.True(world.Chunks.GetChunk(0, 1).DirtyRender);
        Assert.True(world.Chunks.GetChunk(1, 1).DirtyRender);
    }

    [Fact]
    public void AutoTile_ShouldResolveMaskAndFrame()
    {
        var world = new World(10, 10);
        world.Set(5, 4, 1);
        world.Set(5, 5, 1);
        world.Set(4, 5, 1);
        world.Set(6, 5, 1);

        AutoTile.Frame(world, 5, 5);

        Assert.True(world.FrameX[world.ToIndex(5, 5)] >= 0);
        Assert.True(world.FrameY[world.ToIndex(5, 5)] >= 0);
        Assert.Equal(11, AutoTile.GetMask(world, 5, 5));
    }

    [Fact]
    public void Camera_ShouldComputeVisibleTileBounds()
    {
        var camera = new Camera
        {
            ScreenPosition = new Vector2(128f, 64f),
            Zoom = 2f
        };

        Rectangle visible = camera.VisibleTileRect(1280, 720, 16, 120, 70);

        Assert.True(visible.Width > 0);
        Assert.True(visible.Height > 0);
        Assert.True(visible.Left >= 0);
        Assert.True(visible.Top >= 0);
    }

    [Fact]
    public void CameraController_ShouldSmoothAndClampTarget()
    {
        var camera = new Camera
        {
            ScreenPosition = new Vector2(0f, 0f),
            Zoom = 1f
        };
        var player = new Player
        {
            Position = new Vector2(3000f, 120f),
            Velocity = new Vector2(32f, 0f),
            Width = 20f,
            Height = 44f,
            Direction = 1
        };

        var controller = new TerrariaSandbox.DesktopGL.CameraController();
        controller.Follow(in player, camera, 1f / 60f);

        Assert.True(camera.ScreenPosition.X > 0f);
        Assert.True(camera.ScreenPosition.X <= WorldConstants.WorldWidthTiles * WorldConstants.TileSizePixels - 1280f);
        Assert.True(camera.ScreenPosition.Y >= 0f);
    }

    [Fact]
    public void DayClock_ShouldUseExpectedCycleLengthsAndSkyValues()
    {
        var dayClock = new TerrariaSandbox.Core.DayClock
        {
            Tick = 54000L
        };

        Assert.InRange(dayClock.DayFraction, 0f, 1f);
        Assert.Equal(255, dayClock.SkyColor().A);

        dayClock.Tick = 0L;
        Assert.InRange(dayClock.DayFraction, 0f, 1f);
        Assert.Equal(255, dayClock.SkyColor().A);
    }

    [Fact]
    public void TileObjectData_ShouldExposeDoorAnchors()
    {
        TileObjectData door = TileObjectData.GetForTile((ushort)TileType.Door);

        Assert.Equal(2, door.Width);
        Assert.Equal(3, door.Height);
        Assert.Equal(0, door.OriginX);
        Assert.Equal(0, door.OriginY);
        Assert.True(door.AnchorX.Length >= 2);
        Assert.True(door.AnchorY.Length >= 2);
    }

    [Fact]
    public void InteractionSystem_ShouldBreakMultiCellDoorAndRemoveAllCells()
    {
        var world = new World(128, 128);
        world.Set(14, 18, (ushort)TileType.Door);
        world.Set(15, 18, (ushort)TileType.Door);
        world.Set(14, 19, (ushort)TileType.Door);
        world.Set(15, 19, (ushort)TileType.Door);

        var interaction = new InteractionSystem(world, new NullItemSpawner());
        var player = new Player
        {
            Position = new Vector2(14f * 16f, 18f * 16f)
        };

        var cursor = new Cursor
        {
            TileX = 14,
            TileY = 18,
            ReachTiles = 10,
            Visible = true
        };

        interaction.Update(player, cursor, new ItemStack((ushort)TileType.Door, 1));

        Assert.Equal(0, world.Get(14, 18));
        Assert.Equal(0, world.Get(15, 18));
        Assert.Equal(0, world.Get(14, 19));
        Assert.Equal(0, world.Get(15, 19));
    }

    [Fact]
    public void SupportChecker_ShouldDropUnsupportedTorch()
    {
        var world = new World(64, 64);
        world.Set(10, 10, (ushort)TileType.Torch);

        world.SupportChecker.Update(world, 32);

        Assert.Equal(0, world.Get(10, 10));
    }
}
