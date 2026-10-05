using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core;

/// <summary>
/// Flags that steer the axis-aligned tile collision solver.
/// </summary>
public struct CollisionOptions
{
    public bool IgnorePlatforms;
    public bool StepUp;
    public bool TileCollide;
}

/// <summary>
/// Result data emitted by the collision solver for the current tick.
/// </summary>
public struct CollisionResult
{
    public bool OnGround;
    public bool HitCeiling;
    public bool HitWallL;
    public bool HitWallR;
    public byte LiquidType;

    public bool InLiquid => LiquidType != 0;
}

/// <summary>
/// Dense, allocation-free tile collision solver for the world grid.
/// </summary>
public static class Collision
{
    public const float TileSize = 16f;
    public const float MaxSubdivisionStep = 15f;

    public static void Move(
        World w,
        ref Vector2 pos,
        ref Vector2 vel,
        int width,
        int height,
        in CollisionOptions opt,
        out CollisionResult res)
    {
        res = default;

        if (w is null || width <= 0 || height <= 0 || !opt.TileCollide)
        {
            return;
        }

        float maxMove = MathF.Max(MathF.Abs(vel.X), MathF.Abs(vel.Y));
        if (maxMove > MaxSubdivisionStep)
        {
            int subdivisions = Math.Max(1, (int)MathF.Ceiling(maxMove / MaxSubdivisionStep));
            float stepX = vel.X / subdivisions;
            float stepY = vel.Y / subdivisions;

            for (int i = 0; i < subdivisions; ++i)
            {
                Vector2 localPos = pos;
                Vector2 localVel = new(stepX, stepY);
                MoveSingleStep(w, ref localPos, ref localVel, width, height, in opt, out CollisionResult localRes);
                pos = localPos;
                vel = localVel;

                res.OnGround |= localRes.OnGround;
                res.HitCeiling |= localRes.HitCeiling;
                res.HitWallL |= localRes.HitWallL;
                res.HitWallR |= localRes.HitWallR;
                if (res.LiquidType == 0 && localRes.LiquidType != 0)
                {
                    res.LiquidType = localRes.LiquidType;
                }
            }

            return;
        }

        MoveSingleStep(w, ref pos, ref vel, width, height, in opt, out res);
    }

    public static bool SolidCollision(
        World world,
        int tileX,
        int tileY,
        float left,
        float top,
        float right,
        float bottom,
        bool ignorePlatforms,
        bool movingDown,
        float previousBottom)
    {
        if (!world.InBounds(tileX, tileY))
        {
            return false;
        }

        ushort tileType = world.Get(tileX, tileY);
        if (tileType == 0)
        {
            return false;
        }

        TileDef def = TileDefs.GetTile(tileType);
        if (!def.Solid)
        {
            if (def.Name == "Platform")
            {
                if (ignorePlatforms)
                {
                    return false;
                }

                float platformTop = tileY * TileSize;
                if (movingDown && previousBottom <= platformTop + 6f)
                {
                    return right > tileX * TileSize && left < (tileX + 1) * TileSize && bottom > platformTop && top < platformTop + 6f;
                }

                return false;
            }

            return false;
        }

        float tileLeft = tileX * TileSize;
        float tileRight = tileLeft + TileSize;
        float tileTop = tileY * TileSize;
        float tileBottom = tileTop + TileSize;

        ushort flags = world.Flags[world.ToIndex(tileX, tileY)];
        if ((flags & TileFlags.HalfBrick) != 0)
        {
            tileTop += 8f;
            tileBottom = tileTop + 8f;
        }

        int slope = TileFlags.GetSlope(flags);
        if (slope != 0)
        {
            float localX = Math.Clamp(right - tileLeft, 0f, TileSize);
            float slopeY = SlopeFloorY(world, tileX, tileY, localX);
            return bottom > tileTop && top < slopeY && right > tileLeft && left < tileRight;
        }

        return right > tileLeft && left < tileRight && bottom > tileTop && top < tileBottom;
    }

