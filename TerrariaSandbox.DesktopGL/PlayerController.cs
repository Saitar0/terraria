using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Converts raw input into a physics intent so input and simulation stay decoupled.
/// </summary>
public static class PlayerController
{
    public static PlayerIntent Update(in InputState input)
    {
        return new PlayerIntent
        {
            Left = input.Left,
            Right = input.Right,
            Jump = input.Jump,
            Down = input.Down
        };
    }
}
