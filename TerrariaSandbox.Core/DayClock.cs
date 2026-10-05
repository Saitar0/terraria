using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Tracks a simplified 24-hour clock with smooth sky interpolation.
/// </summary>
public sealed class DayClock
{
    public const long DayTicks = 54000L;
    public const long NightTicks = 32400L;
    public const long TotalTicks = DayTicks + NightTicks;

    public long Tick;

    public float DayFraction
    {
        get
        {
            return (Tick % TotalTicks) / (float)TotalTicks;
        }
    }

    public Color SkyColor()
    {
        float time = DayFraction;

        Color night = new Color(10, 16, 30);
        Color dawn = new Color(110, 120, 180);
        Color day = new Color(120, 200, 255);
        Color noon = new Color(214, 236, 255);
        Color dusk = new Color(255, 167, 92);
        Color night2 = new Color(26, 34, 66);

        if (time < 0.18f)
        {
            return InterpolateColor(night, night2, time / 0.18f);
        }

        if (time < 0.30f)
        {
            return InterpolateColor(night2, dawn, (time - 0.18f) / (0.30f - 0.18f));
        }

        if (time < 0.58f)
        {
            return InterpolateColor(dawn, day, (time - 0.30f) / (0.58f - 0.30f));
        }

        if (time < 0.68f)
        {
            return InterpolateColor(day, noon, (time - 0.58f) / (0.68f - 0.58f));
        }

        if (time < 0.85f)
        {
            return InterpolateColor(noon, dusk, (time - 0.68f) / (0.85f - 0.68f));
        }

        return InterpolateColor(dusk, night, (time - 0.85f) / (1f - 0.85f));
    }

    private static Color InterpolateColor(Color a, Color b, float t)
    {
        float clamped = Math.Clamp(t, 0f, 1f);
        return new Color(
            (byte)Math.Round(MathHelper.Lerp(a.R, b.R, clamped)),
            (byte)Math.Round(MathHelper.Lerp(a.G, b.G, clamped)),
            (byte)Math.Round(MathHelper.Lerp(a.B, b.B, clamped)),
            (byte)255);
    }
}