    public static float SlopeFloorY(World world, int tileX, int tileY, float localX)
    {
        if (!world.InBounds(tileX, tileY))
        {
            return (tileY + 1) * TileSize;
        }

        ushort flags = world.Flags[world.ToIndex(tileX, tileY)];
        int slope = TileFlags.GetSlope(flags);

        float tileTop = tileY * TileSize;
        float clampedX = Math.Clamp(localX, 0f, TileSize);

        return slope switch
        {
            1 => tileTop + (TileSize - clampedX),
            2 => tileTop + clampedX,
            3 => tileTop + (TileSize - clampedX),
            4 => tileTop + clampedX,
            _ => tileTop + TileSize
        };
    }

    private static void MoveSingleStep(
        World w,
        ref Vector2 pos,
        ref Vector2 vel,
        int width,
        int height,
        in CollisionOptions opt,
        out CollisionResult res)
    {
        res = default;
        float previousBottom = pos.Y + height;

        MoveAxisX(w, ref pos, ref vel, width, height, in opt, out CollisionResult xRes, previousBottom);
        MoveAxisY(w, ref pos, ref vel, width, height, in opt, out CollisionResult yRes, previousBottom);

        res.HitWallL = xRes.HitWallL || yRes.HitWallL;
        res.HitWallR = xRes.HitWallR || yRes.HitWallR;
        res.HitCeiling = xRes.HitCeiling || yRes.HitCeiling;
        res.OnGround = xRes.OnGround || yRes.OnGround;
        res.LiquidType = yRes.LiquidType;

        if (res.InLiquid)
        {
            if (res.LiquidType == 1)
            {
                vel.X *= 0.75f;
                vel.Y *= 0.8f;
            }
            else if (res.LiquidType == 2)
            {
                vel.X *= 0.5f;
                vel.Y *= 0.9f;
            }
        }
    }

    private static void MoveAxisX(
        World w,
        ref Vector2 pos,
        ref Vector2 vel,
        int width,
        int height,
        in CollisionOptions opt,
        out CollisionResult res,
        float previousBottom)
    {
        res = default;

        if (vel.X == 0f)
        {
            return;
        }

        float startX = pos.X;
        float oldLeft = startX;
        float oldRight = startX + width;
        pos.X += vel.X;
        float left = pos.X;
        float right = pos.X + width;
        float top = pos.Y;
        float bottom = pos.Y + height;
        int minTileX = (int)MathF.Floor(Math.Min(oldLeft, left) / TileSize);
        int maxTileX = (int)MathF.Floor(Math.Max(oldRight, right) / TileSize);
        int minTileY = (int)MathF.Floor(Math.Min(top, pos.Y) / TileSize);
        int maxTileY = (int)MathF.Floor(Math.Max(bottom, pos.Y + height) / TileSize);

        for (int tileY = minTileY; tileY <= maxTileY; ++tileY)
        {
            for (int tileX = minTileX; tileX <= maxTileX; ++tileX)
            {
                if (!SolidCollision(w, tileX, tileY, left, top, right, bottom, opt.IgnorePlatforms, vel.Y >= 0f, previousBottom))
                {
                    continue;
                }

                float moveDirX = MathF.Sign(vel.X);
                if (vel.X > 0f)
                {
                    pos.X = tileX * TileSize - width;
                    vel.X = 0f;
                    res.HitWallR = true;
                }
                else if (vel.X < 0f)
                {
                    pos.X = (tileX + 1) * TileSize;
                    vel.X = 0f;
                    res.HitWallL = true;
                }

                if (opt.StepUp && MathF.Abs(moveDirX) > 0.001f)
                {
                    TryStepUp(w, ref pos, ref vel, width, height, tileX, tileY, moveDirX, in opt);
                }

                return;
            }
        }
    }

