namespace TerrariaSandbox.Core.Gen;

/// <summary>
/// Runs a Moore-neighborhood cellular automaton to form cave walls and cavities.
/// </summary>
public sealed class CellularCaves
{
    private bool[] _current = Array.Empty<bool>();
    private bool[] _next = Array.Empty<bool>();

    public CellularCaves(int width, int height)
    {
        Resize(width, height);
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            _current = Array.Empty<bool>();
            _next = Array.Empty<bool>();
            return;
        }

        int size = width * height;
        _current = new bool[size];
        _next = new bool[size];
    }

    public void Apply(World world, int minY, int maxY, int iterations = 5, int solidThreshold = 4)
    {
        if (world is null || minY < 0 || maxY <= minY || maxY > world.Height)
        {
            return;
        }

        int width = world.Width;
        int height = world.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (_current.Length < width * height || _next.Length < width * height)
        {
            Resize(width, height);
        }

        Parallel.For(0, height, y =>
        {
            int rowOffset = y * width;
            for (int x = 0; x < width; ++x)
            {
                bool isSolid = y >= minY && y < maxY && world.Get(x, y) != 0;
                _current[rowOffset + x] = isSolid;
            }
        });

        for (int iteration = 0; iteration < iterations; ++iteration)
        {
            Parallel.For(minY, maxY, y =>
            {
                int rowOffset = y * width;
                for (int x = 1; x < width - 1; ++x)
                {
                    int index = rowOffset + x;
                    int solidNeighbors = 0;
                    for (int yy = y - 1; yy <= y + 1; ++yy)
                    {
                        if ((uint)yy >= (uint)height)
                        {
                            continue;
                        }

                        int neighborRow = yy * width;
                        for (int xx = x - 1; xx <= x + 1; ++xx)
                        {
                            if ((uint)xx >= (uint)width || (xx == x && yy == y))
                            {
                                continue;
                            }

                            if (_current[neighborRow + xx])
                            {
                                solidNeighbors++;
                            }
                        }
                    }

                    bool isSolid = _current[index];
                    _next[index] = solidNeighbors >= solidThreshold || (isSolid && solidNeighbors >= solidThreshold - 1);
                }
            });

            var swap = _current;
            _current = _next;
            _next = swap;
        }

        Parallel.For(minY, maxY, y =>
        {
            int rowOffset = y * width;
            for (int x = 0; x < width; ++x)
            {
                if (!_current[rowOffset + x] && world.Get(x, y) != 0)
                {
                    world.Set(x, y, 0);
                }
            }
        });
    }
}
