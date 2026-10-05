namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Creates a compact dungeon by carving connected rectangular rooms and hallways.
/// </summary>
public sealed class DungeonPass : IGenPass
{
    private struct Room
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public string Name => "Dungeon";
    public float Weight => 1.4f;

    public void Run(WorldGenContext ctx, IProgress<float> p)
    {
        if (ctx is null || ctx.World is null)
        {
            p.Report(1f);
            return;
        }

        var world = ctx.World;
        int width = world.Width;
        int height = world.Height;
        int originY = Math.Max(28, ctx.worldSurface + 18);
        int roomMinSize = 5;
        int roomMaxSize = 18;
        var rooms = new List<Room>();

        for (int i = 0; i < 18; ++i)
        {
            int roomX = ctx.Rng.NextRange(40, width - 80);
            int roomY = ctx.Rng.NextRange(originY, height - 120);
            int roomW = ctx.Rng.NextRange(roomMinSize, Math.Min(roomMaxSize, width - roomX - 20));
            int roomH = ctx.Rng.NextRange(roomMinSize, Math.Min(roomMaxSize, height - roomY - 30));
            rooms.Add(new Room { X = roomX, Y = roomY, Width = roomW, Height = roomH });
        }

        Room first = rooms[0];
        FillRect(world, first.X, first.Y, first.Width, first.Height, (ushort)TileType.Brick, (ushort)TileType.DirtWall);
        for (int i = 1; i < rooms.Count; ++i)
        {
            Room next = rooms[i];
            FillRect(world, next.X, next.Y, next.Width, next.Height, (ushort)TileType.Brick, (ushort)TileType.DirtWall);
            CarveCorridor(world, first.X + first.Width / 2, first.Y + first.Height / 2, next.X + next.Width / 2, next.Y + next.Height / 2);
            first = next;
        }

        int doorX = rooms[0].X + rooms[0].Width / 2;
        int doorY = rooms[0].Y + rooms[0].Height / 2;
        world.Set(doorX, doorY, (ushort)TileType.Air);
        world.Set(doorX + 1, doorY, (ushort)TileType.Air);

        for (int x = 0; x < width; ++x)
        {
            p.Report((x + 1f) / width);
        }

        p.Report(1f);
    }

    private static void FillRect(World world, int x, int y, int width, int height, ushort tileType, ushort wallType)
    {
        for (int yy = y; yy < y + height; ++yy)
        {
            for (int xx = x; xx < x + width; ++xx)
            {
                if (!world.InBounds(xx, yy))
                {
                    continue;
                }

                world.Set(xx, yy, tileType);
                world.SetWall(xx, yy, wallType);
            }
        }
    }

    private static void CarveCorridor(World world, int x1, int y1, int x2, int y2)
    {
        int cx = x1;
        int cy = y1;
        while (cx != x2)
        {
            world.Set(cx, cy, (ushort)TileType.Air);
            cx += Math.Sign(x2 - cx);
        }

        while (cy != y2)
        {
            world.Set(cx, cy, (ushort)TileType.Air);
            cy += Math.Sign(y2 - cy);
        }

        world.Set(cx, cy, (ushort)TileType.Air);
    }
}
