using System.Runtime.CompilerServices;

namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Deterministic 1D/2D noise primitives used by the terrain generator.
/// </summary>
public sealed class Noise
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Perlin1D(float x, uint seed)
    {
        int x0 = (int)MathF.Floor(x);
        float xf = x - x0;
        float u = Fade(xf);

        float a = Gradient1D(Hash1D(x0, seed), xf);
        float b = Gradient1D(Hash1D(x0 + 1, seed), xf - 1f);

        return Lerp(a, b, u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Perlin2D(float x, float y, uint seed)
    {
        int x0 = (int)MathF.Floor(x);
        int y0 = (int)MathF.Floor(y);
        float xf = x - x0;
        float yf = y - y0;
        float u = Fade(xf);
        float v = Fade(yf);

        float n00 = Gradient2D(Hash2D(x0, y0, seed), xf, yf);
        float n10 = Gradient2D(Hash2D(x0 + 1, y0, seed), xf - 1f, yf);
        float n01 = Gradient2D(Hash2D(x0, y0 + 1, seed), xf, yf - 1f);
        float n11 = Gradient2D(Hash2D(x0 + 1, y0 + 1, seed), xf - 1f, yf - 1f);

        float x1 = Lerp(n00, n10, u);
        float x2 = Lerp(n01, n11, u);
        return Lerp(x1, x2, v) * 0.8f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Simplex1D(float x, uint seed)
    {
        return Perlin1D(x, seed) + 0.35f * Perlin1D(x * 2.17f, seed + 17u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Simplex2D(float x, float y, uint seed)
    {
        return Perlin2D(x, y, seed) + 0.35f * Perlin2D(x * 2.17f, y * 2.17f, seed + 17u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Fbm1D(float x, int octaves, float persistence, float lacunarity, uint seed)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float total = 0f;
        float normalizer = 0f;

        for (int octave = 0; octave < octaves; ++octave)
        {
            total += Perlin1D(x * frequency, seed + (uint)octave * 997u) * amplitude;
            normalizer += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / MathF.Max(0.0001f, normalizer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Fbm2D(float x, float y, int octaves, float persistence, float lacunarity, uint seed)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float total = 0f;
        float normalizer = 0f;

        for (int octave = 0; octave < octaves; ++octave)
        {
            total += Perlin2D(x * frequency, y * frequency, seed + (uint)octave * 997u) * amplitude;
            normalizer += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / MathF.Max(0.0001f, normalizer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Ridge1D(float x, int octaves, float persistence, float lacunarity, uint seed)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float normalizer = 0f;

        for (int octave = 0; octave < octaves; ++octave)
        {
            float sample = 1f - MathF.Abs(Perlin1D(x * frequency, seed + (uint)octave * 997u));
            total += sample * amplitude;
            normalizer += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / MathF.Max(0.0001f, normalizer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Ridge2D(float x, float y, int octaves, float persistence, float lacunarity, uint seed)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float normalizer = 0f;

        for (int octave = 0; octave < octaves; ++octave)
        {
            float sample = 1f - MathF.Abs(Perlin2D(x * frequency, y * frequency, seed + (uint)octave * 997u));
            total += sample * amplitude;
            normalizer += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / MathF.Max(0.0001f, normalizer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash1D(int x, uint seed)
    {
        uint value = (uint)(x * 374761393) ^ seed;
        value = (value ^ (value >> 13)) * 127773U;
        value ^= value >> 7;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Hash2D(int x, int y, uint seed)
    {
        uint value = ((uint)x * 374761393U) ^ ((uint)y * 668265263U) ^ seed;
        value = (value ^ (value >> 13)) * 127773U;
        value ^= value >> 7;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Gradient1D(uint hash, float x)
    {
        int h = (int)(hash & 0xF);
        float value = (h & 1) == 0 ? 1f : -1f;
        return value * x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Gradient2D(uint hash, float x, float y)
    {
        int h = (int)(hash & 7);
        float u = (h < 4) ? x : y;
        float v = (h < 4) ? y : x;
        return (((h & 1) == 0) ? u : -u) + (((h & 2) == 0) ? v : -v);
    }
}
