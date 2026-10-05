namespace TerrariaSandbox.Core.IO;

/// <summary>
/// Background autosave service with snapshot copying to avoid in-flight race conditions.
/// </summary>
public sealed class AutosaveService : IDisposable
{
    private readonly World _world;
    private readonly string _path;
    private readonly TimeSpan _interval;
    private readonly WorldSerializer _serializer = new();
    private readonly Thread _thread;
    private volatile bool _running;

    public AutosaveService(World world, string path, TimeSpan? interval = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _interval = interval ?? TimeSpan.FromSeconds(5);
        _thread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "WorldAutosave"
        };
    }

    public void Start()
    {
        if (_running)
        {
            return;
        }

        _running = true;
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
        _thread.Join(TimeSpan.FromSeconds(2));
    }

    public void Dispose()
    {
        Stop();
    }

    private void RunLoop()
    {
        while (_running)
        {
            Thread.Sleep(_interval);
            World snapshot = CreateSnapshot();
            _serializer.SaveToFile(_path, snapshot);
        }
    }

    private World CreateSnapshot()
    {
        World copy = new World(_world.Width, _world.Height)
        {
            Name = _world.Name,
            Seed = _world.Seed,
            Spawn = _world.Spawn,
            WorldSurface = _world.WorldSurface,
            RockLayer = _world.RockLayer,
            TimeOfDay = _world.TimeOfDay,
            BossFlags = _world.BossFlags
        };

        Array.Copy(_world.Types, copy.Types, _world.Types.Length);
        Array.Copy(_world.Walls, copy.Walls, _world.Walls.Length);
        Array.Copy(_world.Liquid, copy.Liquid, _world.Liquid.Length);
        Array.Copy(_world.LiquidType, copy.LiquidType, _world.LiquidType.Length);
        Array.Copy(_world.Flags, copy.Flags, _world.Flags.Length);
        Array.Copy(_world.FrameX, copy.FrameX, _world.FrameX.Length);
        Array.Copy(_world.FrameY, copy.FrameY, _world.FrameY.Length);
        Array.Copy(_world.Paint, copy.Paint, _world.Paint.Length);

        return copy;
    }
}
