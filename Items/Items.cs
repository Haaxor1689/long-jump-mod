using Allumeria;
using Allumeria.Items;
using Allumeria.Items.ItemTypes;
using Allumeria.Items.LootTables;
using Ignitron.Aluminium.Extensions;
using LongJumpMod.Effects;

namespace LongJumpMod.Items;

public static class ItemRegistry
{
    public static Item long_jump = null!;

    internal static void Initialize(string modId)
    {
        long_jump = ItemHelper
            .Create(() => new ItemTrinket($"{modId}.long_jump", EffectRegistry.long_jump))
            .SetItemSprite($"ignitron.{modId}.textures.atlas.items.long_jump")
            .SellValue(200);

        // Add long jump module to the forest dungeon iron chests as an extra otpion for trinket drop
        LootDescription
            .iron_chest.entries[0]
            .childEntries[0]
            .AddEntry(new LootFixedItem(long_jump, 1));

        // Also add as a very low chance drop from forest dungeon enemies for players who already generated the world
        LootDescription
            .forest_dungeon_enemy_loot.entries[0]
            .AddEntry(new LootChance(0.02f).AddEntry(new LootFixedItem(long_jump, 1)));
    }
}
