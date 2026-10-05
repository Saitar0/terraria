using Microsoft.Xna.Framework;

namespace TerrariaSandbox.Core.Ai
{
    public static class SlimeAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            if (npc.Life <= 0)
            {
                npc.Ai0 = AiState.Dead;
                return;
            }

            Vector2 toPlayer = context.PlayerPosition - npc.Position;
            float distance = toPlayer.Length();
            float aggroRange = 190f;

            if (npc.Ai0 == 0)
            {
                npc.Ai0 = AiState.Idle;
            }

            if (npc.Ai0 == AiState.Hurt)
            {
                npc.Ai1 = Math.Max(0, npc.Ai1 - 1);
                if (npc.Ai1 <= 0)
                {
                    npc.Ai0 = distance < aggroRange ? AiState.Chase : AiState.Wander;
                }
            }

            switch (npc.Ai0)
            {
                case AiState.Idle:
                    npc.Ai1 = 0;
                    npc.Ai2++;
                    if (npc.Ai2 > 30)
                    {
                        npc.Ai0 = AiState.Wander;
                        npc.Ai2 = 0;
                    }
                    break;

                case AiState.Wander:
                    npc.Ai1++;
                    npc.Ai2 = Math.Max(0, npc.Ai2 - 1);
                    npc.Velocity.X = MathHelper.Lerp(npc.Velocity.X, npc.Direction * 0.6f, 0.15f);
                    if (distance < aggroRange)
                    {
                        npc.Ai0 = AiState.Chase;
                    }
                    else if (npc.Ai1 > 45)
                    {
                        npc.Ai0 = AiState.Idle;
                        npc.Direction *= -1;
                        npc.Ai1 = 0;
                    }
                    break;

                case AiState.Chase:
                    npc.Ai1 = 0;
                    npc.Ai3++;
                    if (distance > 0f)
                    {
                        float dir = MathF.Sign(toPlayer.X);
                        if (dir != 0f)
                        {
                            npc.Direction = (int)dir;
                        }

                        float targetSpeed = 1.35f;
                        if (npc.Ai3 % 18 == 0)
                        {
                            npc.Ai2 = 14;
                        }

                        if (npc.Ai2 > 0)
                        {
                            npc.Ai2--;
                            npc.Velocity.Y = -5.2f;
                            npc.Velocity.X = npc.Direction * targetSpeed * 1.6f;
                        }
                        else
                        {
                            npc.Velocity.X = MathHelper.Lerp(npc.Velocity.X, npc.Direction * targetSpeed, 0.14f);
                        }
                    }

                    if (distance >= aggroRange * 1.6f)
                    {
                        npc.Ai0 = AiState.Wander;
                    }
                    break;

                case AiState.Attack:
                    npc.Ai1++;
                    if (npc.Ai1 > 8)
                    {
                        npc.Ai0 = distance < aggroRange ? AiState.Chase : AiState.Wander;
                    }
                    break;

                case AiState.Dead:
                    break;
            }

            npc.Ai3 = Math.Max(0, npc.Ai3 - 1);
            if (npc.Ai3 < 0)
            {
                npc.Ai3 = 0;
            }

            npc.Velocity.Y += 0.18f;
            npc.Position += npc.Velocity * context.DeltaSeconds * 60f;

