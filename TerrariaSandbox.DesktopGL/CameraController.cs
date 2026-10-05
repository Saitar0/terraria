using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Smoothly follows the player while applying a small look-ahead and optional screen shake.
/// </summary>
public sealed class CameraController
{
    private float _shakeAmplitude;
    private int _shakeTicks;
    private int _shakeTimer;
    private float _lastScreenShakeX;
    private float _lastScreenShakeY;

    public void Follow(in Player player, Camera camera, float dt)
    {
        if (camera.ViewportWidth <= 0 || camera.ViewportHeight <= 0)
        {
            return;
        }

        float halfWidth = camera.ViewportWidth / (2f * Math.Max(camera.Zoom, 0.0001f));
        float halfHeight = camera.ViewportHeight / (2f * Math.Max(camera.Zoom, 0.0001f));

        float targetX = player.Position.X + (player.Width * 0.5f) - halfWidth;
        float targetY = player.Position.Y + (player.Height * 0.5f) - halfHeight;

        float lookAhead = player.Velocity.X * 0.18f;
        targetX += lookAhead;

        float maxX = Math.Max(0f, camera.WorldPixelWidth - camera.ViewportWidth / Math.Max(camera.Zoom, 0.0001f));
        float maxY = Math.Max(0f, camera.WorldPixelHeight - camera.ViewportHeight / Math.Max(camera.Zoom, 0.0001f));

        targetX = Math.Clamp(targetX, 0f, maxX);
        targetY = Math.Clamp(targetY, 0f, maxY);

        float response = 10f;
        float smoothing = 1f - MathF.Exp(-response * dt);
        float currentX = MathHelper.Lerp(camera.ScreenPosition.X, targetX, smoothing);
        float currentY = MathHelper.Lerp(camera.ScreenPosition.Y, targetY, smoothing);

        if (_shakeTimer > 0)
        {
            _shakeTimer--;
            float shakeProgress = _shakeTicks > 0 ? (float)(_shakeTicks - _shakeTimer) / _shakeTicks : 1f;
            float shakeStrength = _shakeAmplitude * (1f - Math.Clamp(shakeProgress, 0f, 1f));
            currentX += (float)Math.Sin(_shakeTimer * 0.9f) * shakeStrength;
            currentY += (float)Math.Cos(_shakeTimer * 1.2f) * shakeStrength;
            _lastScreenShakeX = currentX;
            _lastScreenShakeY = currentY;
        }
        else
        {
            _lastScreenShakeX = 0f;
            _lastScreenShakeY = 0f;
        }

        camera.ScreenPosition = new Vector2(
            Math.Clamp(currentX, 0f, maxX),
            Math.Clamp(currentY, 0f, maxY));
    }

    public void Shake(float amplitude, int ticks)
    {
        if (ticks <= 0)
        {
            return;
        }

        _shakeAmplitude = amplitude;
        _shakeTicks = ticks;
        _shakeTimer = ticks;
    }
}
