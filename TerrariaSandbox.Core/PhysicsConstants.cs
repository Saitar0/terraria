namespace TerrariaSandbox.Core;

/// <summary>
/// Fixed-step physics tuning used by the player simulation.
/// </summary>
public struct PhysicsConstants
{
    public float Gravity;
    public float MaxFall;
    public float JumpSpeed;
    public float JumpHoldTicks;
    public float RunAccel;
    public float MaxRun;
    public float Friction;
    public float IceFriction;
    public int CoyoteTimeTicks;
    public int JumpBufferTicks;

    public static readonly PhysicsConstants Default = new()
    {
        Gravity = 0.4f,
        MaxFall = 10f,
        JumpSpeed = 5.01f,
        JumpHoldTicks = 15f,
        RunAccel = 0.08f,
        MaxRun = 3f,
        Friction = 0.2f,
        IceFriction = 0.05f,
        CoyoteTimeTicks = 6,
        JumpBufferTicks = 6
    };
}
