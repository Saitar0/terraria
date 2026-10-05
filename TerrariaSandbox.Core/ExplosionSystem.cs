using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Simple explosion effect that clears nearby blocks while respecting a per-explosion tile budget.
/// </summary>
public static class ExplosionSystem
{
    public const int DefaultTileBudget = 256;

    public static int Blast(World world, Vector2 center, int radius, int maxTiles = DefaultTileBudget)
    {
        if (world is null)
        {
            return 0;
        }

        int centerTileX = (int)MathF.Floor(center.X / 16f);
        int centerTileY = (int)MathF.Floor(center.Y / 16f);
        var queue = new Queue<(int x, int y)>();
        var visited = new HashSet<(int x, int y)>();
        int destroyed = 0;

        queue.Enqueue((centerTileX, centerTileY));
        visited.Add((centerTileX, centerTileY));

        while (queue.Count > 0 && destroyed < maxTiles)
        {
            var current = queue.Dequeue();
            int dx = Math.Abs(current.x - centerTileX);
            int dy = Math.Abs(current.y - centerTileY);
            if (dx * dx + dy * dy > radius * radius)
            {
                continue;
            }

            if (world.InBounds(current.x, current.y))
            {
                ushort tileType = world.Get(current.x, current.y);
                if (tileType != 0)
                {
                    TileDef def = TileDefs.GetTile(tileType);
                    if (def.Breakable)
                    {
                        world.SetTile(current.x, current.y, 0);
                        destroyed++;
                    }
                }
            }

            if (destroyed >= maxTiles)
            {
                break;
            }

            int[] offsets = { -1, 0, 1 };
            for (int ox = 0; ox < offsets.Length; ++ox)
            {
                for (int oy = 0; oy < offsets.Length; ++oy)
                {
                    if (ox == 1 && oy == 1)
                    {
                        continue;
                    }

                    int nx = current.x + offsets[ox];
                    int ny = current.y + offsets[oy];
                    var key = (nx, ny);
                    if (visited.Contains(key))
                    {
                        continue;
                    }

                    visited.Add(key);
                    queue.Enqueue(key);
                }
            }
        }

        return destroyed;
    }
}
