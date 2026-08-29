using Allumeria.Items;
using Allumeria.Items.ItemTypes;
using Ignitron.Aluminium.Extensions;
using LongJumpModule.Effects;

namespace LongJumpModule.Items;

public static class ItemRegistry
{
    public static Item long_jump = null!;

    internal static void Initialize()
    {
        long_jump = ItemHelper
            .Create(() => new ItemTrinket($"{Mod.ModId}.long_jump", EffectRegistry.long_jump))
            .SetItemSprite(Mod.ItemSpriteKey("long_jump"))
            .SellValue(200);
    }
}
