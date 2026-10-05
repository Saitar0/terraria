namespace TerrariaSandbox.DesktopGL.Game.Audio;

/// <summary>
/// Very small procedural music loop generator for ambient biome themes.
/// </summary>
public sealed class MusicSynth
{
    private readonly int _sampleRate;
    private readonly double _tempo;

    public MusicSynth(int sampleRate = 44100, double tempo = 96.0)
    {
        _sampleRate = sampleRate;
        _tempo = tempo;
    }

    public string CurrentBiome { get; set; } = "Forest";
    public int CurrentHour { get; set; } = 8;

    public double[] GenerateLoop(int durationMs)
    {
        int sampleCount = _sampleRate * durationMs / 1000;
        var output = new double[sampleCount];
        double noteTime = 60.0 / _tempo;

        for (int i = 0; i < sampleCount; i++)
        {
            double t = i / (double)_sampleRate;
            double note = (Math.Sin(2.0 * Math.PI * (CurrentHour % 12 + 1) * t / noteTime) +
                Math.Sin(2.0 * Math.PI * (CurrentHour % 9 + 2) * t / noteTime * 0.5)) * 0.3;
            double sway = Math.Sin(2.0 * Math.PI * (0.25 + (CurrentBiome.Length % 7) * 0.1) * t);
            output[i] = note + sway * 0.12;
        }

        return output;
    }
}
