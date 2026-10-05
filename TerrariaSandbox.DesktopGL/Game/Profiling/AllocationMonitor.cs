namespace TerrariaSandbox.DesktopGL.Game.Profiling;

/// <summary>
/// Simple allocation monitor used to keep a regression budget and observe peak allocations.
/// </summary>
public sealed class AllocationMonitor
{
    public long MaxAllocatedBytes { get; private set; }
    public long ObservedPeakBytes { get; private set; }

    public void RecordSample(long allocatedBytes)
    {
        if (allocatedBytes < 0)
        {
            allocatedBytes = 0;
        }

        MaxAllocatedBytes = Math.Max(MaxAllocatedBytes, allocatedBytes);
        ObservedPeakBytes = Math.Max(ObservedPeakBytes, allocatedBytes);
    }
}
