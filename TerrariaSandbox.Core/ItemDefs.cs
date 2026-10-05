namespace TerrariaSandbox.Core;

/// <summary>
/// Immutable metadata for a single item type.
/// </summary>
public sealed class ItemDef
{
    public ItemDef(
        ushort type,
        string name,
        int maxStack,
        int value,
        byte useStyle,
        int useTime,
        int damage,
        ushort tileToPlace,
        ushort wallToPlace,
        int pickPower,
        int axePower,
        int hammerPower,
        int rarity,
        ushort ammoType = 0,
        ushort projectileType = 0,
        int manaCost = 0,
        int cooldown = 0,
        byte weaponType = 0)
    {
        Type = type;
        Name = name;
        MaxStack = maxStack;
        Value = value;
        UseStyle = useStyle;
        UseTime = useTime;
        Damage = damage;
        TileToPlace = tileToPlace;
        WallToPlace = wallToPlace;
        PickPower = pickPower;
        AxePower = axePower;
        HammerPower = hammerPower;
        Rarity = rarity;
        AmmoType = ammoType;
        ProjectileType = projectileType;
        ManaCost = manaCost;
        Cooldown = cooldown;
        WeaponType = weaponType;
    }

    public ushort Type { get; }
    public string Name { get; }
    public int MaxStack { get; }
    public int Value { get; }
    public byte UseStyle { get; }
    public int UseTime { get; }
    public int Damage { get; }
    public ushort TileToPlace { get; }
    public ushort WallToPlace { get; }
    public int PickPower { get; }
    public int AxePower { get; }
    public int HammerPower { get; }
    public int Rarity { get; }
    public ushort AmmoType { get; }
    public ushort ProjectileType { get; }
    public int ManaCost { get; }
    public int Cooldown { get; }
    public byte WeaponType { get; }
}

/// <summary>
/// Static definitions for the item database used by the inventory and world logic.
/// </summary>
public static class ItemDefs
{
    public static readonly ItemDef[] Items = CreateItems();

    private static ItemDef[] CreateItems()
    {
        var items = new ItemDef[58];

        items[0] = new(0, "Empty", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[1] = new(1, "Dirt Block", 999, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0);
        items[2] = new(2, "Stone Block", 999, 2, 0, 0, 0, 3, 0, 0, 0, 0, 0);
        items[3] = new(3, "Torch", 99, 3, 1, 20, 0, 25, 0, 0, 0, 0, 0);
        items[4] = new(4, "Wood", 99, 5, 0, 0, 0, 6, 0, 0, 0, 0, 0);
        items[5] = new(5, "Sand", 99, 2, 0, 0, 0, 4, 0, 0, 0, 0, 0);
        items[6] = new(6, "Brick", 99, 8, 0, 0, 0, 10, 0, 0, 0, 0, 0);
        items[7] = new(7, "Copper Ore", 99, 12, 0, 0, 0, 11, 0, 0, 0, 0, 0);
        items[8] = new(8, "Silver Ore", 99, 18, 0, 0, 0, 12, 0, 0, 0, 0, 0);
        items[9] = new(9, "Gold Ore", 99, 24, 0, 0, 0, 13, 0, 0, 0, 0, 0);
        items[10] = new(10, "Gem", 99, 32, 0, 0, 0, 14, 0, 0, 0, 0, 0);
        items[11] = new(11, "Copper Pickaxe", 1, 25, 1, 20, 6, 0, 0, 20, 0, 0, 5, weaponType: 1);
        items[12] = new(12, "Copper Axe", 1, 25, 1, 20, 4, 0, 0, 0, 20, 0, 5, weaponType: 1);
        items[13] = new(13, "Hammer", 1, 35, 1, 20, 6, 0, 0, 0, 0, 20, 5, weaponType: 1);
        items[14] = new(14, "Torch Item", 99, 3, 1, 18, 0, 25, 0, 0, 0, 0, 0);
        items[15] = new(15, "Weapon", 1, 25, 1, 18, 12, 0, 0, 0, 0, 0, 5, weaponType: 1);
        items[16] = new(16, "Sword", 1, 28, 1, 18, 12, 0, 0, 0, 0, 0, 5, weaponType: 2, cooldown: 12);
        items[17] = new(17, "Bow", 1, 32, 1, 20, 8, 0, 0, 0, 0, 0, 5, ammoType: 54, projectileType: ProjectileTypes.Arrow, cooldown: 18, weaponType: 3);
        items[18] = new(18, "Magic Staff", 1, 40, 1, 18, 10, 0, 0, 0, 0, 0, 5, projectileType: ProjectileTypes.Fireball, manaCost: 8, cooldown: 20, weaponType: 4);
        items[19] = new(19, "Bomb", 99, 25, 1, 28, 18, 0, 0, 0, 0, 0, 5, ammoType: 0, projectileType: ProjectileTypes.Bomb, cooldown: 24, weaponType: 5);

        for (int i = 20; i < 50; ++i)
        {
            items[i] = new((ushort)i, $"Item{i}", 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        items[50] = new(50, "Copper Coin", 999, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[51] = new(51, "Silver Coin", 999, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[52] = new(52, "Gold Coin", 999, 10000, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[53] = new(53, "Platinum Coin", 999, 1000000, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[54] = new(54, "Arrow", 999, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[55] = new(55, "Bullet", 999, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[56] = new(56, "Rocket", 999, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        items[57] = new(57, "Dart", 999, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        return items;
    }

    public const ushort CopperCoin = 50;
    public const ushort SilverCoin = 51;
    public const ushort GoldCoin = 52;
    public const ushort PlatinumCoin = 53;

    public static ItemDef Get(ushort type)
    {
        return type < Items.Length ? Items[type] : Items[0];
    }

    public static int GetCoinValue(ushort type)
    {
        return type switch
        {
            CopperCoin => 1,
            SilverCoin => 100,
            GoldCoin => 10000,
            PlatinumCoin => 1000000,
            _ => 0
        };
    }
}
