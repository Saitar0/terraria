using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Built-in movement styles supported by the projectile AI system.
/// </summary>
public static class ProjectileAiStyles
{
    public const int None = 0;
    public const int Straight = 1;
    public const int Arc = 2;
    public const int Boomerang = 3;
    public const int Homing = 4;
    public const int Sticky = 5;
}

/// <summary>
/// Built-in projectile archetypes used by the prototype combat system.
/// </summary>
public static class ProjectileTypes
{
    public const ushort Empty = 0;
    public const ushort Arrow = 1;
    public const ushort Fireball = 2;
    public const ushort Boomerang = 3;
    public const ushort Bomb = 4;
    public const ushort StickyBolt = 5;
}

/// <summary>
/// Describes a projectile archetype and defaults for a spawned instance.
/// </summary>
public sealed class ProjectileDef
{
    public ProjectileDef(
        ushort type,
        string name,
        int width,
        int height,
        int damage,
        int penetrate,
        int timeLeft,
        int aiStyle,
        float speed,
        bool hostile = false,
        bool tileCollide = true,
        bool ricochet = false,
        int explosionRadius = 0,
        int childType = 0,
        int dustType = 0)
    {
        Type = type;
        Name = name;
        Width = width;
        Height = height;
        Damage = damage;
        Penetrate = penetrate;
        TimeLeft = timeLeft;
        AiStyle = aiStyle;
        Speed = speed;
        Hostile = hostile;
        TileCollide = tileCollide;
        Ricochet = ricochet;
        ExplosionRadius = explosionRadius;
        ChildType = childType;
        DustType = dustType;
    }

    public ushort Type { get; }
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int Damage { get; }
    public int Penetrate { get; }
    public int TimeLeft { get; }
    public int AiStyle { get; }
    public float Speed { get; }
    public bool Hostile { get; }
    public bool TileCollide { get; }
    public bool Ricochet { get; }
    public int ExplosionRadius { get; }
    public int ChildType { get; }
    public int DustType { get; }
}

/// <summary>
/// Runtime projectile instance stored in a fixed-capacity pool.
/// </summary>
public struct Projectile
{
    public int Type;
    public Vector2 Position;
    public Vector2 Velocity;
    public int Damage;
    public int Penetrate;
    public int TimeLeft;
    public int AiStyle;
    public int Ai0;
    public int Ai1;
    public int Ai2;
    public int Owner;
    public bool Hostile;
    public bool TileCollide;
    public bool Active;
    public bool Ricochet;
    public int Width;
    public int Height;
    public int DustType;
    public int ChildType;
    public float Gravity;
}

/// <summary>
/// Static definition registry for projectile archetypes.
/// </summary>
public static class ProjectileDefs
{
    public static readonly ProjectileDef[] Table =
    [
        new(0, "Empty", 0, 0, 0, 0, 0, ProjectileAiStyles.None, 0f),
        new(1, "Arrow", 6, 6, 8, 1, 240, ProjectileAiStyles.Straight, 7f, false, true, false, 0, 0, 1),
        new(2, "Fireball", 8, 8, 12, 2, 180, ProjectileAiStyles.Arc, 4f, false, true, false, 24, 0, 3),
        new(3, "Boomerang", 10, 10, 14, 3, 150, ProjectileAiStyles.Boomerang, 6f, false, true, false, 0, 0, 0),
        new(4, "Bomb", 12, 12, 18, 1, 180, ProjectileAiStyles.Straight, 4.5f, false, true, false, 48, 0, 2),
        new(5, "StickyBolt", 6, 6, 11, 3, 300, ProjectileAiStyles.Sticky, 5f, false, true, false, 0, 0, 0)
    ];

    public static ProjectileDef Get(int type)
    {
        return (uint)type < (uint)Table.Length ? Table[type] : Table[0];
    }
}

