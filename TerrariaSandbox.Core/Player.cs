using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Holds the player intention produced each tick from input.
/// </summary>
public struct PlayerIntent
{
    public bool Left;
    public bool Right;
    public bool Jump;
    public bool Down;
}

/// <summary>
/// A fixed-step player controller state. This type is intentionally stack-based to avoid allocations.
/// </summary>
public struct Player
{
    public const float DefaultWidth = 20f;
    public const float DefaultHeight = 42f;
    public const float GroundLevel = 360f;

    public Vector2 Position;
    public Vector2 Velocity;
    public float Width;
    public float Height;
    public int Direction;
    public bool OnGround;
    public bool Idle;
    public bool Running;
    public bool Jumping;
    public bool Falling;
    public int JumpHoldTimer;
    public int CoyoteTimer;
    public int JumpBufferTimer;
    public int Tick;

    public Player(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Width = DefaultWidth;
        Height = DefaultHeight;
        Direction = 1;
        OnGround = false;
        Idle = true;
        Running = false;
        Jumping = false;
        Falling = false;
        JumpHoldTimer = 0;
        CoyoteTimer = 0;
        JumpBufferTimer = 0;
        Tick = 0;
    }

    public static void ApplyVelocity(ref Vector2 position, ref Vector2 velocity)
    {
        position += velocity;
    }

    public void Step(in PlayerIntent intent, in PhysicsConstants constants, World? world = null)
    {
        Tick++;

        float moveAxis = 0f;

        if (intent.Left && !intent.Right)
        {
            moveAxis = -1f;
            Direction = -1;
        }
        else if (intent.Right && !intent.Left)
        {
            moveAxis = 1f;
            Direction = 1;
        }

        if (intent.Jump)
        {
            JumpBufferTimer = constants.JumpBufferTicks;
        }
        else if (JumpBufferTimer > 0)
        {
            JumpBufferTimer--;
        }

        if (OnGround)
        {
            CoyoteTimer = constants.CoyoteTimeTicks;
        }
        else if (CoyoteTimer > 0)
        {
            CoyoteTimer--;
        }

        if (moveAxis != 0f)
        {
            float target = moveAxis * constants.MaxRun;
            float delta = target - Velocity.X;
            if (MathF.Abs(delta) <= constants.RunAccel)
            {
                Velocity.X = target;
            }
            else
            {
                Velocity.X += MathF.Sign(delta) * constants.RunAccel;
            }
        }
        else if (OnGround)
        {
            if (Velocity.X > 0f)
            {
                Velocity.X = MathF.Max(0f, Velocity.X - constants.Friction);
            }
            else if (Velocity.X < 0f)
            {
                Velocity.X = MathF.Min(0f, Velocity.X + constants.Friction);
            }
        }

        if (JumpBufferTimer > 0 && (OnGround || CoyoteTimer > 0))
        {
            Velocity.Y = -constants.JumpSpeed;
            OnGround = false;
            JumpHoldTimer = (int)constants.JumpHoldTicks;
            JumpBufferTimer = 0;
            CoyoteTimer = 0;
        }
        else if (JumpHoldTimer > 0 && intent.Jump && Velocity.Y < 0f)
        {
            Velocity.Y -= 0.16f;
            JumpHoldTimer--;
        }
        else if (JumpHoldTimer > 0)
        {
            JumpHoldTimer = 0;
        }

        if (!OnGround)
        {
            Velocity.Y += constants.Gravity;
        }

        Velocity.X = Math.Clamp(Velocity.X, -constants.MaxRun, constants.MaxRun);
        Velocity.Y = Math.Clamp(Velocity.Y, -constants.MaxFall, constants.MaxFall);

        if (world is not null)
        {
            var options = new CollisionOptions
            {
                IgnorePlatforms = intent.Down,
                StepUp = true,
                TileCollide = true
            };

            Collision.Move(world, ref Position, ref Velocity, (int)MathF.Ceiling(Width), (int)MathF.Ceiling(Height), in options, out CollisionResult result);
            OnGround = result.OnGround;
            if (result.InLiquid)
            {
                if (result.LiquidType == 1)
                {
                    Velocity.Y = Math.Clamp(Velocity.Y, -constants.MaxFall * 0.6f, constants.MaxFall * 0.6f);
                    Velocity.X = Math.Clamp(Velocity.X, -constants.MaxRun * 0.45f, constants.MaxRun * 0.45f);
                }
                else if (result.LiquidType == 2)
                {
                    Velocity.Y = Math.Clamp(Velocity.Y, -constants.MaxFall * 0.2f, constants.MaxFall * 0.2f);
                    Velocity.X = Math.Clamp(Velocity.X, -constants.MaxRun * 0.25f, constants.MaxRun * 0.25f);
                }
            }
        }
        else
        {
            ApplyVelocity(ref Position, ref Velocity);

            if (Position.Y + Height >= GroundLevel && Velocity.Y >= 0f)
            {
                Position.Y = GroundLevel - Height;
                Velocity.Y = 0f;
                OnGround = true;
                JumpHoldTimer = 0;
            }
            else
            {
                OnGround = false;
            }
        }

        Idle = OnGround && moveAxis == 0f && MathF.Abs(Velocity.X) < 0.1f;
        Running = OnGround && moveAxis != 0f && MathF.Abs(Velocity.X) > 0.1f;
        Jumping = !OnGround && Velocity.Y < 0f;
        Falling = !OnGround && Velocity.Y >= 0f;
    }
}
