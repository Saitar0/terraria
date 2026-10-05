using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Lightweight gore pool with fade-out behavior and no per-frame allocations.
/// </summary>
public struct Gore
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float Scale;
    public int Life;
    public int MaxLife;
    public int Type;
    public Color Color;
    public bool Active;
}

public sealed class GoreSystem
{
    private readonly Pool<Gore> _gore;

    public GoreSystem(int capacity)
    {
        _gore = new Pool<Gore>(capacity);
    }

    public Pool<Gore> Pool => _gore;

    public int Spawn(Vector2 position, Vector2 velocity, float rotation, float scale, Color color, int life, int type = 0)
    {
        Gore gore = new()
        {
            Position = position,
            Velocity = velocity,
            Rotation = rotation,
            Scale = scale,
            Life = life,
            MaxLife = Math.Max(1, life),
            Type = type,
            Color = color,
            Active = true
        };

        return _gore.Spawn(gore);
    }

    public void Update(float deltaSeconds)
    {
        for (int i = 0; i < _gore.Capacity; ++i)
        {
            if (!_gore.IsActive(i))
            {
                continue;
            }

            ref Gore gore = ref _gore[i];
            gore.Position += gore.Velocity * deltaSeconds * 60f;
            gore.Velocity *= 0.96f;
            gore.Life--;
            if (gore.Life <= 0)
            {
                _gore.Despawn(i);
            }
        }
    }
}