/// <summary>
/// Static projectile AI handlers for movement, impact and stick behavior.
/// </summary>
public static class ProjectileAi
{
    public static void Update(ref Projectile projectile, World? world, Vector2 targetPosition, float deltaSeconds)
    {
        if (!projectile.Active)
        {
            return;
        }

        projectile.TimeLeft--;
        if (projectile.TimeLeft <= 0)
        {
            projectile.Active = false;
            return;
        }

        float dt = Math.Max(0.016f, deltaSeconds);

        switch (projectile.AiStyle)
        {
            case ProjectileAiStyles.Straight:
                projectile.Position += projectile.Velocity * dt;
                break;
            case ProjectileAiStyles.Arc:
                projectile.Velocity.Y += projectile.Gravity * dt * 60f;
                projectile.Position += projectile.Velocity * dt;
                break;
            case ProjectileAiStyles.Boomerang:
                if (projectile.Ai0 == 0)
                {
                    projectile.Ai0 = 1;
                    projectile.Ai1 = 0;
                }

                projectile.Ai1++;
                if (projectile.Ai1 < 28)
                {
                    projectile.Position += projectile.Velocity * dt;
                }
                else
                {
                    Vector2 home = targetPosition - projectile.Position;
                    if (home.LengthSquared() > 0.1f)
                    {
                        home.Normalize();
                        projectile.Velocity = Vector2.Lerp(projectile.Velocity, home * Math.Max(2f, projectile.Velocity.Length() * 0.9f), 0.08f);
                    }

                    projectile.Position += projectile.Velocity * dt;
                }

                break;
            case ProjectileAiStyles.Homing:
                Vector2 delta = targetPosition - projectile.Position;
                if (delta.LengthSquared() > 0.1f)
                {
                    delta.Normalize();
                    float turnRate = 0.12f;
                    Vector2 desired = delta * projectile.Velocity.Length();
                    projectile.Velocity = Vector2.Lerp(projectile.Velocity, desired, turnRate);
                }

                projectile.Position += projectile.Velocity * dt;
                break;
            case ProjectileAiStyles.Sticky:
                projectile.Position += projectile.Velocity * dt;
                break;
            default:
                projectile.Position += projectile.Velocity * dt;
                break;
        }

        if (world is not null && projectile.TileCollide)
        {
            if (CollidesWithTile(world, projectile.Position, projectile.Width, projectile.Height))
            {
                HandleImpact(ref projectile, true);
            }
        }
    }

    public static void HandleImpact(ref Projectile projectile, bool tileHit)
    {
        if (!projectile.Active)
        {
            return;
        }

        if (projectile.Penetrate > 0)
        {
            projectile.Penetrate--;
            if (projectile.Penetrate > 0)
            {
                if (projectile.Ricochet && tileHit)
                {
                    projectile.Velocity = -projectile.Velocity * 0.5f;
                }

                if (projectile.AiStyle == ProjectileAiStyles.Sticky && tileHit)
                {
                    projectile.Velocity = Vector2.Zero;
                    projectile.TileCollide = false;
                }

                return;
            }
        }

        projectile.Active = false;
        projectile.TimeLeft = 0;
    }

