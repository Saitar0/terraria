namespace TerrariaSandbox.Core.IO;

/// <summary>
/// Serializable snapshot of the player save data.
/// </summary>
public sealed class PlayerSaveData
{
    public Inventory Inventory { get; set; } = new();
    public ushort[] Equipment { get; set; } = new ushort[10];
    public int Health { get; set; } = 100;
    public int Mana { get; set; } = 20;
    public string[] Cosmetics { get; set; } = Array.Empty<string>();
    public Vector2Int Position { get; set; }
}

/// <summary>
/// Serializer for the separate .plr file.
/// </summary>
public sealed class PlayerSerializer
{
    public void Save(PlayerSaveData player, Stream destination)
    {
        if (player is null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        using var writer = new BinaryWriter(destination, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(player.Health);
        writer.Write(player.Mana);
        writer.Write(player.Position.X);
        writer.Write(player.Position.Y);
        writer.Write(player.Equipment.Length);
        for (int i = 0; i < player.Equipment.Length; ++i)
        {
            writer.Write(player.Equipment[i]);
        }

        writer.Write(Inventory.SlotCount);
        for (int i = 0; i < Inventory.SlotCount; ++i)
        {
            var item = player.Inventory[i];
            writer.Write(item.Type);
            writer.Write(item.Stack);
            writer.Write(item.Prefix);
        }

        writer.Write(player.Cosmetics.Length);
        for (int i = 0; i < player.Cosmetics.Length; ++i)
        {
            writer.Write(player.Cosmetics[i] ?? string.Empty);
        }
    }

    public PlayerSaveData Load(Stream source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        using var reader = new BinaryReader(source, System.Text.Encoding.UTF8, leaveOpen: true);
        var player = new PlayerSaveData
        {
            Health = reader.ReadInt32(),
            Mana = reader.ReadInt32(),
            Position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32())
        };

        int equipmentCount = reader.ReadInt32();
        player.Equipment = new ushort[equipmentCount];
        for (int i = 0; i < equipmentCount; ++i)
        {
            player.Equipment[i] = reader.ReadUInt16();
        }

        int slotCount = reader.ReadInt32();
        player.Inventory = new Inventory();
        for (int i = 0; i < slotCount && i < Inventory.SlotCount; ++i)
        {
            ushort type = reader.ReadUInt16();
            short stack = reader.ReadInt16();
            byte prefix = reader.ReadByte();
            player.Inventory.Slots[i] = new ItemStack(type, stack, prefix);
        }

        int cosmeticCount = reader.ReadInt32();
        player.Cosmetics = new string[cosmeticCount];
        for (int i = 0; i < cosmeticCount; ++i)
        {
            player.Cosmetics[i] = reader.ReadString();
        }

        return player;
    }
}
