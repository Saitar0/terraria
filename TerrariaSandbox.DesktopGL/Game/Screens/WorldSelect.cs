using System.Security.Cryptography;
using System.Text;

namespace TerrariaSandbox.DesktopGL.Game.Screens;

/// <summary>
/// World selection, creation and deterministic generation configuration.
/// </summary>
public sealed class WorldSelect : ScreenBase
{
    public string WorldName { get; set; } = "New World";
    public string CharacterName { get; set; } = "Aria";
    public int Seed { get; set; } = 1337;
    public WorldSize WorldSize { get; set; } = WorldSize.Medium;
    public int GenerationProgress { get; set; }
    public bool IsGenerating { get; set; }

    public string CreateWorldSignature()
    {
        string payload = $"{WorldName}|{CharacterName}|{Seed}|{WorldSize}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash)[..16];
    }

    public string SerializeState()
    {
        return $"{WorldName};{CharacterName};{Seed};{(int)WorldSize};{GenerationProgress};{IsGenerating}";
    }

    public static WorldSelect DeserializeState(string payload)
    {
        string[] parts = payload.Split(';', StringSplitOptions.None);
        if (parts.Length < 6)
        {
            return new WorldSelect();
        }

        return new WorldSelect
        {
            WorldName = parts[0],
            CharacterName = parts[1],
            Seed = int.TryParse(parts[2], out int seed) ? seed : 1337,
            WorldSize = Enum.TryParse<WorldSize>(parts[3], out var size) ? size : WorldSize.Medium,
            GenerationProgress = int.TryParse(parts[4], out int progress) ? progress : 0,
            IsGenerating = bool.TryParse(parts[5], out bool generating) && generating
        };
    }
}

public enum WorldSize
{
    Small,
    Medium,
    Large,
    Huge
}
