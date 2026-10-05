using Microsoft.Xna.Framework.Input;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Input snapshot captured from the keyboard and translated to player intentions.
/// </summary>
public struct InputState
{
    public bool Left;
    public bool Right;
    public bool Jump;
    public bool Down;

    public static InputState FromKeyboard(KeyboardState keyboard)
    {
        return new InputState
        {
            Left = keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left),
            Right = keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right),
            Jump = keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.Space),
            Down = keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)
        };
    }
}
