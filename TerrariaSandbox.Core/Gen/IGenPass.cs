namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// A single deterministic world-generation pass.
/// </summary>
public interface IGenPass
{
    string Name { get; }
    float Weight { get; }
    void Run(WorldGenContext ctx, IProgress<float> p);
}
