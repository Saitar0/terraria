using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.Tests;

public class PlayerPhysicsTests
{
    [Fact]
    public void Step_ShouldBeDeterministic_ForTheSameIntentSequenceOver1000Ticks()
    {
        var first = new Player
        {
            Position = new Vector2(80f, 160f),
            Velocity = Vector2.Zero,
            Width = Player.DefaultWidth,
            Height = Player.DefaultHeight,
            Direction = 1,
            OnGround = true
        };

        var second = new Player
        {
            Position = new Vector2(80f, 160f),
            Velocity = Vector2.Zero,
            Width = Player.DefaultWidth,
            Height = Player.DefaultHeight,
            Direction = 1,
            OnGround = true
        };

        var intents = new PlayerIntent[1000];
        for (int i = 0; i < intents.Length; i++)
        {
            var intent = new PlayerIntent();
            if ((i % 31) == 0)
            {
                intent.Right = true;
            }
            if ((i % 49) == 0)
            {
                intent.Left = true;
            }
            if ((i % 67) == 0)
            {
                intent.Jump = true;
            }
            intents[i] = intent;
        }

        for (int i = 0; i < intents.Length; i++)
        {
            first.Step(in intents[i], in PhysicsConstants.Default);
            second.Step(in intents[i], in PhysicsConstants.Default);
        }

        Assert.Equal(first.Position.X, second.Position.X, 6);
        Assert.Equal(first.Position.Y, second.Position.Y, 6);
        Assert.Equal(first.Velocity.X, second.Velocity.X, 6);
        Assert.Equal(first.Velocity.Y, second.Velocity.Y, 6);
    }

    [Fact]
    public void Step_ShouldRespectJumpAndFallLimits()
    {
        var player = new Player
        {
            Position = new Vector2(40f, 200f),
            Velocity = Vector2.Zero,
            Width = Player.DefaultWidth,
            Height = Player.DefaultHeight,
            Direction = 1,
            OnGround = true
        };

        var intent = new PlayerIntent { Jump = true };
        for (int i = 0; i < 16; i++)
        {
            player.Step(in intent, in PhysicsConstants.Default);
        }

        Assert.True(player.Velocity.Y <= PhysicsConstants.Default.MaxFall + 0.01f);
        Assert.True(MathF.Abs(player.Position.Y - 200f) > 0f || player.Velocity.Y < 0f);
    }

    [Fact]
    public void Collision_ShouldStopHorizontalMovementAgainstSolidTile()
    {
        var world = new World(64, 64);
        world.Set(8, 4, 1);

        Vector2 position = new Vector2(120f, 64f);
        Vector2 velocity = new Vector2(30f, 0f);
        var options = new CollisionOptions { TileCollide = true };

        Collision.Move(world, ref position, ref velocity, 16, 16, in options, out CollisionResult result);

        Assert.True(result.HitWallR);
        Assert.True(position.X <= 8 * 16f - 16f + 0.001f);
        Assert.Equal(0f, velocity.X, 5);
    }

    [Fact]
    public void Collision_ShouldStepUpSingleTileWhenEnabled()
    {
        var world = new World(64, 64);
        world.Set(8, 5, 1);
        world.Set(8, 4, 1);

        Vector2 position = new Vector2(7 * 16f - 1f, 5 * 16f - 16f);
        Vector2 velocity = new Vector2(10f, 0f);
        var options = new CollisionOptions { TileCollide = true, StepUp = true };

        Collision.Move(world, ref position, ref velocity, 16, 16, in options, out CollisionResult result);

        Assert.True(position.Y < 5 * 16f - 16f);
        Assert.True(position.X > 7 * 16f - 1f);
    }

    [Fact]
    public void Collision_ShouldAllowDownwardPassThroughOneWayPlatform()
    {
        var world = new World(64, 64);
        world.Set(8, 5, 9);

        Vector2 position = new Vector2(8 * 16f, 4 * 16f);
        Vector2 velocity = new Vector2(0f, 20f);
        var options = new CollisionOptions { TileCollide = true, IgnorePlatforms = true };

        Collision.Move(world, ref position, ref velocity, 16, 16, in options, out CollisionResult result);

        Assert.True(position.Y > 4 * 16f);
    }

    [Fact]
    public void Collision_ShouldResolveSlopeFloorHeightAheadOfLocalX()
    {
        var world = new World(64, 64);
        int x = 4, y = 4;
        world.Set(x, y, 1);
        world.Flags[world.ToIndex(x, y)] = TileFlags.SetSlope(0, 1);

        float floorY = Collision.SlopeFloorY(world, x, y, 8f);

        Assert.True(floorY >= y * 16f && floorY <= (y + 1) * 16f);
        Assert.True(float.IsFinite(floorY));
    }
}
