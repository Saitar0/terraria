using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

public enum LightingMode
{
    Color,
    White,
    Retro
}

public struct RectI
{
    public RectI(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public int Right => Left + Width;
    public int Bottom => Top + Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Include(int x, int y)
    {
        int left = Math.Min(Left, x);
        int top = Math.Min(Top, y);
        int right = Math.Max(Right, x + 1);
        int bottom = Math.Max(Bottom, y + 1);
        Left = left;
        Top = top;
        Width = right - left;
        Height = bottom - top;
    }
}

public readonly struct SunInfo
{
    public SunInfo(float r, float g, float b, bool active)
    {
        R = r;
        G = g;
        B = b;
        Active = active;
    }

    public float R { get; }
    public float G { get; }
    public float B { get; }
    public bool Active { get; }
}

public readonly struct LightEmitter
{
    public LightEmitter(int x, int y, float r, float g, float b)
    {
        X = x;
        Y = y;
        R = r;
        G = g;
        B = b;
    }

    public int X { get; }
    public int Y { get; }
    public float R { get; }
    public float G { get; }
    public float B { get; }
}

public struct LightCell
{
    public LightCell(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
    }

    public float R;
    public float G;
    public float B;
}

public sealed class LightMap
{
    private const int DefaultMargin = 32;

    private float[] _red;
    private float[] _green;
    private float[] _blue;
    private int _width;
    private int _height;
    private int _margin;

    public LightMap(int width, int height, int margin = DefaultMargin)
    {
        _margin = Math.Max(0, margin);
        _width = Math.Max(1, width + _margin * 2);
        _height = Math.Max(1, height + _margin * 2);
        _red = GC.AllocateUninitializedArray<float>(_width * _height);
        _green = GC.AllocateUninitializedArray<float>(_width * _height);
        _blue = GC.AllocateUninitializedArray<float>(_width * _height);
    }

