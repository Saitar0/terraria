namespace TerrariaSandbox.DesktopGL.Game.Profiling;

/// <summary>
/// Tracks cumulative frame time for core game systems.
/// </summary>
public sealed class FrameProfiler
{
    private readonly Dictionary<string, double> _systemMs = new(StringComparer.OrdinalIgnoreCase);

    public double TotalFrameMs { get; private set; }

    public IReadOnlyDictionary<string, double> SystemMs => _systemMs;

    public void Record(string systemName, double milliseconds)
    {
        if (string.IsNullOrWhiteSpace(systemName))
        {
            return;
        }

        _systemMs[systemName] = milliseconds;
        TotalFrameMs = _systemMs.Values.Sum();
    }

    public void Reset()
    {
        _systemMs.Clear();
        TotalFrameMs = 0d;
    }
}