    private static void MoveAxisY(
        World w,
        ref Vector2 pos,
        ref Vector2 vel,
        int width,
        int height,
        in CollisionOptions opt,
        out CollisionResult res,
        float previousBottom)
    {
        res = default;

        if (vel.Y == 0f)
        {
            return;
        }

        float startY = pos.Y;
        float oldTop = startY;
        float oldBottom = startY + height;
        pos.Y += vel.Y;
        float left = pos.X;
        float right = pos.X + width;
        float top = pos.Y;
        float bottom = pos.Y + height;
        int minTileX = (int)MathF.Floor(Math.Min(left, pos.X) / TileSize);
        int maxTileX = (int)MathF.Floor(Math.Max(right, pos.X + width) / TileSize);
        int minTileY = (int)MathF.Floor(Math.Min(oldTop, top) / TileSize);
        int maxTileY = (int)MathF.Floor(Math.Max(oldBottom, bottom) / TileSize);

        for (int tileY = minTileY; tileY <= maxTileY; ++tileY)
        {
            for (int tileX = minTileX; tileX <= maxTileX; ++tileX)
            {
                if (!SolidCollision(w, tileX, tileY, left, top, right, bottom, opt.IgnorePlatforms, vel.Y >= 0f, previousBottom))
                {
                    continue;
                }

                if (vel.Y > 0f)
                {
                    pos.Y = tileY * TileSize - height;
                    vel.Y = 0f;
                    res.OnGround = true;
                }
                else if (vel.Y < 0f)
                {
                    pos.Y = (tileY + 1) * TileSize;
                    vel.Y = 0f;
                    res.HitCeiling = true;
                }

                return;
            }
        }

        if (MathF.Abs(vel.Y) > 0.0001f)
        {
            res.LiquidType = ResolveLiquidType(w, pos, width, height);
        }
    }

    private static void TryStepUp(
        World w,
        ref Vector2 pos,
        ref Vector2 vel,
        int width,
        int height,
        int tileX,
        int tileY,
        float moveDirX,
        in CollisionOptions opt)
    {
        if (!opt.StepUp)
        {
            return;
        }

        float centerX = pos.X + width * 0.5f;
        int requestedX = (int)MathF.Floor((centerX + moveDirX * 8f) / TileSize);
        int nextTileY = (int)MathF.Floor((pos.Y + height - 1f) / TileSize);

        if (requestedX == tileX)
        {
            requestedX = tileX + (moveDirX > 0f ? 1 : -1);
        }

        if (!w.InBounds(tileX, tileY) || !w.InBounds(requestedX, nextTileY))
        {
            return;
        }

        float proposedY = pos.Y - TileSize;
        float targetLeft = pos.X;
        float targetRight = pos.X + width;
        float targetTop = proposedY;
        float targetBottom = proposedY + height;

        bool fits = !SolidCollision(w, requestedX, nextTileY, targetLeft, targetTop, targetRight, targetBottom, false, false, pos.Y + height)
            && !SolidCollision(w, requestedX, nextTileY - 1, targetLeft, targetTop, targetRight, targetBottom, false, false, pos.Y + height)
            && !SolidCollision(w, tileX, tileY - 1, targetLeft, targetTop, targetRight, targetBottom, false, false, pos.Y + height);

        if (fits)
        {
            pos.Y = proposedY;
            vel.Y = -0.01f;
            vel.X = moveDirX * 0.5f;
        }
    }

    private static byte ResolveLiquidType(World world, Vector2 pos, int width, int height)
    {
        int left = (int)MathF.Floor(pos.X / TileSize);
        int right = (int)MathF.Floor((pos.X + width) / TileSize);
        int top = (int)MathF.Floor(pos.Y / TileSize);
        int bottom = (int)MathF.Floor((pos.Y + height) / TileSize);

        for (int y = top; y <= bottom; ++y)
        {
            for (int x = left; x <= right; ++x)
            {
                if (!world.InBounds(x, y))
                {
                    continue;
                }

                ushort type = world.Get(x, y);
                if (type == 9)
                {
                    return 1;
                }

                if (type == 10)
                {
                    return 2;
                }
            }
        }

        return 0;
    }
}