    public int Width => _width;
    public int Height => _height;
    public int Margin => _margin;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(int x, int y)
    {
        return (uint)x < (uint)_width && (uint)y < (uint)_height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        int targetWidth = Math.Max(_width, width);
        int targetHeight = Math.Max(_height, height);

        if (targetWidth == _width && targetHeight == _height)
        {
            return;
        }

        int length = targetWidth * targetHeight;
        float[] newRed = GC.AllocateUninitializedArray<float>(length);
        float[] newGreen = GC.AllocateUninitializedArray<float>(length);
        float[] newBlue = GC.AllocateUninitializedArray<float>(length);

        Array.Copy(_red, newRed, Math.Min(_red.Length, newRed.Length));
        Array.Copy(_green, newGreen, Math.Min(_green.Length, newGreen.Length));
        Array.Copy(_blue, newBlue, Math.Min(_blue.Length, newBlue.Length));

        _width = targetWidth;
        _height = targetHeight;
        _red = newRed;
        _green = newGreen;
        _blue = newBlue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_red, 0, _red.Length);
        Array.Clear(_green, 0, _green.Length);
        Array.Clear(_blue, 0, _blue.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y, float r, float g, float b)
    {
        if (!IsInside(x, y))
        {
            return;
        }

        int index = y * _width + x;
        _red[index] = r;
        _green[index] = g;
        _blue[index] = b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Max(int x, int y, float r, float g, float b)
    {
        if (!IsInside(x, y))
        {
            return;
        }

        int index = y * _width + x;
        _red[index] = Math.Max(_red[index], r);
        _green[index] = Math.Max(_green[index], g);
        _blue[index] = Math.Max(_blue[index], b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LightCell Get(int x, int y)
    {
        if (!IsInside(x, y))
        {
            return default;
        }

        int index = y * _width + x;
        return new LightCell(_red[index], _green[index], _blue[index]);
    }
}

public sealed class LightingEngine : IWorldListener
{
    public const float MinimumAmbient = 0.12f;

    private const int DirtyRadius = 32;
    private const float Epsilon = 0.03f;

    private readonly LightMap _map;
    private readonly LightEmitter[] _emitters;
    private readonly float[] _sunColor = new float[3];
    private LightCell _cachedCell;
    private int _emitterCount;
    private RectI _dirtyRect;
    private bool _dirty;
    private bool _mutableEmitterDirty;

    public LightingEngine(int maxEmitters = 128, int viewportWidth = 1280, int viewportHeight = 720)
    {
        _map = new LightMap(viewportWidth, viewportHeight, DirtyRadius);
        _emitters = new LightEmitter[maxEmitters];
        AmbientLight = MinimumAmbient;
        Brightness = 1f;
        Gamma = 1f;
        Sun = new SunInfo(0.15f, 0.2f, 0.4f, false);
    }

    public LightingMode Mode { get; set; } = LightingMode.Color;
    public World? World { get; set; }
    public SunInfo Sun { get; set; }
    public float AmbientLight { get; set; }
    public float Brightness { get; set; }
    public float Gamma { get; set; }

    public bool IsDirty => _dirty || _mutableEmitterDirty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddEmitter(in LightEmitter emitter)
    {
        if (_emitterCount >= _emitters.Length)
        {
            return;
        }

        _emitters[_emitterCount++] = emitter;
        _mutableEmitterDirty = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearEmitters()
    {
        _emitterCount = 0;
        _mutableEmitterDirty = true;
    }

    public ref readonly LightCell Get(int x, int y)
    {
        int localX = x + _map.Margin;
        int localY = y + _map.Margin;
        _cachedCell = _map.Get(localX, localY);
        return ref _cachedCell;
    }

    public Color GetSample(int worldX, int worldY)
    {
        LightCell cell = Get(worldX, worldY);
        float red = Exposure(cell.R);
        float green = Exposure(cell.G);
        float blue = Exposure(cell.B);

        return new Color(
            (byte)ClampChannel(red),
            (byte)ClampChannel(green),
            (byte)ClampChannel(blue),
            (byte)255);
    }

    public void Update(World w, RectI view, in SunInfo sun)
    {
        if (w is null || view.Width <= 0 || view.Height <= 0)
        {
            return;
        }

        if (!IsDirty && _emitterCount == 0 && !sun.Active)
        {
            return;
        }

        _map.Resize(view.Width + _map.Margin * 2, view.Height + _map.Margin * 2);
        _map.Clear();

        ApplySunlight(w, view, in sun);
        ApplyEmitters(w, view);
        Propagate(w, view);

        _dirty = false;
        _mutableEmitterDirty = false;
    }

    public void OnTileChanged(int x, int y)
    {
        _dirty = true;

        if (_dirtyRect.Width == 0)
        {
            _dirtyRect = new RectI(x - DirtyRadius, y - DirtyRadius, DirtyRadius * 2 + 1, DirtyRadius * 2 + 1);
            return;
        }

        int left = Math.Min(_dirtyRect.Left, x - DirtyRadius);
        int top = Math.Min(_dirtyRect.Top, y - DirtyRadius);
        int right = Math.Max(_dirtyRect.Right, x + DirtyRadius + 1);
        int bottom = Math.Max(_dirtyRect.Bottom, y + DirtyRadius + 1);
        _dirtyRect = new RectI(left, top, right - left, bottom - top);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplySunlight(World w, RectI view, in SunInfo sun)
    {
        if (!sun.Active)
        {
            return;
        }

        _sunColor[0] = sun.R;
        _sunColor[1] = sun.G;
        _sunColor[2] = sun.B;

        int minX = Math.Max(0, view.Left);
        int maxX = Math.Min(w.Width - 1, view.Right);
        int minY = Math.Max(0, view.Top);
        int maxY = Math.Min(w.Height - 1, view.Bottom);

        for (int worldX = minX; worldX <= maxX; ++worldX)
        {
            int localX = worldX - view.Left + _map.Margin;
            int firstBlockedY = -1;

            for (int worldY = minY; worldY <= maxY; ++worldY)
            {
                int localY = worldY - view.Top + _map.Margin;
                if (IsBlocked(w, worldX, worldY))
                {
                    firstBlockedY = worldY;
                    break;
                }

                _map.Max(localX, localY, sun.R, sun.G, sun.B);
            }

            if (firstBlockedY >= 0)
            {
                for (int worldY = firstBlockedY + 1; worldY <= maxY; ++worldY)
                {
                    float decay = 0.22f + ((worldY - firstBlockedY) * 0.06f);
                    if (decay > 1f)
                    {
                        decay = 1f;
                    }

                    int localY = worldY - view.Top + _map.Margin;
                    _map.Max(localX, localY, sun.R * decay, sun.G * decay, sun.B * decay);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyEmitters(World w, RectI view)
    {
        for (int i = 0; i < _emitterCount; ++i)
        {
            var emitter = _emitters[i];
            if (emitter.R <= 0f && emitter.G <= 0f && emitter.B <= 0f)
            {
                continue;
            }

            int centerX = emitter.X - view.Left + _map.Margin;
            int centerY = emitter.Y - view.Top + _map.Margin;
            int radius = 5;

            for (int dy = -radius; dy <= radius; ++dy)
            {
                int sampleY = centerY + dy;
                if ((uint)sampleY >= (uint)_map.Height)
                {
                    continue;
                }

                for (int dx = -radius; dx <= radius; ++dx)
                {
                    int sampleX = centerX + dx;
                    if ((uint)sampleX >= (uint)_map.Width)
                    {
                        continue;
                    }

                    int dist = dx * dx + dy * dy;
                    if (dist > radius * radius)
                    {
                        continue;
                    }

                    float attenuation = 1f - (dist / (float)(radius * radius + 1));
                    float r = emitter.R * attenuation;
                    float g = emitter.G * attenuation;
                    float b = emitter.B * attenuation;
                    _map.Max(sampleX, sampleY, r, g, b);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Propagate(World w, RectI view)
    {
        for (int iteration = 0; iteration < 2; ++iteration)
        {
            SweepDirectional(w, view, 1, 0);
            SweepDirectional(w, view, -1, 0);
            SweepDirectional(w, view, 0, 1);
            SweepDirectional(w, view, 0, -1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SweepDirectional(World w, RectI view, int dx, int dy)
    {
        int xStart = dx > 0 ? 1 : _map.Width - 2;
        int xEnd = dx > 0 ? _map.Width - 1 : 0;
        int xStep = dx > 0 ? 1 : -1;

        int yStart = dy > 0 ? 1 : _map.Height - 2;
        int yEnd = dy > 0 ? _map.Height - 1 : 0;
        int yStep = dy > 0 ? 1 : -1;

        for (int localY = yStart; (dy > 0 && localY <= yEnd) || (dy < 0 && localY >= yEnd); localY += yStep)
        {
            for (int localX = xStart; (dx > 0 && localX <= xEnd) || (dx < 0 && localX >= xEnd); localX += xStep)
            {
                int sourceX = localX - dx;
                int sourceY = localY - dy;
                if ((uint)sourceX >= (uint)_map.Width || (uint)sourceY >= (uint)_map.Height)
                {
                    continue;
                }

                LightCell source = _map.Get(sourceX, sourceY);
                if (source.R <= Epsilon && source.G <= Epsilon && source.B <= Epsilon)
                {
                    continue;
                }

                int worldX = view.Left + localX - _map.Margin;
                int worldY = view.Top + localY - _map.Margin;
                if (!w.InBounds(worldX, worldY))
                {
                    continue;
                }

                float decayR;
                float decayG;
                float decayB;
                GetDecay(w, worldX, worldY, out decayR, out decayG, out decayB);

                float candidateR = source.R * decayR;
                float candidateG = source.G * decayG;
                float candidateB = source.B * decayB;

                LightCell current = _map.Get(localX, localY);
                if (candidateR > current.R)
                {
                    _map.Set(localX, localY, candidateR, current.G, current.B);
                }

                if (candidateG > current.G)
                {
                    _map.Set(localX, localY, _map.Get(localX, localY).R, candidateG, _map.Get(localX, localY).B);
                }

                if (candidateB > current.B)
                {
                    var cell = _map.Get(localX, localY);
                    _map.Set(localX, localY, cell.R, cell.G, candidateB);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float Exposure(float light)
    {
        float ambient = Math.Clamp(AmbientLight, MinimumAmbient, 1f);
        float baseValue = Math.Max(ambient, light);
        float adjusted = baseValue * Brightness;
        adjusted = Math.Clamp(adjusted, 0f, 1f);

        if (Gamma <= 0.0001f)
        {
            return adjusted;
        }

        return MathF.Pow(adjusted, 1f / Gamma);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ClampChannel(float value)
    {
        return (byte)Math.Clamp(MathF.Round(value * 255f), 0f, 255f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GetDecay(World w, int worldX, int worldY, out float r, out float g, out float b)
    {
        r = 0.91f;
        g = 0.91f;
        b = 0.91f;

        if (!w.InBounds(worldX, worldY))
        {
            return;
        }

        ushort tile = w.Get(worldX, worldY);
        int wallIndex = w.ToIndex(worldX, worldY);
        ushort wall = w.Walls[wallIndex];

        if (wall != 0 && TileDefs.GetWall(wall).Opaque)
        {
            r = 0.86f;
            g = 0.86f;
            b = 0.86f;
            return;
        }

        if (tile == 0)
        {
            return;
        }

        TileDef def = TileDefs.GetTile(tile);
        if (def.Solid)
        {
            r = 0.56f;
            g = 0.56f;
            b = 0.56f;
            return;
        }

        if (tile == (ushort)TileType.Water)
        {
            r = 0.82f;
            g = 0.90f;
            b = 0.96f;
            return;
        }

        if (tile == (ushort)TileType.Wood)
        {
            r = 0.91f;
            g = 0.91f;
            b = 0.91f;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBlocked(World w, int x, int y)
    {
        if (!w.InBounds(x, y))
        {
            return true;
        }

        ushort tile = w.Get(x, y);
        TileDef def = TileDefs.GetTile(tile);
        if (def.Solid)
        {
            return true;
        }

        int wallIndex = w.ToIndex(x, y);
        ushort wall = w.Walls[wallIndex];
        return wall != 0 && TileDefs.GetWall(wall).Opaque;
    }
}