    public static bool CollidesWithTile(World world, Vector2 position, int width, int height)
    {
        int left = (int)MathF.Floor(position.X / 16f);
        int right = (int)MathF.Floor((position.X + width) / 16f);
        int top = (int)MathF.Floor(position.Y / 16f);
        int bottom = (int)MathF.Floor((position.Y + height) / 16f);

        for (int y = top; y <= bottom; ++y)
        {
            for (int x = left; x <= right; ++x)
            {
                if (!world.InBounds(x, y))
                {
                    continue;
                }

                ushort tileType = world.Get(x, y);
                if (tileType == 0)
                {
                    continue;
                }

                TileDef def = TileDefs.GetTile(tileType);
                if (def.Solid)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

/// <summary>
/// Fixed-capacity projectile storage and simple spawn logic.
/// </summary>
public sealed class ProjectileSystem
{
    private readonly Pool<Projectile> _projectiles;
    private readonly DustSystem? _dustSystem;
    private readonly GoreSystem? _goreSystem;
    private readonly Random _random;

    public ProjectileSystem(Pool<Projectile>? pool = null, DustSystem? dustSystem = null, GoreSystem? goreSystem = null)
    {
        _projectiles = pool ?? new Pool<Projectile>(1000);
        _dustSystem = dustSystem;
        _goreSystem = goreSystem;
        _random = new Random(1337);
    }

    public Pool<Projectile> Pool => _projectiles;

    public int Spawn(int type, Vector2 position, Vector2 velocity, int owner, bool hostile, int damage = 0, int penetrate = 0, int timeLeft = 0, int aiStyle = 0, bool tileCollide = true, bool ricochet = false, int width = 0, int height = 0)
    {
        ProjectileDef def = ProjectileDefs.Get(type);
        Projectile projectile = new()
        {
            Type = type,
            Position = position,
            Velocity = velocity,
            Damage = damage > 0 ? damage : def.Damage,
            Penetrate = penetrate > 0 ? penetrate : def.Penetrate,
            TimeLeft = timeLeft > 0 ? timeLeft : def.TimeLeft,
            AiStyle = aiStyle > 0 ? aiStyle : def.AiStyle,
            Owner = owner,
            Hostile = hostile,
            TileCollide = tileCollide || def.TileCollide,
            Ricochet = ricochet || def.Ricochet,
            Width = width > 0 ? width : def.Width,
            Height = height > 0 ? height : def.Height,
            DustType = def.DustType,
            ChildType = def.ChildType,
            Gravity = 0.12f,
            Active = true
        };

        if (projectile.AiStyle == ProjectileAiStyles.Arc)
        {
            projectile.Gravity = 0.22f;
        }

        return _projectiles.Spawn(projectile);
    }

    public void Update(World world, Vector2 targetPosition, float deltaSeconds)
    {
        for (int i = 0; i < _projectiles.Capacity; ++i)
        {
            if (!_projectiles.IsActive(i))
            {
                continue;
            }

            ref Projectile projectile = ref _projectiles[i];
            if (!projectile.Active)
            {
                _projectiles.Despawn(i);
                continue;
            }

            ProjectileAi.Update(ref projectile, world, targetPosition, deltaSeconds);
            _projectiles[i] = projectile;

            if (!projectile.Active)
            {
                OnProjectileDeath(ref projectile, i);
            }
        }
    }

    public static bool TryConsumeAmmo(Inventory inventory, ushort ammoType, int count)
    {
        if (count <= 0)
        {
            return true;
        }

        int remaining = count;
        for (int slot = Inventory.AmmoStart; slot <= Inventory.AmmoEnd && remaining > 0; ++slot)
        {
            if (inventory.Slots[slot].Type != ammoType || inventory.Slots[slot].IsEmpty)
            {
                continue;
            }

            int taken = Math.Min(remaining, inventory.Slots[slot].Stack);
            inventory.Slots[slot].Stack -= (short)taken;
            remaining -= taken;
            if (inventory.Slots[slot].Stack <= 0)
            {
                inventory.Slots[slot] = ItemStack.Empty;
            }
        }

        if (remaining > 0)
        {
            return false;
        }

        return true;
    }

    private void OnProjectileDeath(ref Projectile projectile, int index)
    {
        if (_dustSystem is not null)
        {
            _dustSystem.Spawn(projectile.Position, Vector2.Zero, new Color(200, 120, 70), 12, 3);
        }

        if (projectile.ChildType > 0 && _projectiles.Capacity > index)
        {
            Spawn(projectile.ChildType, projectile.Position, -projectile.Velocity * 0.7f, projectile.Owner, projectile.Hostile, projectile.Damage / 2, 1, 120, ProjectileAiStyles.Straight, true);
        }

        if (projectile.DustType > 0 && _dustSystem is not null)
        {
            _dustSystem.Spawn(projectile.Position, Vector2.Zero, new Color(80, 80, 80), 18, projectile.DustType);
        }
    }
}
