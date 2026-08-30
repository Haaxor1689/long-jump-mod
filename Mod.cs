using Allumeria.Items.LootTables;
using HarmonyLib;
using HarmonyLib.Tools;
using Ignitron.Aluminium.Assets;
using Ignitron.Aluminium.Assets.Providers;
using Ignitron.Aluminium.Events;
using Ignitron.Aluminium.Registries;
using Ignitron.Aluminium.Translation;
using Ignitron.Loader;
using LongJumpModule.Effects;
using LongJumpModule.Items;

namespace LongJumpModule;

public sealed class Mod : IModEntrypoint
{
    public const string ModId = "long_jump_mod";

    public void Main(ModBox box)
    {
#if DEBUG
        HarmonyFileLog.Enabled = true;
#endif
        // Apply harmony patches
        new Harmony($"{box.Metadata.Contributors.First().Name}.{box.Metadata.Id}").PatchAll();

        // Initialize asset manager for loading resources
        var assetManager = AssetManager.CreateDefault(box.RootPath, $"ignitron/{ModId}");

        // Register resources
        Allumeria.DataManagement.AssetLoading.AssetManager.blockAtlas.ScanDirectory(
            assetManager,
            "textures/atlas/blocks",
            16
        );
        Allumeria.DataManagement.AssetLoading.AssetManager.itemAtlas.ScanDirectory(
            assetManager,
            "textures/atlas/items",
            16
        );

        // Register translation keys
        AluminiumRegistries.Translators.Register(
            ModId,
            new DefaultTranslator(
                assetManager.Load("translations/keys.txt", TranslationAssetProvider.Default)
            )
        );

        ContentRegistryEvents.Items += () =>
        {
            EffectRegistry.Initialize();
            ItemRegistry.Initialize();

            // Add long jump module to the forest dungeon iron chests as an extra otpion for trinket drop
            LootDescription
                .iron_chest.entries[0]
                .childEntries[0]
                .AddEntry(new LootFixedItem(ItemRegistry.long_jump, 1));

            // Also add as a very low chance drop from forest dungeon enemies for players who already generated the world
            LootDescription
                .forest_dungeon_enemy_loot.entries[0]
                .AddEntry(
                    new LootChance(0.02f).AddEntry(new LootFixedItem(ItemRegistry.long_jump, 1))
                );
        };
    }

    internal static string ItemSpriteKey(string name) =>
        $"ignitron.{ModId}.textures.atlas.items.{name}";

    internal static string BlockSpriteKey(string name) =>
        $"ignitron.{ModId}.textures.atlas.blocks.{name}";

    internal static string UiSpriteKey(string name) => $"ignitron.{ModId}.textures.atlas.ui.{name}";

    internal static string ModelKey(string name) => $"ignitron.{ModId}.models.{name}";

    internal static string TextureKey(string name) => $"ignitron.{ModId}.textures.{name}";
}
