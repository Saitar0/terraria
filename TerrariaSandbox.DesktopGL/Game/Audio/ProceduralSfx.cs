using Microsoft.Xna.Framework.Audio;

namespace TerrariaSandbox.DesktopGL.Game.Audio;

/// <summary>
/// Build simple PCM-driven sound effects procedurally for game actions.
/// </summary>
public static class ProceduralSfx
{
    public static SoundEffect CreateSwing(int sampleRate = 44100, int durationMs = 120)
    {
        return CreateTone(sampleRate, durationMs, 420f, 0.18f, 0.2f, 0.18f, 0.0f);
    }

    public static SoundEffect CreateBlockPlace(int sampleRate = 44100, int durationMs = 180)
    {
        return CreateTone(sampleRate, durationMs, 180f, 0.25f, 0.15f, 0.22f, 0.10f);
    }

    public static SoundEffect CreateJump(int sampleRate = 44100, int durationMs = 170)
    {
        return CreateTone(sampleRate, durationMs, 240f, 0.35f, 0.22f, 0.30f, 0.15f);
    }

    public static SoundEffect CreateDamage(int sampleRate = 44100, int durationMs = 160)
    {
        return CreateTone(sampleRate, durationMs, 120f, 0.4f, 0.16f, 0.35f, 0.18f);
    }

    public static SoundEffect CreateExplosion(int sampleRate = 44100, int durationMs = 400)
    {
        return CreateTone(sampleRate, durationMs, 70f, 0.75f, 0.25f, 0.90f, 0.35f);
    }

    private static SoundEffect CreateTone(int sampleRate, int durationMs, float baseFrequency, float amplitude, float attack, float decay, float noise)
    {
        int sampleCount = sampleRate * durationMs / 1000;
        short[] data = new short[sampleCount];
        double twoPi = Math.PI * 2.0;

        for (int i = 0; i < sampleCount; i++)
        {
            double t = i / (double)sampleRate;
            double envelope = Math.Min(1.0, Math.Max(0.0, i / (double)Math.Max(1, sampleCount * attack))) *
                              Math.Max(0.0, 1.0 - (i / (double)Math.Max(1, sampleCount * decay)));
            double phase = twoPi * baseFrequency * t;
            double sample = Math.Sin(phase) * amplitude * envelope;
            if (noise > 0.0)
            {
                double hiss = ((Math.Sin(phase * 11.0) + Math.Sin(phase * 23.0)) * 0.5) * noise;
                sample += hiss;
            }

            data[i] = (short)Math.Clamp(sample * 32767.0, short.MinValue, short.MaxValue);
        }

        byte[] buffer = new byte[data.Length * sizeof(short)];
        Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);
        return new SoundEffect(buffer, sampleRate, AudioChannels.Mono);
    }
}
