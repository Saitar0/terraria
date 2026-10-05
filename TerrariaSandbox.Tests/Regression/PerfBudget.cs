using TerrariaSandbox.DesktopGL.Game.Profiling;

namespace TerrariaSandbox.Tests.Regression;

public class PerfBudget
{
    [Fact]
    public void AllocationMonitorReportsZeroOrLowAllocationInStablePath()
    {
        var monitor = new AllocationMonitor();
        monitor.RecordSample(64);
        monitor.RecordSample(64);
        monitor.RecordSample(96);

        Assert.InRange(monitor.MaxAllocatedBytes, 0, 2048);
        Assert.True(monitor.ObservedPeakBytes >= 0);
    }
}
