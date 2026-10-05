using Microsoft.Xna.Framework.Audio;

namespace TerrariaSandbox.DesktopGL.Game.Audio;

/// <summary>
/// Combines category volumes and simple 3D attenuation for procedural audio.
/// </summary>
public sealed class AudioManager
{
    private readonly List<SoundEffectInstance> _activeInstances = new();

    public float MasterVolume { get; set; } = 1f;
    public float MusicVolume { get; set; } = 0.8f;
    public float SfxVolume { get; set; } = 0.9f;
    public float AmbientVolume { get; set; } = 0.6f;

    public void Update(float listenerX, float listenerY, float sourceX, float sourceY)
    {
        float dx = sourceX - listenerX;
        float dy = sourceY - listenerY;
        float distance = MathF.Sqrt(dx * dx + dy * dy);
        float attenuation = MathF.Max(0f, 1f - distance / 1000f);

        foreach (SoundEffectInstance instance in _activeInstances)
        {
            instance.Volume = attenuation * MasterVolume * SfxVolume;
        }
    }

    public SoundEffectInstance Play(SoundEffect effect)
    {
        SoundEffectInstance instance = effect.CreateInstance();
        instance.Volume = MasterVolume * SfxVolume;
        instance.Play();
        _activeInstances.Add(instance);
        return instance;
    }

    public void StopExpired()
    {
        for (int i = _activeInstances.Count - 1; i >= 0; i--)
        {
            if (_activeInstances[i].State == SoundState.Stopped)
            {
                _activeInstances.RemoveAt(i);
            }
        }
    }
}
