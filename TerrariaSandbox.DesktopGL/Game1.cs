using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TerrariaSandbox.Core;
using TerrariaSandbox.DesktopGL.Game;
using TerrariaSandbox.DesktopGL.Game.UI;

namespace TerrariaSandbox.DesktopGL;

/// <summary>
/// Sandbox prototype using a chunk-aware world, a smooth camera controller, and layered parallax.
/// </summary>
public sealed class Game1 : Microsoft.Xna.Framework.Game
{
    public static readonly BlendState LightBlend = new()
    {
        ColorSourceBlend = Blend.DestinationColor,
        ColorDestinationBlend = Blend.InverseSourceColor,
        AlphaSourceBlend = Blend.DestinationAlpha,
        AlphaDestinationBlend = Blend.InverseSourceAlpha
    };

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _atlas = null!;
    private Rectangle[] _atlasLookup = Array.Empty<Rectangle>();
    private World _world = null!;
    private Camera _camera = null!;
    private CameraController _cameraController = null!;
    private ParallaxRenderer _parallaxRenderer = null!;
    private DayClock _dayClock = new();
    private TileRenderer _tileRenderer = null!;
    private LiquidRenderer _liquidRenderer = null!;
    private LightingEngine _lightingEngine = null!;
    private LightRenderer _lightRenderer = null!;
    private Player _player;
    private NpcSystem _npcSystem = null!;
    private NpcRenderer _npcRenderer = null!;
    private ProjectileSystem _projectileSystem = null!;
    private ProjectileRenderer _projectileRenderer = null!;
    private SpriteFont? _debugFont;
    private Texture2D _debugPixel = null!;
    private Inventory _inventory = null!;
    private HotbarUI _hotbarUi = null!;
    private InventoryUI _inventoryUi = null!;
    private BossBar _bossBar = null!;
    private TooltipRenderer _tooltipRenderer = null!;
    private KeyboardState _previousKeyboardState;
    private int _previousScrollWheelValue;
    private bool _showDebug;
    private int _tilesDrawn;
    private int _drawCalls;
    private double _drawMilliseconds;
    private double _lightMilliseconds;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        Content.RootDirectory = "Content";
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / WorldConstants.TicksPerSecond);
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
        Window.Title = "TerrariaSandbox";
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugPixel = new Texture2D(GraphicsDevice, 1, 1);
        _debugPixel.SetData(new[] { Color.White });

        _inventory = new Inventory();
        _hotbarUi = new HotbarUI { Scale = 2f, SelectedSlot = 0 };
        _inventoryUi = new InventoryUI(_inventory) { Scale = 2 };
        _bossBar = new BossBar();
        _tooltipRenderer = new TooltipRenderer();

        var atlasBuilder = new AtlasBuilder();
        (_atlas, _atlasLookup) = atlasBuilder.Build(GraphicsDevice);

        _world = new World(WorldConstants.WorldWidthTiles, WorldConstants.WorldHeightTiles);
        _world.GenerateTerrain(1337);

        _camera = new Camera
        {
            ScreenPosition = Vector2.Zero,
            Zoom = 1f,
            ViewportWidth = _graphics.PreferredBackBufferWidth,
            ViewportHeight = _graphics.PreferredBackBufferHeight,
            WorldPixelWidth = WorldConstants.WorldWidthTiles * WorldConstants.TileSizePixels,
            WorldPixelHeight = WorldConstants.WorldHeightTiles * WorldConstants.TileSizePixels
        };

        _cameraController = new CameraController();
        _parallaxRenderer = new ParallaxRenderer(GraphicsDevice);

        _player = new Player
        {
            Position = new Vector2(200f, 180f),
            Velocity = Vector2.Zero,
            Width = Player.DefaultWidth,
            Height = Player.DefaultHeight,
            Direction = 1,
            OnGround = true
        };

        _npcSystem = new NpcSystem(new Pool<Npc>(200), new Random(1337));
        _npcRenderer = new NpcRenderer(GraphicsDevice, new Color(205, 92, 92));
        _projectileSystem = new ProjectileSystem(new Pool<Projectile>(1000), new DustSystem(6000), new GoreSystem(256));
        _projectileRenderer = new ProjectileRenderer(GraphicsDevice);

        _tileRenderer = new TileRenderer(GraphicsDevice);
        _liquidRenderer = new LiquidRenderer(GraphicsDevice);
        _hotbarUi.RefreshLayout(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _inventoryUi.RefreshLayout(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _lightingEngine = new LightingEngine
        {
            AmbientLight = LightingEngine.MinimumAmbient,
            Brightness = 1.0f,
            Gamma = 1.0f,
            World = _world,
            Sun = new SunInfo(0.12f, 0.14f, 0.18f, true)
        };
        _lightRenderer = new LightRenderer(GraphicsDevice, 16)
        {
            UseSmoothMesh = false
        };

        try
        {
            _debugFont = Content.Load<SpriteFont>("DebugFont");
        }
        catch
        {
            _debugFont = null;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (keyboard.IsKeyDown(Keys.F3) && !_previousKeyboardState.IsKeyDown(Keys.F3))
        {
            _showDebug = !_showDebug;
        }

        if (keyboard.IsKeyDown(Keys.L) && !_previousKeyboardState.IsKeyDown(Keys.L))
        {
            _lightingEngine.Mode = _lightingEngine.Mode == LightingMode.Color ? LightingMode.White : LightingMode.Color;
            _lightRenderer.UseSmoothMesh = !_lightRenderer.UseSmoothMesh;
        }

        if (keyboard.IsKeyDown(Keys.OemOpenBrackets) && !_previousKeyboardState.IsKeyDown(Keys.OemOpenBrackets))
        {
            _lightingEngine.Gamma = Math.Clamp(_lightingEngine.Gamma - 0.1f, 0.5f, 2.5f);
        }

        if (keyboard.IsKeyDown(Keys.OemCloseBrackets) && !_previousKeyboardState.IsKeyDown(Keys.OemCloseBrackets))
        {
            _lightingEngine.Gamma = Math.Clamp(_lightingEngine.Gamma + 0.1f, 0.5f, 2.5f);
        }

        AutoTile.ProcessQueue(_world, 256);

        InputState input = InputState.FromKeyboard(keyboard);
        PlayerIntent intent = PlayerController.Update(in input);
        _player.Step(in intent, in PhysicsConstants.Default, _world);
        _npcSystem.Update(_world, _player.Position, 1f / WorldConstants.TicksPerSecond, 0, 0, 0);
        _projectileSystem.Update(_world, _player.Position, 1f / WorldConstants.TicksPerSecond);

        _dayClock.Tick += 1L;
        _camera.ViewportWidth = _graphics.PreferredBackBufferWidth;
        _camera.ViewportHeight = _graphics.PreferredBackBufferHeight;
        _camera.WorldPixelWidth = WorldConstants.WorldWidthTiles * WorldConstants.TileSizePixels;
        _camera.WorldPixelHeight = WorldConstants.WorldHeightTiles * WorldConstants.TileSizePixels;

        _lightingEngine.World = _world;
        _lightingEngine.ClearEmitters();

        int torchTileX = (int)MathF.Floor((_player.Position.X + (_player.Direction >= 0 ? _player.Width : 0f) + 10f) / WorldConstants.TileSizePixels);
        int torchTileY = (int)MathF.Floor((_player.Position.Y + 18f) / WorldConstants.TileSizePixels);
        _lightingEngine.AddEmitter(new LightEmitter(torchTileX, torchTileY, 0.9f, 0.7f, 0.38f));

        _lightingEngine.Sun = new SunInfo(
            _dayClock.SkyColor().R / 255f * 0.28f,
            _dayClock.SkyColor().G / 255f * 0.35f,
            _dayClock.SkyColor().B / 255f * 0.50f,
            _dayClock.DayFraction > 0.15f && _dayClock.DayFraction < 0.85f);

        int scrollDelta = mouse.ScrollWheelValue - _previousScrollWheelValue;
        if (scrollDelta != 0)
        {
            float nextZoom = _camera.Zoom + (Math.Sign(scrollDelta) * 0.25f);
            nextZoom = Math.Clamp(nextZoom, 1f, 3f);
            ApplyZoom(nextZoom);
        }

        _cameraController.Follow(in _player, _camera, 1f / WorldConstants.TicksPerSecond);

        if (keyboard.IsKeyDown(Keys.OemPlus) && !_previousKeyboardState.IsKeyDown(Keys.OemPlus))
        {
            ApplyZoom(Math.Clamp(_camera.Zoom + 0.25f, 1f, 3f));
        }

        if (keyboard.IsKeyDown(Keys.OemMinus) && !_previousKeyboardState.IsKeyDown(Keys.OemMinus))
        {
            ApplyZoom(Math.Clamp(_camera.Zoom - 0.25f, 1f, 3f));
        }

        _camera.ScreenPosition = new Vector2(
            Math.Clamp(_camera.ScreenPosition.X, 0f, Math.Max(0f, _camera.WorldPixelWidth - _camera.ViewportWidth / Math.Max(_camera.Zoom, 0.0001f))),
            Math.Clamp(_camera.ScreenPosition.Y, 0f, Math.Max(0f, _camera.WorldPixelHeight - _camera.ViewportHeight / Math.Max(_camera.Zoom, 0.0001f))));

        _hotbarUi.RefreshLayout(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _inventoryUi.RefreshLayout(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        for (int i = 0; i < _npcSystem.Pool.Capacity; ++i)
        {
            if (!_npcSystem.Pool.IsActive(i))
            {
                continue;
            }

            ref Npc npc = ref _npcSystem.Pool[i];
            if (npc.Type == 4)
            {
                _bossBar.UpdateFromNpc(in npc);
                break;
            }
        }

        if (!_bossBar.Visible)
        {
            _bossBar.Current = 0;
            _bossBar.Max = 1;
        }

        if (keyboard.IsKeyDown(Keys.D1) && !_previousKeyboardState.IsKeyDown(Keys.D1))
        {
            _hotbarUi.TrySelectByKey(1);
        }

        if (keyboard.IsKeyDown(Keys.D2) && !_previousKeyboardState.IsKeyDown(Keys.D2))
        {
            _hotbarUi.TrySelectByKey(2);
        }

        if (keyboard.IsKeyDown(Keys.D0) && !_previousKeyboardState.IsKeyDown(Keys.D0))
        {
            _hotbarUi.TrySelectByKey(0);
        }

        _previousKeyboardState = keyboard;
        _previousScrollWheelValue = mouse.ScrollWheelValue;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var start = System.Diagnostics.Stopwatch.StartNew();

        GraphicsDevice.Clear(_dayClock.SkyColor());
        _parallaxRenderer.Draw(_spriteBatch, _camera, _dayClock, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _liquidRenderer.Draw(_spriteBatch, _world, _camera, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _tileRenderer.Draw(_spriteBatch, _world, _camera, _atlas, _atlasLookup, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred);
        _npcRenderer.Draw(_spriteBatch, _npcSystem.Pool, _camera.RenderPosition);
        _projectileRenderer.Draw(_spriteBatch, _projectileSystem, _camera.RenderPosition);
        PlayerRenderer.Draw(_spriteBatch, _debugPixel, in _player, _camera.RenderPosition);
        _hotbarUi.Draw(_spriteBatch, _inventory, _debugPixel, _debugFont);
        _inventoryUi.Draw(_spriteBatch, _debugPixel, _debugFont);
        _bossBar.Draw(_spriteBatch, _debugPixel, _debugFont, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _spriteBatch.End();

        _lightRenderer.Apply(_spriteBatch, _lightingEngine, _camera);
        _lightMilliseconds = _lightRenderer.LastFrameMilliseconds;

        _tilesDrawn = _tileRenderer.TilesDrawn;
        _drawCalls = _tileRenderer.DrawCalls;

        if (_showDebug)
        {
            DrawDebugOverlay();
        }

        start.Stop();
        _drawMilliseconds = start.Elapsed.TotalMilliseconds;
        base.Draw(gameTime);
    }

    private void ApplyZoom(float targetZoom)
    {
        float previousZoom = _camera.Zoom;
        Vector2 centerWorld = _camera.ScreenPosition + new Vector2(
            _graphics.PreferredBackBufferWidth / (2f * Math.Max(previousZoom, 0.0001f)),
            _graphics.PreferredBackBufferHeight / (2f * Math.Max(previousZoom, 0.0001f)));

        _camera.Zoom = Math.Clamp(targetZoom, 1f, 3f);

        float halfWidth = _graphics.PreferredBackBufferWidth / (2f * Math.Max(_camera.Zoom, 0.0001f));
        float halfHeight = _graphics.PreferredBackBufferHeight / (2f * Math.Max(_camera.Zoom, 0.0001f));
        _camera.ScreenPosition = centerWorld - new Vector2(halfWidth, halfHeight);
    }

    private void DrawDebugOverlay()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.Deferred);

        string text = $"tiles: {_tilesDrawn}\ndraw calls: {_drawCalls}\nms: {_drawMilliseconds:F2}\nlight ms: {_lightMilliseconds:F2}\nmode: {_lightingEngine.Mode}\nzoom: {_camera.Zoom:F2}";

        if (_debugFont is not null)
        {
            _spriteBatch.DrawString(_debugFont, text, new Vector2(12f, 12f), Color.White);
        }
        else
        {
            _spriteBatch.Draw(_debugPixel, new Rectangle(12, 12, 200, 90), new Color(0, 0, 0, 180));
        }

        _spriteBatch.End();
    }
}
