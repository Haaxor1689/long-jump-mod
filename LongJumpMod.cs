using Allumeria.DataManagement.Permissions;
using HarmonyLib;
using HarmonyLib.Tools;
using Ignitron.Aluminium.Assets;
using Ignitron.Aluminium.Assets.Providers;
using Ignitron.Aluminium.Events;
using Ignitron.Aluminium.Registries;
using Ignitron.Aluminium.Translation;
using Ignitron.Loader;
using LongJumpMod.Effects;
using LongJumpMod.Items;
using Logger = Allumeria.Logger;

namespace LongJumpMod;

public sealed class LongJumpMod : IModEntrypoint
{
    public void Main(ModBox box)
    {
#if DEBUG
        HarmonyFileLog.Enabled = true;
#endif
        // Apply harmony patches
        new Harmony($"{box.Metadata.Contributors.First().Name}.{box.Metadata.Id}").PatchAll();

        // Initialize asset manager for loading resources
        var assetManager = AssetManager.CreateDefault(box.RootPath, $"ignitron/{box.Metadata.Id}");

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
            box.Metadata.Id,
            new DefaultTranslator(
                assetManager.Load("translations/keys.txt", TranslationAssetProvider.Default)
            )
        );

        ContentRegistryEvents.Items += () =>
        {
            EffectRegistry.Initialize(box.Metadata.Id);
            ItemRegistry.Initialize(box.Metadata.Id);
        };

#if DEBUG
        // Enable creative menu and noclip for dev
        PlayerEvents.Spawned += (player, world) =>
        {
            player.permissions.permissions.TryGetValue(
                PermissionRegistry.allow_creative_menu.shortID,
                out var creativePerm
            );
            creativePerm?.SetValue(true);

            player.permissions.permissions.TryGetValue(
                PermissionRegistry.allow_noclip.shortID,
                out var noclipPerm
            );
            noclipPerm?.SetValue(true);
        };
#endif

        Logger.Init($"Initializing {box.Metadata.DisplayName}!");
    }
}
