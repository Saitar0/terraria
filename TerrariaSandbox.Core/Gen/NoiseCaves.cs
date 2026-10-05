namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Applies thresholded 2D FBM noise to carve spaghetti caves and chamber-like cavities.
/// </summary>
public sealed class NoiseCaves
{
    public void Apply(World world, int minY, int maxY, uint seed, int surfaceY)
    {
        if (world is null || minY < 0 || maxY <= minY || maxY > world.Height)
        {
            return;
        }

        var noise = new Noise();
        int width = world.Width;
        int height = world.Height;

        Parallel.For(minY, maxY, y =>
        {
            float depth = 1f - Math.Clamp((y - surfaceY) / (float)Math.Max(1, height - surfaceY), 0f, 1f);
            float threshold = 0.22f + depth * 0.10f;
            float chamberThreshold = 0.82f + depth * 0.03f;

            for (int x = 0; x < width; ++x)
            {
                if (world.Get(x, y) == 0)
                {
                    continue;
                }

                float cave = noise.Fbm2D(x * 0.05f, y * 0.05f, 5, 0.56f, 2.12f, seed + (uint)(x * 131 + y * 17));
                float magnitude = MathF.Abs(cave);

                if (magnitude < threshold || cave > chamberThreshold)
                {
                    world.Set(x, y, 0);
                }
            }
        });
    }
}