            if (context.World is not null)
            {
                Vector2 pos = npc.Position;
                Vector2 vel = npc.Velocity;
                Collision.Move(context.World, ref pos, ref vel, (int)MathF.Ceiling(npc.Width), (int)MathF.Ceiling(npc.Height), new CollisionOptions { TileCollide = true, StepUp = true }, out _);
                npc.Position = pos;
                npc.Velocity = vel;
            }
        }
    }

    public static class ZombieAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            if (npc.Life <= 0)
            {
                npc.Ai0 = AiState.Dead;
                return;
            }

            Vector2 toPlayer = context.PlayerPosition - npc.Position;
            float distanceSquared = toPlayer.LengthSquared();
            float aggroRange = 220f;
            bool canSeeTarget = true;

            if (context.World is not null)
            {
                canSeeTarget = EnemySense.HasLineOfSight(context.World, npc.Position, context.PlayerPosition, 240f);
            }

            if (npc.Ai0 == 0)
            {
                npc.Ai0 = AiState.Idle;
            }

            switch (npc.Ai0)
            {
                case AiState.Idle:
                    npc.Ai1 = 0;
                    if (distanceSquared < aggroRange * aggroRange && canSeeTarget)
                    {
                        npc.Ai0 = AiState.Chase;
                    }
                    break;

                case AiState.Chase:
                    if (distanceSquared > 0f)
                    {
                        float dir = MathF.Sign(toPlayer.X);
                        npc.Direction = dir != 0f ? (int)dir : npc.Direction;
                        float targetSpeed = 1.15f;
                        npc.Velocity.X = MathHelper.Lerp(npc.Velocity.X, npc.Direction * targetSpeed, 0.12f);
                        if (Math.Abs(toPlayer.X) < 24f && npc.Ai2 <= 0)
                        {
                            npc.Ai0 = AiState.Attack;
                            npc.Ai2 = 18;
                        }
                    }

                    if (context.World is not null)
                    {
                        float aheadX = npc.Position.X + (npc.Direction * 22f);
                        int tileX = (int)MathF.Floor((aheadX + (npc.Direction > 0 ? npc.Width : 0f)) / 16f);
                        int tileY = (int)MathF.Floor((npc.Position.Y + npc.Height * 0.5f) / 16f);
                        int tileYAbove = tileY - 1;
                        if (IsSolid(context.World, tileX, tileY) && !IsSolid(context.World, tileX, tileYAbove))
                        {
                            npc.Velocity.Y = -4.2f;
                        }
                    }

                    if (distanceSquared > aggroRange * aggroRange * 1.6f || !canSeeTarget)
                    {
                        npc.Ai0 = AiState.Wander;
                    }
                    break;

                case AiState.Wander:
                    npc.Velocity.X = MathHelper.Lerp(npc.Velocity.X, 0f, 0.08f);
                    if (distanceSquared < aggroRange * aggroRange && canSeeTarget)
                    {
                        npc.Ai0 = AiState.Chase;
                    }
                    break;

                case AiState.Attack:
                    npc.Ai2 = Math.Max(0, npc.Ai2 - 1);
                    npc.Velocity.X *= 0.9f;
                    if (npc.Ai2 <= 0)
                    {
                        npc.Ai0 = AiState.Chase;
                    }
                    break;

                case AiState.Hurt:
                    npc.Ai1 = Math.Max(0, npc.Ai1 - 1);
                    if (npc.Ai1 <= 0)
                    {
                        npc.Ai0 = AiState.Chase;
                    }
                    break;

                case AiState.Dead:
                    break;
            }

            if (npc.Ai3 > 0)
            {
                npc.Ai3--;
            }

            npc.Velocity.Y += 0.18f;
            npc.Position += npc.Velocity * context.DeltaSeconds * 60f;

            if (context.World is not null)
            {
                Vector2 pos = npc.Position;
                Vector2 vel = npc.Velocity;
                Collision.Move(context.World, ref pos, ref vel, (int)MathF.Ceiling(npc.Width), (int)MathF.Ceiling(npc.Height), new CollisionOptions { TileCollide = true, StepUp = true }, out _);
                npc.Position = pos;
                npc.Velocity = vel;
            }
        }

        private static bool IsSolid(World world, int tileX, int tileY)
        {
            return world.InBounds(tileX, tileY) && world.Get(tileX, tileY) != 0;
        }
    }

    public static class FlyerAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            if (npc.Life <= 0)
            {
                npc.Ai0 = AiState.Dead;
                return;
            }

            Vector2 toPlayer = context.PlayerPosition - npc.Position;
            float distanceSquared = toPlayer.LengthSquared();
            if (distanceSquared > 0f)
            {
                float distance = MathF.Sqrt(distanceSquared);
                float dirX = toPlayer.X / distance;
                float dirY = toPlayer.Y / distance;
                float targetX = dirX * 2.1f;
                float targetY = dirY * 1.5f;
                npc.Velocity.X = MathHelper.Lerp(npc.Velocity.X, targetX, 0.12f);
                npc.Velocity.Y = MathHelper.Lerp(npc.Velocity.Y, targetY, 0.12f);
                npc.Direction = dirX >= 0f ? 1 : -1;
                npc.Ai0 = AiState.Chase;
            }

            npc.Position += npc.Velocity * context.DeltaSeconds * 60f;
        }
    }

    public static class BossAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            if (npc.Life <= 0)
            {
                npc.Ai0 = AiState.Dead;
                return;
            }

            int phase = npc.Life <= (int)(npc.LifeMax * 0.4f) ? 2 : 1;
            if (npc.Ai0 == 0)
            {
                npc.Ai0 = phase == 1 ? AiState.Wander : AiState.Chase;
                npc.Ai1 = phase == 1 ? 90 : 60;
            }

            if (phase == 1)
            {
                npc.Ai1--;
                if (npc.Ai1 <= 0)
                {
                    npc.Ai0 = npc.Ai0 == AiState.Wander ? AiState.Chase : AiState.Wander;
                    npc.Ai1 = 90;
                }

                Vector2 toPlayer = context.PlayerPosition - npc.Position;
                float distance = toPlayer.Length();
                if (distance > 0f)
                {
                    if (npc.Ai0 == AiState.Chase)
                    {
                        float direction = MathF.Sign(toPlayer.X);
                        npc.Direction = direction != 0f ? (int)direction : npc.Direction;
                        npc.Velocity.X = MathHelper.Lerp(npc.Velocity.X, npc.Direction * 2.5f, 0.12f);
                    }
                    else
                    {
                        npc.Velocity.X = MathHelper.Lerp(npc.Velocity.X, MathF.Cos((float)npc.Ai2 * 0.2f) * 1.8f, 0.08f);
                    }
                }
            }
            else
            {
                npc.Ai0 = AiState.Chase;
                npc.Ai1--;
                if (npc.Ai1 <= 0)
                {
                    npc.Ai1 = 50;
                    npc.Ai2 = (npc.Ai2 + 1) % 8;
                    npc.Velocity = new Vector2(npc.Direction * 7.5f, -1.6f);
                }

                if (npc.Ai2 > 3)
                {
                    float rotation = npc.Ai2 * 0.45f;
                    npc.Velocity.X = MathF.Cos(rotation) * 6f;
                    npc.Velocity.Y = MathF.Sin(rotation) * 2f;
                }
            }

            npc.Position += npc.Velocity * context.DeltaSeconds * 60f;
        }
    }

    public static class EnemySense
    {
        public static bool HasLineOfSight(World world, Vector2 origin, Vector2 target, float maxDistance)
        {
            if (world is null)
            {
                return true;
            }

            Vector2 delta = target - origin;
            float length = delta.Length();
            if (length <= 0f)
            {
                return true;
            }

            if (length > maxDistance)
            {
                return false;
            }

            float step = 6f;
            Vector2 direction = delta / length;
            Vector2 sample = origin;
            int steps = (int)MathF.Ceiling(length / step);

            for (int i = 0; i <= steps; ++i)
            {
                int tileX = (int)MathF.Floor(sample.X / 16f);
                int tileY = (int)MathF.Floor(sample.Y / 16f);
                if (world.InBounds(tileX, tileY) && world.Get(tileX, tileY) != 0)
                {
                    return false;
                }

                sample += direction * step;
            }

            return true;
        }
    }
}

namespace TerrariaSandbox.Core
{
    public static class BossAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            TerrariaSandbox.Core.Ai.BossAi.Update(ref npc, in context);
        }
    }

    public static class SlimeAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            TerrariaSandbox.Core.Ai.SlimeAi.Update(ref npc, in context);
        }
    }

    public static class ZombieAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            TerrariaSandbox.Core.Ai.ZombieAi.Update(ref npc, in context);
        }
    }

    public static class FlyerAi
    {
        public static void Update(ref Npc npc, in AiContext context)
        {
            TerrariaSandbox.Core.Ai.FlyerAi.Update(ref npc, in context);
        }
    }
}
