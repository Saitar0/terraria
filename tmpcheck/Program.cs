using System;
using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

var world = new World(64,64);
world.Set(8,4,1);
Vector2 position = new Vector2(120f, 64f);
Vector2 velocity = new Vector2(30f, 0f);
var options = new CollisionOptions { TileCollide = true };
Collision.Move(world, ref position, ref velocity, 16, 16, in options, out var result);
Console.WriteLine($"collision => pos=({position.X},{position.Y}) vel=({velocity.X},{velocity.Y}) hitWallR={result.HitWallR} hitWallL={result.HitWallL}");

var npc = new Npc
{
    Type = 4,
    Position = new Vector2(200f, 200f),
    Width = 44f,
    Height = 52f,
    Life = 50,
    LifeMax = 100,
    AiStyle = 3,
    Direction = 1,
    Ai0 = 0,
    Ai1 = 0,
    Ai2 = 0,
    Ai3 = 0
};

var context = new AiContext
{
    World = null,
    PlayerPosition = new Vector2(300f, 200f),
    DeltaSeconds = 1f / 60f,
    Biome = 0,
    Depth = 0,
    LightLevel = 0,
    Random = new Random(5)
};

BossAi.Update(ref npc, in context);
Console.WriteLine($"after 50 => ai0={npc.Ai0} ai1={npc.Ai1}");
npc.Life = 49;
BossAi.Update(ref npc, in context);
Console.WriteLine($"after 49 => ai0={npc.Ai0} ai1={npc.Ai1}");
npc.Life = 40;
BossAi.Update(ref npc, in context);
Console.WriteLine($"after 40 => ai0={npc.Ai0} ai1={npc.Ai1}");
