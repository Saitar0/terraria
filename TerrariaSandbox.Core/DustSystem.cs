using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Fixed-capacity dust pool used for impact sparks and death effects.
/// </summary>
public struct Dust
{
    public Vector2 Position;
    public Vector2 Velocity;
    public int Life;
    public int MaxLife;
    public int Type;
    public byte R;
    public byte G;
    public byte B;
    public byte A;
    public bool Active;
}

public sealed class DustSystem
{
    private readonly Pool<Dust> _dust;

    public DustSystem(int capacity)
    {
        _dust = new Pool<Dust>(capacity);
    }

    public Pool<Dust> Pool => _dust;

    public int Spawn(Vector2 position, Vector2 velocity, Color color, int life, int type = 0)
    {
        Dust dust = new()
        {
            Position = position,
            Velocity = velocity,
            Life = life,
            MaxLife = Math.Max(1, life),
            Type = type,
            R = color.R,
            G = color.G,
            B = color.B,
            A = color.A,
            Active = true
        };

        return _dust.Spawn(dust);
    }

    public void Update(float deltaSeconds)
    {
        for (int i = 0; i < _dust.Capacity; ++i)
        {
            if (!_dust.IsActive(i))
            {
                continue;
            }

            ref Dust dust = ref _dust[i];
            dust.Position += dust.Velocity * deltaSeconds * 60f;
            dust.Velocity *= 0.94f;
            dust.Life--;
            if (dust.Life <= 0)
            {
                _dust.Despawn(i);
            }
        }
    }
}
