namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Generates worm-like tunnels with a noisy heading and falloff radius.
/// </summary>
public sealed class TileRunner
{
    private readonly World _world;
    private readonly Pcg32 _rng;
    private readonly Noise _noise;

    public TileRunner(World world, Pcg32? rng = null, Noise? noise = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _rng = rng ?? new Pcg32(0u);
        _noise = noise ?? new Noise();
    }

    public static void Run(World world, Pcg32 rng, int x, int y, int strength, int steps, ushort type, bool replaceOnly)
    {
        var runner = new TileRunner(world, rng);
        runner.Run(x, y, strength, steps, type, replaceOnly);
    }

    public void Run(int x, int y, int strength, int steps, ushort type, bool replaceOnly)
    {
        if (strength <= 0 || steps <= 0)
        {
            return;
        }

        float heading = (float)((_rng.NextFloat() * Math.PI * 2.0) - Math.PI);
        int cx = Math.Clamp(x, 0, _world.Width - 1);
        int cy = Math.Clamp(y, 0, _world.Height - 1);
        int maxRadius = Math.Max(1, strength);

        for (int step = 0; step < steps; ++step)
        {
            float drift = _noise.Fbm2D(cx * 0.18f, cy * 0.18f, 3, 0.55f, 2.1f, (uint)(step * 131u + 17u));
            heading += (drift - 0.5f) * 1.6f + (_rng.NextFloat() - 0.5f) * 0.9f;

            float dx = MathF.Cos(heading);
            float dy = MathF.Sin(heading);
            int advanceX = (int)MathF.Round(dx * (2f + strength * 0.15f));
            int advanceY = (int)MathF.Round(dy * (2f + strength * 0.15f));

            cx = Math.Clamp(cx + advanceX, 0, _world.Width - 1);
            cy = Math.Clamp(cy + advanceY, 0, _world.Height - 1);

            int radius = Math.Max(1, maxRadius - (step * maxRadius) / Math.Max(1, steps));

            for (int yy = cy - radius; yy <= cy + radius; ++yy)
            {
                if ((uint)yy >= (uint)_world.Height)
                {
                    continue;
                }

                for (int xx = cx - radius; xx <= cx + radius; ++xx)
                {
                    if ((uint)xx >= (uint)_world.Width)
                    {
                        continue;
                    }

                    int dxSq = xx - cx;
                    int dySq = yy - cy;
                    if ((dxSq * dxSq) + (dySq * dySq) > radius * radius)
                    {
                        continue;
                    }

                    ushort current = _world.Get(xx, yy);
                    if (replaceOnly)
                    {
                        if (type == 0)
                        {
                            if (current != 0)
                            {
                                _world.Set(xx, yy, 0);
                            }
                        }
                        else if (IsGroundTile(current))
                        {
                            _world.Set(xx, yy, type);
                        }
                    }
                    else if (type == 0)
                    {
                        if (current != 0)
                        {
                            _world.Set(xx, yy, 0);
                        }
                    }
                    else if (current == 0 || current == type || IsGroundTile(current))
                    {
                        _world.Set(xx, yy, type);
                    }
                }
            }
        }
    }

    private static bool IsGroundTile(ushort tile)
    {
        return tile == (ushort)TileType.Dirt ||
               tile == (ushort)TileType.Grass ||
               tile == (ushort)TileType.Stone ||
               tile == (ushort)TileType.Sand ||
               tile == (ushort)TileType.Snow ||
               tile == (ushort)TileType.CorruptStone ||
               tile == (ushort)TileType.JungleDirt ||
               tile == (ushort)TileType.Sandstone ||
               tile == (ushort)TileType.Ice;
    }
}
