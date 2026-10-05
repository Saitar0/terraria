using Microsoft.Xna.Framework;
using TerrariaSandbox.Core;

var world = new World(64,64);
world.Set(8,5,1);
world.Set(8,4,1);
Vector2 position = new Vector2(7 * 16f - 1f, 5 * 16f - 16f);
Vector2 velocity = new Vector2(10f,0f);
var options = new CollisionOptions { TileCollide=true, StepUp=true };
Collision.Move(world, ref position, ref velocity, 16, 16, in options, out var result);
Console.WriteLine($"pos=({position.X},{position.Y}) vel=({velocity.X},{velocity.Y}) result=onGround:{result.OnGround}, wallR:{result.HitWallR}, wallL:{result.HitWallL}, ceiling:{result.HitCeiling}");
