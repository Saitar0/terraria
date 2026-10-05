namespace TerrariaSandbox.Core;

/// <summary>
/// Item-spawning hook used to emit drops after mining.
/// </summary>
public interface IItemSpawner
{
    void SpawnItem(int tileX, int tileY, ushort itemId, int count = 1);
}

/// <summary>
/// Empty item spawner used by tests and the default sandbox world.
/// </summary>
public sealed class NullItemSpawner : IItemSpawner
{
    public void SpawnItem(int tileX, int tileY, ushort itemId, int count = 1)
    {
    }
}

/// <summary>
/// Handles tile digging, placement, and simple cursor validation.
/// </summary>
public sealed class InteractionSystem
{
    private readonly World _world;
    private readonly IItemSpawner _itemSpawner;
    private readonly MiningState _miningState;

    public InteractionSystem(World world, IItemSpawner? itemSpawner = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _itemSpawner = itemSpawner ?? new NullItemSpawner();
        _miningState = new MiningState();
    }

    public World World => _world;

    public void Update(Player p, Cursor c, in ItemStack held)
    {
        if (!c.Visible || !c.CanReach(p))
        {
            return;
        }

        if (TryMineTile(c.TileX, c.TileY, 18f))
        {
            return;
        }

        if (!held.IsEmpty && CanPlaceTile(c.TileX, c.TileY, held.Type))
        {
            PlaceTile(c.TileX, c.TileY, held.Type);
        }
    }

    public bool TryMineTile(int tileX, int tileY, float miningPower)
    {
        if (!_world.InBounds(tileX, tileY))
        {
            return false;
        }

        ushort type = _world.Get(tileX, tileY);
        if (type == 0)
        {
            return false;
        }

        TileDef tileDef = TileDefs.GetTile(type);
        if (tileDef.Hardness <= 0f)
        {
            BreakMultiCellObject(tileX, tileY);
            return true;
        }

        float required = MathF.Max(1f, tileDef.Hardness * 16f);
        float effectivePower = MathF.Max(miningPower, required);
        if (_miningState.Advance(tileX, tileY, effectivePower, tileDef.Hardness, out float remaining))
        {
            BreakMultiCellObject(tileX, tileY);
            _itemSpawner.SpawnItem(tileX, tileY, type, 1);
            return true;
        }

        return false;
    }

    public bool CanPlaceTile(int tileX, int tileY, ushort tileType)
    {
        if (!_world.InBounds(tileX, tileY))
        {
            return false;
        }

        if (_world.Get(tileX, tileY) != 0)
        {
            return false;
        }

        TileObjectData objectData = TileObjectData.GetForTile(tileType);
        if (objectData.Width <= 1 && objectData.Height <= 1)
        {
            return true;
        }

        int minX = tileX + objectData.OriginX;
        int minY = tileY + objectData.OriginY;
        for (int y = 0; y < objectData.Height; ++y)
        {
            for (int x = 0; x < objectData.Width; ++x)
            {
                int worldX = minX + x;
                int worldY = minY + y;
                if (!_world.InBounds(worldX, worldY) || _world.Get(worldX, worldY) != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void PlaceTile(int tileX, int tileY, ushort tileType)
    {
        if (!CanPlaceTile(tileX, tileY, tileType))
        {
            return;
        }

        TileObjectData objectData = TileObjectData.GetForTile(tileType);
        int minX = tileX + objectData.OriginX;
        int minY = tileY + objectData.OriginY;

        for (int y = 0; y < objectData.Height; ++y)
        {
            for (int x = 0; x < objectData.Width; ++x)
            {
                int worldX = minX + x;
                int worldY = minY + y;
                _world.Set(worldX, worldY, tileType);
            }
        }
    }

    public bool HasLineOfSight(int fromX, int fromY, int toX, int toY, int maxTiles)
    {
        int dx = Math.Abs(toX - fromX);
        int dy = Math.Abs(toY - fromY);
        int steps = Math.Max(dx, dy);
        if (steps <= 0 || steps > maxTiles)
        {
            return steps <= maxTiles;
        }

        int stepX = toX >= fromX ? 1 : -1;
        int stepY = toY >= fromY ? 1 : -1;
        int x = fromX;
        int y = fromY;

        for (int i = 0; i <= steps; ++i)
        {
            if (x == toX && y == toY)
            {
                return true;
            }

            if (_world.Get(x, y) != 0)
            {
                return false;
            }

            if (dx > dy)
            {
                x += stepX;
                dy -= Math.Abs(stepY);
            }
            else
            {
                y += stepY;
                dx -= Math.Abs(stepX);
            }
        }

        return true;
    }

    private void BreakMultiCellObject(int tileX, int tileY)
    {
        ushort tileType = _world.Get(tileX, tileY);
        TileObjectData objectData = TileObjectData.GetForTile(tileType);

        int minX = tileX + objectData.OriginX;
        int minY = tileY + objectData.OriginY;
        int width = objectData.Width;
        int height = objectData.Height;

        if (width <= 1 && height <= 1)
        {
            _world.Set(tileX, tileY, 0);
            return;
        }

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int worldX = minX + x;
                int worldY = minY + y;
                if (_world.InBounds(worldX, worldY))
                {
                    _world.Set(worldX, worldY, 0);
                }
            }
        }
    }
}
